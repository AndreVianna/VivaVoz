namespace VivaVoz.ViewModels;

public partial class MainViewModel : ObservableObject {
    private readonly AppDbContext _dbContext;
    private readonly IAudioRecorder _recorder;
    private readonly ITranscriptionManager _transcriptionManager;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;
    private readonly IModelManager _modelManager;
    private readonly IDialogService _dialogService;
    private readonly IExportService _exportService;
    private readonly INotificationService _notificationService;
    private readonly ICrashRecoveryService? _crashRecoveryService;
    private readonly ITrayIconService? _trayIconService;
    private readonly IHotkeyService? _hotkeyService;
    public AudioPlayerViewModel AudioPlayer { get; }
    public RecordingDetailViewModel Detail { get; }

    [ObservableProperty]
    public partial ObservableCollection<Recording> Recordings { get; set; } = [];

    [ObservableProperty]
    public partial Recording? SelectedRecording { get; set; }

    [ObservableProperty]
    public partial string DetailHeader { get; set; } = "No recording selected";

    [ObservableProperty]
    public partial string DetailBody { get; set; } = "Select a recording from the list to view details.";

    [ObservableProperty]
    public partial ObservableCollection<Recording> FilteredRecordings { get; set; } = [];

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsRecording { get; set; }

    [ObservableProperty]
    public partial string CopyButtonLabel { get; set; } = "Copy";

    [ObservableProperty]
    public partial bool HasOrphanedRecording { get; set; }

    /// <summary>
    /// The list of currently installed Whisper models, for the Re-Transcribe model picker.
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> InstalledModelIds { get; set; } = [];

    /// <summary>
    /// The model selected in the Re-Transcribe dropdown. Defaults to the active model from Settings.
    /// </summary>
    [ObservableProperty]
    public partial string SelectedRetranscribeModel { get; set; } = "base";

    /// <summary>
    /// Computed transcript text for display. Reflects the current recording state:
    /// - Transcribing → "Transcribing..."
    /// - PendingTranscription → "Waiting to transcribe..."
    /// - Failed → "Transcription failed."
    /// - Complete with empty/null transcript → "No speech detected."
    /// - Complete with transcript → the transcript text
    /// - No selection → empty string
    /// </summary>
    public string TranscriptDisplay => SelectedRecording switch {
        null => string.Empty,
        { Status: RecordingStatus.Transcribing } => "Transcribing...",
        { Status: RecordingStatus.PendingTranscription } => "Waiting to transcribe...",
        { Status: RecordingStatus.Recording } => string.Empty,
        { Status: RecordingStatus.Failed } => "Transcription failed.",
        { Status: RecordingStatus.Complete, Transcript: null or "" } => "No speech detected.",
        { Transcript: { } transcript } => transcript,
        _ => string.Empty
    };

    /// <summary>
    /// True when the selected recording is currently being transcribed.
    /// Used to show a loading indicator in the UI.
    /// </summary>
    public bool IsTranscribing => SelectedRecording?.Status == RecordingStatus.Transcribing;

    /// <summary>
    /// True when the selected recording's transcription has failed.
    /// Used for visual styling (e.g., red text).
    /// </summary>
    public bool IsTranscriptionFailed => SelectedRecording?.Status == RecordingStatus.Failed;

    /// <summary>
    /// True when the transcript section should be visible (any recording is selected).
    /// </summary>
    public bool ShowTranscriptSection => SelectedRecording is not null;

    /// <summary>
    /// True when the selected recording has a non-empty transcript that can be copied.
    /// </summary>
    public bool CanCopyTranscript => SelectedRecording is { Status: RecordingStatus.Complete, Transcript.Length: > 0 };

    /// <summary>
    /// True when the selected recording can be (re-)transcribed:
    /// PendingTranscription, Failed, or Complete. Hidden during Recording and active Transcribing.
    /// </summary>
    public bool CanRetranscribe => SelectedRecording?.Status is
        RecordingStatus.PendingTranscription or
        RecordingStatus.Failed or
        RecordingStatus.Complete;

    /// <summary>
    /// Button label: "Re-transcribe" for completed recordings, "Transcribe" otherwise.
    /// </summary>
    public string RetranscribeButtonLabel => SelectedRecording?.Status == RecordingStatus.Complete
        ? "Re-transcribe"
        : "Transcribe";

