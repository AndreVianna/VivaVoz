namespace VivaVoz.Services.Audio;

public interface IAudioRecorder {
    event EventHandler? RecordingStarted;

    event EventHandler<AudioRecordingStoppedEventArgs>? RecordingStopped;

    bool IsRecording { get; }

    void StartRecording();

    void StopRecording();

    IReadOnlyList<string> GetAvailableDevices();
}
