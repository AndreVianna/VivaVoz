namespace VivaVoz.ViewModels;

public partial class AudioPlayerViewModel : ObservableObject {
    private readonly IAudioPlayer _audioPlayer;
    private readonly DispatcherTimer _timer;
    private string? _currentPath;
    private bool _suppressProgressUpdate;

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial TimeSpan CurrentPosition { get; set; }

    [ObservableProperty]
    public partial TimeSpan TotalDuration { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial bool HasAudio { get; set; }

    public AudioPlayerViewModel(IAudioPlayer audioPlayer) {
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _audioPlayer.PlaybackStopped += OnPlaybackStopped;

        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _timer.Tick += OnTimerTick;
    }

    public string PlayPauseLabel => IsPlaying ? "Pause" : "Play";

    public void LoadRecording(Recording? recording) {
        StopPlayback();

        if (recording is null) {
            _currentPath = null;
            HasAudio = false;
            TotalDuration = TimeSpan.Zero;
            return;
        }

        var resolvedPath = ResolvePath(recording.AudioFileName);
        _currentPath = resolvedPath;
        HasAudio = File.Exists(resolvedPath);
        TotalDuration = recording.Duration;
        CurrentPosition = TimeSpan.Zero;
        Progress = 0;
    }

    [RelayCommand]
    private void TogglePlayPause() {
        if (!HasAudio || string.IsNullOrWhiteSpace(_currentPath)) {
            return;
        }

        if (_audioPlayer.IsPlaying) {
            _audioPlayer.Pause();
            _timer.Stop();
            IsPlaying = false;
            return;
        }

        _audioPlayer.Play(_currentPath);
        IsPlaying = _audioPlayer.IsPlaying;

        var duration = _audioPlayer.TotalDuration;
        if (duration > TimeSpan.Zero) {
            TotalDuration = duration;
        }

        _timer.Start();
    }

    [RelayCommand]
    private void Stop() => StopPlayback();

    partial void OnIsPlayingChanged(bool value) {
        OnPropertyChanged(nameof(PlayPauseLabel));
    }

    partial void OnProgressChanged(double value) {
        if (_suppressProgressUpdate || !HasAudio || TotalDuration <= TimeSpan.Zero) {
            return;
        }

        var targetSeconds = TotalDuration.TotalSeconds * value;
        var targetPosition = TimeSpan.FromSeconds(targetSeconds);
        _audioPlayer.Seek(targetPosition);
        CurrentPosition = _audioPlayer.CurrentPosition;
    }

    private void OnTimerTick(object? sender, EventArgs e) => UpdateFromPlayer();

    private void UpdateFromPlayer() {
        var duration = _audioPlayer.TotalDuration;
        if (duration > TimeSpan.Zero) {
            TotalDuration = duration;
        }

        CurrentPosition = _audioPlayer.CurrentPosition;

        var progressValue = duration > TimeSpan.Zero
            ? CurrentPosition.TotalSeconds / duration.TotalSeconds
            : 0;

        _suppressProgressUpdate = true;
        Progress = progressValue;
        _suppressProgressUpdate = false;

        IsPlaying = _audioPlayer.IsPlaying;
    }

    private void StopPlayback() {
        _audioPlayer.Stop();
        _timer.Stop();
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
        Progress = 0;
    }

    private void OnPlaybackStopped(object? sender, EventArgs e) => Dispatcher.UIThread.Post(() => {
        _timer.Stop();
        IsPlaying = false;
        _audioPlayer.Seek(TimeSpan.Zero);
        CurrentPosition = TimeSpan.Zero;
        Progress = 0;
    });

    private static string ResolvePath(string audioFileName) => Path.IsPathRooted(audioFileName)
            ? audioFileName
            : Path.Combine(FilePaths.AudioDirectory, audioFileName);
}
