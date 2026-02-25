using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using VivaVoz.Data;
using VivaVoz.Models;

using Xunit;

namespace VivaVoz.Tests.Data;

public class AppDbContextTests {
    [Fact]
    public void EnsureCreated_WhenNewDatabase_ShouldReturnTrue() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var created = context.Database.EnsureCreated();

        created.Should().BeTrue();
    }

    [Fact]
    public void Recordings_WhenAdded_ShouldBeRetrievable() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        context.Database.EnsureCreated();

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        context.Recordings.Add(new Recording {
            Id = id,
            Title = "Test",
            AudioFileName = "file.wav",
            Transcript = "Hello",
            Status = RecordingStatus.Complete,
            Language = "en",
            Duration = TimeSpan.FromSeconds(10),
            CreatedAt = now,
            UpdatedAt = now,
            WhisperModel = "tiny",
            FileSize = 256
        });
        context.SaveChanges();

        var fetched = context.Recordings.AsNoTracking().Single();

        fetched.Id.Should().Be(id);
        fetched.Title.Should().Be("Test");
        fetched.AudioFileName.Should().Be("file.wav");
        fetched.Status.Should().Be(RecordingStatus.Complete);
    }

    [Fact]
    public void Settings_WhenAdded_ShouldBeRetrievable() {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);
        context.Database.EnsureCreated();

        context.Settings.Add(new Settings {
            Id = 1,
            HotkeyConfig = "Ctrl+Shift+R",
            WhisperModelSize = "tiny",
            AudioInputDevice = null,
            StoragePath = "C:\\Temp\\VivaVoz",
            Theme = "System",
            Language = "auto",
            AutoUpdate = false
        });
        context.SaveChanges();

        var fetched = context.Settings.AsNoTracking().Single();

        fetched.Id.Should().Be(1);
        fetched.HotkeyConfig.Should().Be("Ctrl+Shift+R");
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
        return new AppDbContext(options);
    }
}
