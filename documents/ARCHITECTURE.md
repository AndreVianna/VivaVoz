# VivaVoz — Architecture Document

**Product:** VivaVoz
**Version:** 0.1 (Draft)
**Date:** 2026-02-18
**Authors:** Lola Lovelace (Product Lead), Andre Vianna (Founder & Architect)
**Company:** Casulo AI Labs

---

## Executive Summary

This document defines the architectural boundaries and extension points for VivaVoz. It is deliberately **not** a prescriptive infrastructure document — it identifies where flexibility matters and where concrete implementation is the right choice.

**Philosophy:** Identify where change will happen and put seams there — and nowhere else. Over-abstraction is as dangerous as under-abstraction. The art is knowing which walls are load-bearing and which are movable.

**Two active seams** define VivaVoz's extensibility:
1. **Transcription Engine** — what turns audio into text
2. **Output Targets** — where transcribed text goes

**One documented future seam** is identified but deferred:
3. **Text Transformation** — AI-powered processing between transcription and output (v2)

Everything else is built concrete for MVP.

---

## Change Log

| Version | Date       | Author        | Changes                                                      |
|---------|------------|---------------|--------------------------------------------------------------|
| 0.1     | 2026-02-18 | Lola Lovelace | Initial draft — seams, pipeline, contracts, fixed components |

---

## Index

