namespace VivaVoz.ViewModels;

public partial class MainViewModel : ObservableObject {
    private readonly AppDbContext _dbContext;
    private readonly IAudioRecorder _recorder;
    private readonly ITranscriptionManager _transcriptionManager;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsService _settingsService;
    public AudioPlayerViewModel AudioPlayer { get; }

    [ObservableProperty]
    public partial ObservableCollection<Recording> Recordings { get; set; } = [];

    [ObservableProperty]
    public partial Recording? SelectedRecording { get; set; }

    [ObservableProperty]
    public partial string DetailHeader { get; set; } = "No recording selected";

    [ObservableProperty]
    public partial string DetailBody { get; set; } = "Select a recording from the list to view details.";

    [ObservableProperty]
    public partial bool IsRecording { get; set; }

    [ObservableProperty]
    public partial string CopyButtonLabel { get; set; } = "Copy";

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

    public MainViewModel(
        IAudioRecorder recorder,
        IAudioPlayer audioPlayer,
        AppDbContext dbContext,
        ITranscriptionManager transcriptionManager,
        IClipboardService clipboardService,
        ISettingsService? settingsService = null) {
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transcriptionManager = transcriptionManager ?? throw new ArgumentNullException(nameof(transcriptionManager));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _settingsService = settingsService ?? new SettingsService(() => new AppDbContext());
        AudioPlayer = new AudioPlayerViewModel(audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer)));

#pragma warning disable IDE0305, IDE0028 // Simplify collection initialization
        Recordings = new ObservableCollection<Recording>(
            _dbContext.Recordings.OrderByDescending(recording => recording.CreatedAt).ToList());
#pragma warning restore IDE0028, IDE0305 // Simplify collection initialization

        IsRecording = _recorder.IsRecording;
        _recorder.RecordingStopped += OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted += OnTranscriptionCompleted;
    }

    public bool HasSelection => SelectedRecording is not null;
    public bool NoSelection => SelectedRecording is null;
    public bool IsNotRecording => !IsRecording;

    partial void OnIsRecordingChanged(bool value) {
        OnPropertyChanged(nameof(IsNotRecording));
    }

    [RelayCommand]
    private void SelectRecording(Recording? recording) => SelectedRecording = recording;

    [RelayCommand]
    private void StartRecording() {
        try {
            _recorder.StartRecording();
            IsRecording = _recorder.IsRecording;
        }
        catch (MicrophoneNotFoundException ex) {
            ShowMicrophoneNotFoundDialog(ex.Message);
        }
    }

    [RelayCommand]
    private void StopRecording() {
        _recorder.StopRecording();
        IsRecording = _recorder.IsRecording;
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
    private void Retranscribe() {
        if (SelectedRecording is null) return;
        var audioPath = Path.Combine(FilePaths.AudioDirectory, SelectedRecording.AudioFileName);
        if (!File.Exists(audioPath)) {
            SelectedRecording.Status = RecordingStatus.Failed;
            NotifyTranscriptProperties();
            return;
        }
        SelectedRecording.Status = RecordingStatus.Transcribing;
        NotifyTranscriptProperties();
        _transcriptionManager.EnqueueTranscription(SelectedRecording.Id, audioPath);
    }

    partial void OnSelectedRecordingChanged(Recording? value) {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NoSelection));
        NotifyTranscriptProperties();
        AudioPlayer.LoadRecording(value);

        if (value is null) {
            DetailHeader = "No recording selected";
            DetailBody = "Select a recording from the list to view details.";
            return;
        }

        var title = string.IsNullOrWhiteSpace(value.Title) ? "Recording selected" : value.Title;
        DetailHeader = title;
        DetailBody = "Detail view placeholder.";
    }

    private void NotifyTranscriptProperties() {
        OnPropertyChanged(nameof(TranscriptDisplay));
        OnPropertyChanged(nameof(IsTranscribing));
        OnPropertyChanged(nameof(IsTranscriptionFailed));
        OnPropertyChanged(nameof(ShowTranscriptSection));
        OnPropertyChanged(nameof(CanCopyTranscript));
        OnPropertyChanged(nameof(CanRetranscribe));
        OnPropertyChanged(nameof(RetranscribeButtonLabel));
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

        _transcriptionManager.EnqueueTranscription(recording.Id, e.FilePath);
    });

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
                recording.WhisperModel = e.ModelUsed ?? recording.WhisperModel;
            }
            else {
                recording.Status = RecordingStatus.Failed;
            }

            recording.UpdatedAt = DateTime.UtcNow;

            // Recording implements INPC, so list item bindings (Transcript, Status) auto-update.
            // Notify ViewModel computed properties if this is the currently selected recording.
            if (SelectedRecording?.Id == e.RecordingId) {
                OnPropertyChanged(nameof(SelectedRecording));
                NotifyTranscriptProperties();
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

    [ExcludeFromCodeCoverage]
    private static void ShowMicrophoneNotFoundDialog(string message) {
        var text = string.IsNullOrWhiteSpace(message)
            ? "No microphone device detected."
            : message;

        var window = new Window {
            Title = "Microphone not found",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new TextBlock {
                Text = text,
                Margin = new Thickness(24),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            }
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null) {
            window.ShowDialog(desktop.MainWindow);
            return;
        }

        window.Show();
    }
}
