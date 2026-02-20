# Task 1.8: Audio Playback

**Goal:** Enable users to listen to their recordings directly within the application, completing the capture-verify loop.
**Part of:** Delivery 1a

## Context
A voice recorder is useless if you can't hear what you captured. This task adds playback functionality to the detail view, allowing users to review their recordings immediately after creation.

## Requirements

### Functional
- **Play/Pause Control:**
  - A play/pause button in the recording detail panel.
  - Toggles between "Play" and "Pause" states based on current playback.
- **Progress Indicator:**
  - A slider or progress bar showing the current playback position.
  - Updates in real-time as audio plays.
  - Allows seeking (optional for MVP, but good for UX; let's say "basic seeking" or just display for now to keep it simple. Let's aim for display + seek if easy with slider).
- **Stop Functionality:**
  - Stops playback and resets position to start.
- **Auto-Reset:**
  - When playback finishes, the player automatically resets to the beginning.

### Technical
- **Library:** NAudio (`WaveOutEvent` or `WasapiOut`).
- **Service:** `IAudioPlayer` with methods `Play(string path)`, `Pause()`, `Stop()`.
- **State Management:** `IsPlaying` (bool), `CurrentPosition` (TimeSpan), `TotalDuration` (TimeSpan) exposed via ViewModel.
- **Event Handling:** Subscribe to `PlaybackStopped` to update UI state when audio ends naturally.

### File Path Conventions
- Service: `/home/andre/projects/VivaVoz/source/VivaVoz/Services/Audio/AudioPlayerService.cs`
- ViewModel: `/home/andre/projects/VivaVoz/source/VivaVoz/ViewModels/AudioPlayerViewModel.cs` (or part of DetailViewModel)

## Acceptance Criteria (Verification Steps)

- [ ] **Playback Start**
  - Select a recording from the list.
  - Click the "Play" button.
  - Verify audio starts playing (sound is audible).
  - Verify the "Play" button changes to a "Pause" icon/text.
  - Verify the progress bar advances smoothly.

- [ ] **Pause Playback**
  - While audio is playing, click the "Pause" button.
  - Verify audio stops immediately.
  - Verify the progress bar stops advancing.
  - Verify the button changes back to "Play".

- [ ] **Playback Completion**
  - Play a short recording (or seek near the end).
  - Allow playback to finish naturally.
  - Verify the audio stops.
  - Verify the progress bar resets to the start (0:00).
  - Verify the button shows "Play".

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
- **AudioPlayerService state management:** Verify `IsPlaying` is false initially. Verify `Play()` with a valid path sets `IsPlaying` to true. Verify `Pause()` sets `IsPlaying` to false. Verify `Stop()` sets `IsPlaying` to false and resets `CurrentPosition` to zero.
- **AudioPlayerService error handling:** Verify `Play()` with a non-existent file throws `FileNotFoundException` (or handles gracefully). Verify `Stop()` when not playing is a no-op (no throw).
- **AudioPlayerViewModel:** Verify `PlayCommand` toggles `IsPlaying`. Verify `StopCommand` resets position. Verify `CurrentPosition` and `TotalDuration` are exposed as bindable properties.
- **Minimum:** 7 tests. Use mocked `IAudioPlayer` for ViewModel tests. AudioPlayerService tests may need a real WAV file fixture (create a small test WAV in test setup).
