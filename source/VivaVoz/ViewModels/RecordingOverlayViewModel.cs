namespace VivaVoz.ViewModels;

public partial class RecordingOverlayViewModel : ObservableObject, IDisposable {
    private readonly IAudioRecorder _recorder;
    private DispatcherTimer? _timer;
    private DateTime _recordingStartTime;

    [ObservableProperty]
    public partial string DurationText { get; set; } = "00:00";

    [ObservableProperty]
    public partial bool IsRecording { get; private set; }

    public RecordingOverlayViewModel(IAudioRecorder recorder) {
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        _recorder.RecordingStarted += OnRecordingStarted;
        _recorder.RecordingStopped += OnRecordingStopped;
    }

    [RelayCommand]
    private void StopRecording() => _recorder.StopRecording();

    [ExcludeFromCodeCoverage]
    private void OnRecordingStarted(object? sender, EventArgs e)
        => Dispatcher.UIThread.Post(StartTimer);

    [ExcludeFromCodeCoverage]
    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e)
        => Dispatcher.UIThread.Post(StopTimer);

    [ExcludeFromCodeCoverage]
    private void StartTimer() {
        _recordingStartTime = DateTime.Now;
        IsRecording = true;
        DurationText = "00:00";
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    [ExcludeFromCodeCoverage]
    private void StopTimer() {
        _timer?.Stop();
        _timer = null;
        IsRecording = false;
        DurationText = "00:00";
    }

    [ExcludeFromCodeCoverage]
    private void OnTimerTick(object? sender, EventArgs e) {
        var elapsed = DateTime.Now - _recordingStartTime;
        DurationText = FormatDuration(elapsed);
    }

    internal static string FormatDuration(TimeSpan duration) {
        var minutes = (int)duration.TotalMinutes;
        var seconds = duration.Seconds;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public void Dispose() {
        _recorder.RecordingStarted -= OnRecordingStarted;
        _recorder.RecordingStopped -= OnRecordingStopped;
        _timer?.Stop();
        _timer = null;
        GC.SuppressFinalize(this);
    }
}
