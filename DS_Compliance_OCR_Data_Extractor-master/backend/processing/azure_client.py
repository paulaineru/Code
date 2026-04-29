"""
Azure Document Intelligence client - ZERO Django imports.
Operates on bytes, returns dataclasses.
"""
import re
import time
from dataclasses import dataclass, field
from typing import Any

from processing.review_annotations import spans_overlap


_MODEL_MAP = {
    "generic": "prebuilt-layout",
    "receipt": "prebuilt-receipt",
    "invoice": "prebuilt-invoice",
    "bank_statement": "prebuilt-layout",
    "payroll": "prebuilt-layout",
}

_CLIENT_CACHE: dict = {}  # keyed by (endpoint, key) so credential changes take effect

_MONEY_HEADER_RE = re.compile(
    r"\b(amount|pay|salary|total|gross|net|balance|value|price|cost|tax|fee|deduction|contribution|nssf|nhif|paye)\b",
    re.IGNORECASE,
)
_NAME_HEADER_RE = re.compile(r"\b(name|employee|payee|employer|member)\b", re.IGNORECASE)
_CURRENCY_MARKER_RE = re.compile(r"[$\u20ac\u00a3\u00a5]|(?:USD|UGX|KES|TZS|RWF|EUR|GBP)\b", re.IGNORECASE)
_LEADING_CURRENCY_CODE_RE = re.compile(r"^(?:USD|UGX|KES|TZS|RWF|EUR|GBP)\s*", re.IGNORECASE)
_TRAILING_CURRENCY_CODE_RE = re.compile(r"\s*(?:USD|UGX|KES|TZS|RWF|EUR|GBP)$", re.IGNORECASE)
_EDGE_SYMBOL_RE = re.compile(r"^[\s=#$%&/\u00a3\u20ac\u00a5]+|[\s=#$%&/\u00a3\u20ac\u00a5]+$")
_NUMERIC_RE = re.compile(r"^-?[\d,]+(?:\.\d+)?$")


def get_client(endpoint: str, key: str):
    cache_key = (endpoint, key)
    if cache_key not in _CLIENT_CACHE:
        from azure.ai.documentintelligence import DocumentIntelligenceClient
        from azure.core.credentials import AzureKeyCredential

        _CLIENT_CACHE[cache_key] = DocumentIntelligenceClient(
            endpoint=endpoint, credential=AzureKeyCredential(key)
        )
    return _CLIENT_CACHE[cache_key]


@dataclass
class AnalysisResult:
    words: list = field(default_factory=list)
    tables: list = field(default_factory=list)
    full_text: str = ""
    avg_confidence: float = 0.0
    low_confidence_words: list = field(default_factory=list)
    extracted_fields: dict = field(default_factory=dict)
    raw_response: Any = None
    latency_s: float = 0.0


