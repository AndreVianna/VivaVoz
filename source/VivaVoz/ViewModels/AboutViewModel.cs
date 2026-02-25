using System.Net;
using System.Reflection;

namespace VivaVoz.ViewModels;

public partial class AboutViewModel : ObservableObject {
    public string AppName => "VivaVoz";
    public string Tagline => "Your voice, alive.";
    public string Credits => "Created by Andre Vianna";
    public string GitHubUrl => "https://github.com/AndreVianna/VivaVoz";
    public string IssuesUrl => "https://github.com/AndreVianna/VivaVoz/issues";

    public string AppVersion { get; }
    public string HotkeyDisplay { get; }

    /// <summary>Production constructor — reads version from the executing assembly and hotkey from settings.</summary>
    public AboutViewModel(ISettingsService settingsService) {
        ArgumentNullException.ThrowIfNull(settingsService);
        AppVersion = ResolveVersion();
        var hotkeyConfig = settingsService.Current?.HotkeyConfig;
        HotkeyDisplay = BuildHotkeyDisplay(hotkeyConfig);
    }

    /// <summary>Test constructor — accepts explicit version and hotkey strings.</summary>
    public AboutViewModel(string appVersion, string hotkeyConfig) {
        AppVersion = appVersion ?? "0.0.0";
        HotkeyDisplay = BuildHotkeyDisplay(hotkeyConfig);
    }

    [RelayCommand]
    private void OpenGitHub() => OpenUrl(GitHubUrl);

    [RelayCommand]
    private void OpenIssues() => OpenUrl(IssuesUrl);

    private static void OpenUrl(string url) {
        try {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch {
            // Best-effort; silently ignore if shell cannot open URL
        }
    }

    private static string ResolveVersion() {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static string BuildHotkeyDisplay(string? hotkeyConfig) =>
        string.IsNullOrWhiteSpace(hotkeyConfig)
            ? HotkeyConfig.Default.ToString().Replace("+", " + ")
            : hotkeyConfig.Replace("+", " + ");
}
