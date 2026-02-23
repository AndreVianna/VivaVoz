namespace VivaVoz.Models;

public enum RecordingStatus {
    Recording,
    PendingTranscription,
    Transcribing,
    Complete,
    Failed
}

public partial class Recording : ObservableObject {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AudioFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Transcript { get; set; }

    [ObservableProperty]
    public partial RecordingStatus Status { get; set; }

    public string Language { get; set; } = "auto";
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string WhisperModel { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