def analyse(
    image_bytes: bytes,
    content_type: str,
    doc_type: str,
    threshold: float,
    endpoint: str = "",
    key: str = "",
) -> AnalysisResult:
    """
    Call Azure Document Intelligence and return an AnalysisResult.
    endpoint and key must be supplied by the caller (read from SystemConfig or settings).
    doc_type must be one of the keys in _MODEL_MAP.
    threshold: words with confidence below this are flagged as low-confidence.
    """

    model_id = _MODEL_MAP.get(doc_type, "prebuilt-layout")
    client = get_client(endpoint, key)

    t0 = time.perf_counter()

    poller = client.begin_analyze_document(
        model_id,
        body=image_bytes,
        content_type=content_type,
        string_index_type="unicodeCodePoint",
    )
    result = poller.result()
    latency = time.perf_counter() - t0

    words = []
    low_conf = []
    texts = []

    if result.pages:
        for page in result.pages:
            if page.words:
                for w in page.words:
                    conf = w.confidence if w.confidence is not None else 1.0
                    polygon = _extract_polygon(w.polygon) if hasattr(w, "polygon") else None
                    span = getattr(w, "span", None)
                    entry = {
                        "text": w.content,
                        "confidence": conf,
                        "polygon": polygon,
                        "span_offset": getattr(span, "offset", None),
                        "span_length": getattr(span, "length", None),
                        "page_number": getattr(page, "page_number", None),
                    }
                    words.append(entry)
                    if conf < threshold:
                        low_conf.append(dict(entry))
            if page.lines:
                for line in page.lines:
                    texts.append(line.content)

    tables = _extract_tables(result, words, threshold)
    for entry in low_conf:
        entry.update(_find_table_cell_for_word(entry, tables))
    avg_conf = (sum(w["confidence"] for w in words) / len(words)) if words else 0.0

    # Try to get extracted fields from prebuilt models
    extracted_fields = {}
    if hasattr(result, "documents") and result.documents:
        for doc in result.documents:
            if hasattr(doc, "fields") and doc.fields:
                for k, v in doc.fields.items():
                    if hasattr(v, "content") and v.content:
                        content = str(v.content)
                        extracted_fields[k] = content.upper() if _is_name_header(k) else content

    return AnalysisResult(
        words=words,
        tables=tables,
        full_text=getattr(result, "content", "") or "\n".join(texts),
        avg_confidence=round(avg_conf, 4),
        low_confidence_words=low_conf,
        extracted_fields=extracted_fields,
        raw_response=None,  # skip serializing the full response
        latency_s=round(latency, 3),
    )


def _extract_polygon(polygon) -> list:
    """Normalize Azure polygon to a flat list of [x, y, ...] coords."""
    if polygon is None:
        return None
    try:
        if hasattr(polygon[0], "x"):
            return [coord for pt in polygon for coord in (pt.x, pt.y)]
        return list(polygon)
    except Exception:
        return None


def _extract_spans(spans) -> list:
    result = []
    for span in spans or []:
        offset = getattr(span, "offset", None)
        length = getattr(span, "length", None)
        if offset is None or length is None:
            continue
        result.append({"offset": int(offset), "length": int(length)})
    return result


def _extract_bounding_polygon(bounding_regions) -> list | None:
    if not bounding_regions:
        return None
    try:
        return _extract_polygon(bounding_regions[0].polygon)
    except Exception:
        return None


def _is_money_header(value: str) -> bool:
    return bool(_MONEY_HEADER_RE.search((value or "").strip()))


def _is_name_header(value: str) -> bool:
    return bool(_NAME_HEADER_RE.search((value or "").strip()))


def _clean_money_cell(value: str) -> str:
    raw = (value or "").strip()
    if not raw:
        return ""

    cleaned = _LEADING_CURRENCY_CODE_RE.sub("", raw)
    cleaned = _TRAILING_CURRENCY_CODE_RE.sub("", cleaned)
    cleaned = _EDGE_SYMBOL_RE.sub("", cleaned).strip()
    return cleaned if _NUMERIC_RE.match(cleaned) else raw


def _looks_money_column(values: list[str]) -> bool:
    for value in values:
        raw = (value or "").strip()
        if not raw:
            continue
        if _CURRENCY_MARKER_RE.search(raw):
            return True
        cleaned = _clean_money_cell(raw)
        if cleaned != raw and _NUMERIC_RE.match(cleaned):
            return True
    return False


