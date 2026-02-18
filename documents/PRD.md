# VivaVoz — Product Requirements Document

**Product:** VivaVoz
**Tagline:** Your voice, alive.
**Version:** 1.0 (MVP)
**Date:** 2026-02-17
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs

---

## Executive Summary

VivaVoz is a $5 Windows desktop app that captures voice, transcribes it locally using Whisper, and exports recordings as audio or text. No cloud. No subscription. No account needed.

The market opportunity: Mac has 5+ polished voice-to-text tools (SuperWhisper, Auto, Wispr Flow). Windows has nothing comparable at an affordable price point. Wispr Flow ($81M funded) is entering Windows but is cloud-only and subscription-based, leaving the local-first, privacy-respecting niche wide open.

VivaVoz fills that gap. Built with .NET 10 + Avalonia UI for a polished, cross-platform experience. MVP in 4 weeks. $5 impulse buy. Microsoft Store + direct download.

*Your voice, alive.* 🎙️

---

## Change Log

| Version | Date       | Author                       | Changes                                                                                                                                                                                                            |
|---------|------------|------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0.1     | 2026-02-17 | Lola Lovelace                | Initial PRD — MVP scope, tech stack, data model                                                                                                                                                                    |
| 0.2     | 2026-02-17 | Andre Vianna / Lola Lovelace | Resolved open questions: pricing ($5), format (MP3 default), distribution (Store + direct), Whisper bundling (tiny + on-demand)                                                                                    |
| 0.3     | 2026-02-17 | Lola Lovelace                | Added executive summary, change log, index. Casulo document standard.                                                                                                                                              |
| 0.4     | 2026-02-17 | Andre Vianna / Lola Lovelace | Tech stack changed: Blazor Hybrid → Avalonia UI (MIT, prettier, cross-platform).                                                                                                                                   |
| 0.5     | 2026-02-17 | Andre Vianna / Lola Lovelace | Added storage strategy: file system layout, principles, expanded data model with InstalledModel entity.                                                                                                            |
| 0.6     | 2026-02-17 | Andre Vianna / Lola Lovelace | Detailed hardware requirements per Whisper model size (disk, RAM, CPU, speed). Three tiers: minimum, recommended, power user.                                                                                      |
| 0.7     | 2026-02-17 | Andre Vianna / Lola Lovelace | Added support strategy: in-app FAQ, website, GitHub Issues, email. User support flow defined.                                                                                                                      |
| 0.8     | 2026-02-17 | Lola Lovelace                | Fixed stale Blazor refs → Avalonia. Trimmed MVP: cut tags, batch export, manual language override, multi-sort, high contrast theme, exports folder.                                                                |
| 0.9     | 2026-02-17 | Andre Vianna / Lola Lovelace | Added first-run onboarding flow (4-step wizard). Subject to user testing refinement.                                                                                                                               |
| 0.10    | 2026-02-17 | Andre Vianna / Lola Lovelace | Added error handling strategy: 3 severity levels, 5 principles, known scenarios table. Strategy-first, not list-first.                                                                                             |
| 0.11    | 2026-02-17 | Lola Lovelace                | Added legal & licensing: all deps MIT, privacy policy/EULA requirements for Store submission.                                                                                                                      |
| 0.12    | 2026-02-17 | Andre Vianna / Lola Lovelace | Added update strategy: Store auto, direct download opt-in auto-update (off by default).                                                                                                                            |
| 0.13    | 2026-02-17 | Andre Vianna                 | Decision: No analytics/telemetry in MVP. 100% local is the brand promise. Revisit in v2 (always opt-in, always disableable).                                                                                       |
| 0.14    | 2026-02-17 | Lola Lovelace                | Fixed section numbering. Added Recording Status field. No concurrent recordings. Clarified hotkey conflict UX. Removed stale open question.                                                                        |
| 0.15    | 2026-02-17 | Andre Vianna / Lola Lovelace | Renamed Dito → VivaVoz. Trademark conflict with existing Windows voice-to-text product at getdito.com. New name from Portuguese "viva" (alive) + "voz" (voice). Domain: vivavoz.app. Accessibility deferred to v2. |
| 0.16    | 2026-02-17 | Lola Lovelace                | Fixed domain refs (vivavoz-app.com → vivavoz.app). Accessibility explicitly marked as v2 in NFR section. Final review grade: A.                                                                                    |

---

