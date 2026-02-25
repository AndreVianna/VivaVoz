namespace VivaVoz.Services.Transcription;

public interface ITranscriptionManager {
    event EventHandler<TranscriptionCompletedEventArgs>? TranscriptionCompleted;

    void EnqueueTranscription(Guid recordingId, string audioFilePath, string? modelOverride = null);
}
