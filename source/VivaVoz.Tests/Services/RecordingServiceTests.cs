using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class RecordingServiceTests : IDisposable {
    private readonly string _tempDir;
    private readonly SqliteConnection _connection;

    public RecordingServiceTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-test-{Guid.NewGuid()}");
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

    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullContextFactory_ShouldThrowArgumentNullException() {
        var act = () => new RecordingService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("contextFactory");
    }

    [Fact]
    public void Constructor_WithValidContextFactory_ShouldNotThrow() {
        var act = () => new RecordingService(() => CreateContext(_connection));

        act.Should().NotThrow();
    }

    // ========== UpdateAsync tests ==========

    [Fact]
    public async Task UpdateAsync_WithNullRecording_ShouldThrowArgumentNullException() {
        EnsureDatabase(_connection);
        var service = CreateService(_connection);

        var act = async () => await service.UpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordingExists_ShouldPersistTranscriptChange() {
        EnsureDatabase(_connection);
        var recording = await SeedRecordingAsync(_connection, transcript: "Hello worl");
        var service = CreateService(_connection);

        recording.Transcript = "Hello World";
        await service.UpdateAsync(recording);

        await using var verifyContext = CreateContext(_connection);
        var persisted = await verifyContext.Recordings.FindAsync(recording.Id);
        persisted!.Transcript.Should().Be("Hello World");
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordingDoesNotExist_ShouldNotThrow() {
        EnsureDatabase(_connection);
        var service = CreateService(_connection);
        var recording = CreateRecording();

        var act = async () => await service.UpdateAsync(recording);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_WhenRecordingExists_ShouldUpdateUpdatedAt() {
        EnsureDatabase(_connection);
        var original = await SeedRecordingAsync(_connection);
        var service = CreateService(_connection);

        var before = DateTime.UtcNow.AddSeconds(-1);
        original.UpdatedAt = DateTime.UtcNow;
        await service.UpdateAsync(original);

        await using var verifyContext = CreateContext(_connection);
        var persisted = await verifyContext.Recordings.FindAsync(original.Id);
        persisted!.UpdatedAt.Should().BeAfter(before);
    }

    // ========== DeleteAsync tests ==========

    [Fact]
    public async Task DeleteAsync_WhenRecordingExists_ShouldRemoveFromDatabase() {
        EnsureDatabase(_connection);
        var recording = await SeedRecordingAsync(_connection);
        var service = CreateService(_connection);

        await service.DeleteAsync(recording.Id);

        await using var verifyContext = CreateContext(_connection);
        var persisted = await verifyContext.Recordings.FindAsync(recording.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenAudioFileExists_ShouldDeleteFile() {
        EnsureDatabase(_connection);
        var audioFile = Path.Combine(_tempDir, "test.wav");
        File.WriteAllText(audioFile, "fake audio");
        var recording = await SeedRecordingAsync(_connection, audioFileName: "test.wav");
        var service = CreateService(_connection);

        await service.DeleteAsync(recording.Id);

        File.Exists(audioFile).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenAudioFileDoesNotExist_ShouldNotThrow() {
        EnsureDatabase(_connection);
        var recording = await SeedRecordingAsync(_connection, audioFileName: "nonexistent.wav");
        var service = CreateService(_connection);

        var act = async () => await service.DeleteAsync(recording.Id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecordingDoesNotExist_ShouldNotThrow() {
        EnsureDatabase(_connection);
        var service = CreateService(_connection);

        var act = async () => await service.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    // ========== Helper methods ==========

    private RecordingService CreateService(SqliteConnection connection)
        => new(() => CreateContext(connection), _tempDir);

    private static SqliteConnection CreateConnection() {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        return new AppDbContext(options);
    }

    private static void EnsureDatabase(SqliteConnection connection) {
        using var context = CreateContext(connection);
        context.Database.EnsureCreated();
    }

    private static async Task<Recording> SeedRecordingAsync(
        SqliteConnection connection,
        string? transcript = null,
        string audioFileName = "recording.wav") {
        await using var context = CreateContext(connection);
        var recording = CreateRecording(transcript, audioFileName);
        context.Recordings.Add(recording);
        await context.SaveChangesAsync();
        return recording;
    }

    private static Recording CreateRecording(string? transcript = null, string audioFileName = "recording.wav") {
        var now = DateTime.UtcNow;
        return new Recording {
            Id = Guid.NewGuid(),
            Title = "Test Recording",
            AudioFileName = audioFileName,
            Transcript = transcript,
            Status = RecordingStatus.Complete,
            Language = "en",
            Duration = TimeSpan.FromSeconds(10),
            CreatedAt = now,
            UpdatedAt = now,
            WhisperModel = "tiny",
            FileSize = 1024
        };
    }
}
