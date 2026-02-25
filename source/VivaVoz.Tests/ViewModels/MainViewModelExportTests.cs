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

public class MainViewModelExportTests : IDisposable {
    private readonly string _tempDir;
    private readonly SqliteConnection _connection;

    public MainViewModelExportTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-export-vm-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _connection = CreateConnection();
    }

    public void Dispose() {
        _connection.Dispose();
        try {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best effort */ }

        GC.SuppressFinalize(this);
    }

    // ========== CanExportText tests ==========

    [Fact]
    public void CanExportText_WhenNoSelection_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);

        vm.CanExportText.Should().BeFalse();
    }

    [Fact]
    public void CanExportText_WhenCompleteWithTranscript_ShouldBeTrue() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello!");

        vm.CanExportText.Should().BeTrue();
    }

    [Fact]
    public void CanExportText_WhenCompleteButEmptyTranscript_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "");

        vm.CanExportText.Should().BeFalse();
    }

    [Fact]
    public void CanExportText_WhenTranscribing_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);
        var rec = CreateCompleteRecording(transcript: "text");
        rec.Status = RecordingStatus.Transcribing;
        vm.SelectedRecording = rec;

        vm.CanExportText.Should().BeFalse();
    }

    // ========== CanExportAudio tests ==========

    [Fact]
    public void CanExportAudio_WhenNoSelection_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);

        vm.CanExportAudio.Should().BeFalse();
    }

    [Fact]
    public void CanExportAudio_WhenRecordingSelectedWithAudioFile_ShouldBeTrue() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);
        vm.SelectedRecording = CreateCompleteRecording();

        vm.CanExportAudio.Should().BeTrue();
    }

    [Fact]
    public void CanExportAudio_WhenRecordingHasEmptyAudioFileName_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context);
        var rec = CreateCompleteRecording();
        rec.AudioFileName = string.Empty;
        vm.SelectedRecording = rec;

        vm.CanExportAudio.Should().BeFalse();
    }

    // ========== ExportTextCommand tests ==========

    [Fact]
    public async Task ExportTextCommand_WhenCompleteWithTranscript_ShouldCallExportService() {
        await using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportTextAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        var destPath = Path.Combine(_tempDir, "out.txt");
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>(destPath));

        var vm = CreateViewModel(context, dialogService: dialogService, exportService: exportService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello world");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await exportService.Received(1).ExportTextAsync("Hello world", destPath);
    }

    [Fact]
    public async Task ExportTextCommand_WhenDialogCancelled_ShouldNotCallExportService() {
        await using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>(null));

        var vm = CreateViewModel(context, dialogService: dialogService, exportService: exportService);
        vm.SelectedRecording = CreateCompleteRecording(transcript: "Hello");

        await vm.ExportTextCommand.ExecuteAsync(null);

        await exportService.DidNotReceive().ExportTextAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExportTextCommand_WhenNoTranscript_ShouldNotShowDialog() {
        await using var context = CreateContext(_connection);
        var dialogService = Substitute.For<IDialogService>();
        var vm = CreateViewModel(context, dialogService: dialogService);
        var rec = CreateCompleteRecording(transcript: "");
        vm.SelectedRecording = rec;

        await vm.ExportTextCommand.ExecuteAsync(null);

        await dialogService.DidNotReceive().ShowSaveFileDialogAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>());
    }

    [Fact]
    public async Task ExportTextCommand_WhenNoSelection_ShouldNotShowDialog() {
        await using var context = CreateContext(_connection);
        var dialogService = Substitute.For<IDialogService>();
        var vm = CreateViewModel(context, dialogService: dialogService);

        await vm.ExportTextCommand.ExecuteAsync(null);

        await dialogService.DidNotReceive().ShowSaveFileDialogAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>());
    }

    // ========== ExportAudioCommand tests ==========

    [Fact]
    public async Task ExportAudioCommand_WhenRecordingSelected_ShouldCallExportService() {
        await using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        exportService.ExportAudioAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);
        var dialogService = Substitute.For<IDialogService>();
        var destPath = Path.Combine(_tempDir, "out.wav");
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>(destPath));

        var vm = CreateViewModel(context, dialogService: dialogService, exportService: exportService);
        vm.SelectedRecording = CreateCompleteRecording();

        await vm.ExportAudioCommand.ExecuteAsync(null);

        await exportService.Received(1).ExportAudioAsync(Arg.Any<string>(), destPath);
    }

    [Fact]
    public async Task ExportAudioCommand_WhenDialogCancelled_ShouldNotCallExportService() {
        await using var context = CreateContext(_connection);
        var exportService = Substitute.For<IExportService>();
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ShowSaveFileDialogAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(Task.FromResult<string?>(null));

        var vm = CreateViewModel(context, dialogService: dialogService, exportService: exportService);
        vm.SelectedRecording = CreateCompleteRecording();

        await vm.ExportAudioCommand.ExecuteAsync(null);

        await exportService.DidNotReceive().ExportAudioAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExportAudioCommand_WhenNoSelection_ShouldNotShowDialog() {
        await using var context = CreateContext(_connection);
        var dialogService = Substitute.For<IDialogService>();
        var vm = CreateViewModel(context, dialogService: dialogService);

        await vm.ExportAudioCommand.ExecuteAsync(null);

        await dialogService.DidNotReceive().ShowSaveFileDialogAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>());
    }

    // ========== Crash recovery — HasOrphanedRecording tests ==========

    [Fact]
    public void HasOrphanedRecording_WhenCrashRecoveryServiceHasNoOrphan_ShouldBeFalse() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(false);

        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.HasOrphanedRecording.Should().BeFalse();
    }

    [Fact]
    public void HasOrphanedRecording_WhenCrashRecoveryServiceHasOrphan_ShouldBeTrue() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);

        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.HasOrphanedRecording.Should().BeTrue();
    }

    [Fact]
    public void HasOrphanedRecording_WhenNoCrashRecoveryService_ShouldBeFalse() {
        using var context = CreateContext(_connection);

        var vm = CreateViewModel(context, crashRecoveryService: null);

        vm.HasOrphanedRecording.Should().BeFalse();
    }

    // ========== DismissOrphanCommand tests ==========

    [Fact]
    public void DismissOrphanCommand_ShouldCallServiceDismiss() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.DismissOrphanCommand.Execute(null);

        crashService.Received(1).Dismiss();
    }

    [Fact]
    public void DismissOrphanCommand_ShouldSetHasOrphanedRecordingToFalse() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.DismissOrphanCommand.Execute(null);

        vm.HasOrphanedRecording.Should().BeFalse();
    }

    [Fact]
    public void DismissOrphanCommand_WhenNoCrashRecoveryService_ShouldNotThrow() {
        using var context = CreateContext(_connection);
        var vm = CreateViewModel(context, crashRecoveryService: null);

        var act = () => vm.DismissOrphanCommand.Execute(null);

        act.Should().NotThrow();
    }

    // ========== RecoverOrphanCommand tests ==========

    [Fact]
    public void RecoverOrphanCommand_WhenOrphanPathIsNull_ShouldDismissAndClearFlag() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        crashService.GetOrphanPath().Returns((string?)null);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.RecoverOrphan();

        crashService.Received(1).Dismiss();
        vm.HasOrphanedRecording.Should().BeFalse();
    }

    [Fact]
    public void RecoverOrphanCommand_WhenOrphanFileDoesNotExist_ShouldDismissAndClearFlag() {
        using var context = CreateContext(_connection);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        crashService.GetOrphanPath().Returns("/nonexistent/orphan.wav");
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.RecoverOrphan();

        crashService.Received(1).Dismiss();
        vm.HasOrphanedRecording.Should().BeFalse();
    }

    [Fact]
    public void RecoverOrphanCommand_WhenOrphanFileExists_ShouldAddRecordingToCollection() {
        using var context = CreateContext(_connection);
        var orphanFile = Path.Combine(_tempDir, $"{Guid.NewGuid()}.wav");
        File.WriteAllBytes(orphanFile, new byte[44 + 32000]); // WAV header + 1s of audio
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        crashService.GetOrphanPath().Returns(orphanFile);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.RecoverOrphan();

        vm.Recordings.Should().ContainSingle(r => r.Title.StartsWith("Recovered Recording"));
    }

    [Fact]
    public void RecoverOrphanCommand_WhenOrphanFileExists_ShouldDismissAndClearFlag() {
        using var context = CreateContext(_connection);
        var orphanFile = Path.Combine(_tempDir, $"{Guid.NewGuid()}.wav");
        File.WriteAllBytes(orphanFile, []);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        crashService.GetOrphanPath().Returns(orphanFile);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.RecoverOrphan();

        crashService.Received(1).Dismiss();
        vm.HasOrphanedRecording.Should().BeFalse();
    }

    [Fact]
    public void RecoverOrphanCommand_WhenOrphanFileExists_ShouldSetStatusToPendingTranscription() {
        using var context = CreateContext(_connection);
        var orphanFile = Path.Combine(_tempDir, $"{Guid.NewGuid()}.wav");
        File.WriteAllBytes(orphanFile, []);
        var crashService = Substitute.For<ICrashRecoveryService>();
        crashService.HasOrphan().Returns(true);
        crashService.GetOrphanPath().Returns(orphanFile);
        var vm = CreateViewModel(context, crashRecoveryService: crashService);

        vm.RecoverOrphan();

        vm.Recordings.Should().ContainSingle(r => r.Status == RecordingStatus.PendingTranscription);
    }

    // ========== Helper methods ==========

    private static MainViewModel CreateViewModel(
        AppDbContext context,
        IDialogService? dialogService = null,
        IExportService? exportService = null,
        ICrashRecoveryService? crashRecoveryService = null) {
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        return new MainViewModel(
            recorder, player, context,
            Substitute.For<ITranscriptionManager>(),
            Substitute.For<IClipboardService>(),
            dialogService: dialogService,
            exportService: exportService,
            crashRecoveryService: crashRecoveryService);
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

    private static SqliteConnection CreateConnection() {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        return conn;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
