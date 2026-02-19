namespace VivaVoz.Services.Audio;

public interface IAudioRecorder {
    event EventHandler<AudioRecordingStoppedEventArgs>? RecordingStopped;

    bool IsRecording { get; }

    void StartRecording();

    void StopRecording();
}
