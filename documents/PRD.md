# Dito — Product Requirements Document

**Product:** Dito
**Tagline:** Said and done.
**Version:** 1.0 (MVP)
**Date:** 2026-02-17
**Authors:** Andre Vianna (Founder), Lola Lovelace (Product Lead)
**Company:** Casulo AI Labs

---

## Executive Summary

Dito is a $5 Windows desktop app that captures voice, transcribes it locally using Whisper, and exports recordings as audio or text. No cloud. No subscription. No account needed.

The market opportunity: Mac has 5+ polished voice-to-text tools (SuperWhisper, Auto, Wispr Flow). Windows has nothing comparable at an affordable price point. Wispr Flow ($81M funded) is entering Windows but is cloud-only and subscription-based, leaving the local-first, privacy-respecting niche wide open.

Dito fills that gap. Built with .NET 10 + Blazor Hybrid for portability. MVP in 4 weeks. $5 impulse buy. Microsoft Store + direct download.

*Dito e feito — said and done.*

---

## Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 0.1 | 2026-02-17 | Lola Lovelace | Initial PRD — MVP scope, tech stack, data model |
| 0.2 | 2026-02-17 | Andre Vianna / Lola Lovelace | Resolved open questions: pricing ($5), format (MP3 default), distribution (Store + direct), Whisper bundling (tiny + on-demand) |
| 0.3 | 2026-02-17 | Lola Lovelace | Added executive summary, change log, index. Casulo document standard. |
| 0.4 | 2026-02-17 | Andre Vianna / Lola Lovelace | Tech stack changed: Blazor Hybrid → Avalonia UI (MIT, prettier, cross-platform). |
| 0.5 | 2026-02-17 | Andre Vianna / Lola Lovelace | Added storage strategy: file system layout, principles, expanded data model with InstalledModel entity. |
| 0.6 | 2026-02-17 | Andre Vianna / Lola Lovelace | Detailed hardware requirements per Whisper model size (disk, RAM, CPU, speed). Three tiers: minimum, recommended, power user. |
| 0.7 | 2026-02-17 | Andre Vianna / Lola Lovelace | Added support strategy: in-app FAQ, website, GitHub Issues, email. User support flow defined. |

---

## Index

