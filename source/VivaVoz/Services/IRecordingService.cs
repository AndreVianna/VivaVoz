namespace VivaVoz.Services;

/// <summary>
/// Service for persisting recording changes: updating transcripts and deleting recordings.
/// </summary>
public interface IRecordingService {
    /// <summary>
    /// Persists changes to an existing recording (e.g., edited transcript) to the database.
    /// </summary>
    Task UpdateAsync(Recording recording);

    /// <summary>
    /// Deletes a recording from the database and removes its audio file from disk.
    /// </summary>
    Task DeleteAsync(Guid id);
}
