using System.Net.Http;
using System.Security.Cryptography;

namespace VivaVoz.Services.Transcription;

public sealed class WhisperModelService : IModelManager {
    private const string BaseUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/";
    private const int BufferSize = 81920; // 80 KB chunks

    private readonly WhisperModelManager _modelManager;
    private readonly HttpClient _httpClient;
    private readonly IReadOnlyDictionary<string, string> _expectedHashes;

    public WhisperModelService(
        WhisperModelManager modelManager,
        HttpClient httpClient,
        IReadOnlyDictionary<string, string>? expectedHashes = null) {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _expectedHashes = expectedHashes ?? new Dictionary<string, string>();
    }

    public IReadOnlyList<string> GetAvailableModelIds() => WhisperModelManager.GetAvailableModelIds();

    public bool IsModelDownloaded(string modelId) => _modelManager.IsModelDownloaded(modelId);

    public async Task DownloadModelAsync(
        string modelId,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        if (_modelManager.IsModelDownloaded(modelId)) {
            progress?.Report(1.0);
            return;
        }

        var modelPath = _modelManager.GetModelPath(modelId);
        var tempPath = modelPath + ".tmp";

        Directory.CreateDirectory(_modelManager.ModelsDirectory);

        var url = $"{BaseUrl}ggml-{modelId}.bin";
        try {
            using var response = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;

            await using var contentStream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            await using var fileStream = File.Create(tempPath);

            var buffer = new byte[BufferSize];
            long downloadedBytes = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0) {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                downloadedBytes += bytesRead;

                if (totalBytes > 0) {
                    progress?.Report((double)downloadedBytes / totalBytes);
                }
            }
        }
        catch {
            TryDeleteFile(tempPath);
            throw;
        }

        if (_expectedHashes.TryGetValue(modelId, out var expectedHash)) {
            var actualHash = ComputeSha256(tempPath);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase)) {
                TryDeleteFile(tempPath);
                throw new InvalidOperationException(
                    $"SHA256 hash mismatch for model '{modelId}'. Expected: {expectedHash}, got: {actualHash}.");
            }
        }

        File.Move(tempPath, modelPath, overwrite: true);
        progress?.Report(1.0);

        Log.Information("[WhisperModelService] Model '{ModelId}' downloaded to {Path}.", modelId, modelPath);
    }

    public void DeleteModel(string modelId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        var modelPath = _modelManager.GetModelPath(modelId);
        if (File.Exists(modelPath)) {
            File.Delete(modelPath);
            Log.Information("[WhisperModelService] Model '{ModelId}' deleted from {Path}.", modelId, modelPath);
        }
    }

    private static string ComputeSha256(string filePath) {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void TryDeleteFile(string path) {
        try { File.Delete(path); }
        catch { /* best effort cleanup */ }
    }
}
