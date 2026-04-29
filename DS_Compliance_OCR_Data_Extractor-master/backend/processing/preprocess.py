"""
Image preprocessing pipeline — ZERO Django imports.
All functions operate on bytes or numpy arrays.
"""
import math
import io

import cv2
import numpy as np


def preprocess(bgr: np.ndarray) -> np.ndarray:
    """
    Full preprocessing pipeline:
    1. Grayscale
    2. Fast Non-Local Means Denoising
    3. CLAHE contrast enhancement
    4. Deskew via largest contour / minAreaRect
    5. Auto-crop to content
    Returns a BGR array (single channel converted back for consistency).
    """
    # 1. Grayscale
    gray = cv2.cvtColor(bgr, cv2.COLOR_BGR2GRAY)

    # 2. Denoise
    denoised = cv2.fastNlMeansDenoising(gray, h=10, templateWindowSize=7, searchWindowSize=21)

    # 3. CLAHE
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    enhanced = clahe.apply(denoised)

    # 4. Deskew
    deskewed = _deskew(enhanced)

    # 5. Auto-crop
    cropped = _autocrop(deskewed)

    # Convert back to BGR so downstream callers always get 3-channel
    return cv2.cvtColor(cropped, cv2.COLOR_GRAY2BGR)


def _deskew(gray: np.ndarray) -> np.ndarray:
    """Rotate image to correct skew using largest contour approach."""
    try:
        _, thresh = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        if not contours:
            return gray
        largest = max(contours, key=cv2.contourArea)
        if cv2.contourArea(largest) < 100:
            return gray
        rect = cv2.minAreaRect(largest)
        angle = rect[2]
        if angle < -45:
            angle += 90
        if abs(angle) < 0.5:  # skip trivial corrections
            return gray
        h, w = gray.shape
        M = cv2.getRotationMatrix2D((w / 2, h / 2), angle, 1.0)
        return cv2.warpAffine(gray, M, (w, h), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_REPLICATE)
    except Exception:
        return gray


def _autocrop(gray: np.ndarray) -> np.ndarray:
    """Trim white borders."""
    try:
        _, thresh = cv2.threshold(gray, 250, 255, cv2.THRESH_BINARY_INV)
        coords = cv2.findNonZero(thresh)
        if coords is None:
            return gray
        x, y, w, h = cv2.boundingRect(coords)
        margin = 10
        x = max(0, x - margin)
        y = max(0, y - margin)
        w = min(gray.shape[1] - x, w + 2 * margin)
        h = min(gray.shape[0] - y, h + 2 * margin)
        return gray[y : y + h, x : x + w]
    except Exception:
        return gray


def compress_for_azure(image: np.ndarray, max_bytes: int = 4 * 1024 * 1024):
    """
    Return (bytes, content_type) fitting within max_bytes.
    Strategy: PNG first; then JPEG 95→80→60; then halve dimensions up to 3 rounds.
    """
    # Try PNG
    ok, buf = cv2.imencode(".png", image)
    if ok and len(buf.tobytes()) <= max_bytes:
        return buf.tobytes(), "image/png"

    # JPEG with decreasing quality
    current = image.copy()
    for _ in range(3):  # up to 3 dimension-halving rounds
        for quality in (95, 80, 60):
            ok, buf = cv2.imencode(".jpg", current, [cv2.IMWRITE_JPEG_QUALITY, quality])
            if ok and len(buf.tobytes()) <= max_bytes:
                return buf.tobytes(), "image/jpeg"
        # Halve dimensions
        h, w = current.shape[:2]
        current = cv2.resize(current, (w // 2, h // 2), interpolation=cv2.INTER_AREA)

    # Last resort: return whatever we have
    ok, buf = cv2.imencode(".jpg", current, [cv2.IMWRITE_JPEG_QUALITY, 60])
    return buf.tobytes(), "image/jpeg"


def pdf_to_bgr_images(pdf_bytes: bytes, dpi: int = 200) -> list:
    """
    Render each PDF page at `dpi` and return list of BGR numpy arrays.
    Tries PyMuPDF first, falls back to pypdfium2.
    """
    try:
        return _pdf_via_pymupdf(pdf_bytes, dpi)
    except ImportError:
        pass
    try:
        return _pdf_via_pypdfium2(pdf_bytes, dpi)
    except ImportError:
        raise RuntimeError("Neither PyMuPDF nor pypdfium2 is installed — cannot render PDF pages.")


def _pdf_via_pymupdf(pdf_bytes: bytes, dpi: int) -> list:
    import fitz  # PyMuPDF

    doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    scale = dpi / 72.0
    mat = fitz.Matrix(scale, scale)
    pages = []
    for page in doc:
        pix = page.get_pixmap(matrix=mat, colorspace=fitz.csRGB)
        arr = np.frombuffer(pix.samples, dtype=np.uint8).reshape(pix.height, pix.width, 3)
        pages.append(cv2.cvtColor(arr, cv2.COLOR_RGB2BGR))
    doc.close()
    return pages


def _pdf_via_pypdfium2(pdf_bytes: bytes, dpi: int) -> list:
    import pypdfium2 as pdfium

    doc = pdfium.PdfDocument(pdf_bytes)
    scale = dpi / 72.0
    pages = []
    for i in range(len(doc)):
        page = doc[i]
        bitmap = page.render(scale=scale, rotation=0)
        pil_img = bitmap.to_pil()
        arr = np.array(pil_img.convert("RGB"))
        pages.append(cv2.cvtColor(arr, cv2.COLOR_RGB2BGR))
    return pages
