# Task 1.7: Recording CRUD — Create + View

**Goal:** Integrate audio capture with the UI and data layer, enabling users to create new recordings that persist to the database and appear in the list.
**Part of:** Delivery 1a

## Context
This task connects the dots. The "Record" button triggers the audio engine (Task 1.5), saves the file, creates a database entry (Task 1.2), and updates the UI (Task 1.6b). It transforms the app from a static shell into a functional recorder.

## Requirements

### Functional
- A prominent "Record" button must be visible in the main window (or tray, but main window for MVP first).
- **Start Recording:**
  - Clicking "Record" starts audio capture.
  - UI updates to show "Recording..." state (e.g., button changes to "Stop", status indicator).
- **Stop Recording:**
  - Clicking "Stop" stops capture.
  - Audio file is finalized on disk.
  - A new `Recording` entity is created in the database with metadata (Duration, Path, CreatedAt).
  - The new recording appears at the top of the list immediately.
- **Persistence:** Created recordings must survive app restart.

### Technical
- **Commands:** `StartRecordingCommand`, `StopRecordingCommand` in `MainViewModel` or `RecorderViewModel`.
- **Services:** `IAudioRecorder`, `IDataService` (EF Core context usage).
- **Event Handling:** Subscribe to `AudioRecorder.Stopped` (or manual stop logic) to trigger DB save.
- **Validation:** Handle microphone unavailability gracefully (show error dialog).

### File Path Conventions
- ViewModel: `/home/andre/projects/VivaVoz/source/VivaVoz/ViewModels/RecorderViewModel.cs`

## Acceptance Criteria (Verification Steps)

- [ ] **Successful Recording Creation**
  - Ensure a microphone is connected and available.
  - Click the "Record" button.
  - Wait for approximately 5 seconds.
  - Click the "Stop" button.
  - Verify a new recording appears at the top of the recordings list.
  - Verify the displayed duration is approximately 00:05.
  - Verify a `.wav` file exists in the designated audio storage folder.
  - Verify a new record exists in the SQLite database (`Recordings` table).

- [ ] **Persistence Check**
  - Create a recording as per the step above.
  - Close the application completely.
  - Reopen the application.
  - Verify the previously created recording is still visible in the list.
  - Verify the details (date, time, duration) match the original recording.

- [ ] **Error Handling - No Microphone**
  - Disconnect or disable the microphone (simulate unavailability).
  - Click the "Record" button.
  - Verify an error message (dialog or notification) is displayed (e.g., "No microphone found").
  - Verify the application remains responsive (does not crash).

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
- **StartRecordingCommand:** Verify command calls `IAudioRecorder.StartRecording()`. Verify UI state updates (`IsRecording = true`). Verify command is disabled while already recording.
- **StopRecordingCommand:** Verify command calls `IAudioRecorder.StopRecording()`. Verify a new `Recording` entity is created with correct metadata (Duration, AudioFileName, Status, CreatedAt). Verify the new recording is added to the list at position 0 (top).
- **Persistence:** Verify `Recording` entity is saved to the database (use in-memory SQLite).
- **Error handling:** Verify `MicrophoneNotFoundException` is caught and surfaced as an error state (not a crash).
- **Minimum:** 7 tests. Use mocked `IAudioRecorder` for command tests.
