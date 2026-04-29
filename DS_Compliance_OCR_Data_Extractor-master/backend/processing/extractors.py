"""
Domain-specific field extractors — ZERO Django imports.
Registry pattern: @register("doc_type") decorates extractor functions.
Each extractor receives an AnalysisResult and returns a dict of extracted fields.
"""
import re

_EXTRACTORS: dict = {}


def register(doc_type: str):
    def decorator(fn):
        _EXTRACTORS[doc_type] = fn
        return fn
    return decorator


def run_extraction(doc_type: str, result) -> dict:
    """Run the registered extractor for doc_type, falling back to empty dict."""
    extractor = _EXTRACTORS.get(doc_type)
    if extractor is None:
        return {}
    try:
        return extractor(result) or {}
    except Exception:
        return {}


# ── Extractors ────────────────────────────────────────────────────────────────

@register("bank_statement")
def _extract_bank_statement(result) -> dict:
    text = result.full_text

    def first(pattern, flags=re.IGNORECASE):
        m = re.search(pattern, text, flags)
        return m.group(1).strip() if m else None

    return {
        k: v for k, v in {
            "iban": first(r"\b([A-Z]{2}\d{2}[A-Z0-9]{1,30})\b"),
            "account_number": first(r"(?:account\s*(?:no\.?|number)[:\s]+)([0-9\-]{6,20})", re.IGNORECASE),
            "sort_code": first(r"(?:sort\s*code[:\s]+)(\d{2}[-–]\d{2}[-–]\d{2})", re.IGNORECASE),
            "statement_period": first(
                r"(?:statement\s*period|period)[:\s]+([A-Za-z0-9\s,/\-–]+?)(?:\n|$)", re.IGNORECASE
            ),
        }.items()
        if v is not None
    }


@register("receipt")
def _extract_receipt(result) -> dict:
    text = result.full_text

    def first(pattern, flags=re.IGNORECASE):
        m = re.search(pattern, text, flags)
        return m.group(1).strip() if m else None

    return {
        k: v for k, v in {
            "receipt_number": first(r"(?:receipt|transaction)\s*(?:no\.?|number|#)[:\s]+([A-Z0-9\-]{3,30})", re.IGNORECASE),
            "total_amount": first(r"(?:total|amount)[:\s]+([0-9,\.]+)", re.IGNORECASE),
            "date": first(r"(?:date)[:\s]+(\d{1,2}[\/\-]\d{1,2}[\/\-]\d{2,4})"),
        }.items()
        if v is not None
    }


@register("invoice")
def _extract_invoice(result) -> dict:
    text = result.full_text

    def first(pattern, flags=re.IGNORECASE):
        m = re.search(pattern, text, flags)
        return m.group(1).strip() if m else None

    return {
        k: v for k, v in {
            "invoice_number": first(r"invoice\s*(?:no\.?|number|#)[:\s]+([A-Z0-9\-/]{2,30})", re.IGNORECASE),
            "total_amount": first(r"(?:total|amount\s*due)[:\s]+([0-9,\.]+)", re.IGNORECASE),
            "due_date": first(r"(?:due\s*date|payment\s*due)[:\s]+(\d{1,2}[\/\-]\d{1,2}[\/\-]\d{2,4})"),
        }.items()
        if v is not None
    }


