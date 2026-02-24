namespace VivaVoz.Services;

/// <summary>
/// Service that registers a system-wide hotkey and routes key events to recording commands.
/// Supports Toggle mode (press to start, press again to stop) and
/// Push-to-Talk mode (hold to record, release to stop).
/// </summary>
public interface IHotkeyService : IDisposable {
    /// <summary>Raised when a recording session should start.</summary>
    event EventHandler? RecordingStartRequested;

    /// <summary>Raised when a recording session should stop.</summary>
    event EventHandler? RecordingStopRequested;

    /// <summary><see langword="true"/> when a hotkey is successfully registered with the OS.</summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Attempts to register <paramref name="config"/> as a system-wide hotkey.
    /// Returns <see langword="false"/> when <paramref name="config"/> is <see langword="null"/>
    /// or when another application has already claimed the key combination.
    /// </summary>
    /// <param name="config">The hotkey to register, or <see langword="null"/> to skip.</param>
    /// <param name="recordingMode">
    /// <c>"Toggle"</c> for press-once/press-again semantics,
    /// or <c>"Push-to-Talk"</c> for hold-to-record semantics.
    /// </param>
    bool TryRegister(HotkeyConfig? config, string recordingMode);

    /// <summary>Unregisters the current hotkey, if any.</summary>
    void Unregister();
}
