namespace VivaVoz.ViewModels;

public partial class RecordingDetailViewModel(IRecordingService recordingService, IDialogService dialogService) : ObservableObject {
    private readonly IRecordingService _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));
    private readonly IDialogService _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    private Recording? _recording;
    private string _originalTranscript = string.Empty;

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string EditText { get; set; } = string.Empty;

    public bool IsNotEditing => !IsEditing;
    public bool CanEdit => _recording is not null && !IsEditing;
    public bool CanDelete => _recording is not null && !IsEditing;

    public event EventHandler<Guid>? RecordingDeleted;

    public void LoadRecording(Recording? recording) {
        _recording = recording;
        _originalTranscript = recording?.Transcript ?? string.Empty;
        IsEditing = false;
        EditText = _originalTranscript;
        NotifyComputedProperties();
    }

    partial void OnIsEditingChanged(bool value) {
        OnPropertyChanged(nameof(IsNotEditing));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanDelete));
        EditCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit() {
        EditText = _recording?.Transcript ?? string.Empty;
        _originalTranscript = EditText;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveAsync() {
        if (_recording is null)
            return;
        _recording.Transcript = EditText;
        _recording.UpdatedAt = DateTime.UtcNow;
        await _recordingService.UpdateAsync(_recording);
        IsEditing = false;
    }

    [RelayCommand]
    private void Cancel() {
        EditText = _originalTranscript;
        IsEditing = false;
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync() {
        if (_recording is null)
            return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            "Delete Recording",
            "Are you sure you want to delete this recording? This cannot be undone.");

        if (!confirmed)
            return;

        var id = _recording.Id;
        await _recordingService.DeleteAsync(id);
        RecordingDeleted?.Invoke(this, id);
    }

    private void NotifyComputedProperties() {
        OnPropertyChanged(nameof(IsNotEditing));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanDelete));
        EditCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }
}
