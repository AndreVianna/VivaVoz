# VivaVoz — Task Breakdown

**Version:** 0.2 (Pass 2 — Four deliveries, vertical slicing)
**Date:** 2026-02-18
**Authors:** Andre Vianna (Architect), Lola Lovelace (Product Lead)
**Source PRD:** PRD v0.16 (Grade A)

---

## Development Cycle

- **Branching:** dev branch → PR to main per completed task. Releases tagged.
- **Testing:** Incremental unit tests alongside code. Not TDD, not afterthought. Grow code and tests together, locking previous steps.
  - **Coverage target:** 80% line coverage on service and model classes
  - **Quality bar:** Every public method gets at least one happy path + one failure/edge case test
  - **Assertion quality:** Assertions must verify specific values/behavior. No `Assert.True(true)`, no `Assert.NotNull(result)` without checking the actual content. Test names describe the behavior being verified (e.g., `StopRecording_WhenNotRecording_ShouldThrow`).
  - **Per-task requirement:** Every task that produces a service, model, or business logic class must include corresponding unit tests in `VivaVoz.Tests`. Tests are deliverables, not afterthoughts. A task is not complete until its tests exist and pass.
- **Deliveries:** No formal sprints. Four deliveries, each demo-able and shippable to testers.
- **Deployment:** Direct download first (local testing + beta). Microsoft Store when stable.
- **Tooling:** Visual Studio 2026 Enterprise + Cursor
- **Execution model:** We write specs → agents execute → we review
- **Tech stack:** .NET 10, Avalonia UI (Fluent theme, MVVM), whisper.cpp via Whisper.net, NAudio, SQLite via EF Core

---

## Delivery 1a: Record → Play Back

*Goal: Prove audio capture works. Record voice, see it in a list, play it back.*

| Task | Name | Description |
|------|------|-------------|
| 1.1 | **Solution Scaffolding** | .NET 10, Avalonia, project structure, MVVM setup, Fluent theme |
| 1.2 | **SQLite + EF Core Setup** | Recording + Settings entities, initial migration, DB creation on startup |
| 1.3 | **File System Layout** | `%LOCALAPPDATA%/VivaVoz/` structure creation on startup (data/, audio/, models/, logs/) |
| 1.4 | **Logging Infrastructure** | `vivavoz.log`, weekly rotation, error logging with severity/timestamp/context |
| 1.5 | **Audio Capture Engine** | NAudio microphone recording → WAV file saved to audio/{yyyy-MM}/{guid}.wav |
| 1.6a | **App Shell + Navigation** | Main window skeleton, MVVM wiring, layout with left/right panels |
| 1.6b | **Recordings List + Detail Panel** | Left panel shows recordings (date, duration, preview). Right panel shows selected recording details. |
| 1.7 | **Recording CRUD — Create + View** | Record button → save to DB + file system → display in list |
| 1.8 | **Audio Playback** | Play back recording in detail view (NAudio, play/pause/stop, progress) |

**Delivery 1a Demo:** Open app → click record → speak → stop → see it in the list → play it back. 9 tasks.

---

## Delivery 1b: + Transcription

*Goal: Prove AI integration works. Recording auto-transcribes, transcript visible, copyable.*

| Task | Name | Description |
|------|------|-------------|
| 1.9 | **Whisper Integration** | whisper.cpp via Whisper.net, bundled tiny model, P/Invoke setup |
| 1.10 | **Transcription Pipeline** | Recording → auto-transcribe on stop → status updates (Recording → Transcribing → Complete/Failed) → store transcript |
| 1.11 | **Detail View — Transcript Display** | Show transcript text in detail panel below audio player |
| 1.12 | **Copy Transcript to Clipboard** | One-click copy button on detail view |
| 1.13 | **Settings Persistence** | Defaults loaded on startup, stored in SQLite, accessible for future settings screen |

**Delivery 1b Demo:** Everything from 1a + transcript appears after recording stops + copy to clipboard. 5 tasks.

---

## Delivery 2a: Full Feature Set

*Goal: A product you'd use daily. Hotkeys, tray, model management, search, edit, delete.*

| Task | Name | Description |
|------|------|-------------|
| 2.1 | **Global Hotkey System** | Push-to-talk + toggle modes, configurable key combo, conflict detection |
| 2.2 | **Recording Overlay** | Floating always-on-top indicator (waveform + duration), draggable, click to stop |
| 2.3 | **System Tray Integration** | Tray icon with state (idle/recording/transcribing), right-click menu (Open, Quick Record, Settings, Exit) |
| 2.4 | **Model Management** | On-demand download of Base/Small/Medium/Large, progress bar, size info, selection persisted |
| 2.5 | **Language Auto-Detection** | Whisper language detection, stored on recording entity |
| 2.6 | **Settings Screen** | Full settings UI: hotkey, model, audio device, storage path, theme, export defaults |
| 2.7 | **Search Recordings** | Search by transcript content, filter recordings list |
| 2.8 | **Recording CRUD — Edit + Delete** | Edit transcript text manually, delete with cascade (DB row + audio file) |

**Delivery 2a Demo:** Hotkeys work, tray icon lives, download bigger models, search recordings, edit transcripts, delete recordings. Full-feature app. 8 tasks.

---

## Delivery 2b: Export + Ship

*Goal: Polished, packaged, ready for beta testers and eventually the Store.*

| Task | Name | Description |
|------|------|-------------|
| 2.9 | **Audio Export** | MP3/WAV/OGG via standard Save dialog |
| 2.10 | **Text Export** | TXT/MD via standard Save dialog |
| 2.11 | **Theme Support** | Light/dark/system, Fluent theme switching |
| 2.12 | **First-Run Onboarding Wizard** | 4-step wizard: welcome → model selection → test recording → hotkey setup |
| 2.13 | **Error Handling Framework** | 3 severity levels (Warning/Recoverable/Catastrophic), toast/modal/full-screen patterns |
| 2.14 | **Graceful Degradation** | Model fallback (preferred → tiny), export fallback (file → clipboard) |
| 2.15 | **Crash Recovery** | Temp buffer auto-save during recording, recovery prompt on next launch |
| 2.16 | **Update Checker** | version.json fetch on launch, non-intrusive banner, opt-in auto-update |
| 2.17 | **In-App Help/FAQ** | Built-in Getting Started guide + FAQ page |
| 2.18 | **Installer/Packaging** | MSIX or WiX, direct download ready, bundled tiny model |

**Delivery 2b Demo:** Export recordings. Polished onboarding. Graceful error handling. Crash recovery. Installer. Ready for beta. 10 tasks.

---

## Summary

| Delivery | Tasks | Goal |
|----------|-------|------|
| 1a | 9 | Record → play back (hardware proof) |
| 1b | 5 | + Transcription (AI proof) |
| 2a | 8 | Full features (product proof) |
| 2b | 10 | Export + polish + ship (distribution proof) |
| **Total** | **32** | **MVP complete** |

---

*Your voice, alive.* 🎙️
