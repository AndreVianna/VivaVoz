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

/// <summary>
/// Tests that <see cref="MainViewModel"/> correctly drives the tray icon
/// state through the recording lifecycle via <see cref="ITrayIconService"/>.
/// </summary>
public class MainViewModelTrayTests {
    // ========== RecordCommand — Recording state ==========

    [Fact]
    public void StartRecordingCommand_WhenStarted_ShouldSetTrayStateToRecording() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var trayIconService = Substitute.For<ITrayIconService>();
        var vm = CreateViewModel(context, trayIconService: trayIconService);

        vm.StartRecordingCommand.Execute(null);

        trayIconService.Received(1).SetState(AppState.Recording);
    }

    // ========== RecordCommand — Transcribing state ==========

    [Fact]
    public void StopRecordingCommand_WhenStopped_ShouldSetTrayStateToTranscribing() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var trayIconService = Substitute.For<ITrayIconService>();
        var vm = CreateViewModel(context, trayIconService: trayIconService);

        vm.StopRecordingCommand.Execute(null);

        trayIconService.Received(1).SetState(AppState.Transcribing);
    }

    // ========== Transcription complete — Ready state ==========

    [Fact]
    public void HandleTranscriptionReadyForTray_WhenSuccess_ShouldSetTrayStateToReady() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var trayIconService = Substitute.For<ITrayIconService>();
        var vm = CreateViewModel(context, trayIconService: trayIconService);

        vm.HandleTranscriptionReadyForTray(success: true);

        trayIconService.Received(1).SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(3));
    }

    // ========== Transcription error — Idle state ==========

    [Fact]
    public void HandleTranscriptionReadyForTray_WhenFailure_ShouldSetTrayStateToIdle() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var trayIconService = Substitute.For<ITrayIconService>();
        var vm = CreateViewModel(context, trayIconService: trayIconService);

        vm.HandleTranscriptionReadyForTray(success: false);

        trayIconService.Received(1).SetState(AppState.Idle);
    }

    // ========== Recording error — Idle state ==========

    [Fact]
    public void StartRecordingCommand_WhenMicrophoneNotFound_ShouldSetTrayStateToIdle() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.When(r => r.StartRecording()).Do(_ => throw new MicrophoneNotFoundException("No mic"));
        var trayIconService = Substitute.For<ITrayIconService>();
        var vm = CreateViewModel(context, recorder: recorder, trayIconService: trayIconService);

        try {
            vm.StartRecordingCommand.Execute(null);
        }
        catch {
            // Avalonia ShowWarningAsync may throw in test context; that's expected.
        }

        trayIconService.Received(1).SetState(AppState.Idle);
    }

    // ========== No tray service — should not throw ==========

    [Fact]
    public void StartRecordingCommand_WithNoTrayService_ShouldNotThrow() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var vm = CreateViewModel(context, trayIconService: null);

        var act = () => vm.StartRecordingCommand.Execute(null);

        act.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void StopRecordingCommand_WithNoTrayService_ShouldNotThrow() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var vm = CreateViewModel(context, trayIconService: null);

        var act = () => vm.StopRecordingCommand.Execute(null);

        act.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void HandleTranscriptionReadyForTray_WithNoTrayService_ShouldNotThrow() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        var vm = CreateViewModel(context, trayIconService: null);

        var act = () => vm.HandleTranscriptionReadyForTray(success: true);

        act.Should().NotThrow<NullReferenceException>();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static MainViewModel CreateViewModel(
        AppDbContext context,
        IAudioRecorder? recorder = null,
        ITrayIconService? trayIconService = null) {
        recorder ??= Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var tm = Substitute.For<ITranscriptionManager>();
        var clipboard = Substitute.For<IClipboardService>();
        return new MainViewModel(
            recorder, player, context, tm, clipboard,
            trayIconService: trayIconService);
    }

    private static SqliteConnection CreateConnection() {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
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