def _extract_tables(result, words: list, threshold: float) -> list:
    """Convert Azure tables to list-of-rows-of-cells dicts."""
    tables = []
    if not result.tables:
        return tables

    for table_idx, tbl in enumerate(result.tables, start=1):
        grid = {}
        table_page_number = None
        if getattr(tbl, "bounding_regions", None):
            table_page_number = getattr(tbl.bounding_regions[0], "page_number", None)

        for cell in tbl.cells:
            r, c = cell.row_index, cell.column_index
            if r not in grid:
                grid[r] = {}
            cell_page_number = table_page_number
            if getattr(cell, "bounding_regions", None):
                cell_page_number = getattr(cell.bounding_regions[0], "page_number", None) or cell_page_number

            cell_spans = _extract_spans(getattr(cell, "spans", None))
            cell_words = [
                {
                    "text": word["text"],
                    "confidence": word["confidence"],
                    "span_offset": word.get("span_offset"),
                    "span_length": word.get("span_length"),
                    "polygon": word.get("polygon"),
                }
                for word in words
                if (cell_page_number is None or word.get("page_number") == cell_page_number)
                and any(
                    spans_overlap(
                        word.get("span_offset"),
                        word.get("span_length"),
                        span["offset"],
                        span["length"],
                    )
                    for span in cell_spans
                )
            ]
            flagged_tokens = [
                token
                for token in cell_words
                if float(token.get("confidence", 1.0)) < threshold
            ]

            grid[r][c] = {
                "content": (cell.content or "").strip(),
                "raw_content": (cell.content or "").strip(),
                "table_index": table_idx,
                "row_index": r,
                "column_index": c,
                "row_span": getattr(cell, "row_span", None),
                "column_span": getattr(cell, "column_span", None),
                "kind": str(getattr(cell, "kind", "") or ""),
                "page_number": cell_page_number,
                "spans": cell_spans,
                "polygon": _extract_bounding_polygon(getattr(cell, "bounding_regions", None)),
                "flagged_tokens": flagged_tokens,
                "has_low_confidence": bool(flagged_tokens),
                "min_confidence": min((token["confidence"] for token in flagged_tokens), default=None),
            }

        raw_rows = []
        row_count = max(getattr(tbl, "row_count", 0), (max(grid.keys()) + 1) if grid else 0)
        row_indexes = range(row_count)
        for r in row_indexes:
            row_dict = grid.get(r, {})
            row = []
            for c in range(tbl.column_count):
                cell_data = dict(
                    row_dict.get(
                        c,
                        {
                            "content": "",
                            "raw_content": "",
                            "table_index": table_idx,
                            "row_index": r,
                            "column_index": c,
                            "page_number": table_page_number,
                            "spans": [],
                            "flagged_tokens": [],
                            "has_low_confidence": False,
                            "min_confidence": None,
                        },
                    )
                )
                row.append(cell_data)
            raw_rows.append(row)

        headers = [str(cell.get("raw_content", "")) for cell in (raw_rows[0] if raw_rows else [])]
        money_cols = {idx for idx, header in enumerate(headers) if _is_money_header(header)}
        name_cols = {idx for idx, header in enumerate(headers) if _is_name_header(header)}

        for c in range(tbl.column_count):
            if c in money_cols:
                continue
            col_values = [str(row[c].get("raw_content", "")) for row in raw_rows if c < len(row)]
            if _looks_money_column(col_values):
                money_cols.add(c)

        rows = []
        for row_idx, row in enumerate(raw_rows):
            cleaned_row = []
            for idx, cell_data in enumerate(row):
                raw_value = str(cell_data.get("raw_content", ""))
                content = _clean_money_cell(raw_value) if idx in money_cols else raw_value
                if idx in name_cols and row_idx > 0 and content:
                    content = content.upper()
                merged = dict(cell_data)
                merged["content"] = content
                cleaned_row.append(merged)
            rows.append(cleaned_row)

        tables.append(rows)

    return tables


def _find_table_cell_for_word(word: dict, tables: list) -> dict:
    page_number = word.get("page_number")
    offset = word.get("span_offset")
    length = word.get("span_length")
    if page_number is None or offset is None or not length:
        return {}

    for table in tables or []:
        for row in table or []:
            for cell in row or []:
                if not isinstance(cell, dict):
                    continue
                if cell.get("page_number") not in (None, page_number):
                    continue
                for span in cell.get("spans", []):
                    if spans_overlap(offset, length, span.get("offset"), span.get("length")):
                        return {
                            "table_index": cell.get("table_index"),
                            "row_index": cell.get("row_index"),
                            "column_index": cell.get("column_index"),
                        }
    return {}
