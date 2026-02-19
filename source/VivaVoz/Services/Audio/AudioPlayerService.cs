namespace VivaVoz.Services.Audio;

public sealed class AudioPlayerService : IAudioPlayer {
    private readonly Lock _sync = new();
    private WaveOutEvent? _output;
    private AudioFileReader? _reader;
    private string? _currentPath;

    public event EventHandler? PlaybackStopped;

    public bool IsPlaying {
        get {
            lock (_sync) {
                return _output?.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    public TimeSpan CurrentPosition {
        get {
            lock (_sync) {
                return _reader?.CurrentTime ?? TimeSpan.Zero;
            }
        }
    }

    public TimeSpan TotalDuration {
        get {
            lock (_sync) {
                return _reader?.TotalTime ?? TimeSpan.Zero;
            }
        }
    }

    public void Play(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            Log.Warning("[AudioPlayerService] Play called with empty path.");
            return;
        }

        if (!File.Exists(path)) {
            Log.Warning("[AudioPlayerService] Audio file not found: {Path}", path);
            return;
        }

        lock (_sync) {
            if (_output is null || _reader is null || !Path.Equals(_currentPath, path)) {
                InitializePlayer(path);
            }

            if (_reader is not null && _reader.CurrentTime >= _reader.TotalTime) {
                _reader.CurrentTime = TimeSpan.Zero;
            }

            _output?.Play();
        }
    }

    public void Pause() {
        lock (_sync) {
            if (_output?.PlaybackState == PlaybackState.Playing) {
                _output.Pause();
            }
        }
    }

    public void Stop() {
        lock (_sync) {
            if (_output is null) {
                return;
            }

            try {
                _output.Stop();
            }
            finally {
                _reader?.CurrentTime = TimeSpan.Zero;
            }
        }
    }

    public void Seek(TimeSpan position) {
        lock (_sync) {
            if (_reader is null) {
                return;
            }

            var clamped = position;
            if (clamped < TimeSpan.Zero) {
                clamped = TimeSpan.Zero;
            }

            if (clamped > _reader.TotalTime) {
                clamped = _reader.TotalTime;
            }

            _reader.CurrentTime = clamped;
        }
    }

    private void InitializePlayer(string path) {
        DisposePlayer();

        try {
            _reader = new AudioFileReader(path);
            _output = new WaveOutEvent();
            _output.Init(_reader);
            _output.PlaybackStopped += OnPlaybackStopped;
            _currentPath = path;
            Log.Information("[AudioPlayerService] Loaded audio file: {Path}", path);
        }
        catch (Exception ex) {
            Log.Error(ex, "[AudioPlayerService] Failed to initialize audio playback.");
            DisposePlayer();
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e) {
        if (e.Exception is not null) {
            Log.Error(e.Exception, "[AudioPlayerService] Playback stopped due to an error.");
        }

        lock (_sync) {
            _reader?.CurrentTime = TimeSpan.Zero;
        }

        PlaybackStopped?.Invoke(this, EventArgs.Empty);
    }

    private void DisposePlayer() {
        if (_output is not null) {
            _output.PlaybackStopped -= OnPlaybackStopped;
            _output.Dispose();
            _output = null;
        }

        _reader?.Dispose();
        _reader = null;

        _currentPath = null;
    }
}
