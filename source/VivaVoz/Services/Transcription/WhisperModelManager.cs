using Whisper.net.Ggml;

namespace VivaVoz.Services.Transcription;

public sealed class WhisperModelManager(string? modelsDirectory = null) {
    private static readonly Dictionary<string, GgmlType> _modelMap = new(StringComparer.OrdinalIgnoreCase) {
        ["tiny"] = GgmlType.Tiny,
        ["base"] = GgmlType.Base,
        ["small"] = GgmlType.Small,
        ["medium"] = GgmlType.Medium,
        ["large-v3"] = GgmlType.LargeV3,
    };

    private const QuantizationType _defaultQuantization = QuantizationType.NoQuantization;

    public string ModelsDirectory { get; } = modelsDirectory ?? FilePaths.ModelsDirectory;

    public string GetModelPath(string modelId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        return Path.Combine(ModelsDirectory, $"ggml-{modelId}.bin");
    }

    public bool IsModelDownloaded(string modelId) {
        var path = GetModelPath(modelId);
        return File.Exists(path);
    }

    public async Task<string> EnsureModelAsync(string modelId, CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        var modelPath = GetModelPath(modelId);
        if (File.Exists(modelPath)) {
            Log.Debug("[WhisperModelManager] Model '{ModelId}' already exists at {Path}.", modelId, modelPath);
            return modelPath;
        }

        if (!_modelMap.TryGetValue(modelId, out var ggmlType)) {
            throw new ArgumentException($"Unknown model id: '{modelId}'. Supported models: {string.Join(", ", _modelMap.Keys)}.", nameof(modelId));
        }

        Log.Information("[WhisperModelManager] Downloading model '{ModelId}'...", modelId);

        Directory.CreateDirectory(ModelsDirectory);

        var tempPath = modelPath + ".tmp";
        try {
            await using var modelStream = await WhisperGgmlDownloader.Default
                .GetGgmlModelAsync(ggmlType, _defaultQuantization, cancellationToken)
                .ConfigureAwait(false);

            await using var fileStream = File.Create(tempPath);
            await modelStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
        catch {
            TryDeleteFile(tempPath);
            throw;
        }

        File.Move(tempPath, modelPath, overwrite: true);
        Log.Information("[WhisperModelManager] Model '{ModelId}' downloaded to {Path}.", modelId, modelPath);

        return modelPath;
    }

    public static IReadOnlyList<string> GetAvailableModelIds() => _modelMap.Keys.ToList().AsReadOnly();

    private static void TryDeleteFile(string path) {
        try {
            File.Delete(path);
        }
        catch { /* best effort cleanup */ }
    }
}
