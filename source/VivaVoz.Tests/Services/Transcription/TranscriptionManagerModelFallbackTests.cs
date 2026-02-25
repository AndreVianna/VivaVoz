using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NSubstitute;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class TranscriptionManagerModelFallbackTests : IDisposable {
    private readonly SqliteConnection _connection;

    public TranscriptionManagerModelFallbackTests() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    // ========== SelectModelWithFallback unit tests ==========

    [Fact]
    public void SelectModelWithFallback_WhenModelManagerIsNull_ShouldReturnPreferredModel() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var manager = new TranscriptionManager(engine, CreateContext); // no modelManager

        var result = manager.SelectModelWithFallback("medium");

        result.Should().Be("medium");
    }

    [Fact]
    public void SelectModelWithFallback_WhenPreferredModelIsInstalled_ShouldReturnPreferred() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("medium").Returns(true);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("medium");

        result.Should().Be("medium");
    }

    [Fact]
    public void SelectModelWithFallback_WhenPreferredNotInstalled_ShouldFallBackToNextSmaller() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("medium").Returns(false);
        modelManager.IsModelDownloaded("small").Returns(true);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("medium");

        result.Should().Be("small");
    }

    [Fact]
    public void SelectModelWithFallback_WhenLargeNotInstalled_ShouldFallBackToMedium() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("large-v3").Returns(false);
        modelManager.IsModelDownloaded("medium").Returns(true);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("large-v3");

        result.Should().Be("medium");
    }

    [Fact]
    public void SelectModelWithFallback_WhenNoModelIsInstalled_ShouldReturnTiny() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded(Arg.Any<string>()).Returns(false);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("medium");

        result.Should().Be("tiny");
    }

    [Fact]
    public void SelectModelWithFallback_WhenUnknownModelId_ShouldReturnThatModelId() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("unknown-model");

        result.Should().Be("unknown-model");
    }

    [Fact]
    public void SelectModelWithFallback_WhenTinyIsPreferredAndInstalled_ShouldReturnTiny() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("tiny").Returns(true);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("tiny");

        result.Should().Be("tiny");
    }

    [Fact]
    public void SelectModelWithFallback_WhenPreferredNotInstalledAndMultipleSmallerAvailable_ShouldReturnNextSmaller() {
        var engine = Substitute.For<ITranscriptionEngine>();
        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("large-v3").Returns(false);
        modelManager.IsModelDownloaded("medium").Returns(false);
        modelManager.IsModelDownloaded("small").Returns(true);
        modelManager.IsModelDownloaded("base").Returns(true);
        var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager);

        var result = manager.SelectModelWithFallback("large-v3");

        result.Should().Be("small");
    }

    // ========== Integration: settings service wires model to engine ==========

    [Fact]
    public async Task EnqueueTranscription_WhenSettingsServiceProvided_ShouldUseConfiguredModel() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        var engine = Substitute.For<ITranscriptionEngine>();
        engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello", "en", TimeSpan.FromSeconds(1), "medium"));

        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns(new Settings { WhisperModelSize = "medium", Language = "en" });

        using var manager = new TranscriptionManager(engine, CreateContext, settingsService: settingsService);

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Is<TranscriptionOptions>(o => o.ModelId == "medium"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSettingsServiceProvided_ShouldUseConfiguredLanguage() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        var engine = Substitute.For<ITranscriptionEngine>();
        engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Bonjour", "fr", TimeSpan.FromSeconds(1), "tiny"));

        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns(new Settings { WhisperModelSize = "tiny", Language = "fr" });

        using var manager = new TranscriptionManager(engine, CreateContext, settingsService: settingsService);

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Is<TranscriptionOptions>(o => o.Language == "fr"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnqueueTranscription_WhenPreferredModelNotInstalled_ShouldFallbackToInstalledModel() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        var engine = Substitute.For<ITranscriptionEngine>();
        engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hi", "en", TimeSpan.FromSeconds(1), "base"));

        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns(new Settings { WhisperModelSize = "large-v3", Language = "auto" });

        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("large-v3").Returns(false);
        modelManager.IsModelDownloaded("medium").Returns(false);
        modelManager.IsModelDownloaded("small").Returns(false);
        modelManager.IsModelDownloaded("base").Returns(true);

        using var manager = new TranscriptionManager(engine, CreateContext, modelManager: modelManager, settingsService: settingsService);

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Is<TranscriptionOptions>(o => o.ModelId == "base"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnqueueTranscription_WhenModelOverrideProvided_ShouldUseOverrideInsteadOfSettings() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        var engine = Substitute.For<ITranscriptionEngine>();
        engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello", "en", TimeSpan.FromSeconds(1), "small"));

        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns(new Settings { WhisperModelSize = "base", Language = "en" });

        var modelManager = Substitute.For<IModelManager>();
        modelManager.IsModelDownloaded("small").Returns(true);

        using var manager = new TranscriptionManager(engine, CreateContext,
            modelManager: modelManager, settingsService: settingsService);

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        manager.EnqueueTranscription(recordingId, audioPath, modelOverride: "small");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Is<TranscriptionOptions>(o => o.ModelId == "small"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnqueueTranscription_WhenNoSettingsService_ShouldDefaultToBase() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        var engine = Substitute.For<ITranscriptionEngine>();
        engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hi", "en", TimeSpan.FromSeconds(1), "base"));

        using var manager = new TranscriptionManager(engine, CreateContext); // no settings, no modelManager

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Is<TranscriptionOptions>(o => o.ModelId == "base"),
            Arg.Any<CancellationToken>());
    }

    // ========== Helper methods ==========

    private Guid SeedRecording() {
        var id = Guid.NewGuid();
        using var context = CreateContext();
        context.Recordings.Add(new Recording {
            Id = id,
            Title = "Test Recording",
            AudioFileName = "test.wav",
            Status = RecordingStatus.PendingTranscription,
            Language = "auto",
            Duration = TimeSpan.FromSeconds(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            WhisperModel = "tiny",
            FileSize = 1000
        });
        context.SaveChanges();
        return id;
    }

    private AppDbContext CreateContext() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new AppDbContext(options);
    }
}
