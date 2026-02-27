# VivaVoz — Product Requirements Document (v2.0) — DRAFT

**Product:** VivaVoz
**Tagline:** Your voice, alive. Intelligent.
**Version:** 2.0 (DRAFT — pending PO interview)
**Date:** 2026-02-27
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs
**Price:** $1.99 (Microsoft Store) — or consider price increase with AI features
**Previous:** [PRD-v1.1.md](PRD-v1.1.md)

---

## Change Log

| Version | Date       | Author        | Changes                    |
|---------|------------|---------------|----------------------------|
| 2.0.0   | 2026-02-27 | Lola Lovelace | Initial draft from v1.1 deferred features + new vision |

---

## 1. Vision

v1.0 was a voice recorder with transcription.
v1.1 added accessibility and grammar correction.
v2.0 makes VivaVoz **intelligent** — it doesn't just transcribe what you said, it transforms it into what you meant.

The core addition is a local LLM (Phi-3 Mini) that enables:
- Tone and style formatting
- Custom AI prompts
- Continuous dictation with intelligent segmentation
- Full accessibility suite (visual + motor + auditory)
- BYOK (Bring Your Own Key) for cloud AI models

---

## 2. New Features

### F11: AI Text Enhancement (Phi-3 Mini via ONNX)

Local LLM-powered text transformation. Runs entirely on-device.

**Model:** Microsoft Phi-3 Mini (3.8B parameters)
- ONNX Runtime optimized, INT4 quantized (~2.3GB download)
- Runs on CPU (8GB+ RAM) or GPU if available
- Download on demand to models/ folder

**Tone Presets:**
- **Fix errors only** — grammar + spelling (uses ByT5 from v1.1 for speed, Phi-3 as fallback)
- **Formal** — professional language, complete sentences, formal register
- **Professional** — clear and direct, suitable for business communication
- **Casual** — natural, conversational, contractions allowed
- **Academic** — structured, precise, citation-ready
- **Custom** — user writes their own prompt template

**Pipeline:**
```
audio → Whisper → raw text → ByT5 (grammar) → Phi-3 (tone/style) → final text
```

Note: ByT5 runs first (fast, lightweight) for basic corrections. Phi-3 runs second for tone transformation. If Phi-3 is not downloaded, pipeline stops at ByT5 (graceful degradation).

**UX:**
- Settings → Post-Processing → "AI Enhancement" with preset dropdown
- Quick-switch in recording detail panel (re-process with different tone)
- Side-by-side view: raw → corrected → enhanced
- Processing indicator with cancel option
- Custom prompt editor with variables: `{text}`, `{language}`, `{tone}`

**Settings additions:**
```
Settings
├── AIEnhancementEnabled (bool, default: false)
├── AITonePreset (enum: FixOnly | Formal | Professional | Casual | Academic | Custom)
├── AICustomPrompt (string, nullable)
├── AIModelPath (string — path to Phi-3 model)
```

### F12: Continuous Dictation Mode

Microphone stays open for extended hands-free operation.

**The Queue Problem (from v1.1 research):**
Slow models (medium/large Whisper) can't keep up with continuous speech. Solution:

**Adaptive model strategy:**
- Continuous mode always uses tiny or base Whisper for real-time segmentation
- After session ends, user can optionally re-transcribe with a better model
- This decouples "live capture" speed from "final quality" accuracy

**Voice Activity Detection (VAD):**
- Silero VAD via ONNX (tiny model, <10MB)
- Segments on silence > configurable threshold (default: 1.5s)
- Each segment transcribed independently, results concatenated

**Output modes:**
- **Clipboard** — each segment auto-copies to clipboard
- **Active window paste** — paste into the window that was active when dictation started (safe: locked to one target)
- **Internal buffer** — accumulate in VivaVoz, copy/export when done

**Settings additions:**
```
Settings
├── DictationMode (enum: PushToTalk | Toggle | Continuous)
├── ContinuousWhisperModel (enum: Tiny | Base — forced to fast models)
├── ContinuousSilenceThresholdMs (int, default: 1500)
├── ContinuousOutputMode (enum: Clipboard | ActiveWindowPaste | InternalBuffer)
├── ContinuousAutoCorrect (bool, default: true — apply ByT5 per segment)
```

### F13: Full Accessibility Suite

Expanding from motor-only (v1.1) to comprehensive accessibility.

**Visual accessibility:**
- Screen reader support (NVDA + Narrator) via Avalonia AutomationPeer
- High contrast mode (respects Windows system setting)
- Adjustable font sizes throughout UI
- Color-blind-safe status indicators (shapes + colors, not color alone)

**Auditory feedback:**
- Audio cues for recording start/stop (configurable sounds)
- Text-to-speech readback of transcription (Windows SAPI or local TTS)
- Vibration patterns for supported input devices

**Motor (inherited from v1.1):**
- Single-key hotkeys
- Sequential hotkeys
- Large touch targets
- Focus indicators
- Foot pedal support

**Settings → Accessibility panel restructured:**
```
Accessibility
├── Motor
│   ├── LargeTouchTargets
│   ├── EnhancedFocusIndicators
│   ├── HotkeyMode (Simultaneous | SingleKey | Sequential)
│   └── SequentialWindowMs
├── Visual
│   ├── HighContrast (follow system | force on | off)
│   ├── FontScale (100% | 125% | 150% | 200%)
│   └── ColorBlindMode (off | deuteranopia | protanopia | tritanopia)
└── Auditory
    ├── RecordingStartSound (on/off + sound selection)
    ├── RecordingStopSound (on/off + sound selection)
    └── TranscriptReadback (on/off)
```