1. [Overview](#1-overview)
2. [Problem Statement](#2-problem-statement)
3. [Target Users](#3-target-users)
4. [MVP Scope (v1.0)](#4-mvp-scope-v10)
5. [User Interface](#5-user-interface)
6. [Tech Stack](#6-tech-stack)
7. [Data Model & Storage Strategy](#7-data-model--storage-strategy)
8. [Support Strategy](#8-support-strategy)
9. [Future Versions](#9-future-versions-out-of-mvp-scope)
10. [Success Metrics](#10-success-metrics)
11. [Decisions Made](#11-decisions-made)
12. [Open Questions](#12-open-questions)

---

## 1. Overview

Dito is a Windows desktop application that captures voice input, transcribes it locally using Whisper, and lets users export recordings as audio files or text. No cloud dependency. No subscription. Your voice stays on your machine.

**Origin:** Inspired by SuperWhisper and Auto (Mac-only tools). No polished, local-first, affordable voice-to-text tool exists for Windows. Dito fills that gap.

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
- Global hotkey to start/stop recording (configurable)
- Push-to-talk mode (hold key to record, release to stop)
- Toggle mode (press to start, press again to stop)
- Visual indicator showing recording state (system tray + overlay)
- Support for default system microphone

#### F2: Local Transcription
- On-device transcription using Whisper (no cloud)
- Multiple model sizes (tiny → large) — user selects based on speed/accuracy preference
- Language auto-detection
- Manual language selection override
- Transcription happens automatically after recording stops

#### F3: Recording Management (CRUD)
- List all recordings with metadata (date, duration, language, transcript preview)
- View full transcript for any recording
- Play back original audio
- Edit transcript text manually
- Delete recordings
- Search recordings by transcript content
- Sort by date, duration, or title

#### F4: Export
- Export as audio file (default: MP3; options: WAV, OGG)
- Export as text file (TXT/MD)
- Copy transcript to clipboard (one-click)
- Batch export selected recordings

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

| Model | Disk Size | RAM Usage | Min Hardware | Transcription Speed | Best For |
|-------|-----------|-----------|-------------|---------------------|----------|
| **Tiny** | 75 MB | ~128 MB | 4 GB RAM, 2-core CPU | ~10x real-time | Quick notes, low-end PCs |
| **Base** | 142 MB | ~256 MB | 4 GB RAM, 2-core CPU | ~7x real-time | Daily use, good balance |
| **Small** | 466 MB | ~512 MB | 8 GB RAM, 4-core CPU | ~4x real-time | Better accuracy, most users |
| **Medium** | 1.5 GB | ~1.5 GB | 8 GB RAM, 4-core CPU | ~2x real-time | High accuracy |
| **Large** | 2.9 GB | ~3 GB | 16 GB RAM, 6-core CPU | ~1x real-time | Maximum accuracy |

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

#### Accessibility
- Keyboard-navigable UI
- Screen reader compatible
- High contrast theme support

## 5. User Interface

### 5.1 System Tray
- Dito lives in the system tray when not actively in use
- Tray icon shows recording state (idle / recording / transcribing)
- Right-click menu: Open Dito, Quick Record, Settings, Exit

### 5.2 Main Window
- **Recordings List** — Left panel, chronological, searchable
- **Detail View** — Right panel showing selected recording's transcript + audio player
- **Quick Actions** — Copy, Export, Delete, Edit

### 5.3 Recording Overlay
- Minimal floating indicator during recording (waveform + duration)
- Always-on-top, draggable, dismissable
- Click to stop recording

### 5.4 Settings
- Hotkey configuration
- Whisper model selection (with download manager)
- Audio input device selection
- Storage location
- Export defaults (format, output directory)
- Theme (light/dark/system)

## 6. Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 10 + Avalonia UI (MIT, cross-platform) |
| **UI** | Avalonia MVVM + Fluent theme |
| **Transcription** | whisper.cpp via Whisper.net (P/Invoke) |
| **Audio Capture** | NAudio or platform APIs |
| **Storage** | SQLite (via EF Core) |
| **Audio Playback** | NAudio |
| **Installer** | MSIX or WiX |

## 7. Data Model & Storage Strategy

### 7.1 File System Layout

Dito uses `%LOCALAPPDATA%/Dito/` as its root. SQLite holds structured data; the file system holds binary assets.

```
%LOCALAPPDATA%/Dito/
├── data/
│   └── dito.db              ← SQLite: recordings, settings, tags
├── audio/
│   └── {yyyy-MM}/
│       ├── {guid}.wav       ← raw recording (original, never modified)
│       └── {guid}.mp3       ← converted export (generated on demand)
├── models/
│   ├── whisper-tiny.bin      ← bundled with installer
│   ├── whisper-base.bin      ← downloaded on demand
│   └── whisper-large.bin     ← downloaded on demand
└── exports/
    └── (user-exported files land here by default, configurable)
```

**Principles:**
- **Audio is immutable** — raw `.wav` is the source of truth, never overwritten
- **Monthly subfolders** — prevents flat directories with thousands of files
- **GUID filenames** — no collisions, no special character issues
- **Models are separate** — large binaries isolated, easy to manage/delete
- **Exports are separate** — user-facing output distinct from internal storage
- **Delete = DB row + audio file** — cascading cleanup, no orphans
- **Configurable root** — user can move the entire Dito folder (e.g. to a larger drive)

### 7.2 Entities

```
Recording
├── Id (GUID)
├── Title (auto-generated from first words, or user-set)
├── AudioFileName (relative path: {yyyy-MM}/{guid}.wav)
├── Transcript (text)
├── Language (detected or manually set)
├── Duration (TimeSpan)
├── CreatedAt (DateTime UTC)
├── UpdatedAt (DateTime UTC)
├── WhisperModel (which model was used for transcription)
├── FileSize (bytes — for storage management)
└── Tags (optional, for organization)

Settings
├── HotkeyConfig (key combo + mode: push-to-talk or toggle)
├── WhisperModelSize (tiny/base/small/medium/large)
├── AudioInputDevice (system default or specific device)
├── StoragePath (root folder, default: %LOCALAPPDATA%/Dito)
├── ExportFormat (default: MP3, options: WAV, OGG)
├── ExportPath (default: {StoragePath}/exports)
├── Theme (light/dark/system)
└── Language (default: auto-detect, or fixed language)

InstalledModel
├── ModelName (tiny/base/small/medium/large)
├── FilePath (relative to models/)
├── FileSize (bytes)
├── DownloadedAt (DateTime UTC)
└── IsDefault (bool)
```

## 8. Support Strategy

### MVP (v1)

| Channel | Implementation | Purpose |
|---------|---------------|---------|
| **In-app Help** | Built-in Getting Started + FAQ page | First stop — reduces support volume |
| **Product Website** | dito-app.com — landing page, FAQ, download links | Public face, purchase, documentation |
| **GitHub Issues** | Public repo with issue templates (bug report, feature request) | Transparent, community-driven, power users |
| **Contact Email** | support@casuloailabs.com | Non-technical users, private issues |

**User support flow:**
1. Problem → **In-app FAQ** (immediate, no internet needed)
2. Still stuck → **dito-app.com/faq** (more detailed, searchable)
3. Bug or feature request → **GitHub Issues** (templates guide the report)
4. Private or non-technical → **Email**

### Future (v2+)
- Discord community (when user base justifies it)
- Knowledge base / docs site
- In-app feedback widget

## 9. Future Versions (Out of MVP Scope)

### v2: AI Cleanup + Community
- BYOK (Bring Your Own Key) — OpenAI, Anthropic, Gemini
- Mode system — custom AI cleanup prompts per task
- Clipboard auto-paste into active window

### v3: Smart Features
- App-aware mode switching
- Custom vocabulary / jargon dictionary
- Streaming transcription (real-time as you speak)

### v4: Platform Expansion
- Web companion (Blazor WebAssembly)
- Mac support (via MAUI)
- Team/enterprise features

## 10. Success Metrics

- **Build:** Working MVP in 4 weeks
- **Validate:** 100 beta users in first month
- **Revenue:** First paid download within 6 weeks of launch ($5 one-time, impulse price point)

## 11. Decisions Made

1. **Whisper model distribution** — Bundle smallest model (tiny). Larger models download on demand via in-app model manager.
2. **Audio format** — Default export: MP3. Configurable dropdown: MP3, WAV, OGG.
3. **Pricing** — $5 one-time. Gain clients first.
4. **Distribution** — Both Microsoft Store and direct download (dito-app.com).
5. **Domain** — dito-app.com (available, to be registered).

## 12. Open Questions

1. **Name trademark** — Need to check "Dito" availability

---

*Dito e feito.* 🎙️
