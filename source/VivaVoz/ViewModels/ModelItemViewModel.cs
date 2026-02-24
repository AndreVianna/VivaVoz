namespace VivaVoz.ViewModels;

public partial class ModelItemViewModel(string modelId, IModelManager modelManager, Action<string>? onSelect = null) : ObservableObject {
    private static readonly Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase) {
        ["tiny"] = "Tiny",
        ["base"] = "Base",
        ["small"] = "Small",
        ["medium"] = "Medium",
        ["large-v3"] = "Large (v3)",
    };

    private static readonly Dictionary<string, string> _expectedSizes = new(StringComparer.OrdinalIgnoreCase) {
        ["tiny"] = "~75 MB",
        ["base"] = "~142 MB",
        ["small"] = "~466 MB",
        ["medium"] = "~1.5 GB",
        ["large-v3"] = "~2.9 GB",
    };

    private readonly IModelManager _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
    private CancellationTokenSource? _downloadCts;

    public string ModelId { get; } = modelId ?? throw new ArgumentNullException(nameof(modelId));
    public string DisplayName { get; } = _displayNames.TryGetValue(modelId, out var name) ? name : modelId;
    public string ExpectedSize { get; } = _expectedSizes.TryGetValue(modelId, out var size) ? size : "Unknown";

    [ObservableProperty]
    public partial bool IsInstalled { get; set; } = modelManager.IsModelDownloaded(modelId);

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    public string StatusText => IsDownloading
        ? $"Downloading {DownloadProgress * 100:F0}%..."
        : IsInstalled ? "Installed" : "Not installed";

    public bool CanDownload => !IsInstalled && !IsDownloading;
    public bool CanCancel => IsDownloading;
    public bool CanDelete => IsInstalled && !IsDownloading;
    public bool CanSelect => IsInstalled && !IsSelected;

    [RelayCommand(CanExecute = nameof(CanDownload))]
    private async Task DownloadAsync() {
        _downloadCts = new CancellationTokenSource();
        IsDownloading = true;
        DownloadProgress = 0;

        try {
            var progress = new Progress<double>(p => {
                DownloadProgress = p;
                OnPropertyChanged(nameof(StatusText));
            });

            await _modelManager.DownloadModelAsync(ModelId, progress, _downloadCts.Token);
            IsInstalled = true;
        }
        catch (OperationCanceledException) {
            // Download was cancelled — expected, nothing to do
        }
        catch (Exception ex) {
            Log.Error(ex, "[ModelItemViewModel] Failed to download model '{ModelId}'.", ModelId);
        }
        finally {
            IsDownloading = false;
            _downloadCts?.Dispose();
            _downloadCts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void CancelDownload() => _downloadCts?.Cancel();

    [RelayCommand(CanExecute = nameof(CanSelect))]
    private void Select() => onSelect?.Invoke(ModelId);

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete() {
        _modelManager.DeleteModel(ModelId);
        IsInstalled = false;
    }

    partial void OnIsInstalledChanged(bool value) {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanDownload));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(CanSelect));
        DownloadCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        SelectCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSelectedChanged(bool value) {
        OnPropertyChanged(nameof(CanSelect));
        SelectCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDownloadingChanged(bool value) {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanDownload));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanDelete));
        DownloadCommand.NotifyCanExecuteChanged();
        CancelDownloadCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnDownloadProgressChanged(double value) {
        OnPropertyChanged(nameof(StatusText));
    }
}
