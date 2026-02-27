# VivaVoz — Product Requirements Document (v1.1)

**Product:** VivaVoz
**Tagline:** Your voice, alive. For everyone.
**Version:** 1.1
**Date:** 2026-02-27
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs
**Price:** $1.99 (Microsoft Store)
**Previous:** [PRD-v1.0.md](PRD-v1.0.md) — DELIVERED 2026-02-27

---

## Change Log

| Version | Date       | Author                       | Changes                                     |
|---------|------------|------------------------------|---------------------------------------------|
| 1.1.0   | 2026-02-27 | Andre Vianna / Lola Lovelace | Initial v1.1 PRD — multi-arch + Store accessibility relisting |
| 1.1.1   | 2026-02-27 | Andre Vianna / Lola Lovelace | Scoped down: moved hotkeys, accessibility UI, grammar correction to v1.2 |

---

## 1. What Changed Since v1.0

v1.0 shipped x64 only with generic "productivity" positioning. v1.1 is a **quick turnaround release** that:
1. Adds ARM64 and x86 builds (broader hardware coverage)
2. Relists the Store entry with accessibility positioning and keywords

No code changes to the application itself. This is packaging + marketing.

**Estimated effort: 2 days.**

---

## 2. Changes

### F5: Multi-Architecture Builds

v1.0 shipped x64 only. v1.1 adds:

| Architecture | Status | Notes |
|-------------|--------|-------|
| **x64** | ✅ Existing | Primary target, 95%+ of Windows desktops |
| **ARM64** | 🆕 v1.1 | Surface Pro X, Snapdragon laptops. Runtime exists in Whisper.net |
| **x86** | 🆕 v1.1 | Legacy support. Runtime exists in Whisper.net |

**Implementation:**
- `publish-msix.ps1` already supports `-Arch` parameter
- Add `x86` to validation set
- Add `-All` switch: generates x64 + ARM64 + x86 MSIXs sequentially
- Submit all three to Partner Center — Windows installs the correct one automatically

### F6: Store Relisting — Accessibility Positioning

**Categories:** Productivity AND Accessibility (dual listing)

**Keywords (new):**
- motor disability, typing difficulty, RSI, repetitive strain injury
- assistive technology, voice typing, hands-free, speech to text
- accessibility, adaptive input, speech impediment

**Description lead (new):**
"VivaVoz lets you type without touching your keyboard. Built for people with motor disabilities, RSI, or anyone who thinks better by speaking. 100% local — your voice never leaves your computer."

**Screenshots:** Add 1-2 accessibility-focused scenarios showing hands-free use case.

**Accessibility declaration:** Submit in Partner Center.

### F7: Promotional Code Strategy

Generate Store promotional codes for future distribution to:
- Accessibility organizations (Neil Squire Society, etc.)
- Assistive technology reviewers and bloggers
- Partnership outreach

This is post-launch marketing, not a code change. Partner Center handles promo code generation. Outreach happens after v1.2 ships real accessibility features.

---

## 3. Deferred to v1.2

All application-level accessibility features moved to v1.2:
- Accessible hotkey system (single-key + sequential chord mode)
- Motor accessibility UI (large touch targets, focus indicators)
- Grammar correction (ByT5-text-correction)
- Speech impediment correction use case

See [PRD-v1.2.md](PRD-v1.2.md) for details.

---

## 4. Delivery Plan

- **Day 1:** Update `publish-msix.ps1` (add x86, add -All flag). Generate and test 3 MSIXs.
- **Day 2:** Update Store listing (categories, keywords, description, screenshots). Submit.

---

## 5. Decisions Made

1. **Scope down** — v1.1 is packaging + marketing only. No code changes to the app.
2. **Ship fast** — 2 days, not 2 weeks. Get accessibility keywords live while v1.2 builds real features.
3. **Promo codes deferred** — generate after v1.2 ships actual accessibility features.

---

*Your voice, alive. For everyone.* 🎙️♿
