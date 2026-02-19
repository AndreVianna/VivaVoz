namespace VivaVoz.Models;

public enum RecordingStatus {
    Recording,
    Transcribing,
    Complete,
    Failed
}

public class Recording {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AudioFileName { get; set; } = string.Empty;
    public string? Transcript { get; set; }
    public RecordingStatus Status { get; set; }
    public string Language { get; set; } = "auto";
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string WhisperModel { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
