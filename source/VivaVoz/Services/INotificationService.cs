namespace VivaVoz.Services;

/// <summary>
/// Service for displaying user-facing notifications with three severity levels.
/// </summary>
public interface INotificationService {
    /// <summary>
    /// Shows a transient warning notification that auto-dismisses after a short delay.
    /// Use for non-critical issues the user should be aware of (e.g., microphone unavailable).
    /// </summary>
    Task ShowWarningAsync(string message);

    /// <summary>
    /// Shows a recoverable error dialog with a primary action button and a cancel option.
    /// Returns true if the user chose the primary action.
    /// Use for errors where the user can take corrective action (e.g., export failed → use clipboard).
    /// </summary>
    Task<bool> ShowRecoverableErrorAsync(
        string title,
        string message,
        string primaryLabel = "Retry",
        string cancelLabel = "Cancel");

    /// <summary>
    /// Shows a catastrophic error dialog that blocks the UI until the user acknowledges it.
    /// Offers Restart and Dismiss buttons for unrecoverable errors.
    /// </summary>
    Task ShowCatastrophicErrorAsync(string title, string message);
}
