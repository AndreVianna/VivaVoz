# TASK LOTE 2 (Delivery 2b) - Theme, Error Handling, Graceful Degradation

## Tarefas Incluídas:
- **2.11 Theme Support:** Light/Dark/System theme switching via Avalonia's `RequestedThemeVariant`. Add a Theme selector in Settings (ComboBox: Light/Dark/System). Persist the choice in the existing settings infrastructure. Apply theme on startup from saved preference. (Note: a basic ThemeService already exists from a prior fix — build on it.)
- **2.13 Error Handling Framework:** Implement 3 severity levels: Warning (toast/snackbar auto-dismiss), Recoverable (modal with retry/cancel), Catastrophic (full-screen with details + restart). Create an `INotificationService` with methods for each level. Wire into MainViewModel for microphone errors, transcription failures, export failures. Use Avalonia's notification/dialog system.
- **2.14 Graceful Degradation:** If the preferred Whisper model fails or is unavailable, automatically fall back to the next smaller installed model (e.g., medium → small → tiny). If export to file fails (permissions, disk full), offer clipboard as fallback for text export. Add fallback logic in `TranscriptionService` and `ExportService`.

## Sub-agent Instructions
- Work on branch `delivery-2b`.
- Follow existing code patterns and architecture (MVVM, dependency injection, interfaces).
- Write xUnit tests for all new logic.
- When all tests pass (`dotnet test`), commit as 'feat: implement theme switching, error handling framework, and graceful degradation' and PUSH to origin delivery-2b.
