namespace VivaVoz.Services;

/// <summary>
/// EF Core-backed settings service. Loads settings from the database on startup,
/// creating defaults if none exist. Provides save capability for future settings UI.
/// </summary>
/// <remarks>
/// Creates a new SettingsService with a DbContext factory.
/// Using a factory avoids long-lived DbContext issues and allows
/// scoped operations per load/save call.
/// </remarks>
/// <param name="contextFactory">Factory that creates AppDbContext instances.</param>
public class SettingsService(Func<AppDbContext> contextFactory) : ISettingsService {
    private readonly Func<AppDbContext> _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

    /// <inheritdoc />
    public Settings? Current { get; private set; }

    /// <inheritdoc />
    public async Task<Settings> LoadSettingsAsync() {
        await using var context = _contextFactory();

        var settings = await context.Settings.FirstOrDefaultAsync();
        if (settings is not null) {
            Current = settings;
            return settings;
        }

        settings = CreateDefaults();
        context.Settings.Add(settings);
        await context.SaveChangesAsync();

        Current = settings;
        return settings;
    }

    /// <inheritdoc />
    public async Task SaveSettingsAsync(Settings settings) {
        ArgumentNullException.ThrowIfNull(settings);

        await using var context = _contextFactory();

        var existing = await context.Settings.FindAsync(settings.Id);
        if (existing is not null) {
            context.Entry(existing).CurrentValues.SetValues(settings);
        }
        else {
            context.Settings.Add(settings);
        }

        await context.SaveChangesAsync();
        Current = settings;
    }

    private static Settings CreateDefaults() => new() {
        WhisperModelSize = "tiny",
        StoragePath = GetDefaultStoragePath(),
        Theme = "System",
        Language = "auto",
        HotkeyConfig = string.Empty,
        AudioInputDevice = null,
        AutoUpdate = false,
        MinimizeToTray = true,
        StartMinimized = false,
        RunAtStartup = false,
        RecordingMode = "Toggle"
    };

    private static string GetDefaultStoragePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "VivaVoz");
    }
}
