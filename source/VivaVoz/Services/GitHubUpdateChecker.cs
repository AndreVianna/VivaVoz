using System.Net.Http;
using System.Text.Json;

namespace VivaVoz.Services;

/// <summary>
/// Checks for updates by querying the GitHub Releases API.
/// All errors (no internet, rate limit, parse failures) are swallowed and return null.
/// </summary>
public sealed class GitHubUpdateChecker : IUpdateChecker {
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://api.github.com/repos/AndreVianna/VivaVoz/releases/latest";

    public GitHubUpdateChecker(HttpClient httpClient) {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VivaVoz");
    }

    /// <inheritdoc />
    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default) {
        try {
            var response = await _httpClient.GetAsync(ApiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() : null;
            var htmlUrl = root.TryGetProperty("html_url", out var urlEl) ? urlEl.GetString() : null;
            var body = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() ?? string.Empty : string.Empty;

            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            // Strip leading 'v' to get plain semver
            var releaseVersionStr = tagName.TrimStart('v');

            if (!Version.TryParse(releaseVersionStr, out var releaseVersion))
                return null;

            var currentVersionStr = GetCurrentVersion();
            if (!Version.TryParse(currentVersionStr, out var currentVersion))
                return null;

            if (releaseVersion <= currentVersion)
                return null;

            return new UpdateInfo(releaseVersionStr, htmlUrl ?? string.Empty, body);
        }
        catch {
            // Silently ignore all errors (no internet, API rate limit, parse errors, etc.)
            return null;
        }
    }

    /// <summary>Returns the current application version as a 3-part string (major.minor.patch).</summary>
    internal static string GetCurrentVersion() {
        var assembly = System.Reflection.Assembly.GetEntryAssembly()
                    ?? System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString(3) ?? "0.0.0";
    }
}
