namespace VivaVoz.ViewModels;

public record LanguageOption(string Code, string DisplayName) {
    public override string ToString() => DisplayName;
}

public partial class SettingsViewModel : ObservableObject {
    private readonly ISettingsService _settingsService;
    private readonly IModelManager _modelManager;
    private readonly IThemeService _themeService;
    private readonly Settings _settings;
    private readonly bool _isInitializing = true;

    public static IReadOnlyList<string> AvailableThemes { get; } = ["System", "Light", "Dark"];
    public static IReadOnlyList<string> AvailableModels { get; } = ["tiny", "base", "small", "medium", "large-v3"];
    public static IReadOnlyList<string> AvailableExportFormats { get; } = ["MP3", "WAV", "OGG", "TXT", "MD"];
    public static IReadOnlyList<string> AvailableRecordingModes { get; } = ["Toggle", "Push-to-Talk"];
    public static IReadOnlyList<string> AvailableLanguages { get; } = ["auto", "en", "fr", "de", "es", "pt", "it", "ja", "zh"];
    public static IReadOnlyList<LanguageOption> AvailableLanguageOptions { get; } = [
        new("auto", "Auto-detect"),
        new("en", "English"),
        new("fr", "French"),
        new("de", "German"),
        new("es", "Spanish"),
        new("pt", "Portuguese"),
        new("it", "Italian"),
        new("ja", "Japanese"),
        new("zh", "Chinese"),
    ];

    public IReadOnlyList<string> AvailableDevices { get; }

    public ObservableCollection<ModelItemViewModel> Models { get; }

    public IReadOnlyList<string> InstalledModels => [.. Models.Where(m => m.IsInstalled).Select(m => m.ModelId)];

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

    [ObservableProperty]
    public partial bool IsListeningForHotkey { get; set; }

    public string HotkeyDisplayText => IsListeningForHotkey
        ? "Press combination..."
        : string.IsNullOrEmpty(HotkeyConfig) ? "Not Set" : HotkeyConfig.Replace("+", " + ");

    public LanguageOption? SelectedLanguageOption {
        get => AvailableLanguageOptions.FirstOrDefault(o => o.Code == Language);
        set { if (value is not null) Language = value.Code; }
    }

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

        Models = [.. modelManager.GetAvailableModelIds().Select(id => new ModelItemViewModel(id, modelManager, SelectModel))];

        foreach (var model in Models)
            model.PropertyChanged += (_, args) => {
                if (args.PropertyName == nameof(ModelItemViewModel.IsInstalled))
                    OnPropertyChanged(nameof(InstalledModels));
            };

        UpdateModelSelection(WhisperModelSize);

        _isInitializing = false;
    }

    partial void OnWhisperModelSizeChanged(string value) {
        SaveSetting(s => s.WhisperModelSize = value);
        UpdateModelSelection(value);
    }
    partial void OnAudioInputDeviceChanged(string? value) => SaveSetting(s => s.AudioInputDevice = value);
    partial void OnStoragePathChanged(string value) => SaveSetting(s => s.StoragePath = value);
    partial void OnExportFormatChanged(string value) => SaveSetting(s => s.ExportFormat = value);
    partial void OnThemeChanged(string value) {
        SaveSetting(s => s.Theme = value);
        if (!_isInitializing) _themeService.ApplyTheme(value);
    }
    partial void OnLanguageChanged(string value) {
        SaveSetting(s => s.Language = value);
        OnPropertyChanged(nameof(SelectedLanguageOption));
    }
    partial void OnRunAtStartupChanged(bool value) => SaveSetting(s => s.RunAtStartup = value);
    partial void OnMinimizeToTrayChanged(bool value) => SaveSetting(s => s.MinimizeToTray = value);
    partial void OnHotkeyConfigChanged(string value) {
        SaveSetting(s => s.HotkeyConfig = value);
        OnPropertyChanged(nameof(HotkeyDisplayText));
    }
    partial void OnRecordingModeChanged(string value) => SaveSetting(s => s.RecordingMode = value);
    partial void OnIsListeningForHotkeyChanged(bool value) => OnPropertyChanged(nameof(HotkeyDisplayText));

    [RelayCommand]
    private void StartSetHotkey() => IsListeningForHotkey = true;

    internal void AcceptHotkeyCapture(string config) {
        HotkeyConfig = config;
        IsListeningForHotkey = false;
    }

    private void SelectModel(string modelId) => WhisperModelSize = modelId;

    private void UpdateModelSelection(string selectedId) {
        if (Models is null) return;
        foreach (var m in Models)
            m.IsSelected = m.ModelId == selectedId;
    }

    private void SaveSetting(Action<Settings> update) {
        if (_isInitializing)
            return;
        update(_settings);
        _ = _settingsService.SaveSettingsAsync(_settings);
    }
}
