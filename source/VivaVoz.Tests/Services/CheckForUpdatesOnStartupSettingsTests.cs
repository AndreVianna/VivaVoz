using AwesomeAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

/// <summary>
/// Tests for the CheckForUpdatesOnStartup Settings field — persistence and default value.
/// </summary>
public class CheckForUpdatesOnStartupSettingsTests {
    // ========== Default value ==========

    [Fact]
    public async Task LoadSettingsAsync_WhenNoSettingsExist_ShouldDefaultCheckForUpdatesOnStartupToTrue() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup.Should().BeTrue();
    }

    // ========== Persist true ==========

    [Fact]
    public async Task SaveSettingsAsync_WithCheckForUpdatesOnStartupTrue_ShouldPersist() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup = true;
        await service.SaveSettingsAsync(settings);

        await using var verifyContext = CreateContext(connection);
        var persisted = await verifyContext.Settings.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.CheckForUpdatesOnStartup.Should().BeTrue();
    }

    // ========== Persist false ==========

    [Fact]
    public async Task SaveSettingsAsync_WithCheckForUpdatesOnStartupFalse_ShouldPersist() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup = false;
        await service.SaveSettingsAsync(settings);

        await using var verifyContext = CreateContext(connection);
        var persisted = await verifyContext.Settings.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.CheckForUpdatesOnStartup.Should().BeFalse();
    }

    // ========== Update current ==========

    [Fact]
    public async Task SaveSettingsAsync_ShouldUpdateCurrentCheckForUpdatesOnStartup() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup = false;
        await service.SaveSettingsAsync(settings);

        service.Current!.CheckForUpdatesOnStartup.Should().BeFalse();
    }

    // ========== Round-trip ==========

    [Fact]
    public async Task LoadSettingsAsync_AfterSavingFalse_ShouldLoadFalse() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        // Seed a settings row with CheckForUpdatesOnStartup = false
        await using (var seedContext = CreateContext(connection)) {
            seedContext.Settings.Add(new Settings {
                CheckForUpdatesOnStartup = false,
                StoragePath = "/test",
                HotkeyConfig = string.Empty
            });
            await seedContext.SaveChangesAsync();
        }

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup.Should().BeFalse();
    }

    [Fact]
    public async Task LoadSettingsAsync_AfterSavingTrue_ShouldLoadTrue() {
        await using var connection = CreateConnection();
        EnsureDatabase(connection);

        await using (var seedContext = CreateContext(connection)) {
            seedContext.Settings.Add(new Settings {
                CheckForUpdatesOnStartup = true,
                StoragePath = "/test",
                HotkeyConfig = string.Empty
            });
            await seedContext.SaveChangesAsync();
        }

        var service = new SettingsService(() => CreateContext(connection));
        var settings = await service.LoadSettingsAsync();

        settings.CheckForUpdatesOnStartup.Should().BeTrue();
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
