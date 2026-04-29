from collections import defaultdict
import re


def spans_overlap(offset_a, length_a, offset_b, length_b):
    try:
        start_a = int(offset_a)
        end_a = start_a + int(length_a)
        start_b = int(offset_b)
        end_b = start_b + int(length_b)
    except (TypeError, ValueError):
        return False
    if end_a <= start_a or end_b <= start_b:
        return False
    return start_a < end_b and start_b < end_a


def serialize_flag(flag):
    page_result = getattr(flag, "page_result", None)
    return {
        "word": getattr(flag, "word", "") or "",
        "confidence": getattr(flag, "confidence", 0.0) or 0.0,
        "reviewed": bool(getattr(flag, "reviewed", False)),
        "corrected_value": getattr(flag, "corrected_value", "") or "",
        "page_number": getattr(page_result, "page_number", None),
        "span_offset": getattr(flag, "span_offset", None),
        "span_length": getattr(flag, "span_length", None),
        "table_index": getattr(flag, "table_index", None),
        "row_index": getattr(flag, "row_index", None),
        "column_index": getattr(flag, "column_index", None),
    }


def build_review_indexes(review_flags, unresolved_only=True):
    text_ranges_by_page = defaultdict(list)
    cell_flags_by_page = defaultdict(lambda: defaultdict(list))

    for flag in review_flags or []:
        item = serialize_flag(flag)
        if unresolved_only and item["reviewed"]:
            continue

        page_number = item["page_number"]
        if page_number is None:
            continue

        if item["span_offset"] is not None and item["span_length"]:
            text_ranges_by_page[page_number].append(item)

        if None not in (item["table_index"], item["row_index"], item["column_index"]):
            locator = (int(item["table_index"]), int(item["row_index"]), int(item["column_index"]))
            cell_flags_by_page[page_number][locator].append(item)

    for page_number in text_ranges_by_page:
        text_ranges_by_page[page_number].sort(
            key=lambda item: (int(item["span_offset"]), int(item["span_length"]))
        )

    return text_ranges_by_page, cell_flags_by_page


def collect_legacy_cell_flags(review_flags, page_number, cell_text, unresolved_only=True):
    if not cell_text:
        return []

    matches = []
    haystack = str(cell_text).strip()
    if not haystack:
        return matches

    for flag in review_flags or []:
        item = serialize_flag(flag)
        if unresolved_only and item["reviewed"]:
            continue
        if item["page_number"] != page_number:
            continue
        if None not in (item["table_index"], item["row_index"], item["column_index"]):
            continue
        word = (item.get("word") or "").strip()
        if not word:
            continue
        pattern = re.compile(re.escape(word), re.IGNORECASE)
        if pattern.search(haystack):
            matches.append(item)

    return matches


def annotate_tables_with_review_state(page_number, tables, review_flags, unresolved_only=True):
    _, cell_flags_by_page = build_review_indexes(review_flags, unresolved_only=unresolved_only)
    page_cells = cell_flags_by_page.get(page_number, {})
    annotated_tables = []

    for table_idx, table in enumerate(tables or [], start=1):
        annotated_rows = []
        for row_idx, row in enumerate(table or []):
            annotated_cells = []
            for col_idx, cell in enumerate(row or []):
                if isinstance(cell, dict):
                    cell_data = dict(cell)
                else:
                    cell_data = {"content": str(cell or "")}

                locator = (table_idx, row_idx, col_idx)
                cell_flags = list(page_cells.get(locator, []))
                if not cell_flags:
                    cell_flags = collect_legacy_cell_flags(
                        review_flags,
                        page_number,
                        cell_data.get("content", ""),
                        unresolved_only=unresolved_only,
                    )
                token_summary = [
                    f'{flag["word"]} ({float(flag["confidence"]) * 100:.1f}%)'
                    for flag in cell_flags
                    if flag.get("word")
                ]

                cell_data.setdefault("table_index", table_idx)
                cell_data.setdefault("row_index", row_idx)
                cell_data.setdefault("column_index", col_idx)
                cell_data["needs_review"] = bool(cell_flags)
                cell_data["review_tokens"] = cell_flags
                cell_data["review_title"] = (
                    "Low-confidence OCR: " + ", ".join(token_summary) if token_summary else ""
                )
                annotated_cells.append(cell_data)
            annotated_rows.append(annotated_cells)
        annotated_tables.append(annotated_rows)

    return annotated_tables
