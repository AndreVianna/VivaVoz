namespace VivaVoz.Services;

/// <summary>
/// EF Core-backed service for recording CRUD operations.
/// Uses a DbContext factory for scoped operations per call.
/// </summary>
/// <param name="contextFactory">Factory that creates AppDbContext instances.</param>
/// <param name="audioDirectory">
/// Base directory for audio files. Defaults to <see cref="FilePaths.AudioDirectory"/>.
/// Override in tests to point at a temp directory.
/// </param>
public class RecordingService(Func<AppDbContext> contextFactory, string? audioDirectory = null) : IRecordingService {
    private readonly Func<AppDbContext> _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    private readonly string _audioDirectory = audioDirectory ?? FilePaths.AudioDirectory;

    /// <inheritdoc />
    public async Task UpdateAsync(Recording recording) {
        ArgumentNullException.ThrowIfNull(recording);

        await using var context = _contextFactory();

        var existing = await context.Recordings.FindAsync(recording.Id).ConfigureAwait(false);
        if (existing is null)
            return;

        context.Entry(existing).CurrentValues.SetValues(recording);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id) {
        string audioFileName;

        await using (var context = _contextFactory()) {
            var recording = await context.Recordings.FindAsync(id).ConfigureAwait(false);
            if (recording is null)
                return;

            audioFileName = recording.AudioFileName;
            context.Recordings.Remove(recording);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        TryDeleteFile(Path.Combine(_audioDirectory, audioFileName));
    }

    private static void TryDeleteFile(string path) {
        try {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex) {
            Log.Warning(ex, "[RecordingService] Could not delete audio file: {Path}", path);
        }
    }
}
