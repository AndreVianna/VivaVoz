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
| 1.1.0   | 2026-02-27 | Andre Vianna / Lola Lovelace | Initial v1.1 PRD from PO interview — motor accessibility, multi-arch, accessible hotkeys |
| 1.1.1   | 2026-02-27 | Andre Vianna / Lola Lovelace | Added grammar correction (GECToR/ByT5), refined interview answers, closed open questions |

---

## 1. What Changed Since v1.0

v1.0 shipped as a functional voice-to-text tool for Windows (x64 only). v1.1 repositions VivaVoz as an **accessibility-first** application for people with motor disabilities, adds automatic grammar correction, and expands platform support.

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

**Scope:** v1.1 focuses exclusively on **motor disabilities** (RSI, typing difficulty, limited hand coordination, tremors, speech impediments). Visual accessibility (screen reader, high contrast) and other disabilities are deferred to v2+.

---

## 2. New Features

### F5: Accessible Hotkey System

The existing hotkey system requires simultaneous key combinations (e.g., Ctrl+Shift+R). For users with motor disabilities, this can be impossible.

v1.1 adds:

**Single-key hotkeys:**
- Allow binding to a single key (e.g., F9, Pause, Scroll Lock, Insert)
- No modifier required
- Ideal for users who can only reliably press one key at a time
- Enables **foot pedal support** for free — USB foot pedals register as keyboard keys

**Sequential hotkeys (chord mode):**
- Instead of holding keys simultaneously, press them in sequence
- Example: press A, then press L within a configurable time window
- Default window: 500ms (configurable in Settings)
- Audio click feedback on first key press ("waiting for second key...")
- If window expires without second key, first key passes through normally (no lost input)

