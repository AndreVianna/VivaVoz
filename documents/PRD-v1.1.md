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

## What Changed Since v1.0

v1.0 shipped as a functional voice-to-text tool for Windows. v1.1 repositions VivaVoz as an **accessibility-first** application while adding polish features deferred from v1.0.

---

## Change Log

| Version | Date       | Author                       | Changes                                     |
|---------|------------|------------------------------|---------------------------------------------|
| 1.1.0   | 2026-02-27 | Andre Vianna / Lola Lovelace | Initial v1.1 PRD — accessibility pivot, deferred v1.0 features |

---

## 1. Strategic Pivot: Accessibility First

### Why

VivaVoz competes poorly as "generic STT" against free tools like Buzz. But as an **accessibility tool for people with motor disabilities, RSI, or typing difficulties**, the positioning changes completely:

- **Different market** — assistive technology has dedicated funding, grants, and corporate programs
- **Price justified** — $1.99 for accessibility is trivial; competitors charge hundreds per license
- **Discovery** — accessibility keywords have less competition on the Microsoft Store
- **Local-first is a feature** — privacy matters for health/accessibility contexts. Zero audio leaves the machine.
- **Microsoft visibility** — the Store highlights accessibility apps with badges and featured placement
- **Goodwill** — assistive tech gets more organic coverage, reviews, and community support

### The Reframe

**Old positioning:** "Voice-to-text for Windows power users"
**New positioning:** "Type without touching your keyboard. Accessible voice-to-text for everyone."

This doesn't exclude the general audience — it changes the **lead** of the marketing. Accessibility-first, useful for everyone.

---

## 2. New Features (v1.1)

### F5: Accessibility Enhancements
- **Full keyboard navigation** — every UI element reachable without mouse
- **Screen reader support** — NVDA and Narrator compatible (Avalonia automation peers)
- **High contrast mode** — respects Windows high contrast settings
- **Larger click targets** — minimum 44x44px touch/click targets throughout UI
- **Focus indicators** — visible focus ring on all interactive elements
- **Reduced motion option** — disable animations for vestibular sensitivity

### F6: Continuous Dictation Mode
- New mode beyond push-to-talk and toggle: **continuous dictation**
- Microphone stays open, voice activity detection (VAD) segments speech
- Silence pauses transcription, voice resumes it
- Transcribed text auto-copies to clipboard (configurable)
- Designed for users who need hands-free operation for extended periods
- Can paste directly into active window (accessibility use case)

### F7: Store Listing — Accessibility Repositioning
- **Category:** Accessibility (not just Productivity)
- **Keywords:** motor disability, typing difficulty, RSI, repetitive strain, assistive technology, voice typing, hands-free, speech to text accessibility
- **Description lead:** "VivaVoz lets you type without touching your keyboard. Built for people with motor disabilities, RSI, or anyone who thinks better by speaking."
- **Screenshots:** Include accessibility-focused scenarios (hands-free dictation, screen reader in use)
- **Accessibility declaration** in Store listing

### F8: Deferred v1.0 Features
These were scoped for v1.0 but deferred to keep MVP tight:

- **Taskbar icon** with recording state badge
- **Floating icon** — always-on-top mini widget for one-click record/stop
- **Hotkeys-only mode** — zero visible UI for power users
- **Onboarding wizard** — 4-step first-run experience (from PRD v1.0 section 6)
- **Update checker** — in-app version check on launch
- **In-app help** — built-in FAQ page

---

## 3. Technical Changes

### Accessibility Implementation

| Feature | Implementation |
|---------|---------------|
| Keyboard navigation | Avalonia `KeyboardNavigation`, TabIndex on all controls |
| Screen reader | `AutomationPeer` implementations for custom controls |
| High contrast | Bind to `SystemParameters.HighContrast`, swap theme |
| Focus indicators | Custom `FocusAdorner` style in theme |

### Continuous Dictation

- Whisper.net supports streaming inference — use segmented processing
- Voice Activity Detection (VAD): use WebRTC VAD or Silero VAD via ONNX
- Segment on silence > 1.5s (configurable)
- Each segment is a separate transcription job, results concatenated
- Auto-clipboard: `Clipboard.SetTextAsync()` after each segment completes

### Settings Additions

```
Settings
├── DictationMode (enum: PushToTalk | Toggle | Continuous)
├── ContinuousSilenceThresholdMs (int, default: 1500)
├── AutoCopyToClipboard (bool, default: false)
├── AutoPasteToActiveWindow (bool, default: false)
├── ReducedMotion (bool, default: false — follows system preference)
└── HighContrast (bool, default: false — follows system preference)
```

---

## 4. Marketing Changes

### Store Listing

**Title:** VivaVoz — Voice to Text (Accessible)

**Short description:** Type without touching your keyboard. Local, private, accessible voice-to-text for Windows.

**Long description:**
VivaVoz turns your voice into text, right on your machine. No cloud. No subscription. No account. Your voice stays private.

Built for:
• People with motor disabilities or RSI who need hands-free typing
• Knowledge workers who think better by speaking
• Anyone who wants fast, private voice-to-text on Windows

Features:
• Local transcription — nothing leaves your computer
• Multiple Whisper models (tiny to large) — choose speed vs accuracy
• Continuous dictation mode — just talk, VivaVoz types
• Full keyboard navigation and screen reader support
• Light/dark/high-contrast themes
• Export as text or audio

$1.99. One time. No tricks.

### Website (vivavoz.app)

- Add accessibility section to landing page
- Add "For Accessibility" page with detailed use cases
- Link to Microsoft Accessibility resources

---

## 5. Success Metrics (v1.1)

- **Accessibility badge** on Microsoft Store listing
- **Featured in Store accessibility collection** (apply after approval)
- **10 reviews mentioning accessibility** within 3 months
- **Coverage** in at least 1 assistive technology blog/forum

---

## 6. Open Questions

1. **Should we offer a free tier for verified accessibility users?** (e.g., free via accessibility organizations, paid for general public)
2. **Windows Speech Recognition integration?** — use as fallback when Whisper models are too slow on low-end hardware
3. **Partnerships with accessibility organizations?** (e.g., Neil Squire Society in Canada)
4. **ARM64 build?** — Surface Pro X and Snapdragon laptops are popular in accessibility contexts

---

## 7. Delivery Plan

*To be broken down into tasks after PO interview. Tentative structure:*

- **Delivery 2a:** F8 (deferred v1.0 features — taskbar, floating icon, hotkeys-only, onboarding, update checker, help)
- **Delivery 2b:** F5 (accessibility enhancements)
- **Delivery 2c:** F6 (continuous dictation mode)
- **Delivery 2d:** F7 (Store relisting + marketing)

---

*Your voice, alive. For everyone.* 🎙️♿
