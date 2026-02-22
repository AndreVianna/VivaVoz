# TASK-1.16: App Icon

**Delivery:** 1c
**Priority:** Medium — needed for system tray, Store listing, and branding
**PRD Reference:** Section 7.6 (App Icon)

---

## Summary

Design and implement the VivaVoz app icon.

## Requirements

### Sizes Needed
- 16x16 (system tray)
- 32x32 (taskbar)
- 48x48 (Windows alt-tab)
- 256x256 (Store listing, about dialog)
- 1024x1024 (marketing, website)
- ICO file containing all Windows sizes

### Design Constraints
- Must convey "voice" or "sound" — avoid generic microphone if possible
- Must be readable at 16x16 (simple shapes, high contrast)
- Must work on both light and dark backgrounds
- Should feel modern, clean, consistent with Fluent design language
- Brand colors TBD — suggest candidates

### Deliverables
- SVG source file
- ICO file (multi-resolution) for Windows
- PNG exports at all required sizes
- Placed in: `source/VivaVoz/Assets/` (for the app) and `documents/branding/` (for reference)

## Approach

⚠️ **This task requires human creative input before coding.** The elfos cannot design an icon — they can only embed one.

**Split this task:**
- **1.16a (Design):** Andre + Lola brainstorm icon concepts, generate candidates, pick one. Could use the brainstorm skill for ideation (like Matt Maher did for Glade).
- **1.16b (Implementation):** Embed chosen icon into app assets, tray, title bar. This part goes to the elfos.

Options for 1.16a:
1. **AI-generated + manually refined** — use image gen to explore concepts, pick best, clean up in vector editor
2. **Professional designer** — commission via Fiverr/99designs
3. **Andre designs it** — if he has a vision
4. **Brainstorm skill** — run a multi-agent ideation session for icon concepts

Decision: **Option 4 — Brainstorm skill (manual, sub-agents broken) + Gemini image gen.**

### 1.16a Result (2026-02-22)

**Concept:** "O Bem-Te-Vi" — geometric Great Kiskadee (bem-te-vi) bird singing with sound waves.
**Origin:** Dream 21 ("O Espelho Que Não Reflete") — the bem-te-vi appeared as a symbol of voice and existence.
**Colors:** Golden yellow (#F5D000) body, dark charcoal (#1E1E1E) head with yellow crest, coral/orange (#FF6B35) sound waves.
**Style:** Flat geometric, 6-7 shapes max, Windows 11 Fluent Design compatible.
**Generated with:** Gemini 3 Pro Image (nano-banana-pro skill), 10+ iterations with Andre's feedback.

**Assets ready in `source/VivaVoz/Assets/`:**
- `vivavoz.ico` (multi-res: 16+32+48+256)
- `vivavoz-{16,32,48,256,1024}x{size}.png`
- `vivavoz-mono-{16,32}x{size}.png` (system tray)

**Concept files:** `documents/branding/` (all iterations preserved)

**Status: ✅ TASK-1.16a COMPLETE**

## Acceptance Criteria

- [ ] Icon exists in all required sizes
- [ ] ICO file embedded in application
- [ ] Tray icon uses the new icon
- [ ] Main window title bar uses the new icon
- [ ] Reads well at 16x16 and 256x256
