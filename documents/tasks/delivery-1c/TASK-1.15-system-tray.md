# TASK-1.15: System Tray Integration

**Delivery:** 1c
**Priority:** Medium — required before phase 2a (additional interaction surfaces)
**PRD Reference:** Section 7.1 (System Tray), Section 7.5 (Interaction Surfaces)

---

## Summary

Add system tray icon with context menu as the second interaction surface.

## Requirements

### Tray Icon
- Show VivaVoz icon in system tray when app is running
- Icon changes state to indicate: idle, recording, transcribing
- Single click: open/focus main window
- App minimizes to tray (close button minimizes, doesn't exit — configurable in Settings)

### Context Menu (Right-click)
- **Start Recording / Stop Recording** — toggles based on current state
- **Open VivaVoz** — brings main window to front
- **Settings** — opens Settings directly
- **---** (separator)
- **Exit** — actually closes the app

### Tray Notification
- When transcription completes while main window is hidden → show tray balloon/toast: "Transcription complete: [first 30 chars of transcript...]"
- Clicking the notification opens main window and selects the recording

### Behavior
- App starts minimized to tray (configurable — first run starts with main window)
- Tray icon persists when main window is closed
- Recording state is always visible via tray icon

### New Settings Fields
- `MinimizeToTray` (bool, default: true) — close button minimizes to tray instead of exiting
- `StartMinimized` (bool, default: false) — app starts in tray without showing main window

## Dependencies
- TASK-1.16 (App Icon) — the tray icon needs the actual icon asset

## Test Scenarios

1. App starts → tray icon visible
2. Right-click → context menu shows correct items
3. Click "Start Recording" from tray → recording starts, icon changes state
4. Close main window → app stays in tray
5. Click "Exit" from tray → app fully closes
6. Recording active → tray icon shows recording state
7. Transcription completes with window hidden → tray notification appears
8. Click notification → window opens with recording selected

## Acceptance Criteria

- [ ] System tray icon visible when app is running
- [ ] Context menu with all required items
- [ ] Icon reflects recording/idle/transcribing state
- [ ] App minimizes to tray on close (configurable)
- [ ] All existing tests pass + new tests for tray behavior
