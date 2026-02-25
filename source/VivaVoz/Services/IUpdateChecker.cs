namespace VivaVoz.Services;

/// <summary>
/// Checks for available application updates.
/// </summary>
public interface IUpdateChecker {
    /// <summary>
    /// Checks whether a newer version is available.
    /// Returns <see langword="null"/> if no update is found or on any error (no internet, rate limit, etc.).
    /// </summary>
    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);
}
