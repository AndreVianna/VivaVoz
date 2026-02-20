# Task 1.5: Audio Capture Engine

**Goal:** Implement the logic to capture audio from the microphone and save it as a high-quality WAV file suitable for transcription.
**Part of:** Delivery 1a

## Context
This is the heart of VivaVoz. We need a reliable audio capture service that interfaces with the system microphone, handles the recording stream, and saves the data to disk in a format compatible with our transcription engine (Whisper).

## Requirements

### Functional
- Start recording on demand.
- Stop recording on demand.
- Save audio as 16-bit PCM WAV.
- Target path: `%LOCALAPPDATA%/VivaVoz/audio/{yyyy-MM}/{guid}.wav`.
- Throw a specific exception (`MicrophoneNotFoundException` or similar) if no input device is available.

### Technical
- **Library:** NAudio (`NAudio.Core`, `NAudio.Wasapi` preferred for Windows, fallback to `WaveInEvent` if simpler cross-platform but we are Windows-first). Let's use `WaveInEvent` for broad compatibility unless WASAPI is strictly needed for loopback (which we don't need yet).
- **Format:** 16kHz sample rate (Whisper requires 16k), Mono (1 channel).
- **Interface:** `IAudioRecorder` with methods `StartRecording()`, `StopRecording()`.
- **Events:** `RecordingStopped` event to signal completion.
- **Service:** `AudioRecorderService`.

### File Path Conventions
- Interface: `/home/andre/projects/VivaVoz/source/VivaVoz/Services/Audio/IAudioRecorder.cs`
- Implementation: `/home/andre/projects/VivaVoz/source/VivaVoz/Services/Audio/AudioRecorderService.cs`

## Acceptance Criteria (Verification Steps)

- [ ] **Start Recording**: Verify that calling `StartRecording()` changes the recording state to `Recording` and initiates data accumulation.
- [ ] **Stop Recording and Save**: Verify that calling `StopRecording()` changes the state to `Stopped` and saves a valid WAV file to `%LOCALAPPDATA%/VivaVoz/audio/{current-month}/{guid}.wav` with duration matching the recording time.
- [ ] **Audio Format Verification**: Verify that the saved recording file has a sample rate of 16000 Hz, 1 channel (Mono), and 16-bit depth.
- [ ] **No Microphone Handling**: Verify that attempting to `StartRecording()` when no microphone is connected throws a specific exception (e.g., `MicrophoneNotFoundException`) without crashing the application.

### Unit Tests Required

**Testing Standards (apply to ALL tests in this task):**
- **Framework:** xUnit
- **Mocking:** NSubstitute (already in test project — do NOT use Moq or any other framework)
- **Assertions:** AwesomeAssertions (add NuGet package if not present — use fluent assertion syntax)
- **Naming:** GUTs (Good Unit Tests) — `MethodName_Scenario_ExpectedBehavior`
- **Structure:** Arrange-Act-Assert (AAA) pattern, clearly separated
- **Principles:** FIRST — Fast, Isolated, Repeatable, Self-validating, Timely
- **One logical assertion per test** — each test verifies a single behavior
Produce unit tests in `VivaVoz.Tests` covering:
- **AudioRecorderService state management:** Verify `IsRecording` is false initially. Verify `IsRecording` is true after `StartRecording()`. Verify `IsRecording` is false after `StopRecording()`. Verify calling `StopRecording()` when not recording throws or is a no-op (document which).
- **AudioRecorderService file output:** Verify that after a start/stop cycle, a `.wav` file is created in the expected directory pattern (`audio/{yyyy-MM}/{guid}.wav`).
- **AudioRecorderService format:** Verify the WaveFormat is 16kHz, mono, 16-bit PCM.
- **MicrophoneNotFoundException:** Verify the custom exception can be instantiated with a message.
- **AudioRecordingStoppedEventArgs:** Verify `FilePath` and `Duration` properties are set correctly.
- **Minimum:** 7 tests. Use mocking for NAudio's `WaveInEvent` if direct hardware access is unavailable in CI.