**Settings additions:**
```
Settings
├── HotkeyMode (enum: Simultaneous | SingleKey | Sequential)
├── SequentialWindowMs (int, default: 500)
├── SequentialFeedbackSound (bool, default: true)
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

### F7: Automatic Grammar Correction

Post-transcription grammar correction using a lightweight local model. Particularly valuable for users with **speech impediments** — after Whisper transcribes with errors caused by impeded speech, the grammar model corrects the text automatically.

**Pipeline:**
```
audio → Whisper → raw text → Grammar Model → corrected text
```

**Model: ByT5-text-correction**
- Multilingual: supports English, Portuguese, Spanish, French, German, Italian, Dutch, and 9 more languages
- Lightweight: ~300MB (comparable to Whisper Base model)
- Specialized: purpose-built for text correction, not a general LLM
- Runs via ONNX Runtime (.NET native integration)
- Download on demand (like Whisper models)

**What it corrects:**
- Spelling errors from transcription mistakes
- Grammar errors (tense, agreement, articles)
- Punctuation normalization
- Common speech-to-text artifacts

**What it does NOT do (deferred to v2):**
- Tone/style formatting (Formal, Casual, Professional)
- Content rewriting or paraphrasing
- Custom prompt-based processing

**UX:**
- Settings → Post-Processing → "Auto-correct transcription" (off by default)
- When enabled, both raw and corrected text are stored
- User sees corrected text by default, can toggle to view raw transcription
- Visual indicator: "✨ Auto-corrected" badge on corrected transcriptions
- Can be applied retroactively to existing transcriptions via (Re-)Transcribe

**Settings additions:**
```
Settings
├── AutoCorrectEnabled (bool, default: false)
├── ShowRawTranscript (bool, default: false)
```

**Data model addition:**
```
Recording
├── RawTranscript (text — original Whisper output, always preserved)
├── Transcript (text — corrected version when auto-correct is enabled)
├── WasCorrected (bool)
```

### F8: Multi-Architecture Builds

v1.0 shipped x64 only. v1.1 adds:

| Architecture | Status | Notes |
|-------------|--------|-------|
| **x64** | ✅ Existing | Primary target, 95%+ of Windows desktops |
| **ARM64** | 🆕 v1.1 | Surface Pro X, Snapdragon laptops. Runtime exists in Whisper.net |
| **x86** | 🆕 v1.1 | Legacy support. Runtime exists in Whisper.net |

**Implementation:** `publish-msix.ps1` already supports `-Arch` parameter. Add `x86` support. Generate three MSIXs, submit all to Partner Center.

### F9: Store Relisting

**Categories:** Productivity AND Accessibility (dual listing)

**Keywords (new):**
- motor disability, typing difficulty, RSI, repetitive strain injury
- assistive technology, voice typing, hands-free, speech to text
- accessibility, adaptive input, speech impediment
- grammar correction, auto-correct, local AI

**Description lead (new):**
"VivaVoz lets you type without touching your keyboard. Built for people with motor disabilities, RSI, speech impediments, or anyone who thinks better by speaking. Automatic grammar correction fixes transcription errors — especially useful for impeded speech. 100% local — your voice never leaves your computer."

**Screenshots:** Add accessibility-focused scenarios (single-key hotkey setup, large touch targets enabled, Settings accessibility panel, grammar correction before/after)

**Accessibility declaration:** Submit in Partner Center

### F10: Promotional Codes

Generate Store promotional codes for strategic distribution:
- Accessibility organizations (Neil Squire Society, etc.)
- Assistive technology reviewers and bloggers
- Partnership outreach post-launch

Marketing activity, not code changes — Partner Center handles promo code generation.

---

## 3. Deferred to v2+

| Feature | Reason | Target |
|---------|--------|--------|
| **Tone/style formatting** (Formal, Casual, Professional) | Requires LLM (Phi-3 Mini), 2.3GB+ download, 4GB+ RAM | v2.0 |
| **Custom AI prompts** | Requires LLM | v2.0 |
| **Screen reader support** (NVDA/Narrator) | Visual accessibility — out of motor focus | v2.0 |
| **High contrast mode** | Visual accessibility — out of motor focus | v2.0 |
| **Reduced motion** | Vestibular — out of motor focus | v2.0 |
| **Continuous dictation mode** | Queue management for slow models needs careful design | v2.0 |
| **Floating icon** | Redundant with accessible hotkeys | Cut |
| **Organization partnerships** | Post-launch activity | Post v1.1 |

---

## 4. Technical Changes

### Hotkey System Refactor

Current hotkey registration uses `RegisterHotKey` Win32 API with modifier flags. Changes:

1. **Single-key support:** Register without modifiers. Use "dead key" approach — keys with no normal typing function (F-keys, Pause, etc.) recommended by default to avoid stealing input.

2. **Sequential mode:** Two-stage state machine:
   - State 0: Idle → first key pressed → play click sound, start timer, enter State 1
   - State 1: Waiting → second key pressed within window → trigger action, return to State 0
   - State 1: Waiting → timer expires → pass first key through to active app, return to State 0

3. **Settings UI:** New "Hotkey Mode" selector with explanation text for each mode.

### Grammar Correction Integration

- **ByT5-text-correction** model via ONNX Runtime
- NuGet: `Microsoft.ML.OnnxRuntime` (already .NET native)
- Model download on demand to `%LOCALAPPDATA%/VivaVoz/models/byt5-correction/`
- Tokenizer: ByT5 uses byte-level tokenization (no separate tokenizer model needed)
- Processing: runs after Whisper completes, before displaying result
- Both raw and corrected text stored in DB
- Fallback: if correction fails, raw transcript is shown (graceful degradation)

### UI Scaling for Accessibility

- Custom Avalonia style overriding MinHeight/MinWidth on interactive controls
- Applied conditionally when `LargeTouchTargets = true`
- Focus adorner style swap when `EnhancedFocusIndicators = true`

### Multi-Arch Build Pipeline

Update `publish-msix.ps1`:
- Add `x86` to `-Arch` parameter validation
- Add `-All` switch that generates all three MSIXs sequentially

---

## 5. Marketing Plan (Post-Launch)

1. **Store relisting** — update category, keywords, description, screenshots
2. **Blog post** — "VivaVoz: Voice-to-Text Built for Accessibility" (Dev.to, Medium)
3. **Reddit** — r/accessibility, r/RSI, r/disability, r/assistivetech
4. **Outreach** — contact 3-5 accessibility organizations with promo codes
5. **Assistive tech blogs** — pitch for review
6. **vivavoz.app** — add "For Accessibility" page
7. **Speech impediment communities** — key differentiator: auto-correction of impeded speech transcription

---

## 6. Success Metrics

- Accessibility badge on Microsoft Store listing
- 10 reviews mentioning accessibility within 3 months
- Coverage in at least 1 assistive technology blog/forum
- ARM64 + x86 builds passing Store certification
- Grammar correction used by >30% of active users

---

## 7. Decisions Made (v1.1)

1. **Motor-only focus** — v1.1 targets motor disabilities exclusively. Visual/auditory accessibility deferred to v2+.
2. **Accessibility features are opt-in** — default UI unchanged. Users enable in Settings → Accessibility.
3. **Floating icon cut** — accessible hotkeys solve the problem better.
4. **Continuous dictation deferred** — model speed disparity creates queue management problem.
5. **$1.99 for everyone** — no free tier. Promo codes for organizations as needed.
6. **Dual Store category** — Productivity + Accessibility.
7. **Partnerships post-launch** — build the product first, then outreach with promo codes.
8. **Three architectures** — x64, ARM64, x86 all ship in v1.1.
9. **GECToR/ByT5 for v1.1, Phi-3 for v2** — grammar correction now, tone/style formatting later.
10. **Sequential hotkey feedback** — audio click sound (configurable, on by default).
11. **Default single-key hotkey** — F9 (uncommon, easy to reach, no conflict risk).

---

## 8. Delivery Plan

- **Delivery 3a:** Accessible hotkey system (single-key + sequential mode + settings UI)
- **Delivery 3b:** Motor accessibility UI (large targets + focus indicators, opt-in)
- **Delivery 3c:** Grammar correction (ByT5 integration, model download, UI for raw/corrected toggle)
- **Delivery 3d:** Multi-arch builds (ARM64, x86 + publish-msix.ps1 update)
- **Delivery 3e:** Store relisting (categories, keywords, description, screenshots, accessibility declaration)

---

*Your voice, alive. For everyone.* 🎙️♿
