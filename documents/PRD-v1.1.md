# VivaVoz — Product Requirements Document (v1.1)

**Product:** VivaVoz
**Tagline:** Your voice, alive. For everyone.
**Version:** 1.1
**Date:** 2026-02-27
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs
**Price:** $1.99 (Microsoft Store) + promotional codes for accessibility organizations
**Previous:** [PRD-v1.0.md](PRD-v1.0.md) — DELIVERED 2026-02-27

---

## Change Log

| Version | Date       | Author                       | Changes                                     |
|---------|------------|------------------------------|---------------------------------------------|
| 1.1.0   | 2026-02-27 | Andre Vianna / Lola Lovelace | Initial v1.1 PRD from PO interview — motor accessibility pivot, multi-arch, accessible hotkeys |

---

## 1. What Changed Since v1.0

v1.0 shipped as a functional voice-to-text tool for Windows (x64 only). v1.1 repositions VivaVoz as an **accessibility-first** application for people with motor disabilities while expanding platform support.

### Strategic Pivot: Accessibility for Motor Disabilities

**Old positioning:** "Voice-to-text for Windows power users"
**New positioning:** "Type without touching your keyboard."

**Why this works:**
- Different market — assistive technology has dedicated funding, grants, and corporate programs
- $1.99 is trivial for accessibility; competitors charge hundreds per license
- Accessibility keywords have less competition on the Microsoft Store
- Local-first = privacy for health/accessibility contexts. Zero audio leaves the machine.
- Microsoft Store highlights accessibility apps with badges and featured placement
- Assistive tech gets more organic coverage, reviews, and community support

**Scope:** v1.1 focuses exclusively on **motor disabilities** (RSI, typing difficulty, limited hand coordination, tremors). Visual accessibility (screen reader, high contrast) and other disabilities are deferred to v2+.

---

## 2. New Features

### F5: Accessible Hotkey System

The existing hotkey system requires simultaneous key combinations (e.g., Ctrl+Shift+R). For users with motor disabilities, this can be impossible.

v1.1 adds:

**Single-key hotkeys:**
- Allow binding to a single key (e.g., F9, Pause, Scroll Lock, Insert)
- No modifier required
- Ideal for users who can only reliably press one key at a time
- Also enables **foot pedal support** for free — USB foot pedals register as keyboard keys

**Sequential hotkeys (chord mode):**
- Instead of holding keys simultaneously, press them in sequence
- Example: press A, then press L within a configurable time window
- Default window: 500ms (configurable in Settings)
- Visual/audio feedback on first key press ("waiting for second key...")
- If window expires without second key, first key passes through normally (no lost input)

**Settings additions:**
```
Settings
├── HotkeyMode (enum: Simultaneous | Sequential)
├── SequentialWindowMs (int, default: 500)
```

### F6: Motor Accessibility UI Enhancements

**Larger click targets:**
- All interactive elements minimum 44x44px
- Buttons, dropdowns, list items — all enlarged
- Opt-in via Settings → Accessibility → "Large touch targets"

**Focus indicators:**
- Visible focus ring on all interactive elements
- High-visibility ring (3px, contrasting color) for keyboard/switch navigation
- Opt-in via Settings → Accessibility → "Enhanced focus indicators"

**Settings additions:**
```
Settings
├── LargeTouchTargets (bool, default: false)
├── EnhancedFocusIndicators (bool, default: false)
```

Both are opt-in — the default UI remains unchanged for general users.

### F7: Multi-Architecture Builds

v1.0 shipped x64 only. v1.1 adds:

| Architecture | Status | Notes |
|-------------|--------|-------|
| **x64** | ✅ Existing | Primary target, 95%+ of Windows desktops |
| **ARM64** | 🆕 v1.1 | Surface Pro X, Snapdragon laptops. Runtime exists in Whisper.net |
| **x86** | 🆕 v1.1 | Legacy support. Runtime exists in Whisper.net |

**Implementation:** `publish-msix.ps1` already supports `-Arch` parameter. Generate three MSIXs, submit all to Partner Center. Windows installs the correct one automatically.

### F8: Store Relisting

**Categories:** Productivity AND Accessibility (dual listing)

**Keywords (new):**
- motor disability, typing difficulty, RSI, repetitive strain injury
- assistive technology, voice typing, hands-free, speech to text
- accessibility, adaptive input, one-handed typing

**Description lead (new):**
"VivaVoz lets you type without touching your keyboard. Built for people with motor disabilities, RSI, or anyone who thinks better by speaking. 100% local — your voice never leaves your computer."

**Screenshots:** Add accessibility-focused scenarios (single-key hotkey setup, large touch targets enabled, Settings accessibility panel)