### F14: BYOK (Bring Your Own Key)

For users who want cloud AI quality without VivaVoz running its own cloud service.

**Supported providers:**
- OpenAI (GPT-4o-mini, GPT-4o)
- Anthropic (Claude Haiku, Sonnet)
- Google (Gemini Flash, Pro)

**What BYOK enables:**
- Higher quality text enhancement than Phi-3 local
- More languages with better quality
- Faster processing (cloud GPU vs local CPU)
- Custom system prompts with full model capability

**UX:**
- Settings → AI → "Cloud AI (Optional)" → Enter API key
- Provider selector + model selector
- Clear warning: "Your transcription text will be sent to {provider}. Audio stays local."
- Toggle: use cloud AI vs local AI per-transcription
- API key stored encrypted in local settings DB

**Privacy guarantee preserved:**
- Audio NEVER leaves the machine (Whisper always local)
- Only the transcribed TEXT is sent to cloud (if user opts in)
- Can be disabled entirely — local-only remains the default

### F15: Clipboard Auto-Paste into Active Window

After transcription completes:
- Detect the previously active window
- Simulate Ctrl+V to paste transcribed text
- Configurable delay before paste (default: 500ms)
- Works with any text input field

**Use case:** User is in a chat app, presses hotkey, speaks, release → text appears in the chat box automatically. Zero typing.

---

## 3. Technical Architecture Changes

### Phi-3 Integration
- `Microsoft.ML.OnnxRuntimeGenAI` NuGet package
- Phi-3 Mini INT4 ONNX model (~2.3GB)
- Prompt template system with variables
- Streaming output (show text appearing in real-time)
- GPU acceleration via DirectML when available

### Continuous Dictation Engine
- Silero VAD via ONNX Runtime (~10MB model)
- Ring buffer for audio capture (continuous, no gaps)
- Segment queue with worker thread
- Forced tiny/base model for real-time constraint
- Optional post-session re-transcription with larger model

### Accessibility Framework
- Avalonia `AutomationPeer` implementations for all custom controls
- Theme system supporting: Default, Dark, High Contrast, Large Font variants
- Audio feedback system with configurable sound packs
- Windows SAPI integration for transcript readback

### BYOK Client
- HTTP client with provider-specific adapters (OpenAI, Anthropic, Google)
- API key encryption at rest (DPAPI on Windows)
- Rate limiting and cost estimation
- Fallback to local model if API call fails

---

## 4. Pricing Considerations

v2.0 adds significant AI capabilities. Options:

**(a) Keep $1.99** — all features included. Growth through volume. Accessibility angle benefits from low price.

**(b) Freemium** — Basic features free, AI features paid ($4.99 one-time). Increases accessibility reach.

**(c) Bump to $4.99** — still impulse buy, better revenue per user. Promo codes keep accessibility orgs covered.

**Decision: TBD — pending PO interview.**

---

## 5. Deferred to v3+

| Feature | Reason |
|---------|--------|
| **Streaming transcription** (real-time as you speak) | Complex, needs careful UX design |
| **App-aware mode switching** | Detect active app, auto-switch tone preset |
| **Custom vocabulary / jargon dictionary** | Whisper fine-tuning or post-processing dictionary |
| **Web companion** (Avalonia WASM) | Cross-platform expansion |
| **Mac support** | Avalonia supports macOS but needs testing + Store submission |
| **Team/enterprise features** | License management, shared settings, centralized deployment |
| **Plugin system** | Third-party post-processing plugins |

---

## 6. Success Metrics

- Phi-3 model downloaded by >20% of users
- BYOK configured by >5% of users
- Continuous dictation used by >15% of users
- Accessibility panel engagement up 50% from v1.1
- App Store rating ≥ 4.5 stars
- Revenue: consider if v2 justifies price increase

---

## 7. Open Questions (For PO Interview)

1. **Pricing:** Keep $1.99, go freemium, or bump to $4.99?
2. **Continuous dictation:** Is the "always use tiny/base, re-transcribe later" solution acceptable?
3. **BYOK priority:** Is this v2.0 or can it wait for v2.1?
4. **Clipboard auto-paste:** Security concern? Apps might not expect automated Ctrl+V.
5. **Phi-3 model size:** 2.3GB download acceptable? Should we offer a smaller alternative?
6. **TTS readback:** Windows SAPI (free, included) or integrate a better TTS engine?
7. **Mac support:** Start in v2.0 or wait?
8. **Feature gating:** Should AI features require a separate in-app purchase?

---

## 8. Tentative Delivery Plan

- **Delivery 4a:** Continuous dictation (VAD + adaptive model + output modes)
- **Delivery 4b:** Phi-3 integration (tone presets + custom prompts + streaming output)
- **Delivery 4c:** Full accessibility suite (visual + auditory)
- **Delivery 4d:** BYOK (OpenAI, Anthropic, Google adapters + encrypted key storage)
- **Delivery 4e:** Clipboard auto-paste + polish + Store update

---

*Your voice, alive. Intelligent.* 🎙️🧠
