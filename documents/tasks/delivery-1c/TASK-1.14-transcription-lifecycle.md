# TASK-1.14: Transcription Lifecycle Fix

**Delivery:** 1c
**Priority:** High — blocks reliable transcription for all future phases
**PRD Reference:** F2 (Local Transcription), Section 5 (Error Handling), Section 9.2 (Entities)

---

## Summary

Fix the transcription lifecycle to handle orphaned recordings and give users manual control over (re-)transcription.

## Changes Required

### 1. New Status: PendingTranscription

Add `PendingTranscription` to the `RecordingStatus` enum, between `Recording` and `Transcribing`.

**New lifecycle:**
```
Recording → PendingTranscription → Transcribing → Complete | Failed
```

### 2. Recording Flow Update

When recording stops (`OnRecordingStopped`):
- Save recording with status `PendingTranscription` (not `Transcribing`)
- Then immediately enqueue for transcription

**Status transition responsibility:**
- `PendingTranscription` → set by `MainViewModel.OnRecordingStopped` (on save)
- `Transcribing` → set by `TranscriptionManager.ProcessTranscriptionAsync` (at start of processing, before calling engine)
- `Complete` / `Failed` → set by `TranscriptionManager` (on result)

This ensures that if the app dies between saving and transcribing, the status is honest.

### 3. Startup Recovery

On application startup:
- Query all recordings with status `Transcribing`
- Reset them to `PendingTranscription`
- No auto-retry — user decides via the button

### 4. (Re-)Transcribe Button

Add a button to the recording detail panel:
- **Visible when:** Status is `PendingTranscription`, `Failed`, or `Complete`
- **Hidden when:** Status is `Recording` or `Transcribing`
- **Label:** "Transcribe" for PendingTranscription/Failed, "Re-transcribe" for Complete
- **Behavior:** Enqueue the recording for transcription using the currently selected Whisper model from Settings

### 5. Database Migration

- Add `PendingTranscription = 0` to enum (or adjust existing values)
- Migration to convert any existing `Transcribing` records to `PendingTranscription`

### 6. UI List Update Fix

Ensure the recordings list updates when transcription completes:
- Verify `INotifyPropertyChanged` fires for Status and Transcript on the Recording entity
- The list item should refresh to show transcript preview instead of "(No transcript yet)"

## Test Scenarios

1. Record → stop → status is PendingTranscription → auto-transitions to Transcribing → Complete
2. Kill app during transcription → restart → status shows PendingTranscription → click Transcribe → works
3. Complete recording → change model in Settings → click Re-transcribe → new transcription with new model
4. Failed recording → click Transcribe → retries successfully
5. Recording list updates transcript preview after transcription completes
6. During active transcription → button is hidden (cannot double-submit)
7. Status transitions: PendingTranscription → Transcribing happens at engine pickup (not at enqueue)

## Acceptance Criteria

- [ ] PendingTranscription status exists and is used correctly
- [ ] Orphaned Transcribing records are recovered on startup
- [ ] (Re-)Transcribe button works for all applicable states
- [ ] Recording list refreshes after transcription completes
- [ ] All existing tests pass + new tests for lifecycle
- [ ] Database migration handles existing data
