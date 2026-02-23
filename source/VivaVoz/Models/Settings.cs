namespace VivaVoz.Models;

public class Settings {
    public int Id { get; set; }
    public string HotkeyConfig { get; set; } = string.Empty;
    public string WhisperModelSize { get; set; } = "tiny";
    public string? AudioInputDevice { get; set; }
    public string StoragePath { get; set; } = GetDefaultStoragePath();
    public string ExportFormat { get; set; } = "MP3";
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "auto";
    public bool AutoUpdate { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;

    private static string GetDefaultStoragePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return System.IO.Path.Combine(localAppData, "VivaVoz");
    }
}
