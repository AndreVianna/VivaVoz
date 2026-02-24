using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NSubstitute;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class RecordingDetailViewModelTests {
    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullRecordingService_ShouldThrowArgumentNullException() {
        var act = () => new RecordingDetailViewModel(null!, Substitute.For<IDialogService>());

        act.Should().Throw<ArgumentNullException>().WithParameterName("recordingService");
    }

    [Fact]
    public void Constructor_WithNullDialogService_ShouldThrowArgumentNullException() {
        var act = () => new RecordingDetailViewModel(Substitute.For<IRecordingService>(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dialogService");
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow() {
        var act = () => CreateViewModel();

        act.Should().NotThrow();
    }

    // ========== Initial state tests ==========

    [Fact]
    public void IsEditing_WhenNewInstance_ShouldBeFalse() {
        var viewModel = CreateViewModel();

        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void IsNotEditing_WhenNewInstance_ShouldBeTrue() {
        var viewModel = CreateViewModel();

        viewModel.IsNotEditing.Should().BeTrue();
    }

    [Fact]
    public void EditText_WhenNewInstance_ShouldBeEmpty() {
        var viewModel = CreateViewModel();

        viewModel.EditText.Should().BeEmpty();
    }

    [Fact]
    public void CanEdit_WhenNoRecordingLoaded_ShouldBeFalse() {
        var viewModel = CreateViewModel();

        viewModel.CanEdit.Should().BeFalse();
    }

    [Fact]
    public void CanDelete_WhenNoRecordingLoaded_ShouldBeFalse() {
        var viewModel = CreateViewModel();

        viewModel.CanDelete.Should().BeFalse();
    }

    // ========== LoadRecording tests ==========

    [Fact]
    public void LoadRecording_WithNullRecording_ShouldResetState() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording("some text"));

        viewModel.LoadRecording(null);

        viewModel.EditText.Should().BeEmpty();
        viewModel.IsEditing.Should().BeFalse();
        viewModel.CanEdit.Should().BeFalse();
        viewModel.CanDelete.Should().BeFalse();
    }

    [Fact]
    public void LoadRecording_WithRecording_ShouldSetEditTextToTranscript() {
        var viewModel = CreateViewModel();
        var recording = CreateRecording(transcript: "Hello World");

        viewModel.LoadRecording(recording);

        viewModel.EditText.Should().Be("Hello World");
    }

    [Fact]
    public void LoadRecording_WithNullTranscript_ShouldSetEditTextToEmpty() {
        var viewModel = CreateViewModel();
        var recording = CreateRecording(transcript: null);

        viewModel.LoadRecording(recording);

        viewModel.EditText.Should().BeEmpty();
    }

    [Fact]
    public void LoadRecording_WithRecording_ShouldEnableCanEditAndCanDelete() {
        var viewModel = CreateViewModel();

        viewModel.LoadRecording(CreateRecording());

        viewModel.CanEdit.Should().BeTrue();
        viewModel.CanDelete.Should().BeTrue();
    }

    [Fact]
    public void LoadRecording_WhenEditing_ShouldExitEditMode() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());
        viewModel.EditCommand.Execute(null);

        viewModel.LoadRecording(CreateRecording());

        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void LoadRecording_ShouldFirePropertyChangedForCanEditAndCanDelete() {
        var viewModel = CreateViewModel();
        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.LoadRecording(CreateRecording());

        changed.Should().Contain(nameof(RecordingDetailViewModel.CanEdit));
        changed.Should().Contain(nameof(RecordingDetailViewModel.CanDelete));
    }

    // ========== Edit command tests ==========

    [Fact]
    public void EditCommand_WhenNoRecordingLoaded_ShouldNotBeExecutable() {
        var viewModel = CreateViewModel();

        viewModel.EditCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void EditCommand_WhenRecordingLoaded_ShouldBeExecutable() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());

        viewModel.EditCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void EditCommand_WhenExecuted_ShouldSetIsEditingTrue() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());

        viewModel.EditCommand.Execute(null);

        viewModel.IsEditing.Should().BeTrue();
    }

    [Fact]
    public void EditCommand_WhenExecuted_ShouldSetIsNotEditingFalse() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());

        viewModel.EditCommand.Execute(null);

        viewModel.IsNotEditing.Should().BeFalse();
    }

    [Fact]
    public void EditCommand_WhenExecuted_ShouldPopulateEditText() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording(transcript: "Hello World"));

        viewModel.EditCommand.Execute(null);

        viewModel.EditText.Should().Be("Hello World");
    }

    [Fact]
    public void EditCommand_WhenAlreadyEditing_ShouldNotBeExecutable() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());
        viewModel.EditCommand.Execute(null);

        viewModel.EditCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void EditCommand_WhenExecuted_ShouldFirePropertyChangedForIsEditing() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());
        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.EditCommand.Execute(null);

        changed.Should().Contain(nameof(RecordingDetailViewModel.IsEditing));
        changed.Should().Contain(nameof(RecordingDetailViewModel.IsNotEditing));
    }

    // ========== Cancel command tests ==========

    [Fact]
    public void CancelCommand_WhenEditing_ShouldExitEditMode() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording(transcript: "Hello World"));
        viewModel.EditCommand.Execute(null);
        viewModel.EditText = "changed text";

        viewModel.CancelCommand.Execute(null);

        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void CancelCommand_WhenEditing_ShouldRevertEditTextToOriginal() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording(transcript: "Hello World"));
        viewModel.EditCommand.Execute(null);
        viewModel.EditText = "changed text";

        viewModel.CancelCommand.Execute(null);

        viewModel.EditText.Should().Be("Hello World");
    }

    [Fact]
    public void CancelCommand_ShouldFirePropertyChangedForIsNotEditing() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());
        viewModel.EditCommand.Execute(null);
        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.CancelCommand.Execute(null);

        changed.Should().Contain(nameof(RecordingDetailViewModel.IsNotEditing));
    }

    // ========== Save command tests ==========

    [Fact]
    public async Task SaveCommand_WhenEditing_ShouldCallUpdateAsync() {
        var recordingService = Substitute.For<IRecordingService>();
        recordingService.UpdateAsync(Arg.Any<Recording>()).Returns(Task.CompletedTask);
        var viewModel = CreateViewModel(recordingService: recordingService);
        var recording = CreateRecording(transcript: "Hello worl");
        viewModel.LoadRecording(recording);
        viewModel.EditCommand.Execute(null);
        viewModel.EditText = "Hello World";

        await viewModel.SaveCommand.ExecuteAsync(null);

        await recordingService.Received(1).UpdateAsync(recording);
    }

    [Fact]
    public async Task SaveCommand_WhenSaved_ShouldExitEditMode() {
        var recordingService = Substitute.For<IRecordingService>();
        recordingService.UpdateAsync(Arg.Any<Recording>()).Returns(Task.CompletedTask);
        var viewModel = CreateViewModel(recordingService: recordingService);
        viewModel.LoadRecording(CreateRecording());
        viewModel.EditCommand.Execute(null);

        await viewModel.SaveCommand.ExecuteAsync(null);

        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_WhenSaved_ShouldUpdateRecordingTranscript() {
        var recordingService = Substitute.For<IRecordingService>();
        recordingService.UpdateAsync(Arg.Any<Recording>()).Returns(Task.CompletedTask);
        var viewModel = CreateViewModel(recordingService: recordingService);
        var recording = CreateRecording(transcript: "Hello worl");
        viewModel.LoadRecording(recording);
        viewModel.EditCommand.Execute(null);
        viewModel.EditText = "Hello World";

        await viewModel.SaveCommand.ExecuteAsync(null);

        recording.Transcript.Should().Be("Hello World");
    }

    [Fact]
    public async Task SaveCommand_WhenNoRecordingLoaded_ShouldNotCallUpdateAsync() {
        var recordingService = Substitute.For<IRecordingService>();
        var viewModel = CreateViewModel(recordingService: recordingService);

        await viewModel.SaveCommand.ExecuteAsync(null);

        await recordingService.DidNotReceive().UpdateAsync(Arg.Any<Recording>());
    }

    // ========== Delete command tests ==========

    [Fact]
    public void DeleteCommand_WhenNoRecordingLoaded_ShouldNotBeExecutable() {
        var viewModel = CreateViewModel();

        viewModel.DeleteCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void DeleteCommand_WhenRecordingLoaded_ShouldBeExecutable() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());

        viewModel.DeleteCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCommand_WhenConfirmed_ShouldCallDeleteAsync() {
        var recordingService = Substitute.For<IRecordingService>();
        recordingService.DeleteAsync(Arg.Any<Guid>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));
        var viewModel = CreateViewModel(recordingService: recordingService, dialogService: dialogService);
        var recording = CreateRecording();
        viewModel.LoadRecording(recording);

        await viewModel.DeleteCommand.ExecuteAsync(null);

        await recordingService.Received(1).DeleteAsync(recording.Id);
    }

    [Fact]
    public async Task DeleteCommand_WhenCancelled_ShouldNotCallDeleteAsync() {
        var recordingService = Substitute.For<IRecordingService>();
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(false));
        var viewModel = CreateViewModel(recordingService: recordingService, dialogService: dialogService);
        viewModel.LoadRecording(CreateRecording());

        await viewModel.DeleteCommand.ExecuteAsync(null);

        await recordingService.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task DeleteCommand_WhenConfirmed_ShouldFireRecordingDeletedEvent() {
        var recordingService = Substitute.For<IRecordingService>();
        recordingService.DeleteAsync(Arg.Any<Guid>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));
        var viewModel = CreateViewModel(recordingService: recordingService, dialogService: dialogService);
        var recording = CreateRecording();
        viewModel.LoadRecording(recording);

        Guid? deletedId = null;
        viewModel.RecordingDeleted += (_, id) => deletedId = id;

        await viewModel.DeleteCommand.ExecuteAsync(null);

        deletedId.Should().Be(recording.Id);
    }

    [Fact]
    public async Task DeleteCommand_WhenCancelled_ShouldNotFireRecordingDeletedEvent() {
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(false));
        var viewModel = CreateViewModel(dialogService: dialogService);
        viewModel.LoadRecording(CreateRecording());

        var fired = false;
        viewModel.RecordingDeleted += (_, _) => fired = true;

        await viewModel.DeleteCommand.ExecuteAsync(null);

        fired.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCommand_WhenConfirmed_ShouldShowCorrectConfirmationMessage() {
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(false));
        var viewModel = CreateViewModel(dialogService: dialogService);
        viewModel.LoadRecording(CreateRecording());

        await viewModel.DeleteCommand.ExecuteAsync(null);

        await dialogService.Received(1).ShowConfirmAsync(
            "Delete Recording",
            "Are you sure you want to delete this recording? This cannot be undone.");
    }

    [Fact]
    public void DeleteCommand_WhenEditing_ShouldNotBeExecutable() {
        var viewModel = CreateViewModel();
        viewModel.LoadRecording(CreateRecording());
        viewModel.EditCommand.Execute(null);

        viewModel.DeleteCommand.CanExecute(null).Should().BeFalse();
    }

    // ========== MainViewModel integration tests ==========

    [Fact]
    public void MainViewModel_OnRecordingDeleted_ShouldRemoveRecordingFromCollection() {
        using var connection = CreateSqliteConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var recordingService = Substitute.For<IRecordingService>();
        var dialogService = Substitute.For<IDialogService>();
        var viewModel = new MainViewModel(
            recorder, player, context,
            Substitute.For<ITranscriptionManager>(),
            Substitute.For<IClipboardService>(),
            recordingService: recordingService,
            dialogService: dialogService);

        var recording = CreateRecording();
        viewModel.Recordings.Add(recording);
        viewModel.SelectedRecording = recording;

        viewModel.OnRecordingDeleted(null, recording.Id);

        viewModel.Recordings.Should().NotContain(recording);
        viewModel.SelectedRecording.Should().BeNull();
    }

    [Fact]
    public void MainViewModel_OnRecordingDeleted_ShouldUpdateFilteredRecordings() {
        using var connection = CreateSqliteConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(
            recorder, player, context,
            Substitute.For<ITranscriptionManager>(),
            Substitute.For<IClipboardService>());

        var recording = CreateRecording();
        viewModel.Recordings.Add(recording);
        viewModel.ApplyFilter();

        viewModel.OnRecordingDeleted(null, recording.Id);

        viewModel.FilteredRecordings.Should().NotContain(recording);
    }

    [Fact]
    public void MainViewModel_OnRecordingDeleted_WithUnknownId_ShouldNotThrow() {
        using var connection = CreateSqliteConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new MainViewModel(
            recorder, player, context,
            Substitute.For<ITranscriptionManager>(),
            Substitute.For<IClipboardService>());

        var act = () => viewModel.OnRecordingDeleted(null, Guid.NewGuid());

        act.Should().NotThrow();
    }

    // ========== Helper methods ==========

    private static RecordingDetailViewModel CreateViewModel(
        IRecordingService? recordingService = null,
        IDialogService? dialogService = null) => new(
            recordingService ?? Substitute.For<IRecordingService>(),
            dialogService ?? Substitute.For<IDialogService>());

    private static Recording CreateRecording(string? transcript = "Test transcript") => new() {
        Id = Guid.NewGuid(),
        Title = "Test",
        AudioFileName = "file.wav",
        Transcript = transcript,
        Status = RecordingStatus.Complete,
        Language = "en",
        Duration = TimeSpan.FromSeconds(10),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        WhisperModel = "tiny",
        FileSize = 1024
    };

    private static SqliteConnection CreateSqliteConnection() {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
