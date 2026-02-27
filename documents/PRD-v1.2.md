# VivaVoz — Product Requirements Document (v1.2)

**Product:** VivaVoz
**Tagline:** Your voice, alive. For everyone.
**Version:** 1.2
**Date:** 2026-02-27
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs
**Price:** $1.99 (Microsoft Store) + promotional codes for accessibility organizations
**Previous:** [PRD-v1.1.md](PRD-v1.1.md)

---

## Change Log

| Version | Date       | Author                       | Changes                                     |
|---------|------------|------------------------------|---------------------------------------------|
| 1.2.0   | 2026-02-27 | Andre Vianna / Lola Lovelace | Initial v1.2 PRD — motor accessibility features inherited from v1.1 scope-down |

---

## 1. What Changed Since v1.1

v1.1 relisted the Store with accessibility positioning and added multi-arch builds. v1.2 delivers the **actual accessibility features** that back up that positioning:
- Accessible hotkey system for motor disabilities
- UI enhancements for motor accessibility
- Automatic grammar correction (especially for speech impediments)

**Build only after v1.1 shows traction in Store.** If downloads/interest are flat, reassess before investing.

---

## 2. New Features

### F8: Accessible Hotkey System

The existing hotkey system requires simultaneous key combinations (e.g., Ctrl+Shift+R). For users with motor disabilities, this can be impossible.

**Single-key hotkeys:**
- Allow binding to a single key (e.g., F9, Pause, Scroll Lock, Insert)
- No modifier required
- Ideal for users who can only reliably press one key at a time
- Enables **foot pedal support** for free — USB foot pedals register as keyboard keys
- Default single-key: F9

**Sequential hotkeys (chord mode):**
- Press keys in sequence instead of simultaneously
- Example: press A, then press L within a configurable time window
- Default window: 500ms (configurable in Settings)
- Audio click feedback on first key press (configurable, on by default)
- If window expires without second key, first key passes through normally (no lost input)

**Settings additions:**
```
Settings
├── HotkeyMode (enum: Simultaneous | SingleKey | Sequential)
├── SequentialWindowMs (int, default: 500)
├── SequentialFeedbackSound (bool, default: true)
```

**Technical:** Two-stage state machine:
- State 0: Idle → first key pressed → play click, start timer, enter State 1
- State 1: Waiting → second key within window → trigger action, return to State 0
- State 1: Waiting → timer expires → pass first key through, return to State 0

### F9: Motor Accessibility UI Enhancements

**Larger click targets:**
- All interactive elements minimum 44x44px
- Buttons, dropdowns, list items — all enlarged
- Opt-in via Settings → Accessibility → "Large touch targets"

**Focus indicators:**
- Visible focus ring (3px, contrasting color) on all interactive elements
- Opt-in via Settings → Accessibility → "Enhanced focus indicators"

Both are **opt-in** — default UI unchanged for general users.

**Settings additions:**
```
Settings
├── LargeTouchTargets (bool, default: false)
├── EnhancedFocusIndicators (bool, default: false)
```

### F10: Automatic Grammar Correction

Post-transcription grammar correction using a lightweight local model. Particularly valuable for users with **speech impediments** — Whisper transcribes with errors caused by impeded speech, then the grammar model corrects automatically.

**Pipeline:**
```
audio → Whisper → raw text → Grammar Model → corrected text
```

**Model: ByT5-text-correction**
- Multilingual: English, Portuguese, Spanish, French, German, Italian, Dutch + 9 more
- Lightweight: ~300MB (comparable to Whisper Base)
- Specialized: purpose-built for text correction, not a general LLM
- Runs via ONNX Runtime (.NET native)
- Download on demand (like Whisper models)

**⚠️ Technical Risk:** ByT5 via ONNX in .NET needs a spike to confirm:
- Model loads and runs correctly via Microsoft.ML.OnnxRuntime
- Latency is acceptable (< 2s for typical transcript)
- Portuguese correction quality is adequate
- Memory footprint within bounds (~300MB model + inference overhead)

**Spike must pass before committing to this feature.**

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
- Both raw and corrected text stored
- User sees corrected text by default, can toggle to view raw
- Visual indicator: "✨ Auto-corrected" badge
- Can be applied retroactively via (Re-)Transcribe

**Data model changes:**
```
Recording
├── RawTranscript (text — original Whisper output, always preserved)
├── Transcript (text — corrected version when enabled)
├── WasCorrected (bool)
```

**Settings additions:**
```
Settings
├── AutoCorrectEnabled (bool, default: false)
├── ShowRawTranscript (bool, default: false)
```

---

## 3. Deferred to v2.0

| Feature | Reason |
|---------|--------|
| **Tone/style formatting** (Formal, Casual, Professional) | Requires LLM (Phi-3 Mini), 2.3GB+ download |
| **Continuous dictation** | Queue management for slow models needs design |
| **Screen reader support** | Visual accessibility — out of motor focus |
| **High contrast mode** | Visual accessibility — out of motor focus |
| **Reduced motion** | Vestibular — out of motor focus |
| **BYOK (cloud AI)** | Separate feature set |
| **Clipboard auto-paste** | Needs more design |

See [PRD-v2.0-draft.md](PRD-v2.0-draft.md) for details.

---

## 4. Decision Gate

**Build v1.2 ONLY IF:**
- v1.1 Store relisting shows measurable traction (downloads, reviews, or search impressions up)
- ByT5 technical spike passes (model works in .NET, quality acceptable)

If v1.1 shows no traction, reassess whether accessibility investment is justified vs. focusing on other revenue products (ef-data, Config X-Ray).

---

## 5. Delivery Plan

- **Delivery 3a:** ByT5 technical spike (1 day — go/no-go on grammar correction)
- **Delivery 3b:** Accessible hotkey system (single-key + sequential mode)
- **Delivery 3c:** Motor accessibility UI (large targets + focus indicators)
- **Delivery 3d:** Grammar correction (if spike passed: model download, pipeline, UI)
- **Delivery 3e:** Store update (screenshots showing accessibility features, updated description)

---

## 6. Decisions Made

1. **Motor-only focus** — targets motor disabilities exclusively
2. **Accessibility features opt-in** — default UI unchanged
3. **Floating icon cut** — hotkeys solve the motor problem better
4. **Decision gate** — v1.2 only if v1.1 shows traction
5. **ByT5 needs spike** — don't commit until technical feasibility confirmed
6. **Grammar correction for speech impediment** — the differentiator, if spike passes
7. **Sequential hotkey default** — 500ms window, audio click feedback
8. **Default single-key** — F9

---

*Your voice, alive. For everyone.* 🎙️♿
