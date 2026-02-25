namespace VivaVoz.Services;

/// <summary>
/// Represents the overall recording/transcription state of the application,
/// used by <see cref="ITrayIconService"/> to drive the tray icon appearance.
/// </summary>
public enum AppState {
    /// <summary>App is idle — no recording or transcription in progress.</summary>
    Idle,

    /// <summary>Audio is actively being recorded.</summary>
    Recording,

    /// <summary>Transcription is in progress.</summary>
    Transcribing,

    /// <summary>Transcription just completed successfully; transcript is ready in clipboard.</summary>
    Ready,
}