    /// <summary>
    /// Shows which model was used to produce the current transcript, e.g. "Transcribed with: base".
    /// Empty when no model info is recorded or no recording is selected.
    /// </summary>
    public string TranscribedWithInfo =>
        SelectedRecording is { Status: RecordingStatus.Complete } &&
        !string.IsNullOrEmpty(SelectedRecording.WhisperModel)
            ? $"Transcribed with {System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(SelectedRecording.WhisperModel)}"
            : string.Empty;

    public MainViewModel(
        IAudioRecorder recorder,
        IAudioPlayer audioPlayer,
        AppDbContext dbContext,
        ITranscriptionManager transcriptionManager,
        IClipboardService clipboardService,
        ISettingsService? settingsService = null,
        IModelManager? modelManager = null,
        IRecordingService? recordingService = null,
        IDialogService? dialogService = null,
        IExportService? exportService = null,
        ICrashRecoveryService? crashRecoveryService = null,
        INotificationService? notificationService = null,
        ITrayIconService? trayIconService = null,
        IHotkeyService? hotkeyService = null) {
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transcriptionManager = transcriptionManager ?? throw new ArgumentNullException(nameof(transcriptionManager));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _settingsService = settingsService ?? new SettingsService(() => new AppDbContext());
        _modelManager = modelManager ?? new WhisperModelService(new WhisperModelManager(), new System.Net.Http.HttpClient());
        _dialogService = dialogService ?? new DialogService();
        _exportService = exportService ?? new ExportService();
        _notificationService = notificationService ?? new NotificationService();
        _crashRecoveryService = crashRecoveryService;
        _trayIconService = trayIconService;
        AudioPlayer = new AudioPlayerViewModel(audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer)));
        Detail = new RecordingDetailViewModel(
            recordingService ?? new RecordingService(() => new AppDbContext()),
            _dialogService);
        Detail.RecordingDeleted += OnRecordingDeleted;

#pragma warning disable IDE0305, IDE0028 // Simplify collection initialization
        Recordings = new ObservableCollection<Recording>(
            _dbContext.Recordings.OrderByDescending(recording => recording.CreatedAt).ToList());
