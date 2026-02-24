namespace VivaVoz.ViewModels;

public partial class SettingsViewModel : ObservableObject {
    private readonly ISettingsService _settingsService;
    private readonly IModelManager _modelManager;
    private readonly IThemeService _themeService;
    private readonly Settings _settings;
    private readonly bool _isInitializing = true;

    public static IReadOnlyList<string> AvailableThemes { get; } = ["System", "Light", "Dark"];
    public static IReadOnlyList<string> AvailableModels { get; } = ["tiny", "base", "small", "medium", "large"];
    public static IReadOnlyList<string> AvailableExportFormats { get; } = ["MP3", "WAV", "OGG", "TXT", "MD"];
    public static IReadOnlyList<string> AvailableRecordingModes { get; } = ["Toggle", "Push-to-Talk"];
    public static IReadOnlyList<string> AvailableLanguages { get; } = ["auto", "en", "fr", "de", "es", "pt", "it", "ja", "zh"];

    public IReadOnlyList<string> AvailableDevices { get; }

    public ObservableCollection<ModelItemViewModel> Models { get; }

    [ObservableProperty]
    public partial string WhisperModelSize { get; set; }

    [ObservableProperty]
    public partial string? AudioInputDevice { get; set; }

    [ObservableProperty]
    public partial string StoragePath { get; set; }

    [ObservableProperty]
    public partial string ExportFormat { get; set; }

    [ObservableProperty]
    public partial string Theme { get; set; }

    [ObservableProperty]
    public partial string Language { get; set; }

    [ObservableProperty]
    public partial bool RunAtStartup { get; set; }

    [ObservableProperty]
    public partial bool MinimizeToTray { get; set; }

    [ObservableProperty]
    public partial string HotkeyConfig { get; set; }

    [ObservableProperty]
    public partial string RecordingMode { get; set; }

    public SettingsViewModel(ISettingsService settingsService, IAudioRecorder recorder, IModelManager modelManager, IThemeService themeService) {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        ArgumentNullException.ThrowIfNull(recorder);
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));

        _settings = settingsService.Current ?? new Settings();

        WhisperModelSize = _settings.WhisperModelSize;
        AudioInputDevice = _settings.AudioInputDevice;
        StoragePath = _settings.StoragePath;
        ExportFormat = _settings.ExportFormat;
        Theme = _settings.Theme;
        Language = _settings.Language;
        RunAtStartup = _settings.RunAtStartup;
        MinimizeToTray = _settings.MinimizeToTray;
        HotkeyConfig = _settings.HotkeyConfig;
        RecordingMode = _settings.RecordingMode;

        AvailableDevices = recorder.GetAvailableDevices();

        Models = [.. modelManager.GetAvailableModelIds().Select(id => new ModelItemViewModel(id, modelManager))];

        _isInitializing = false;
    }

    partial void OnWhisperModelSizeChanged(string value) => SaveSetting(s => s.WhisperModelSize = value);
    partial void OnAudioInputDeviceChanged(string? value) => SaveSetting(s => s.AudioInputDevice = value);
    partial void OnStoragePathChanged(string value) => SaveSetting(s => s.StoragePath = value);
    partial void OnExportFormatChanged(string value) => SaveSetting(s => s.ExportFormat = value);
    partial void OnThemeChanged(string value) {
        SaveSetting(s => s.Theme = value);
        if (!_isInitializing) _themeService.ApplyTheme(value);
    }
    partial void OnLanguageChanged(string value) => SaveSetting(s => s.Language = value);
    partial void OnRunAtStartupChanged(bool value) => SaveSetting(s => s.RunAtStartup = value);
    partial void OnMinimizeToTrayChanged(bool value) => SaveSetting(s => s.MinimizeToTray = value);
    partial void OnHotkeyConfigChanged(string value) => SaveSetting(s => s.HotkeyConfig = value);
    partial void OnRecordingModeChanged(string value) => SaveSetting(s => s.RecordingMode = value);

    private void SaveSetting(Action<Settings> update) {
        if (_isInitializing)
            return;
        update(_settings);
        _ = _settingsService.SaveSettingsAsync(_settings);
    }
}
