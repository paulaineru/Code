import re
from django import template
from django.utils.html import escape, mark_safe

register = template.Library()


@register.filter
def field_label(value):
    """Convert a snake_case key to a human-readable label: nssf_total → NSSF Total"""
    if not isinstance(value, str):
        return value
    # Split on underscores, uppercase known acronyms, title-case the rest
    ACRONYMS = {"nssf", "nhif", "paye", "vat", "id", "pr", "no"}
    parts = value.replace("-", "_").split("_")
    words = []
    for part in parts:
        if part.lower() in ACRONYMS:
            words.append(part.upper())
        else:
            words.append(part.capitalize())
    return " ".join(words)


@register.filter
def percent(value, decimals=1):
    """Convert a 0..1 confidence value to percent string without the '%' suffix."""
    try:
        return f"{float(value) * 100:.{int(decimals)}f}"
    except (TypeError, ValueError):
        return ""


@register.filter
def render_ocr_text(text, flags=None):
    """
    Convert raw OCR text into clean HTML.
    - Blank lines separate sections (rendered as visual gaps).
    - Short ALL-CAPS lines are treated as section headings.
    - Everything else is rendered line-by-line with readable spacing.
    - If flags (iterable of ReviewFlag) is provided, flagged words are highlighted.
    """
    if not text or not text.strip():
        return mark_safe('<p class="text-slate-400 italic text-sm">No text extracted.</p>')

    flag_items = []
    if flags is not None:
        try:
            flag_items = list(flags)
        except TypeError:
            flag_items = []

    flagged = {
        getattr(flag, "word", None)
        for flag in flag_items
        if getattr(flag, "word", None)
    }

    span_ranges = []
    seen_ranges = set()
    for flag in flag_items:
        offset = getattr(flag, "span_offset", None)
        length = getattr(flag, "span_length", None)
        if offset is None or not length:
            continue
        try:
            start = int(offset)
            end = start + int(length)
        except (TypeError, ValueError):
            continue
        if end <= start:
            continue
        key = (start, end)
        if key not in seen_ranges:
            span_ranges.append(key)
            seen_ranges.add(key)
    span_ranges.sort()

    merged_ranges = []
    for start, end in span_ranges:
        if not merged_ranges or start > merged_ranges[-1][1]:
            merged_ranges.append([start, end])
        else:
            merged_ranges[-1][1] = max(merged_ranges[-1][1], end)

    _highlight_pattern = (
        re.compile("(" + "|".join(re.escape(w) for w in sorted(flagged, key=len, reverse=True)) + ")")
        if flagged and not merged_ranges else None
    )

    def _render_line(raw_line, line_start):
        if merged_ranges:
            line_end = line_start + len(raw_line)
            cursor = 0
            result = []
            for start, end in merged_ranges:
                if end <= line_start or start >= line_end:
                    continue
                rel_start = max(start, line_start) - line_start
                rel_end = min(end, line_end) - line_start
                if rel_start < cursor:
                    rel_start = cursor
                if rel_end <= cursor:
                    continue
                result.append(escape(raw_line[cursor:rel_start]))
                result.append(
                    f'<mark class="bg-amber-100 text-amber-900 rounded px-0.5 font-medium not-italic">{escape(raw_line[rel_start:rel_end])}</mark>'
                )
                cursor = rel_end
            result.append(escape(raw_line[cursor:]))
            return "".join(result)

        if _highlight_pattern is None:
            return escape(raw_line)

        parts = _highlight_pattern.split(raw_line)
        result = []
        for part in parts:
            if part in flagged:
                result.append(
                    f'<mark class="bg-amber-100 text-amber-900 rounded px-0.5 font-medium not-italic">{escape(part)}</mark>'
                )
            else:
                result.append(escape(part))
        return "".join(result)

    def _is_heading(line):
        stripped = line.strip()
        if not stripped or len(stripped) > 80:
            return False
        alpha_chars = [c for c in stripped if c.isalpha()]
        if not alpha_chars:
            return False
        upper_ratio = sum(1 for c in alpha_chars if c.isupper()) / len(alpha_chars)
        return upper_ratio >= 0.75

    line_infos = []
    offset = 0
    for raw_line in text.splitlines(keepends=True):
        line_text = raw_line.rstrip("\r\n")
        line_infos.append((line_text, offset))
        offset += len(raw_line)
    if not line_infos and text:
        line_infos.append((text, 0))

    html_parts = []
    section_lines = []

    def _flush_section(lines):
        if not lines:
            return
        section_html = []
        for line, line_start in lines:
            rendered = _render_line(line, line_start)
            if _is_heading(line) and len(lines) <= 3:
                section_html.append(
                    f'<span class="block text-xs font-bold text-slate-700 uppercase tracking-wider">{rendered}</span>'
                )
            else:
                section_html.append(
                    f'<span class="block text-sm text-slate-700 leading-relaxed">{rendered}</span>'
                )
        html_parts.append(f'<div class="space-y-0.5">{"".join(section_html)}</div>')

    for line, line_start in line_infos:
        if not line.strip():
            _flush_section(section_lines)
            section_lines = []
            continue
        section_lines.append((line, line_start))

    _flush_section(section_lines)

    return mark_safe('<div class="space-y-3">' + "".join(html_parts) + "</div>")


@register.filter
def render_table_cell(cell):
    if isinstance(cell, dict):
        text = str(cell.get("content", "") or "")
        review_tokens = cell.get("review_tokens") or []
    else:
        text = str(cell or "")
        review_tokens = []

    if not text:
        return ""

    terms = []
    for token in review_tokens:
        value = token.get("word") if isinstance(token, dict) else getattr(token, "word", None)
        if value:
            terms.append(str(value))
    if not terms:
        return escape(text)

    pattern = re.compile("|".join(re.escape(term) for term in sorted(set(terms), key=len, reverse=True)), re.IGNORECASE)
    cursor = 0
    result = []
    for match in pattern.finditer(text):
        start, end = match.span()
        if start < cursor:
            continue
        result.append(escape(text[cursor:start]))
        result.append(
            f'<mark class="bg-amber-100 text-amber-900 rounded px-0.5 font-medium">{escape(text[start:end])}</mark>'
        )
        cursor = end
    result.append(escape(text[cursor:]))
    return mark_safe("".join(result))


@register.filter
def unreviewed_count(flags):
    """Count items with reviewed=False in an iterable of flag-like objects."""
    try:
        return sum(1 for flag in flags if not getattr(flag, "reviewed", False))
    except TypeError:
        return 0


@register.filter
def reviewed_flags(job):
    """Return all ReviewFlag instances that have been reviewed for a job."""
    from documents.models import ReviewFlag
    return ReviewFlag.objects.filter(page_result__job=job, reviewed=True).select_related("reviewed_by")
