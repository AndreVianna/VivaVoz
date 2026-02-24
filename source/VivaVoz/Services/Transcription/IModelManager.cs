namespace VivaVoz.Services.Transcription;

public interface IModelManager {
    IReadOnlyList<string> GetAvailableModelIds();
    bool IsModelDownloaded(string modelId);
    Task DownloadModelAsync(string modelId, IProgress<double>? progress, CancellationToken cancellationToken = default);
    void DeleteModel(string modelId);
}
