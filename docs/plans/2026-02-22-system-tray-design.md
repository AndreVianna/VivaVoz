# System Tray Integration Design (TASK-1.15)

**Date:** 2026-02-22
**Branch:** delivery-1c

---

## Summary

Add a system tray icon to VivaVoz that persists when the main window is hidden, reflects recording/transcribing state, and provides a right-click context menu for core actions.

---

## Chosen Approach: Programmatic TrayService

A new `TrayService` class manages the Avalonia `TrayIcon` instance independently of the main window. `App.axaml.cs` instantiates and wires it alongside existing services. This matches the existing manual-DI service pattern and keeps tray logic isolated and testable.

---

## Files to Create / Modify

| File | Action |
|---|---|
| `source/VivaVoz/Models/Settings.cs` | Add `MinimizeToTray` (bool, default true), `StartMinimized` (bool, default false) |
| `source/VivaVoz/Data/AppDbContext.cs` | Add EF column config for new settings fields |
| `source/VivaVoz/Migrations/` | New migration: `AddTraySettings` |
| `source/VivaVoz/Services/SettingsService.cs` | Update `CreateDefaults()` to include new fields |
| `source/VivaVoz/Services/ITrayService.cs` | New interface |
| `source/VivaVoz/Services/TrayService.cs` | New implementation |
| `source/VivaVoz/App.axaml.cs` | Instantiate TrayService; handle `StartMinimized` |
| `source/VivaVoz/Views/MainWindow.axaml.cs` | Override `OnClosing` to minimize to tray |
| `source/VivaVoz.Tests/Models/SettingsTests.cs` | New tests for new settings field defaults |
| `source/VivaVoz.Tests/Services/TrayServiceTests.cs` | New unit tests for tray state logic |

---

## ITrayService Interface

```csharp
public interface ITrayService {
    void Initialize(IClassicDesktopStyleApplicationLifetime desktop);
    void SetState(TrayIconState state);
    void ShowTranscriptionComplete(string transcript);
    void Dispose();
}

public enum TrayIconState { Idle, Recording, Transcribing }
```

---

## TrayService Behavior

- Creates a `TrayIcon` programmatically with `NativeMenu`
- **Left click** (Clicked event): shows and activates main window
- **Context menu items**:
  - Start Recording / Stop Recording (toggles based on `IsRecording`)
  - Open VivaVoz
  - Settings (opens settings; deferred: show settings window or focus existing)
  - Separator
  - Exit (calls `desktop.Shutdown()`)
- **Icon state**:
  - Idle → `avares://VivaVoz/Assets/vivavoz-mono-16x16.png`
  - Recording → `avares://VivaVoz/Assets/vivavoz-16x16.png`
  - Transcribing → `avares://VivaVoz/Assets/vivavoz-16x16.png` (tooltip indicates transcribing)
- **Tooltip**: Updated with current state or transcription result
- **Transcription notification**: When transcription completes and window is hidden, updates tooltip to show first 30 chars of transcript

---

## Settings Changes

```csharp
// Models/Settings.cs additions:
public bool MinimizeToTray { get; set; } = true;
public bool StartMinimized { get; set; } = false;
```

EF migration adds columns with default values (`1` and `0` respectively in SQLite).

---

## Window Close Behavior

`MainWindow.axaml.cs` overrides `OnClosing`:
```csharp
protected override void OnClosing(WindowClosingEventArgs e) {
    if (_settingsService.Current?.MinimizeToTray == true) {
        e.Cancel = true;
        Hide();
    }
    base.OnClosing(e);
}
```

The window needs access to `ISettingsService` — passed via constructor or property from App startup.

---

## StartMinimized Behavior

In `App.axaml.cs`, after creating the main window but before `desktop.MainWindow` is shown:
```csharp
if (settingsService.Current?.StartMinimized == true) {
    desktop.MainWindow.ShowInTaskbar = false;
    // Don't call Show() — window stays hidden, tray icon is visible
} else {
    // default: show window normally (Avalonia shows MainWindow automatically)
}
```

---

## Notification Strategy

Avalonia's `TrayIcon` does not expose a balloon/toast API. Platform-specific balloon notifications (libnotify on Linux, WinRT toast on Windows) are deferred to a future task. For this task, the tray icon tooltip is updated to reflect the transcription result when the window is hidden.

---

## Test Scenarios Covered

1. `Settings` defaults: `MinimizeToTray=true`, `StartMinimized=false`
2. `TrayService` state changes: idle → recording → transcribing → idle
3. `TrayService.ShowTranscriptionComplete`: tooltip updated with transcript snippet
4. Settings persisted: `MinimizeToTray=false` — window close should exit
5. `TrayService.Initialize` subscribes to recorder/transcription events

---

## Out of Scope

- OS-level balloon/toast notifications (future task)
- Settings UI panel for `MinimizeToTray`/`StartMinimized` (separate settings view task)
- "Clicking notification opens window with recording selected" (deferred with balloon notifications)