@register("payroll")
def _extract_payroll(result) -> dict:
    """
    East African payroll header fields and summary totals.

    extracted_fields contains document-level / page-level data:
      employer_name, payroll_title, payment_sheet_number, project_site,
      month, year, employee_count, total_gross_pay, total_nssf_deduction,
      total_net_pay.

    Per-employee row data (name, serial, NSSF no., days, gross pay, net pay,
    etc.) is already captured in tables_json and shown in the Tables tab.
    """
    text = result.full_text

    def first(pattern, flags=re.IGNORECASE) -> str | None:
        m = re.search(pattern, text, flags)
        return m.group(1).strip() if m else None

    def money(pattern, flags=re.IGNORECASE) -> str | None:
        m = re.search(pattern, text, flags)
        if not m:
            return None
        return m.group(1).replace(",", "").strip()

    # ── Document header fields ────────────────────────────────────────────────

    employer_name = first(
        r"(?:employer\s*name|company\s*name|company|employer)\s*[:\-]\s*"
        r"([A-Z][A-Za-z0-9\s&,\.\(\)\-]{2,60}?)(?=\s*(?:\n|employer\s*nssf|nssf\s*no|$))"
    )

    # Payroll title: "PAYROLL FOR THE MONTH OF …" or "PAYROLL TYPE: …"
    payroll_title = first(
        r"(?:payroll\s*(?:title|type|for\s+the\s+month\s+of)[:\s]+)([A-Za-z0-9\s\-\/]{2,50}?)(?=\s*(?:\n|$))"
    ) or first(r"^((?:MONTHLY|WEEKLY|CASUAL|DAILY|ANNUAL)\s+PAYROLL)\b", re.IGNORECASE | re.MULTILINE)

    payment_sheet_number = first(
        r"(?:payment\s*sheet\s*(?:no\.?|number|#)|sheet\s*(?:no\.?|number|#)|"
        r"payroll\s*(?:no\.?|number|#)|p/?r\s*(?:no\.?|number|#))\s*[:\-]?\s*([A-Z0-9\-/]{1,20})"
    )

    project_site = first(
        r"(?:project|site|area|location|zone|region)\s*[:/]\s*"
        r"([A-Za-z0-9\s,\.\-]{2,60}?)(?=\s*(?:\n|$))"
    )

    # ── Period fields ─────────────────────────────────────────────────────────

    _MONTHS = (
        r"(?:January|February|March|April|May|June|"
        r"July|August|September|October|November|December)"
    )
    month = first(
        rf"(?:month\s*(?:of\s*)?[:\-]\s*|for\s+the\s+month\s+of\s+)({_MONTHS})",
        re.IGNORECASE,
    ) or first(
        rf"({_MONTHS})\s+\d{{4}}",
        re.IGNORECASE,
    )

    year = first(r"(?:year)\s*[:\-]\s*(\d{4})", re.IGNORECASE) or first(
        rf"(?:{_MONTHS})[,\s]+(\d{{4}})", re.IGNORECASE
    )

    # ── Summary totals ────────────────────────────────────────────────────────

    total_gross_pay = money(
        r"(?:total\s*gross\s*(?:pay|salary|wage|earnings?)|gross\s*(?:pay\s*)?total)"
        r"[\s:]+([0-9,\.]+)"
    )

    total_nssf_deduction = money(
        r"(?:total\s*nssf\s*(?:deductions?|contributions?|amount)|nssf\s*(?:deductions?\s*)?total)"
        r"[\s:]+([0-9,\.]+)"
    ) or money(r"nssf[\s:]+([0-9,\.]+)")

    total_net_pay = money(
        r"(?:total\s*net\s*(?:pay|salary)|net\s*(?:pay\s*)?total)"
        r"[\s:]+([0-9,\.]+)"
    )

    # ── Employee count from tables ────────────────────────────────────────────

    employee_count = None
    if result.tables:
        # Use the largest table; skip the header row and any trailing total rows.
        largest = max(result.tables, key=lambda t: len(t), default=None)
        if largest and len(largest) > 1:
            data_rows = [
                row for row in largest[1:]
                if any((cell.get("content") or "").strip() for cell in row)
                and not re.search(
                    r"\b(?:total|subtotal|grand\s*total)\b",
                    " ".join((cell.get("content") or "") for cell in row),
                    re.IGNORECASE,
                )
            ]
            if data_rows:
                employee_count = str(len(data_rows))

    return {
        k: v
        for k, v in {
            "employer_name": employer_name,
            "payroll_title": payroll_title,
            "payment_sheet_number": payment_sheet_number,
            "project_site": project_site,
            "month": month,
            "year": year,
            "employee_count": employee_count,
            "total_gross_pay": total_gross_pay,
            "total_nssf_deduction": total_nssf_deduction,
            "total_net_pay": total_net_pay,
        }.items()
        if v is not None
    }


# generic/layout — no domain extraction
@register("generic")
def _extract_generic(result) -> dict:
    return {}
