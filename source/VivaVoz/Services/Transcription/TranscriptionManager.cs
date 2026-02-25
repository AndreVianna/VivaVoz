namespace VivaVoz.Services.Transcription;

public sealed class TranscriptionManager : ITranscriptionManager, IDisposable {
    private static readonly string[] _fallbackOrder = ["large-v3", "medium", "small", "base", "tiny"];

    private readonly ITranscriptionEngine _engine;
    private readonly Func<AppDbContext> _contextFactory;
    private readonly IModelManager? _modelManager;
    private readonly ISettingsService? _settingsService;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public event EventHandler<TranscriptionCompletedEventArgs>? TranscriptionCompleted;

    public TranscriptionManager(
        ITranscriptionEngine engine,
        Func<AppDbContext> contextFactory,
        IModelManager? modelManager = null,
        ISettingsService? settingsService = null) {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(contextFactory);
        _engine = engine;
        _contextFactory = contextFactory;
        _modelManager = modelManager;
        _settingsService = settingsService;
    }

    public void EnqueueTranscription(Guid recordingId, string audioFilePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioFilePath);
        _ = Task.Run(async () => await ProcessTranscriptionAsync(recordingId, audioFilePath, _cts.Token));
    }

    /// <summary>
    /// Selects the best available model starting from the preferred one and falling back to
    /// progressively smaller installed models. Returns <paramref name="preferredModelId"/> unchanged
    /// when no <see cref="IModelManager"/> is injected.
    /// </summary>
    internal string SelectModelWithFallback(string preferredModelId) {
        if (_modelManager is null)
            return preferredModelId;

        var preferredIdx = Array.IndexOf(_fallbackOrder, preferredModelId);
        if (preferredIdx < 0)
            return preferredModelId;

        for (var i = preferredIdx; i < _fallbackOrder.Length; i++) {
            if (_modelManager.IsModelDownloaded(_fallbackOrder[i])) {
                if (i > preferredIdx) {
                    Log.Warning(
                        "[TranscriptionManager] Preferred model '{Preferred}' not available. Falling back to '{Fallback}'.",
                        preferredModelId, _fallbackOrder[i]);
                }

                return _fallbackOrder[i];
            }
        }

        // Nothing installed: use tiny, which auto-downloads on first use.
        Log.Warning(
            "[TranscriptionManager] No installed model found for preferred '{Preferred}'. Defaulting to 'tiny'.",
            preferredModelId);
        return "tiny";
    }

    private async Task ProcessTranscriptionAsync(
        Guid recordingId, string audioFilePath, CancellationToken cancellationToken) {
        Log.Information("[TranscriptionManager] Starting transcription for recording {RecordingId}.", recordingId);

        await SetTranscribingStatusAsync(recordingId, cancellationToken).ConfigureAwait(false);

        try {
            var preferredModelId = _settingsService?.Current?.WhisperModelSize ?? "tiny";
            var modelId = SelectModelWithFallback(preferredModelId);
            var language = _settingsService?.Current?.Language;
            var options = new TranscriptionOptions(Language: language, ModelId: modelId);
            var result = await _engine.TranscribeAsync(audioFilePath, options, cancellationToken)
                .ConfigureAwait(false);

            await UpdateRecordingOnSuccessAsync(recordingId, result, cancellationToken)
                .ConfigureAwait(false);

            Log.Information(
                "[TranscriptionManager] Transcription completed for recording {RecordingId}. Text length: {Length}.",
                recordingId, result.Text.Length);

            TranscriptionCompleted?.Invoke(this,
                TranscriptionCompletedEventArgs.Succeeded(
                    recordingId, result.Text, result.DetectedLanguage, result.ModelUsed));
        }
        catch (OperationCanceledException) {
            Log.Information(
                "[TranscriptionManager] Transcription cancelled for recording {RecordingId}.", recordingId);
        }
        catch (Exception ex) {
            Log.Error(ex,
                "[TranscriptionManager] Transcription failed for recording {RecordingId}.", recordingId);

            await UpdateRecordingOnFailureAsync(recordingId).ConfigureAwait(false);

            TranscriptionCompleted?.Invoke(this,
                TranscriptionCompletedEventArgs.Failed(recordingId, ex.Message));
        }
    }

    private async Task SetTranscribingStatusAsync(Guid recordingId, CancellationToken cancellationToken) {
        try {
            await using var context = _contextFactory();
            var recording = await context.Recordings
                .FindAsync([recordingId], cancellationToken)
                .ConfigureAwait(false);

            if (recording is null)
                return;

            recording.Status = RecordingStatus.Transcribing;
            recording.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) {
            Log.Warning(ex,
                "[TranscriptionManager] Failed to set Transcribing status for recording {RecordingId}.", recordingId);
        }
    }

    private async Task UpdateRecordingOnSuccessAsync(
        Guid recordingId, TranscriptionResult result, CancellationToken cancellationToken) {
        await using var context = _contextFactory();
        var recording = await context.Recordings
            .FindAsync([recordingId], cancellationToken)
            .ConfigureAwait(false);

        if (recording is null) {
            Log.Warning(
                "[TranscriptionManager] Recording {RecordingId} not found in database.", recordingId);
            return;
        }

        recording.Transcript = result.Text;
        recording.Status = RecordingStatus.Complete;
        recording.Language = result.DetectedLanguage;
        recording.LanguageCode = result.DetectedLanguage;
        recording.WhisperModel = result.ModelUsed;
        recording.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task UpdateRecordingOnFailureAsync(Guid recordingId) {
        try {
            await using var context = _contextFactory();
            var recording = await context.Recordings
                .FindAsync(recordingId)
                .ConfigureAwait(false);

            if (recording is null) {
                Log.Warning(
                    "[TranscriptionManager] Recording {RecordingId} not found for failure update.",
                    recordingId);
                return;
            }

            recording.Status = RecordingStatus.Failed;
            recording.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex) {
            Log.Error(ex,
                "[TranscriptionManager] Failed to update recording {RecordingId} status to Failed.",
                recordingId);
        }
    }

    public void Dispose() {
        if (_disposed)
            return;
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}
