namespace VivaVoz.ViewModels;

public partial class OnboardingViewModel : ObservableObject {
    private readonly ISettingsService _settingsService;
    private readonly IModelManager _modelManager;
    private readonly IAudioRecorder _recorder;
    private readonly ITranscriptionEngine _transcriptionEngine;
    private readonly Settings _settings;
    private CancellationTokenSource? _transcriptionCts;

    /// <summary>Raised when the user completes or skips the wizard.</summary>
    public event EventHandler? WizardCompleted;

    // ── Step navigation ──────────────────────────────────────────────────────

    [ObservableProperty]
    public partial int CurrentStep { get; set; } = 0;

    public bool IsFirstStep => CurrentStep == 0;
    public bool IsLastStep => CurrentStep == 3;
    public bool CanGoPrevious => CurrentStep > 0;
    public bool CanGoNext => !IsLastStep;
    public int CurrentStepDisplay => CurrentStep + 1;

    // Visibility helpers for step panels
    public bool IsStep1 => CurrentStep == 0;
    public bool IsStep2 => CurrentStep == 1;
    public bool IsStep3 => CurrentStep == 2;
    public bool IsStep4 => CurrentStep == 3;

    // ── Model-selection step (Step 2) ────────────────────────────────────────

    public ObservableCollection<ModelItemViewModel> Models { get; }

    // ── Test-recording step (Step 3) ─────────────────────────────────────────

    [ObservableProperty]
    public partial bool IsTestRecording { get; set; }

    [ObservableProperty]
    public partial bool IsTestTranscribing { get; set; }

    [ObservableProperty]
    public partial string TestTranscript { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasTestRecorded { get; set; }

    private bool CanStartTestRecording => !IsTestRecording && !IsTestTranscribing;
    private bool CanStopTestRecording => IsTestRecording;

    // ── Hotkey-setup step (Step 4) ────────────────────────────────────────────

    [ObservableProperty]
    public partial string HotkeyConfig { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsListeningForHotkey { get; set; }

    public string HotkeyDisplayText => IsListeningForHotkey
        ? "Press combination..."
        : string.IsNullOrEmpty(HotkeyConfig) ? "Ctrl + Shift + R" : HotkeyConfig.Replace("+", " + ");

    // ── Constructor ───────────────────────────────────────────────────────────

    public OnboardingViewModel(
        ISettingsService settingsService,
        IModelManager modelManager,
        IAudioRecorder recorder,
        ITranscriptionEngine transcriptionEngine) {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _transcriptionEngine = transcriptionEngine ?? throw new ArgumentNullException(nameof(transcriptionEngine));

        _settings = settingsService.Current ?? new Settings();
        HotkeyConfig = _settings.HotkeyConfig;

        Models = [.. modelManager.GetAvailableModelIds()
            .Select(id => new ModelItemViewModel(id, modelManager, SelectModel))];

        UpdateModelSelection(_settings.WhisperModelSize);

        _recorder.RecordingStopped += OnRecordingStopped;
    }

    // ── Step property change propagation ─────────────────────────────────────

    partial void OnCurrentStepChanged(int value) {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CurrentStepDisplay));
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(IsStep4));
        NextCommand.NotifyCanExecuteChanged();
        PreviousCommand.NotifyCanExecuteChanged();
    }

    // ── Navigation commands ───────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => CurrentStep++;

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void Previous() => CurrentStep--;

    [RelayCommand]
    private async Task FinishAsync() {
        _settings.HasCompletedOnboarding = true;
        _settings.HotkeyConfig = HotkeyConfig;
        await _settingsService.SaveSettingsAsync(_settings);
        WizardCompleted?.Invoke(this, EventArgs.Empty);
    }

    // ── Test recording commands ───────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanStartTestRecording))]
    private void StartTestRecording() {
        TestTranscript = string.Empty;
        HasTestRecorded = false;
        _recorder.StartRecording();
        IsTestRecording = true;
    }

    [RelayCommand(CanExecute = nameof(CanStopTestRecording))]
    private void StopTestRecording() {
        _recorder.StopRecording();
        IsTestRecording = false;
    }

    private async void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e) {
        IsTestTranscribing = true;
        try {
            _transcriptionCts?.Cancel();
            _transcriptionCts = new CancellationTokenSource();

            var options = new TranscriptionOptions(
                Language: _settings.Language,
                ModelId: _settings.WhisperModelSize);

            var result = await _transcriptionEngine.TranscribeAsync(
                e.FilePath, options, _transcriptionCts.Token);

            TestTranscript = string.IsNullOrWhiteSpace(result.Text)
                ? "(No speech detected)"
                : result.Text;
        }
        catch (OperationCanceledException) {
            TestTranscript = "(Transcription cancelled)";
        }
        catch (Exception ex) {
            Log.Warning(ex, "[OnboardingViewModel] Test transcription failed.");
            TestTranscript = $"(Transcription failed: {ex.Message})";
        }
        finally {
            IsTestTranscribing = false;
            HasTestRecorded = true;
            StartTestRecordingCommand.NotifyCanExecuteChanged();
        }
    }

    // ── Hotkey commands ───────────────────────────────────────────────────────

    [RelayCommand]
    private void StartSetHotkey() => IsListeningForHotkey = true;

    /// <summary>Called by the window's <c>OnKeyDown</c> when capturing a hotkey.</summary>
    internal void AcceptHotkeyCapture(string config) {
        HotkeyConfig = config;
        IsListeningForHotkey = false;
    }

    // ── Property-change side-effects ──────────────────────────────────────────

    partial void OnHotkeyConfigChanged(string value)
        => OnPropertyChanged(nameof(HotkeyDisplayText));

    partial void OnIsListeningForHotkeyChanged(bool value)
        => OnPropertyChanged(nameof(HotkeyDisplayText));

    partial void OnIsTestRecordingChanged(bool value) {
        StartTestRecordingCommand.NotifyCanExecuteChanged();
        StopTestRecordingCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsTestTranscribingChanged(bool value) {
        StartTestRecordingCommand.NotifyCanExecuteChanged();
        StopTestRecordingCommand.NotifyCanExecuteChanged();
    }

    // ── Model-selection helpers ───────────────────────────────────────────────

    private void SelectModel(string modelId) {
        _settings.WhisperModelSize = modelId;
        UpdateModelSelection(modelId);
    }

    private void UpdateModelSelection(string selectedId) {
        if (Models is null)
            return;
        foreach (var m in Models)
            m.IsSelected = m.ModelId == selectedId;
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    public void Cleanup() {
        _recorder.RecordingStopped -= OnRecordingStopped;
        _transcriptionCts?.Cancel();
        _transcriptionCts?.Dispose();
        _transcriptionCts = null;
    }
}
