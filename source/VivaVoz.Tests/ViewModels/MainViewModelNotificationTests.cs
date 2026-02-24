using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class MainViewModelNotificationTests : IDisposable {
    private readonly SqliteConnection _connection;

    public MainViewModelNotificationTests() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    // ========== StartRecordingCommand + INotificationService ==========

    [Fact]
    public void StartRecordingCommand_WhenMicrophoneNotFound_ShouldShowWarningNotification() {
        using var context = CreateContext(_connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.When(r => r.StartRecording()).Do(_ => throw new MicrophoneNotFoundException("No microphone found"));
        var notificationService = Substitute.For<INotificationService>();
        notificationService.ShowWarningAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var vm = CreateViewModel(context, notificationService: notificationService, recorder: recorder);

        vm.StartRecordingCommand.Execute(null);

        notificationService.Received(1).ShowWarningAsync(Arg.Is<string>(s => s.Contains("No microphone found")));
    }

    [Fact]
    public void StartRecordingCommand_WhenRecordingSucceeds_ShouldNotShowNotification() {
        using var context = CreateContext(_connection);
        var recorder = Substitute.For<IAudioRecorder>();
        var notificationService = Substitute.For<INotificationService>();

        var vm = CreateViewModel(context, notificationService: notificationService, recorder: recorder);

        vm.StartRecordingCommand.Execute(null);

        notificationService.DidNotReceive().ShowWarningAsync(Arg.Any<string>());
    }

    // ========== ExportTextCommand failure + clipboard fallback ==========

    [Fact]
    public async Task ExportTextCommand_WhenExportFails_ShouldShowRecoverableErrorNotification() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportTextAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.txt"));
        var notificationService = Substitute.For<INotificationService>();
        notificationService.ShowRecoverableErrorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false));

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService, notificationService: notificationService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await notificationService.Received(1).ShowRecoverableErrorAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task ExportTextCommand_WhenExportFailsAndUserChoosesClipboard_ShouldCopyToClipboard() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportTextAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new IOException("Disk full"));
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.txt"));
        var notificationService = Substitute.For<INotificationService>();
        notificationService.ShowRecoverableErrorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(true)); // user clicks "Copy to Clipboard"
        var clipboardService = Substitute.For<IClipboardService>();
        clipboardService.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService,
            notificationService: notificationService, clipboardService: clipboardService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello world");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await clipboardService.Received(1).SetTextAsync("Hello world");
    }

    [Fact]
    public async Task ExportTextCommand_WhenExportFailsAndUserChoosesCancel_ShouldNotCopyToClipboard() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportTextAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new IOException("Disk full"));
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.txt"));
        var notificationService = Substitute.For<INotificationService>();
        notificationService.ShowRecoverableErrorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(false)); // user clicks "Cancel"
        var clipboardService = Substitute.For<IClipboardService>();

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService,
            notificationService: notificationService, clipboardService: clipboardService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello world");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await clipboardService.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task ExportTextCommand_WhenExportSucceeds_ShouldNotShowNotification() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.txt"));
        var notificationService = Substitute.For<INotificationService>();

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService, notificationService: notificationService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await notificationService.DidNotReceive().ShowRecoverableErrorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    // ========== ExportAudioCommand failure ==========

    [Fact]
    public async Task ExportAudioCommand_WhenExportFails_ShouldShowWarningNotification() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportAudioAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new IOException("Disk full"));
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.wav"));
        var notificationService = Substitute.For<INotificationService>();
        notificationService.ShowWarningAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService, notificationService: notificationService);
        vm.SelectedRecording = CreateCompleteRecording();

        await vm.ExportAudioCommand.ExecuteAsync(null);

        await notificationService.Received(1).ShowWarningAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task ExportAudioCommand_WhenExportSucceeds_ShouldNotShowNotification() {
        using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportAudioAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>("/tmp/out.wav"));
        var notificationService = Substitute.For<INotificationService>();

        var vm = CreateViewModel(context, exportService: exportService, dialogService: dialogService, notificationService: notificationService);
        vm.SelectedRecording = CreateCompleteRecording();

        await vm.ExportAudioCommand.ExecuteAsync(null);

        await notificationService.DidNotReceive().ShowWarningAsync(Arg.Any<string>());
    }

    // ========== Helper methods ==========

    private MainViewModel CreateViewModel(
        AppDbContext context,
        IDialogService? dialogService = null,
        IExportService? exportService = null,
        ICrashRecoveryService? crashRecoveryService = null,
        INotificationService? notificationService = null,
        IClipboardService? clipboardService = null,
        IAudioRecorder? recorder = null) {
        var player = Substitute.For<IAudioPlayer>();
        return new MainViewModel(
            recorder ?? Substitute.For<IAudioRecorder>(),
            player,
            context,
            Substitute.For<ITranscriptionManager>(),
            clipboardService ?? Substitute.For<IClipboardService>(),
            dialogService: dialogService,
            exportService: exportService,
            crashRecoveryService: crashRecoveryService,
            notificationService: notificationService);
    }

    private static Recording CreateCompleteRecording(string transcript = "Test transcript") => new() {
        Id = Guid.NewGuid(),
        Title = "Test Recording",
        AudioFileName = "2024-01/test.wav",
        Transcript = transcript,
        Status = RecordingStatus.Complete,
        Language = "en",
        Duration = TimeSpan.FromSeconds(10),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        WhisperModel = "tiny",
        FileSize = 320044
    };

    private static AppDbContext CreateContext(SqliteConnection connection) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
