# TASK 2a UI Fixes (Settings Refinements)

## 1. Hotkey Capture UX
**Problem:** Hotkey field uses a simple TextBox, allowing invalid string inputs and preventing proper key combination capture.
**Solution:**
- Replace `TextBox` with a read-only `TextBlock` to display the current combination (e.g., `Ctrl + Alt + R` or `Not Set`).
- Add a "Set Hotkey" (or "Change") button.
- When clicked, enter "Listening Mode" (update TextBlock to "Press combination...").
- Intercept the next `KeyDown` event on the window/control.
- Extract Modifiers (Ctrl/Alt/Shift) and the Key.
- Save the combination and exit Listening Mode.
*(Conflict detection deferred post-MVP)*

## 2. Model Selection Consolidation
**Problem:** The active "Model" dropdown is separated from the "Models" download/management list, causing UX confusion. It also shows models that aren't downloaded yet.
**Solution:**
- Merge the "Model" selection into the "Models" management area.
- Remove the separate "Transcription > Model" dropdown.
- In the "Models" list, add visual indication (e.g., a radio button or "Select" button) to choose the active model.
- Only allow selection of models where the status is "Installed".

## 3. Language Display Names
**Problem:** Language dropdown shows ISO codes (`en`, `pt`, `fr`) instead of readable names.
**Solution:**
- Update the Language dropdown to bind to a list of KeyValuePairs or a custom class.
- Display the full language name (e.g., "English", "Portuguese", "French", "Auto-detect").
- Maintain the underlying ISO code for backend binding (`en`, `pt`, `fr`, `auto`).

## Sub-agent Instructions
- Target files: `SettingsView.axaml`, `SettingsViewModel.cs`, and potentially `HotkeyService.cs` or a new input behavior class for the hotkey capture.
- Ensure Avalonia UI bindings are properly updated for the new Language and Model selection logic.
