namespace VivaVoz.Models;

public class Settings {
    public int Id { get; set; }
    public string HotkeyConfig { get; set; } = string.Empty;
    public string WhisperModelSize { get; set; } = "base";
    public string? AudioInputDevice { get; set; }
    public string StoragePath { get; set; } = GetDefaultStoragePath();
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "auto";
    public bool AutoUpdate { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
    public bool RunAtStartup { get; set; } = false;
    public string RecordingMode { get; set; } = "Toggle";
    public int? OverlayX { get; set; }
    public int? OverlayY { get; set; }
    public bool AutoCopyToClipboard { get; set; } = true;
    public bool HasCompletedOnboarding { get; set; } = false;
    public bool CheckForUpdatesOnStartup { get; set; } = true;

    private static string GetDefaultStoragePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return System.IO.Path.Combine(localAppData, "VivaVoz");
    }
}
