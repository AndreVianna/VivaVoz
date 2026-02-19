namespace VivaVoz.ViewModels;

public partial class MainViewModel : ObservableObject {
    private readonly AppDbContext _dbContext;
    private readonly IAudioRecorder _recorder;
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

    public MainViewModel(IAudioRecorder recorder, IAudioPlayer audioPlayer, AppDbContext dbContext) {
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        AudioPlayer = new AudioPlayerViewModel(audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer)));

#pragma warning disable IDE0305, IDE0028 // Simplify collection initialization
        Recordings = new ObservableCollection<Recording>(
            _dbContext.Recordings.OrderByDescending(recording => recording.CreatedAt).ToList());
#pragma warning restore IDE0028, IDE0305 // Simplify collection initialization

        IsRecording = _recorder.IsRecording;
        _recorder.RecordingStopped += OnRecordingStopped;
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
    private void ClearSelection() => SelectedRecording = null;

    partial void OnSelectedRecordingChanged(Recording? value) {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NoSelection));
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

    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e) => Dispatcher.UIThread.Post(() => {
        IsRecording = _recorder.IsRecording;

        var now = DateTime.Now;
        var audioFileName = GetRelativeAudioPath(e.FilePath);
        var fileSize = GetFileSize(e.FilePath);

        var recording = new Recording {
            Id = Guid.NewGuid(),
            Title = $"Recording {now:MMM dd, yyyy HH:mm}",
            AudioFileName = audioFileName,
            Transcript = null,
            Status = RecordingStatus.Transcribing,
            Language = "auto",
            Duration = e.Duration,
            CreatedAt = now,
            UpdatedAt = now,
            WhisperModel = "tiny",
            FileSize = fileSize
        };

        _dbContext.Recordings.Add(recording);
        _dbContext.SaveChanges();
        Recordings.Insert(0, recording);
    });

    private static string GetRelativeAudioPath(string filePath) {
        try {
            return Path.GetRelativePath(FilePaths.AudioDirectory, filePath);
        }
        catch {
            return filePath;
        }
    }

    private static long GetFileSize(string filePath) {
        try {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }
        catch {
            return 0;
        }
    }

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
