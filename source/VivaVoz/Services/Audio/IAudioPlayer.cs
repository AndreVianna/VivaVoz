namespace VivaVoz.Services.Audio;

public interface IAudioPlayer {
    event EventHandler? PlaybackStopped;

    bool IsPlaying { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }

    void Play(string path);
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
}
