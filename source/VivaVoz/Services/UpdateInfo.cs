namespace VivaVoz.Services;

/// <summary>
/// Holds information about an available application update.
/// </summary>
/// <param name="Version">The new version string (e.g. "1.2.3").</param>
/// <param name="DownloadUrl">URL to the release page or asset download.</param>
/// <param name="ReleaseNotes">Release notes / body text from the GitHub release.</param>
public record UpdateInfo(string Version, string DownloadUrl, string ReleaseNotes);
