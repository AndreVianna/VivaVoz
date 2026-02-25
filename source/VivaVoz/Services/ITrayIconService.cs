namespace VivaVoz.Services;

/// <summary>
/// Manages the visual state of the system-tray icon to reflect the current
/// recording/transcription lifecycle.
/// </summary>
public interface ITrayIconService {
    /// <summary>Gets the current application state.</summary>
    AppState CurrentState { get; }

    /// <summary>
    /// Immediately transitions the tray icon to the given <paramref name="state"/>.
    /// Cancels any pending timed revert.
    /// </summary>
    void SetState(AppState state);

    /// <summary>
    /// Transitions the tray icon to <paramref name="state"/> for the specified
    /// <paramref name="duration"/>, then automatically reverts to
    /// <see cref="AppState.Idle"/>.  A subsequent call to <see cref="SetState"/>
    /// or <see cref="SetStateTemporary"/> cancels the pending revert.
    /// </summary>
    void SetStateTemporary(AppState state, TimeSpan duration);
}