#pragma warning restore IDE0028, IDE0305 // Simplify collection initialization

        ApplyFilter();
        IsRecording = _recorder.IsRecording;
        _recorder.RecordingStopped += OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted += OnTranscriptionCompleted;
        HasOrphanedRecording = _crashRecoveryService?.HasOrphan() ?? false;

        // Populate installed models for the Re-Transcribe dropdown
        RefreshInstalledModels();
        SelectedRetranscribeModel = _settingsService.Current?.WhisperModelSize ?? "base";

        _hotkeyService = hotkeyService;
        if (_hotkeyService is not null) {
            _hotkeyService.RecordingStartRequested += (_, _) => Dispatcher.UIThread.Post(StartRecording);
            _hotkeyService.RecordingStopRequested += (_, _) => Dispatcher.UIThread.Post(StopRecording);
        }
    }

    public bool HasSelection => SelectedRecording is not null;
    public bool NoSelection => SelectedRecording is null;
    public bool IsNotRecording => !IsRecording;
    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);
    public bool NoRecordingsFound => HasSearchText && FilteredRecordings.Count == 0;
    public bool CanExportText => SelectedRecording is { Status: RecordingStatus.Complete, Transcript.Length: > 0 };
    public bool CanExportAudio => SelectedRecording is not null && !string.IsNullOrEmpty(SelectedRecording.AudioFileName);

    partial void OnIsRecordingChanged(bool value) {
        OnPropertyChanged(nameof(IsNotRecording));
    }

    partial void OnSearchTextChanged(string value) {
        OnPropertyChanged(nameof(HasSearchText));
        ApplyFilter();
    }

    partial void OnFilteredRecordingsChanged(ObservableCollection<Recording> value) {
        OnPropertyChanged(nameof(NoRecordingsFound));
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    internal void ApplyFilter() {
        var term = SearchText.Trim();
        var filtered = string.IsNullOrEmpty(term)
            ? Recordings
            : Recordings.Where(r =>
                (r.Transcript?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        FilteredRecordings = [.. filtered];
    }

    [RelayCommand]
    private void SelectRecording(Recording? recording) => SelectedRecording = recording;

    [RelayCommand]
    private void StartRecording() {
        try {
            _recorder.StartRecording();
            IsRecording = _recorder.IsRecording;
            _trayIconService?.SetState(AppState.Recording);
        }
        catch (MicrophoneNotFoundException ex) {
            _trayIconService?.SetState(AppState.Idle);
            _ = _notificationService.ShowWarningAsync(ex.Message);
        }
    }

    [RelayCommand]
    private void StopRecording() {
        _recorder.StopRecording();
        IsRecording = _recorder.IsRecording;
        _trayIconService?.SetState(AppState.Transcribing);
    }

    [RelayCommand]
    private async Task CopyTranscriptAsync() {
        if (!CanCopyTranscript)
            return;

        await _clipboardService.SetTextAsync(SelectedRecording!.Transcript!);
        CopyButtonLabel = "Copied!";

        // Reset button label after a short delay
        await Task.Delay(2000);
        CopyButtonLabel = "Copy";
    }

    [RelayCommand]
    private void ClearSelection() => SelectedRecording = null;

    [RelayCommand]
    private async Task ExportTextAsync() {
        if (SelectedRecording is not { Status: RecordingStatus.Complete, Transcript.Length: > 0 })
            return;

        var safeName = MakeSafeFileName(SelectedRecording.Title) + " transcript";
        var path = await _dialogService.ShowSaveFileDialogAsync(
            "Export Transcript",
            safeName,
            "txt",
            "Text and Markdown files",
            ["*.txt", "*.md"]);

        if (path is null)
            return;

        try {
            await _exportService.ExportTextAsync(SelectedRecording.Transcript!, path);
        }
        catch (Exception ex) {
            Log.Error(ex, "[MainViewModel] Failed to export transcript to {Path}.", path);
            var copyToClipboard = await _notificationService.ShowRecoverableErrorAsync(
                "Export Failed",
                "Could not save the transcript to the file. Copy to clipboard instead?",
                "Copy to Clipboard",
                "Cancel");
            if (copyToClipboard) {
                await _clipboardService.SetTextAsync(SelectedRecording.Transcript!);
            }
        }
    }

    [RelayCommand]
    private async Task ExportAudioAsync() {
        if (SelectedRecording is null || string.IsNullOrEmpty(SelectedRecording.AudioFileName))
            return;

        var safeName = MakeSafeFileName(SelectedRecording.Title);
        var path = await _dialogService.ShowSaveFileDialogAsync(
            "Export Audio",
            safeName,
            "wav",
            "WAV audio files",
            ["*.wav"]);

        if (path is null)
            return;

        var sourcePath = Path.Combine(FilePaths.AudioDirectory, SelectedRecording.AudioFileName);
        try {
            await _exportService.ExportAudioAsync(sourcePath, path);
        }
        catch (Exception ex) {
            Log.Error(ex, "[MainViewModel] Failed to export audio to {Path}.", path);
            await _notificationService.ShowWarningAsync("Could not save the audio file. Please check disk space and permissions.");
        }
    }

    [RelayCommand]
    internal void DismissOrphan() {
        _crashRecoveryService?.Dismiss();
        HasOrphanedRecording = false;
    }

    [RelayCommand]
    internal void RecoverOrphan() {
        var orphanPath = _crashRecoveryService?.GetOrphanPath();
        if (string.IsNullOrEmpty(orphanPath) || !File.Exists(orphanPath)) {
            _crashRecoveryService?.Dismiss();
            HasOrphanedRecording = false;
            return;
        }

        var now = DateTime.Now;
        var audioFileName = GetRelativeAudioPath(orphanPath);
        var fileSize = GetFileSize(orphanPath);
        var duration = EstimateDuration(fileSize);

        var recording = new Recording {
            Id = Guid.NewGuid(),
            Title = $"Recovered Recording {now:MMM dd, yyyy HH:mm}",
            AudioFileName = audioFileName,
            Transcript = null,
            Status = RecordingStatus.PendingTranscription,
            Language = _settingsService.Current?.Language ?? "auto",
            Duration = duration,
            CreatedAt = now,
            UpdatedAt = now,
            WhisperModel = _settingsService.Current?.WhisperModelSize ?? "base",
            FileSize = fileSize
        };

        _dbContext.Recordings.Add(recording);
        _dbContext.SaveChanges();

        Recordings.Insert(0, recording);
        ApplyFilter();

        _transcriptionManager.EnqueueTranscription(recording.Id, orphanPath);

        _crashRecoveryService?.Dismiss();
        HasOrphanedRecording = false;
    }

    private static string MakeSafeFileName(string name) {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private static TimeSpan EstimateDuration(long fileSize) {
        // 16kHz, 16-bit, mono PCM: 32000 bytes/sec, 44-byte WAV header
        const int bytesPerSecond = 32000;
        const int wavHeaderSize = 44;
        var audioBytes = Math.Max(0, fileSize - wavHeaderSize);
        return TimeSpan.FromSeconds(audioBytes / (double)bytesPerSecond);
    }

    [RelayCommand]
    private void Retranscribe() {
        if (SelectedRecording is null)
            return;
        var audioPath = Path.Combine(FilePaths.AudioDirectory, SelectedRecording.AudioFileName);
        if (!File.Exists(audioPath)) {
            SelectedRecording.Status = RecordingStatus.Failed;
            NotifyTranscriptProperties();
            return;
        }

        SelectedRecording.Status = RecordingStatus.Transcribing;
        NotifyTranscriptProperties();
        _transcriptionManager.EnqueueTranscription(SelectedRecording.Id, audioPath, SelectedRetranscribeModel);
    }

    /// <summary>
    /// Refreshes <see cref="InstalledModelIds"/> from the model manager.
    /// Falls back to an empty list when no model manager is configured.
    /// </summary>
    internal void RefreshInstalledModels() {
        var available = _modelManager.GetAvailableModelIds();
        InstalledModelIds = available.Where(_modelManager.IsModelDownloaded).ToList();
    }

    partial void OnSelectedRecordingChanged(Recording? value) {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NoSelection));
        // Reset re-transcribe model to global default when selection changes
        SelectedRetranscribeModel = _settingsService.Current?.WhisperModelSize ?? "base";
        NotifyTranscriptProperties();
        AudioPlayer.LoadRecording(value);
        Detail.LoadRecording(value);

        if (value is null) {
            DetailHeader = "No recording selected";
            DetailBody = "Select a recording from the list to view details.";
            return;
        }

        var title = string.IsNullOrWhiteSpace(value.Title) ? "Recording selected" : value.Title;
        DetailHeader = title;
        DetailBody = "Detail view placeholder.";
    }

    internal void OnRecordingDeleted(object? sender, Guid id) {
        var recording = Recordings.FirstOrDefault(r => r.Id == id);
        if (recording is null)
            return;
        Recordings.Remove(recording);
        ApplyFilter();
        SelectedRecording = null;
    }

    private void NotifyTranscriptProperties() {
        OnPropertyChanged(nameof(TranscriptDisplay));
        OnPropertyChanged(nameof(IsTranscribing));
        OnPropertyChanged(nameof(IsTranscriptionFailed));
        OnPropertyChanged(nameof(ShowTranscriptSection));
        OnPropertyChanged(nameof(CanCopyTranscript));
        OnPropertyChanged(nameof(CanRetranscribe));
        OnPropertyChanged(nameof(RetranscribeButtonLabel));
        OnPropertyChanged(nameof(TranscribedWithInfo));
        OnPropertyChanged(nameof(CanExportText));
        OnPropertyChanged(nameof(CanExportAudio));
        CopyButtonLabel = "Copy";
    }

    [ExcludeFromCodeCoverage]
    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e) => Dispatcher.UIThread.Post(() => {
        IsRecording = _recorder.IsRecording;

        var now = DateTime.Now;
        var audioFileName = GetRelativeAudioPath(e.FilePath);
        var fileSize = GetFileSize(e.FilePath);

        var currentSettings = _settingsService.Current;
        var recording = new Recording {
            Id = Guid.NewGuid(),
            Title = $"Recording {now:MMM dd, yyyy HH:mm}",
            AudioFileName = audioFileName,
            Transcript = null,
            Status = RecordingStatus.PendingTranscription,
            Language = currentSettings?.Language ?? "auto",
            Duration = e.Duration,
            CreatedAt = now,
            UpdatedAt = now,
            WhisperModel = currentSettings?.WhisperModelSize ?? "tiny",
            FileSize = fileSize
        };

        _dbContext.Recordings.Add(recording);
        _dbContext.SaveChanges();

        // Update in-memory status to Transcribing for immediate UI responsiveness.
        // The DB had PendingTranscription (crash-safe); TM will confirm Transcribing in DB.
        recording.Status = RecordingStatus.Transcribing;

        Recordings.Insert(0, recording);
        ApplyFilter();

        _transcriptionManager.EnqueueTranscription(recording.Id, e.FilePath);
    });

    /// <summary>
    /// Updates the tray icon state after transcription.
    /// Exposed as <c>internal</c> for unit testing.
    /// </summary>
    internal void HandleTranscriptionReadyForTray(bool success) {
        if (success) {
            _trayIconService?.SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(3));
        }
        else {
            _trayIconService?.SetState(AppState.Idle);
        }
    }

    /// <summary>
    /// Copies the transcript to the system clipboard if
    /// <c>Settings.AutoCopyToClipboard</c> is <c>true</c> and the transcript is
    /// non-empty.  Clipboard failures are swallowed so they never crash the app.
    /// Exposed as <c>internal</c> for unit testing.
    /// </summary>
    internal async Task TryCopyTranscriptToClipboardAsync(string? transcript) {
        if (!(_settingsService.Current?.AutoCopyToClipboard ?? true))
            return;
        if (string.IsNullOrEmpty(transcript))
            return;
        try {
            await _clipboardService.SetTextAsync(transcript);
            Log.Information("[MainViewModel] Transcript auto-copied to clipboard.");
        }
        catch (Exception ex) {
            Log.Warning(ex, "[MainViewModel] Auto-copy to clipboard failed.");
        }
    }

    [ExcludeFromCodeCoverage]
    private void OnTranscriptionCompleted(object? sender, TranscriptionCompletedEventArgs e)
        => Dispatcher.UIThread.Post(() => {
            var recording = Recordings.FirstOrDefault(r => r.Id == e.RecordingId);
            if (recording is null)
                return;

            if (e.Success) {
                recording.Transcript = e.Transcript;
                recording.Status = RecordingStatus.Complete;
                recording.Language = e.DetectedLanguage ?? recording.Language;
                recording.LanguageCode = e.DetectedLanguage ?? "unknown";
                recording.WhisperModel = e.ModelUsed ?? recording.WhisperModel;
                _ = TryCopyTranscriptToClipboardAsync(e.Transcript);
            }
            else {
                recording.Status = RecordingStatus.Failed;
                _ = _notificationService.ShowWarningAsync(
                    $"Transcription failed: {e.ErrorMessage ?? "Unknown error."}");
            }

            recording.UpdatedAt = DateTime.UtcNow;
            HandleTranscriptionReadyForTray(e.Success);

            // Recording implements INPC, so list item bindings (Transcript, Status) auto-update.
            // Notify ViewModel computed properties if this is the currently selected recording.
            if (SelectedRecording?.Id == e.RecordingId) {
                OnPropertyChanged(nameof(SelectedRecording));
                NotifyTranscriptProperties();
                Detail.LoadRecording(SelectedRecording);
            }
        });

    [ExcludeFromCodeCoverage]
    private static string GetRelativeAudioPath(string filePath) {
        try {
            return Path.GetRelativePath(FilePaths.AudioDirectory, filePath);
        }
        catch {
            return filePath;
        }
    }

    [ExcludeFromCodeCoverage]
    private static long GetFileSize(string filePath) {
        try {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }
        catch {
            return 0;
        }
    }

    [RelayCommand]
    [ExcludeFromCodeCoverage]
    private void OpenSettings() {
        var settingsWindow = new SettingsWindow {
            DataContext = new SettingsViewModel(_settingsService, _recorder, _modelManager, new Services.ThemeService())
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null) {
            settingsWindow.ShowDialog(desktop.MainWindow);
            return;
        }

        settingsWindow.Show();
    }
}