1. [Core Pipeline](#1-core-pipeline)
2. [Seam 1: Transcription Engine](#2-seam-1-transcription-engine)
3. [Seam 2: Output Targets](#3-seam-2-output-targets)
4. [Future Seam: Text Transformation](#4-future-seam-text-transformation)
5. [Fixed Components](#5-fixed-components)
6. [Dependency Direction](#6-dependency-direction)
7. [Decisions Made](#7-decisions-made)
8. [Open Questions](#8-open-questions)

---

## 1. Core Pipeline

VivaVoz's runtime flow is a linear pipeline:

```plaintext
User Action (hotkey) 
  → Audio Capture 
    → [Transcription Engine]     ← SEAM 1
      → Raw Text 
        → [Text Transform]       ← FUTURE SEAM (v2, pass-through for now)
          → [Output Target(s)]   ← SEAM 2
```

The pipeline is sequential. Each step completes before the next begins (no streaming in MVP). The seams are the joints where the pipeline can be extended without modifying the core.

---

## 2. Seam 1: Transcription Engine

### Why This Is a Seam

The transcription engine is the component most likely to change:
- MVP: Local Whisper via whisper.cpp
- v2: Cloud Whisper API (OpenAI), other local models, or hybrid (try local, fall back to cloud)
- Future: Entirely new transcription technologies

Hardwiring Whisper into the core would require surgery to support any alternative. The cost of this abstraction is one interface and one implementation — minimal.

### Contract

```csharp
public interface ITranscriptionEngine
{
    Task<TranscriptionResult> TranscribeAsync(
        string audioFilePath, 
        TranscriptionOptions options, 
        CancellationToken cancellationToken = default);
    
    IReadOnlyList<string> SupportedLanguages { get; }
    bool IsAvailable { get; }
}

public record TranscriptionOptions(
    string? Language = null,        // null = auto-detect
    string? ModelId = null          // null = user's default from Settings
);

public record TranscriptionResult(
    string Text,
    string DetectedLanguage,
    TimeSpan Duration,
    string ModelUsed,
    float? Confidence = null        // optional, engine-dependent
);
```

### MVP Implementation

`WhisperTranscriptionEngine` — wraps Whisper.net (P/Invoke to whisper.cpp). Reads model files from the `models/` directory. Model selection comes from Settings.

### What This Enables (v2+)

- `OpenAiWhisperEngine` — cloud transcription via BYOK API key
- `HybridTranscriptionEngine` — try local first, offer cloud if accuracy is low
- Any future engine that satisfies the contract

---

## 3. Seam 2: Output Targets

### Why This Is a Seam

Output targets are the primary integration surface — where VivaVoz connects to the user's workflow. The PRD already identifies multiple output types, and v2+ expands this significantly:

- MVP: File export (TXT/MD/SRT), clipboard copy
- v2: Auto-paste at cursor position, direct integration with editors (VSCode), AI tools (ChatGPT, Claude), productivity apps
- Future: API endpoints, webhooks, custom plugins

This is the feature that differentiates VivaVoz from a simple recorder. The architecture must make adding new output targets trivial.

### Contract

```csharp
public interface IOutputTarget
{
    string Id { get; }              // e.g., "file-txt", "clipboard", "cursor-paste"
    string DisplayName { get; }     // e.g., "Text File", "Clipboard", "Paste at Cursor"
    bool IsAvailable { get; }       // can this target be used right now?
    
    Task<OutputResult> SendAsync(
        OutputPayload payload, 
        CancellationToken cancellationToken = default);
}

public record OutputPayload(
    string Text,                    // the transcribed (and optionally transformed) text
    string? AudioFilePath = null,   // for targets that want the audio too
    RecordingMetadata? Metadata = null
);

public record RecordingMetadata(
    DateTime CreatedAt,
    TimeSpan Duration,
    string Language,
    string ModelUsed
);

public record OutputResult(
    bool Success,
    string? ErrorMessage = null
);
```

### MVP Implementations

| Target         | Class                      | Description                                         |
|----------------|----------------------------|-----------------------------------------------------|
| **File (TXT)** | `TextFileOutputTarget`     | Save transcript as .txt via Save dialog             |
| **File (MD)**  | `MarkdownFileOutputTarget` | Save transcript as .md via Save dialog              |
| **File (SRT)** | `SrtFileOutputTarget`      | Save transcript as .srt (subtitles) via Save dialog |
| **Audio File** | `AudioFileOutputTarget`    | Export audio as MP3/WAV/OGG via Save dialog         |
| **Clipboard**  | `ClipboardOutputTarget`    | Copy transcript text to system clipboard            |

### What This Enables (v2+)

- `CursorPasteOutputTarget` — clipboard + simulated Ctrl+V at current cursor position (the killer feature)
- `VsCodeOutputTarget` — insert directly into active editor via VSCode extension API
- `WebhookOutputTarget` — POST transcript to a configurable URL
- `NotionOutputTarget`, `GoogleDocsOutputTarget`, etc.

### Registration

Output targets are registered at startup. MVP uses compile-time registration. v2 could support runtime discovery (plugin system).

```csharp
// MVP: explicit registration
services.AddOutputTarget<TextFileOutputTarget>();
services.AddOutputTarget<ClipboardOutputTarget>();
// etc.
```

---

## 4. Future Seam: Text Transformation

### Why This Is Documented But Deferred

The PRD defines v2 features including AI-powered text cleanup, mode-based prompts, and BYOK API keys. These all sit in the pipeline between transcription and output:

```plaintext
Raw Text → [Transform] → Processed Text → Output Target
```

### Why We Don't Build It Now

1. **Pipeline insertion is cheap** — adding a step between two connected components is a linear change, not an architectural one
2. **We don't know the shape yet** — will it be single-transform or chain? Synchronous or streaming? User-configured or automatic? Building the abstraction now means guessing
3. **MVP has no transform** — the pipeline flows directly from transcription to output. No code needs to know a transform step could exist

### What We Document for v2

**Expected location:** Between `TranscriptionResult` and `OutputPayload`

**Likely shape:**
```csharp
// FUTURE — do not implement in MVP
public interface ITextTransformer
{
    string Id { get; }
    string DisplayName { get; }
    
    Task<TransformResult> TransformAsync(
        string text,
        TransformOptions options,
        CancellationToken cancellationToken = default);
}
```

**Expected capabilities:**
- Grammar cleanup
- Formatting (add punctuation, paragraphs)
- Mode-based prompts ("summarize", "formal email", "code comments")
- BYOK: user provides their own API key for OpenAI/Anthropic/Gemini

**Integration approach:** When v2 adds this, the pipeline becomes:
```plaintext
TranscriptionResult.Text 
  → ITextTransformer.TransformAsync() 
    → OutputPayload.Text
```

No existing contracts change. The transformer is injected between the two seams.

---

## 5. Fixed Components

These components are built concrete. No interfaces, no abstraction layers. They are what they are.

### 5.1 Audio Capture

**Technology:** NAudio or Windows platform APIs

**Why fixed:** Audio capture is platform-specific by nature. VivaVoz MVP is Windows-only. When Mac support arrives (v4), it will require platform-specific implementations anyway — an abstraction now would be premature and wrong (we don't know what the Mac API surface looks like).

**Responsibilities:**
- Start/stop recording on hotkey
- Capture from selected input device (or system default)
- Save raw audio as WAV to the `audio/` directory
- Handle microphone-busy and no-microphone errors

### 5.2 User Interface (Avalonia)

**Technology:** Avalonia UI with MVVM pattern and Fluent theme

**Why fixed:** The UI framework is the application. Swapping Avalonia would be a rewrite, not a refactor. MVVM provides enough separation between views and logic without additional abstraction.

**Components:**
- System tray icon + context menu
- Main window (recording list + detail view)
- Recording overlay (floating waveform indicator)
- Settings window
- First-run wizard
- Model download manager

### 5.3 Storage

**Technology:** SQLite via EF Core + file system

**Why fixed:** Local storage is the brand promise. The storage strategy (SQLite for metadata, file system for audio) is not changing. The schema may evolve, but EF Core migrations handle that.

### 5.4 Global Hotkeys

**Technology:** Platform-specific (Windows hooks)

**Why fixed:** Hotkey capture is inherently OS-level. No abstraction makes this simpler. When multi-platform arrives, each platform gets its own implementation — but that's a v4 concern, not an MVP architecture decision.

### 5.5 Settings Management

**Technology:** SQLite (same database as recordings)

**Why fixed:** Settings are a flat key-value structure. No reason to abstract reading preferences. Direct EF Core access.

### 5.6 Error Handling

**Approach:** Follows the three-tier strategy defined in the PRD (Warning / Recoverable / Catastrophic). Implemented as application-level exception handling and logging. No framework beyond standard .NET patterns.

---

## 6. Dependency Direction

```plaintext
┌─────────────────────────────────────┐
│              UI Layer               │
│   (Avalonia Views + ViewModels)     │
│   Fixed. Depends on everything.     │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│          Application Layer          │
│   (Pipeline orchestration, CRUD)    │
│   Owns the recording lifecycle.     │
│   Depends on contracts, not         │
│   implementations.                  │
└──────┬─────────────────┬────────────┘
       │                 │
       ▼                 ▼
┌──────────────┐  ┌──────────────────┐
│ ITranscription│  │ IOutputTarget    │
│ Engine       │  │                  │
│ (Contract)   │  │ (Contract)       │
└──────┬───────┘  └──────┬───────────┘
       │                 │
       ▼                 ▼
┌──────────────┐  ┌──────────────────┐
│ Whisper.net  │  │ File / Clipboard │
│ (Impl)       │  │ (Impls)          │
└──────────────┘  └──────────────────┘
```

**Rule:** The Application Layer depends on contracts (`ITranscriptionEngine`, `IOutputTarget`). Implementations depend on external libraries. The UI Layer depends on the Application Layer. Dependencies always point inward or downward — never upward.

**Fixed components** (audio capture, storage, hotkeys) live in the Application Layer as concrete services. They don't need contracts because they won't be swapped.

---

## 7. Decisions Made

1. **Two seams, not more.** Transcription and Output are the extension points. Everything else is concrete.
2. **Future transform seam documented but not built.** Pipeline insertion is cheap; building the wrong abstraction is expensive.
3. **No plugin system in MVP.** Output targets are registered at compile time. Runtime discovery is a v2 concern.
4. **No audio capture abstraction.** Platform-specific is fine for Windows-only MVP. Multi-platform gets its own implementations later.
5. **MVVM is the UI pattern.** Avalonia's built-in support is sufficient. No additional UI architecture layer.
6. **Dependency injection for seams.** Standard .NET DI container. Contracts registered at startup.

---

## 8. Open Questions

1. **Audio capture library:** NAudio vs Windows.Media.Capture? Need to evaluate which plays better with Avalonia and system tray integration. Decision deferred to implementation.

---

*The architecture is the seams, not the boxes.* 🏗️
