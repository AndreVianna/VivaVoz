using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class TranscriptionManagerTests : IDisposable {
    private readonly SqliteConnection _connection;
    private readonly ITranscriptionEngine _engine;
    private readonly TranscriptionManager _manager;

    public TranscriptionManagerTests() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        InitializeDatabase();
        _engine = Substitute.For<ITranscriptionEngine>();
        _manager = new TranscriptionManager(_engine, CreateContext);
    }

    public void Dispose() {
        _manager.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private void InitializeDatabase() {
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    private AppDbContext CreateContext() {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new AppDbContext(options);
    }

    private Guid SeedRecording(RecordingStatus status = RecordingStatus.PendingTranscription) {
        var id = Guid.NewGuid();
        using var context = CreateContext();
        context.Recordings.Add(new Recording {
            Id = id,
            Title = "Test Recording",
            AudioFileName = "test.wav",
            Status = status,
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

    [Fact]
    public void Constructor_WithNullEngine_ShouldThrow() {
        var act = () => new TranscriptionManager(null!, CreateContext);

        act.Should().Throw<ArgumentNullException>().WithParameterName("engine");
    }

    [Fact]
    public void Constructor_WithNullContextFactory_ShouldThrow() {
        var act = () => new TranscriptionManager(_engine, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("contextFactory");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnqueueTranscription_WithInvalidFilePath_ShouldThrow(string? path) {
        var act = () => _manager.EnqueueTranscription(Guid.NewGuid(), path!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldUpdateRecordingStatusToComplete() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello world", "en", TimeSpan.FromSeconds(2), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeTrue();

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Status.Should().Be(RecordingStatus.Complete);
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldStoreTranscriptText() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";
        const string expectedText = "The quick brown fox jumps over the lazy dog.";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult(expectedText, "en", TimeSpan.FromSeconds(3), "small"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Transcript.Should().Be(expectedText);

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Transcript.Should().Be(expectedText);
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldUpdateLanguageAndModel() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Bonjour", "fr", TimeSpan.FromSeconds(1), "base"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.DetectedLanguage.Should().Be("fr");
        result.ModelUsed.Should().Be("base");

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Language.Should().Be("fr");
        recording.WhisperModel.Should().Be("base");
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldPersistLanguageCode() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Bonjour", "fr", TimeSpan.FromSeconds(1), "base"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.LanguageCode.Should().Be("fr");
    }

    [Fact]
    public async Task EnqueueTranscription_WhenDetectedLanguageIsUnknown_ShouldPersistUnknownLanguageCode() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("", "unknown", TimeSpan.FromSeconds(1), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.LanguageCode.Should().Be("unknown");
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldUpdateUpdatedAtTimestamp() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";
        var beforeTranscription = DateTime.UtcNow;

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hi", "en", TimeSpan.FromSeconds(1), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.UpdatedAt.Should().BeOnOrAfter(beforeTranscription);
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSuccessful_ShouldFireCompletedEventWithCorrectData() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Test text", "pt", TimeSpan.FromSeconds(2), "medium"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.RecordingId.Should().Be(recordingId);
        result.Success.Should().BeTrue();
        result.Transcript.Should().Be("Test text");
        result.DetectedLanguage.Should().Be("pt");
        result.ModelUsed.Should().Be("medium");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task EnqueueTranscription_WhenTranscriptionFails_ShouldUpdateRecordingStatusToFailed() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Whisper crash"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeFalse();

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Status.Should().Be(RecordingStatus.Failed);
    }

    [Fact]
    public async Task EnqueueTranscription_WhenTranscriptionFails_ShouldLeaveTranscriptNull() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FileNotFoundException("Audio file gone"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Transcript.Should().BeNull();
    }

    [Fact]
    public async Task EnqueueTranscription_WhenTranscriptionFails_ShouldFireCompletedEventWithError() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Model not loaded"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.RecordingId.Should().Be(recordingId);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Model not loaded");
        result.Transcript.Should().BeNull();
        result.DetectedLanguage.Should().BeNull();
        result.ModelUsed.Should().BeNull();
    }

    [Fact]
    public async Task EnqueueTranscription_WhenTranscriptionFails_ShouldUpdateUpdatedAtTimestamp() {
        var recordingId = SeedRecording();
        const string audioPath = "/tmp/test-audio.wav";
        var beforeTranscription = DateTime.UtcNow;

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("fail"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.UpdatedAt.Should().BeOnOrAfter(beforeTranscription);
    }

    [Fact]
    public async Task EnqueueTranscription_WithNonExistentRecording_ShouldStillFireCompletedEvent() {
        var nonExistentId = Guid.NewGuid();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello", "en", TimeSpan.FromSeconds(1), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(nonExistentId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeTrue();
        result.RecordingId.Should().Be(nonExistentId);
    }

    [Fact]
    public async Task EnqueueTranscription_WhenFailureUpdateFails_ShouldStillFireCompletedEvent() {
        // Use a non-existent recording ID so the failure update is a no-op (covers the null guard)
        var recordingId = Guid.NewGuid();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("engine crash"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("engine crash");
    }

    [Fact]
    public async Task EnqueueTranscription_ShouldPassCorrectFilePathToEngine() {
        var recordingId = SeedRecording();
        const string audioPath = "/specific/path/to/audio.wav";

        _engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("text", "en", TimeSpan.FromSeconds(1), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _engine.Received(1).TranscribeAsync(
            audioPath,
            Arg.Any<TranscriptionOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow() {
        var manager = new TranscriptionManager(_engine, CreateContext);

        manager.Dispose();
        var act = manager.Dispose;

        act.Should().NotThrow();
    }

    // ========== PendingTranscription lifecycle tests ==========

    [Fact]
    public async Task EnqueueTranscription_WhenPickedUp_ShouldSetStatusToTranscribingBeforeEngine() {
        var recordingId = SeedRecording(RecordingStatus.PendingTranscription);
        const string audioPath = "/tmp/test-audio.wav";

        // Capture status at the moment the engine is called
        RecordingStatus? statusAtEngineCall = null;
        _engine.TranscribeAsync(Arg.Any<string>(), Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(async _ => {
                await using var ctx = CreateContext();
                var r = await ctx.Recordings.FindAsync(recordingId);
                statusAtEngineCall = r?.Status;
                return new TranscriptionResult("Hello", "en", TimeSpan.FromSeconds(1), "tiny");
            });

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        statusAtEngineCall.Should().Be(RecordingStatus.Transcribing);
    }

    [Fact]
    public async Task EnqueueTranscription_StartingFromPendingTranscription_ShouldCompleteSuccessfully() {
        var recordingId = SeedRecording(RecordingStatus.PendingTranscription);
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello world", "en", TimeSpan.FromSeconds(2), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(recordingId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeTrue();

        await using var verifyContext = CreateContext();
        var recording = await verifyContext.Recordings.FindAsync(recordingId);
        recording!.Status.Should().Be(RecordingStatus.Complete);
        recording.Transcript.Should().Be("Hello world");
    }

    [Fact]
    public async Task EnqueueTranscription_WhenSetTranscribingFails_ShouldStillCompleteTranscription() {
        // Use a non-existent recording ID: SetTranscribingStatus will be a no-op (null guard), engine still runs.
        var nonExistentId = Guid.NewGuid();
        const string audioPath = "/tmp/test-audio.wav";

        _engine.TranscribeAsync(audioPath, Arg.Any<TranscriptionOptions>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("text", "en", TimeSpan.FromSeconds(1), "tiny"));

        var tcs = new TaskCompletionSource<TranscriptionCompletedEventArgs>();
        _manager.TranscriptionCompleted += (_, e) => tcs.TrySetResult(e);

        _manager.EnqueueTranscription(nonExistentId, audioPath);
        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        result.Success.Should().BeTrue();
    }
}
