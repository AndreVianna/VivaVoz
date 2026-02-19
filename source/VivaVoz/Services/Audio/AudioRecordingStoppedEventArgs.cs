namespace VivaVoz.Services.Audio;

public sealed class AudioRecordingStoppedEventArgs(string filePath, TimeSpan duration) : EventArgs {
    public string FilePath { get; } = filePath;

    public TimeSpan Duration { get; } = duration;
}
