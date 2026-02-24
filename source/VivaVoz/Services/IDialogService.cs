namespace VivaVoz.Services;

/// <summary>
/// Abstraction over confirmation dialogs for testability.
/// </summary>
public interface IDialogService {
    /// <summary>
    /// Shows a confirmation dialog and returns true if the user confirmed.
    /// </summary>
    Task<bool> ShowConfirmAsync(string title, string message);
}
