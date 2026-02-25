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

/// <summary>
/// Tests for <see cref="MainViewModel.TryCopyTranscriptToClipboardAsync"/> —
/// the auto-copy-to-clipboard behaviour triggered on transcription completion.
/// </summary>
public class MainViewModelAutoClipboardTests {
    // ========== AutoCopy enabled (default) ==========

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenAutoCopyEnabledAndTranscriptNonEmpty_ShouldCopyToClipboard() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var settingsService = CreateSettingsService(autoCopy: true);
        var vm = CreateViewModel(context, clipboard, settingsService);

        await vm.TryCopyTranscriptToClipboardAsync("Hello world");

        await clipboard.Received(1).SetTextAsync("Hello world");
    }

    // ========== AutoCopy disabled ==========

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenAutoCopyDisabled_ShouldNotCopyToClipboard() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var settingsService = CreateSettingsService(autoCopy: false);
        var vm = CreateViewModel(context, clipboard, settingsService);

        await vm.TryCopyTranscriptToClipboardAsync("Hello world");

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    // ========== Null / empty transcript ==========

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenTranscriptIsNull_ShouldNotCopyToClipboard() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var settingsService = CreateSettingsService(autoCopy: true);
        var vm = CreateViewModel(context, clipboard, settingsService);

        await vm.TryCopyTranscriptToClipboardAsync(null);

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenTranscriptIsEmpty_ShouldNotCopyToClipboard() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var settingsService = CreateSettingsService(autoCopy: true);
        var vm = CreateViewModel(context, clipboard, settingsService);

        await vm.TryCopyTranscriptToClipboardAsync(string.Empty);

        await clipboard.DidNotReceive().SetTextAsync(Arg.Any<string>());
    }

    // ========== Clipboard throws — must not propagate ==========

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenClipboardThrows_ShouldNotThrow() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.FromException(new Exception("clipboard unavailable")));
        var settingsService = CreateSettingsService(autoCopy: true);
        var vm = CreateViewModel(context, clipboard, settingsService);

        var act = () => vm.TryCopyTranscriptToClipboardAsync("Some transcript");

        await act.Should().NotThrowAsync();
    }

    // ========== Null settings — defaults to true ==========

    [Fact]
    public async Task TryCopyTranscriptToClipboard_WhenSettingsNull_ShouldDefaultToAutoCopyEnabled() {
        await using var connection = CreateConnection();
        await using var context = CreateContext(connection);
        var clipboard = Substitute.For<IClipboardService>();
        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns((Settings?)null);
        var vm = CreateViewModel(context, clipboard, settingsService);

        await vm.TryCopyTranscriptToClipboardAsync("Hello");

        await clipboard.Received(1).SetTextAsync("Hello");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static ISettingsService CreateSettingsService(bool autoCopy) {
        var settings = new Settings { AutoCopyToClipboard = autoCopy };
        var svc = Substitute.For<ISettingsService>();
        svc.Current.Returns(settings);
        return svc;
    }

    private static MainViewModel CreateViewModel(
        AppDbContext context,
        IClipboardService clipboard,
        ISettingsService settingsService) {
        var recorder = Substitute.For<IAudioRecorder>();
        var player = Substitute.For<IAudioPlayer>();
        var tm = Substitute.For<ITranscriptionManager>();
        return new MainViewModel(recorder, player, context, tm, clipboard, settingsService);
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
