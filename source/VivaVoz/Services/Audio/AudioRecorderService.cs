namespace VivaVoz.Services.Audio;

[ExcludeFromCodeCoverage]
public sealed class AudioRecorderService : IAudioRecorder {
    private readonly Lock _sync = new();
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private WaveFormat? _waveFormat;
    private string? _currentFilePath;
    private long _bytesWritten;

    public event EventHandler? RecordingStarted;

    public event EventHandler<AudioRecordingStoppedEventArgs>? RecordingStopped;

    public bool IsRecording { get; private set; }

    public void StartRecording() {
        lock (_sync) {
            if (IsRecording) {
                Log.Warning("[AudioRecorderService] StartRecording called while already recording.");
                return;
            }

            if (WaveInEvent.DeviceCount < 1) {
                Log.Warning("[AudioRecorderService] No microphone device detected.");
                throw new MicrophoneNotFoundException("No microphone device detected.");
            }

            var monthFolder = DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var targetDirectory = Path.Combine(FilePaths.AudioDirectory, monthFolder);
            Directory.CreateDirectory(targetDirectory);

            _currentFilePath = Path.Combine(targetDirectory, $"{Guid.NewGuid()}.wav");

            _waveIn = new WaveInEvent {
                WaveFormat = new WaveFormat(16000, 16, 1)
            };
            _waveFormat = _waveIn.WaveFormat;
            _bytesWritten = 0;

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _writer = new WaveFileWriter(_currentFilePath, _waveFormat);

            _waveIn.StartRecording();
            IsRecording = true;

            Log.Information("[AudioRecorderService] Recording started: {FilePath}", _currentFilePath);
            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    public IReadOnlyList<string> GetAvailableDevices() {
        var devices = new List<string>();
        for (var i = 0; i < WaveInEvent.DeviceCount; i++) {
            devices.Add(WaveInEvent.GetCapabilities(i).ProductName);
        }

        return devices.AsReadOnly();
    }

    public void StopRecording() {
        WaveInEvent? waveIn;

        lock (_sync) {
            if (!IsRecording) {
                Log.Warning("[AudioRecorderService] StopRecording called while not recording.");
                return;
            }

            waveIn = _waveIn;
        }

        try {
            waveIn?.StopRecording();
        }
        catch (Exception ex) {
            Log.Error(ex, "[AudioRecorderService] Failed to stop recording.");
            CleanupRecording(ex);
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e) {
        lock (_sync) {
            if (_writer is null) {
                return;
            }

            try {
                _writer.Write(e.Buffer, 0, e.BytesRecorded);
                _bytesWritten += e.BytesRecorded;
            }
            catch (Exception ex) {
                Log.Error(ex, "[AudioRecorderService] Failed to write audio buffer.");
            }
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e) {
        lock (_sync) {
            if (!IsRecording && _waveIn is null && _writer is null) {
                return;
            }
        }

        if (e.Exception is not null) {
            Log.Error(e.Exception, "[AudioRecorderService] Recording stopped due to an error.");
        }

        CleanupRecording(e.Exception);
    }

    private void CleanupRecording(Exception? exception) {
        string? filePath;
        TimeSpan duration;

        lock (_sync) {
            filePath = _currentFilePath;

            _writer?.Dispose();
            _writer = null;

            if (_waveIn is not null) {
                _waveIn.DataAvailable -= OnDataAvailable;
                _waveIn.RecordingStopped -= OnRecordingStopped;
                _waveIn.Dispose();
                _waveIn = null;
            }

            duration = CalculateDuration(_bytesWritten, _waveFormat);
            _waveFormat = null;
            _bytesWritten = 0;
            _currentFilePath = null;
            IsRecording = false;
        }

        if (!string.IsNullOrWhiteSpace(filePath)) {
            Log.Information("[AudioRecorderService] Recording stopped: {FilePath} ({Duration})", filePath, duration);
            RecordingStopped?.Invoke(this, new AudioRecordingStoppedEventArgs(filePath, duration));
        }
        else {
            Log.Warning("[AudioRecorderService] Recording stopped without a valid file path.");
        }

        if (exception is not null) {
            Log.Debug(exception, "[AudioRecorderService] Recording cleanup completed after error.");
        }
    }

    private static TimeSpan CalculateDuration(long bytesRecorded, WaveFormat? waveFormat) {
        if (waveFormat is null || waveFormat.AverageBytesPerSecond <= 0) {
            return TimeSpan.Zero;
        }

        var seconds = bytesRecorded / (double)waveFormat.AverageBytesPerSecond;
        return TimeSpan.FromSeconds(seconds);
    }
}
