/**
 * Oryx — app.js
 * Tab switching, confidence overlay, page navigation.
 */

// ── Confidence overlay ─────────────────────────────────────────────────────

function initConfidenceOverlay() {
  const toggle = document.getElementById('overlay-toggle');
  const overlayContainer = document.getElementById('confidence-overlay');
  if (!toggle || !overlayContainer) return;

  toggle.addEventListener('change', () => {
    overlayContainer.style.display = toggle.checked ? 'block' : 'none';
  });

  // Hover link: review row → polygon highlight
  document.querySelectorAll('.review-row[data-flag-id]').forEach(row => {
    const flagId = row.dataset.flagId;
    const rect = overlayContainer.querySelector(`[data-flag="${flagId}"]`);
    if (!rect) return;

    row.addEventListener('mouseenter', () => rect.classList.add('opacity-100'));
    row.addEventListener('mouseleave', () => rect.classList.remove('opacity-100'));
    rect.addEventListener('mouseenter', () => row.classList.add('bg-amber-50'));
    rect.addEventListener('mouseleave', () => row.classList.remove('bg-amber-50'));
  });
}

document.addEventListener('DOMContentLoaded', initConfidenceOverlay);
