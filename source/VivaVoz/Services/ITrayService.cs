namespace VivaVoz.Services;

public enum TrayIconState {
    Idle,
    Recording,
    Transcribing,
    Ready,
}

public interface ITrayService : IDisposable {
    void Initialize();
    void SetState(TrayIconState state);

    /// <summary>
    /// Sets the tray icon state to <paramref name="state"/> for the given
    /// <paramref name="duration"/>, then reverts automatically to
    /// <see cref="TrayIconState.Idle"/>.
    /// </summary>
    void SetStateTemporary(TrayIconState state, TimeSpan duration);

    void ShowTranscriptionComplete(string? transcript);
}
