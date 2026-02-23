using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class SettingsServiceTests {
    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullContextFactory_ShouldThrowArgumentNullException() {
        var act = () => new SettingsService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("contextFactory");
    }

    [Fact]
    public void Constructor_ShouldInitializeCurrentToNull() {
        using var connection = CreateConnection();
        var service = new SettingsService(() => CreateContext(connection));

        service.Current.Should().BeNull();
    }

    // ========== LoadSettingsAsync tests ==========

    [Fact]
    public async Task LoadSettingsAsync_WhenNoSettingsExist_ShouldCreateDefaults() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));

        var settings = await service.LoadSettingsAsync();

        settings.Should().NotBeNull();
        settings.WhisperModelSize.Should().Be("tiny");
        settings.Theme.Should().Be("System");
        settings.Language.Should().Be("auto");
        settings.ExportFormat.Should().Be("MP3");
        settings.HotkeyConfig.Should().Be(string.Empty);
        settings.AudioInputDevice.Should().BeNull();
        settings.AutoUpdate.Should().BeFalse();
        settings.StoragePath.Should().Contain("VivaVoz");
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenNoSettingsExist_ShouldPersistDefaults() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        await service.LoadSettingsAsync();

        // Verify persisted by loading from a fresh context
        await using var verifyContext = CreateContext(connection);
        var persisted = await verifyContext.Settings.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.WhisperModelSize.Should().Be("tiny");
        persisted.Language.Should().Be("auto");
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenNoSettingsExist_ShouldSetCurrent() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));

        var settings = await service.LoadSettingsAsync();

        service.Current.Should().BeSameAs(settings);
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenSettingsExist_ShouldLoadExistingSettings() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        // Pre-seed settings
        await using (var seedContext = CreateContext(connection)) {
            seedContext.Settings.Add(new Settings {
                WhisperModelSize = "base",
                Language = "en",
                Theme = "Dark",
                StoragePath = "/custom/path",
                ExportFormat = "WAV",
                HotkeyConfig = "Ctrl+Shift+R",
                AutoUpdate = true
            });
            await seedContext.SaveChangesAsync();
        }

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.WhisperModelSize.Should().Be("base");
        settings.Language.Should().Be("en");
        settings.Theme.Should().Be("Dark");
        settings.StoragePath.Should().Be("/custom/path");
        settings.ExportFormat.Should().Be("WAV");
        settings.HotkeyConfig.Should().Be("Ctrl+Shift+R");
        settings.AutoUpdate.Should().BeTrue();
    }

    [Fact]
    public async Task LoadSettingsAsync_WhenSettingsExist_ShouldSetCurrent() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        await using (var seedContext = CreateContext(connection)) {
            seedContext.Settings.Add(new Settings { WhisperModelSize = "small" });
            await seedContext.SaveChangesAsync();
        }

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        service.Current.Should().NotBeNull();
        service.Current!.WhisperModelSize.Should().Be("small");
    }

    [Fact]
    public async Task LoadSettingsAsync_CalledTwice_ShouldReturnSameData() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));

        var first = await service.LoadSettingsAsync();
        var second = await service.LoadSettingsAsync();

        // Both should have defaults since first call created them
        first.WhisperModelSize.Should().Be("tiny");
        second.WhisperModelSize.Should().Be("tiny");
    }

    // ========== SaveSettingsAsync tests ==========

    [Fact]
    public async Task SaveSettingsAsync_WithNullSettings_ShouldThrowArgumentNullException() {
        await using var connection = CreateConnection();
        var service = new SettingsService(() => CreateContext(connection));

        var act = () => service.SaveSettingsAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("settings");
    }

    [Fact]
    public async Task SaveSettingsAsync_WithExistingSettings_ShouldPersistChanges() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.WhisperModelSize = "small";
        settings.Language = "fr";
        settings.Theme = "Dark";
        await service.SaveSettingsAsync(settings);

        // Verify persisted
        await using var verifyContext = CreateContext(connection);
        var persisted = await verifyContext.Settings.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.WhisperModelSize.Should().Be("small");
        persisted.Language.Should().Be("fr");
        persisted.Theme.Should().Be("Dark");
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldUpdateCurrent() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.WhisperModelSize = "medium";
        await service.SaveSettingsAsync(settings);

        service.Current.Should().NotBeNull();
        service.Current!.WhisperModelSize.Should().Be("medium");
    }

    [Fact]
    public async Task SaveSettingsAsync_WithNewSettings_ShouldInsert() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));

        var newSettings = new Settings {
            WhisperModelSize = "large",
            Language = "pt",
            Theme = "Light",
            StoragePath = "/my/path",
            ExportFormat = "OGG",
            HotkeyConfig = "Alt+R",
            AutoUpdate = true
        };

        await service.SaveSettingsAsync(newSettings);

        await using var verifyContext = CreateContext(connection);
        var persisted = await verifyContext.Settings.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.WhisperModelSize.Should().Be("large");
        persisted.Language.Should().Be("pt");
    }

    [Fact]
    public async Task SaveSettingsAsync_AfterLoad_ShouldUpdateNotDuplicate() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.WhisperModelSize = "base";
        await service.SaveSettingsAsync(settings);

        // Should have exactly one settings row
        await using var verifyContext = CreateContext(connection);
        var count = await verifyContext.Settings.CountAsync();
        count.Should().Be(1);

        var persisted = await verifyContext.Settings.FirstAsync();
        persisted.WhisperModelSize.Should().Be("base");
    }

    // ========== Defaults verification ==========

    [Fact]
    public async Task LoadSettingsAsync_DefaultStoragePath_ShouldPointToVivaVozAppData() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        var expectedBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        settings.StoragePath.Should().StartWith(expectedBase);
        settings.StoragePath.Should().EndWith("VivaVoz");
    }

    // ========== Helper methods ==========

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
}