## Index

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Target Users](#3-target-users)
4. [MVP Scope (v1.0)](#4-mvp-scope-v10)
5. [Error Handling Strategy](#5-error-handling-strategy)
6. [First-Run Experience](#6-first-run-experience)
7. [User Interface](#7-user-interface)
8. [Tech Stack](#8-tech-stack)
9. [Data Model & Storage Strategy](#9-data-model--storage-strategy)
10. [Legal & Licensing](#10-legal--licensing)
11. [Update Strategy](#11-update-strategy)
12. [Support Strategy](#12-support-strategy)
13. [Future Versions](#13-future-versions-out-of-mvp-scope)
14. [Success Metrics](#14-success-metrics)
15. [Decisions Made](#15-decisions-made)
16. [Open Questions](#16-open-questions)

---

## 1. Overview

VivaVoz is a Windows desktop application that captures voice input, transcribes it locally using Whisper, and lets users export recordings as audio files or text. No cloud dependency. No subscription. Your voice stays on your machine.

**Origin:** Inspired by SuperWhisper and Auto (Mac-only tools). No polished, local-first, affordable voice-to-text tool exists for Windows. VivaVoz fills that gap. The name comes from the Portuguese "viva" (alive) + "voz" (voice) — your voice, kept alive.

## 2. Problem Statement

When people interact with AI or compose text, they type — and in doing so, they self-edit. They strip out context, reasoning, qualifiers, and intent. The result is compressed, imprecise input that produces generic output.

Voice preserves the "mess" — the qualifiers, the second-guessing, the *why* behind the *what*. That mess is what AI (and humans) need to produce meaningful results.

Mac users have multiple polished tools for this. **Windows users have nothing good.**

## 3. Target Users

### Primary
- **Knowledge workers on Windows** who interact with AI tools (ChatGPT, Copilot, Claude) and want richer input
- **Developers** who use coding agents and want to dictate intent rather than type compressed prompts

### Secondary
- **Writers and content creators** who think better out loud
- **Professionals** who need quick voice memos transcribed (meetings, ideas, notes)
- **Non-native English speakers** who express themselves more naturally by speaking

## 4. MVP Scope (v1.0)

### 4.1 Core Features

#### F1: Voice Capture
- Global hotkey to start/stop recording (configurable in Settings)
- Push-to-talk mode (hold key to record, release to stop)
- Toggle mode (press to start, press again to stop)
- Visual indicator showing recording state (system tray + overlay)
- Support for default system microphone
- **No concurrent recordings** — hotkey is disabled while a recording is active (toggle) or while holding (push-to-talk). One recording at a time.
- **Hotkey conflicts:** If the configured hotkey is already in use by another app, VivaVoz shows a warning in Settings when the user saves. The user resolves it by picking a different key combo — VivaVoz does not silently fail or auto-reassign.

#### F2: Local Transcription
- On-device transcription using Whisper (no cloud)
- Multiple model sizes (tiny → large) — user selects based on speed/accuracy preference
- Language auto-detection
- Transcription happens automatically after recording stops

#### F3: Recording Management (CRUD)
- List all recordings with metadata (date, duration, language, transcript preview)
- View full transcript for any recording
- Play back original audio
- Edit transcript text manually
- Delete recordings
- Search recordings by transcript content
- Sorted by date (newest first)

#### F4: Export
- Export as audio file (default: MP3; options: WAV, OGG) via standard Save dialog
- Export as text file (TXT/MD) via standard Save dialog
- Copy transcript to clipboard (one-click)

### 4.2 Non-Functional Requirements

#### Performance
- Recording start latency: < 200ms from hotkey press
- Transcription speed: ≥ real-time (1 min audio ≤ 1 min processing) on modern hardware
- App startup: < 3 seconds
- Memory footprint: < 200MB idle, < 500MB during transcription

#### Privacy
- All processing local by default
- No telemetry without explicit opt-in
- No cloud calls in v1
- Audio files stored in user-controlled local directory

#### Hardware Requirements by Whisper Model

| Model      | Disk Size | RAM Usage | Min Hardware          | Transcription Speed | Best For                    |
|------------|-----------|-----------|-----------------------|---------------------|-----------------------------|
| **Tiny**   | 75 MB     | ~128 MB   | 4 GB RAM, 2-core CPU  | ~10x real-time      | Quick notes, low-end PCs    |
| **Base**   | 142 MB    | ~256 MB   | 4 GB RAM, 2-core CPU  | ~7x real-time       | Daily use, good balance     |
| **Small**  | 466 MB    | ~512 MB   | 8 GB RAM, 4-core CPU  | ~4x real-time       | Better accuracy, most users |
| **Medium** | 1.5 GB    | ~1.5 GB   | 8 GB RAM, 4-core CPU  | ~2x real-time       | High accuracy               |
| **Large**  | 2.9 GB    | ~3 GB     | 16 GB RAM, 6-core CPU | ~1x real-time       | Maximum accuracy            |

*Speeds are approximate for CPU-only inference via whisper.cpp. GPU acceleration (if available) significantly improves performance.*

#### System Requirements

**Minimum (Tiny/Base models):**
- Windows 10 21H2+ or Windows 11
- x64 architecture
- 4 GB RAM
- 2-core CPU
- 500 MB free disk space (app + tiny model)

**Recommended (Small/Medium models):**
- Windows 10/11
- x64 architecture
- 8 GB RAM
- 4-core CPU
- 3 GB free disk space (app + multiple models)

**Power User (Large model):**
- 16 GB RAM
- 6+ core CPU
- 5 GB free disk space
- GPU with 4+ GB VRAM (optional, for acceleration)

ARM64 support: stretch goal for v1, likely v2.

#### Accessibility (v2 — not in MVP)
- Keyboard-navigable UI
- Screen reader compatible
- Full accessibility audit deferred to v2. MVP prioritizes core functionality.

## 5. Error Handling Strategy

### Severity Levels

| Level               | Definition                                          | User Experience                                                   | System Action                                              |
|---------------------|-----------------------------------------------------|-------------------------------------------------------------------|------------------------------------------------------------|
| **⚠️ Warning**      | Unexpected but non-blocking. App continues.         | Toast notification (auto-dismiss 5s)                              | Log to `vivavoz.log`. Continue.                            |
| **🔶 Recoverable**  | Operation failed but app is stable. User can retry. | Modal dialog with message + action buttons (Retry / Skip / Help)  | Log. Preserve partial work. Offer recovery path.           |
| **🔴 Catastrophic** | Cannot continue. Data loss risk.                    | Full-screen error: what happened + what was saved + how to report | Log. Save what's salvageable. Offer crash report (opt-in). |

### Principles

1. **Never lose audio.** If recording started, raw audio buffer saves to temp no matter what. Recover on next launch.
2. **Always log.** Every error → `%LOCALAPPDATA%/VivaVoz/logs/vivavoz.log` with timestamp, severity, context. Rotated weekly.
3. **Always offer a way forward.** Every error has an action: retry, skip, open settings, or contact support. Never a dead end.
4. **User message ≠ developer message.** User sees plain language. Log file gets the stack trace.
5. **Graceful degradation.** Preferred model fails → fall back to tiny. Export fails → offer clipboard. Always a Plan B.

### Known Scenarios

| Scenario                 | Level           | User Sees                                                               | System Does                                       |
|--------------------------|-----------------|-------------------------------------------------------------------------|---------------------------------------------------|
| Microphone busy          | 🔶 Recoverable  | "Microphone in use. Close other apps and try again." [Retry] [Settings] | Log. Wait for retry.                              |
| No microphone detected   | 🔶 Recoverable  | "No microphone found." [Open Sound Settings] [Help]                     | Log. Link to Windows settings.                    |
| Disk full mid-recording  | 🔶 Recoverable  | "Storage full. Recording saved (partial)."                              | Save partial audio to temp. Log.                  |
| Model download fails     | 🔶 Recoverable  | "Download failed." [Retry] [Use Tiny]                                   | Log. Fall back to bundled model.                  |
| Transcription inaccurate | ⚠️ Warning      | "Transcription may be inaccurate." [Re-transcribe] [Edit]               | Log. Keep original audio.                         |
| Hotkey conflict          | ⚠️ Warning      | "Shortcut conflicts with [App]." [Change Shortcut]                      | Log. Suggest alternative.                         |
| Crash during recording   | 🔴 Catastrophic | On next launch: "VivaVoz recovered a recording." [Keep] [Discard]       | Auto-save temp buffer. Recovery on startup.       |
| SQLite corruption        | 🔴 Catastrophic | "Database error. Your audio files are safe."                            | Backup corrupt DB. Create fresh. Audio untouched. |

*This table is a starting point. New scenarios are classified using the same three levels and principles above.*

## 6. First-Run Experience

On first launch, VivaVoz guides the user through a 4-step onboarding wizard:

1. **Welcome** — "Hi, I'm VivaVoz. I turn your voice into text, right on your machine." Brief value prop, no fluff.
2. **Model Selection** — Choose Tiny (fastest, bundled) or download Base (better accuracy). Progress bar for download. User can always change later in Settings.
3. **Test Recording** — "Say something!" Button records a short clip → transcribes → shows the result. User sees VivaVoz work before they need it. Their first recording is already saved.
4. **Hotkey Setup** — Show default hotkey, let them customize. Explain push-to-talk vs toggle. Done.

After the wizard, user lands on the main screen with their test recording visible.

*Note: This flow is a starting point. Will be refined based on real user testing.*

## 7. User Interface

### 7.1 System Tray
- VivaVoz lives in the system tray when not actively in use
- Tray icon shows recording state (idle / recording / transcribing)
- Right-click menu: Open VivaVoz, Quick Record, Settings, Exit

### 7.2 Main Window
- **Recordings List** — Left panel, chronological, searchable
- **Detail View** — Right panel showing selected recording's transcript + audio player
- **Quick Actions** — Copy, Export, Delete, Edit

### 7.3 Recording Overlay
- Minimal floating indicator during recording (waveform + duration)
- Always-on-top, draggable, dismissable
- Click to stop recording

### 7.4 Settings
- Hotkey configuration
- Whisper model selection (with download manager)
- Audio input device selection
- Storage location
- Export defaults (format, output directory)
- Theme (light/dark/system)

## 8. Tech Stack

| Component          | Technology                                  |
|--------------------|---------------------------------------------|
| **Framework**      | .NET 10 + Avalonia UI (MIT, cross-platform) |
| **UI**             | Avalonia MVVM + Fluent theme                |
| **Transcription**  | whisper.cpp via Whisper.net (P/Invoke)      |
| **Audio Capture**  | NAudio or platform APIs                     |
| **Storage**        | SQLite (via EF Core)                        |
| **Audio Playback** | NAudio                                      |
| **Installer**      | MSIX or WiX                                 |

## 9. Data Model & Storage Strategy

### 9.1 File System Layout

VivaVoz uses `%LOCALAPPDATA%/VivaVoz/` as its root. SQLite holds structured data; the file system holds binary assets.

```plaintext
%LOCALAPPDATA%/VivaVoz/
├── data/
│   └── vivavoz.db              ← SQLite: recordings, settings, tags
├── audio/
│   └── {yyyy-MM}/
│       ├── {guid}.wav       ← raw recording (original, never modified)
│       └── {guid}.mp3       ← converted export (generated on demand)
├── models/
│   ├── whisper-tiny.bin      ← bundled with installer
│   ├── whisper-base.bin      ← downloaded on demand
│   └── whisper-large.bin     ← downloaded on demand
```

**Principles:**
- **Audio is immutable** — raw `.wav` is the source of truth, never overwritten
- **Monthly subfolders** — prevents flat directories with thousands of files
- **GUID filenames** — no collisions, no special character issues
- **Models are separate** — large binaries isolated, easy to manage/delete
- **Exports use standard Save dialog** — user picks destination, no dedicated folder
- **Delete = DB row + audio file** — cascading cleanup, no orphans
- **Configurable root** — user can move the entire VivaVoz folder (e.g. to a larger drive)

### 9.2 Entities

```plaintext
Recording
├── Id (GUID)
├── Title (auto-generated from first words, or user-set)
├── AudioFileName (relative path: {yyyy-MM}/{guid}.wav)
├── Transcript (text)
├── Status (enum: Recording → Transcribing → Complete | Failed)
├── Language (detected or manually set)
├── Duration (TimeSpan)
├── CreatedAt (DateTime UTC)
├── UpdatedAt (DateTime UTC)
├── WhisperModel (which model was used for transcription)
└── FileSize (bytes — for storage management)

Settings
├── HotkeyConfig (key combo + mode: push-to-talk or toggle)
├── WhisperModelSize (tiny/base/small/medium/large)
├── AudioInputDevice (system default or specific device)
├── StoragePath (root folder, default: %LOCALAPPDATA%/VivaVoz)
├── ExportFormat (default: MP3, options: WAV, OGG)
├── Theme (light/dark/system)
└── Language (default: auto-detect, or fixed language)

InstalledModel
├── ModelName (tiny/base/small/medium/large)
├── FilePath (relative to models/)
├── FileSize (bytes)
├── DownloadedAt (DateTime UTC)
└── IsDefault (bool)
```

## 10. Legal & Licensing

### Open Source Dependencies

All dependencies are MIT licensed — free for commercial use, no restrictions.

| Dependency  | License       | Purpose                       |
|-------------|---------------|-------------------------------|
| whisper.cpp | MIT           | Whisper inference engine      |
| Whisper.net | MIT           | .NET bindings for whisper.cpp |
| NAudio      | MIT           | Audio capture and playback    |
| Avalonia UI | MIT           | UI framework                  |
| EF Core     | MIT           | SQLite ORM                    |
| SQLite      | Public Domain | Database                      |

### Required Documents

| Document                | Where                                     | When                                                    |
|-------------------------|-------------------------------------------|---------------------------------------------------------|
| **Privacy Policy**      | vivavoz.app/privacy                       | Required for Microsoft Store submission. Before launch. |
| **Terms of Use / EULA** | Embedded in installer + vivavoz.app/terms | Before launch.                                          |

### Privacy Policy Summary

VivaVoz processes everything locally. No data leaves the user's machine. No accounts. No telemetry. No analytics in v1. If opt-in analytics are added in v2, the policy will be updated and users will be prompted to consent.

### EULA Summary

Standard $5 one-time purchase. No warranty. No liability. Non-transferable license. User owns their recordings and transcripts — we claim no rights to their content.

## 11. Update Strategy

### Distribution Channels

| Channel             | Update Mechanism               | User Action               |
|---------------------|--------------------------------|---------------------------|
| **Microsoft Store** | Handled by Store automatically | None — seamless           |
| **Direct download** | In-app update check on launch  | User opts in via Settings |

### Direct Download Update Flow

1. On launch, VivaVoz checks vivavoz.app/version.json for latest version (lightweight HTTP call)
2. If newer version available → non-intrusive banner: "Update available (v1.1). [Update Now] [Later]"
3. **Auto-update setting (off by default):** User can enable in Settings → "Automatically download and install updates"
4. When enabled: download happens in background, installs on next app restart
5. When disabled: banner notification only, user downloads manually

### Principles

- **Never forced.** User always has the choice to skip or delay.
- **Auto-update is opt-in.** Off by default. Respects user control.
- **Update check is lightweight.** Single JSON fetch, no heavy payloads until user consents.
- **No update during recording.** If a recording is active, updates wait.

### Settings Entity Addition

```plaintext
Settings
└── AutoUpdate (bool, default: false)
```

## 12. Support Strategy

### MVP (v1)

| Channel             | Implementation                                                 | Purpose                                    |
|---------------------|----------------------------------------------------------------|--------------------------------------------|
| **In-app Help**     | Built-in Getting Started + FAQ page                            | First stop — reduces support volume        |
| **Product Website** | vivavoz.app — landing page, FAQ, download links                | Public face, purchase, documentation       |
| **GitHub Issues**   | Public repo with issue templates (bug report, feature request) | Transparent, community-driven, power users |
| **Contact Email**   | <support@casuloailabs.com>                                     | Non-technical users, private issues        |

**User support flow:**
1. Problem → **In-app FAQ** (immediate, no internet needed)
2. Still stuck → **vivavoz.app/faq** (more detailed, searchable)
3. Bug or feature request → **GitHub Issues** (templates guide the report)
4. Private or non-technical → **Email**

### Future (v2+)
- Discord community (when user base justifies it)
- Knowledge base / docs site
- In-app feedback widget

## 13. Future Versions (Out of MVP Scope)

### v2: AI Cleanup + Community
- BYOK (Bring Your Own Key) — OpenAI, Anthropic, Gemini
- Mode system — custom AI cleanup prompts per task
- Clipboard auto-paste into active window

### v3: Smart Features
- App-aware mode switching
- Custom vocabulary / jargon dictionary
- Streaming transcription (real-time as you speak)

### v4: Platform Expansion
- Web companion (Avalonia WASM or standalone web app)
- Mac support (via MAUI)
- Team/enterprise features

## 14. Success Metrics

- **Build:** Working MVP in 4 weeks
- **Validate:** 100 beta users in first month
- **Revenue:** First paid download within 6 weeks of launch ($5 one-time, impulse price point)

## 15. Decisions Made

1. **Whisper model distribution** — Bundle smallest model (tiny). Larger models download on demand via in-app model manager.
2. **Audio format** — Default export: MP3. Configurable dropdown: MP3, WAV, OGG.
3. **Pricing** — $5 one-time. Gain clients first.
4. **Distribution** — Both Microsoft Store and direct download (vivavoz.app).
5. **Domain** — vivavoz.app (available, to be registered).

## 16. Open Questions

*None at this time.*

---

*Your voice, alive.* 🎙️