**Accessibility declaration:** Submit in Partner Center

### F9: Promotional Codes

Generate Store promotional codes for strategic distribution:
- Accessibility organizations (Neil Squire Society, etc.)
- Assistive technology reviewers and bloggers
- Partnership outreach post-launch

This is a **marketing activity**, not a product feature. No code changes needed — Partner Center handles promo code generation.

---

## 3. Deferred to v2+

| Feature | Reason |
|---------|--------|
| **Screen reader support** (NVDA/Narrator) | Visual accessibility — out of motor focus |
| **High contrast mode** | Visual accessibility — out of motor focus |
| **Reduced motion** | Vestibular — out of motor focus |
| **Continuous dictation mode** | Queue management for slow models (medium/large) needs careful design. Andre will design solution. |
| **Floating icon** | Redundant with accessible hotkeys (single-key/sequential). Hotkeys solve the motor problem better. |
| **Organization partnerships** | Post-launch activity. Build first, then show. |

---

## 4. Technical Changes

### Hotkey System Refactor

Current hotkey registration uses `RegisterHotKey` Win32 API with modifier flags. Changes needed:

1. **Single-key support:** Register without modifiers. Must handle key passthrough carefully to avoid stealing keys from other apps. Use a "dead key" approach — keys that have no normal typing function (F-keys, Pause, etc.) recommended by default.

2. **Sequential mode:** Two-stage state machine:
   - State 0: Idle → first key pressed → start timer, enter State 1
   - State 1: Waiting → second key pressed within window → trigger action, return to State 0
   - State 1: Waiting → timer expires → pass first key through to active app, return to State 0
   - Visual feedback (tray icon flash or subtle sound) on entering State 1

3. **Settings UI:** New "Hotkey Mode" selector in Settings with explanation text for each mode.

### UI Scaling for Accessibility

Larger touch targets via:
- Custom Avalonia style that overrides MinHeight/MinWidth on Button, ListBoxItem, ComboBox, etc.
- Applied conditionally when `LargeTouchTargets = true`
- Focus adorner style swap when `EnhancedFocusIndicators = true`

### Multi-Arch Build Pipeline

Update `publish-msix.ps1`:
- Add `x86` to `-Arch` parameter validation
- Add `publish-all` mode that generates all three MSIXs
- Naming: `VivaVoz-x64.msix`, `VivaVoz-arm64.msix`, `VivaVoz-x86.msix`

---

## 5. Marketing Plan (Post-Launch)

1. **Store relisting** — update category, keywords, description, screenshots
2. **Blog post** — "VivaVoz: Voice-to-Text Built for Accessibility" (Dev.to, Medium)
3. **Reddit** — r/accessibility, r/RSI, r/disability, r/assistivetech
4. **Outreach** — contact 3-5 accessibility organizations with promo codes
5. **Assistive tech blogs** — pitch for review
6. **vivavoz.app** — add "For Accessibility" page

---

## 6. Success Metrics

- Accessibility badge on Microsoft Store listing
- 10 reviews mentioning accessibility within 3 months
- Coverage in at least 1 assistive technology blog/forum
- ARM64 + x86 builds passing Store certification

---

## 7. Decisions Made (v1.1)

1. **Motor-only focus** — v1.1 targets motor disabilities exclusively. Visual/auditory accessibility deferred to v2+.
2. **Accessibility features are opt-in** — default UI unchanged. Users enable in Settings → Accessibility.
3. **Floating icon cut** — accessible hotkeys (single-key/sequential) solve the problem better.
4. **Continuous dictation deferred** — model speed disparity creates queue management problem. Needs careful design.
5. **$1.99 for everyone** — no free tier. Promo codes for organizations as needed.
6. **Dual Store category** — Productivity + Accessibility.
7. **Partnerships post-launch** — build the product first, then outreach with promo codes.
8. **Three architectures** — x64, ARM64, x86 all ship in v1.1.

---

## 8. Open Questions

1. **Sequential hotkey audio feedback** — beep, click sound, or silent (visual only)?
2. **Default single-key hotkey** — which key? F9? Pause? Should be uncommon but easy to reach.
3. **Foot pedal testing** — do we need to explicitly test with a USB foot pedal, or trust that single-key support covers it?

---

## 9. Delivery Plan

*To be broken down into task specs after PRD approval.*

- **Delivery 3a:** Accessible hotkey system (single-key + sequential mode)
- **Delivery 3b:** Motor accessibility UI (large targets, focus indicators)
- **Delivery 3c:** Multi-arch builds (ARM64, x86) + publish-msix.ps1 update
- **Delivery 3d:** Store relisting (categories, keywords, description, screenshots)

---

*Your voice, alive. For everyone.* 🎙️♿
