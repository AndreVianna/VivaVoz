using AwesomeAssertions;
using VivaVoz.Models;
using Xunit;

namespace VivaVoz.Tests.Models;

public class SettingsTests {
    [Fact]
    public void NewSettings_ShouldDefaultWhisperModelSizeToTiny() {
        var settings = new Settings();

        settings.WhisperModelSize.Should().Be("tiny");
    }

    [Fact]
    public void NewSettings_ShouldDefaultExportFormatToMp3() {
        var settings = new Settings();

        settings.ExportFormat.Should().Be("MP3");
    }

    [Fact]
    public void NewSettings_ShouldDefaultThemeToSystem() {
        var settings = new Settings();

        settings.Theme.Should().Be("System");
    }

    [Fact]
    public void NewSettings_ShouldDefaultAutoUpdateToFalse() {
        var settings = new Settings();

        settings.AutoUpdate.Should().BeFalse();
    }

    [Fact]
    public void NewSettings_ShouldDefaultStoragePathToContainVivaVoz() {
        var settings = new Settings();

        settings.StoragePath.Should().Contain("VivaVoz");
    }

    [Fact]
    public void NewSettings_ShouldDefaultMinimizeToTrayToTrue() {
        var settings = new Settings();

        settings.MinimizeToTray.Should().BeTrue();
    }

    [Fact]
    public void NewSettings_ShouldDefaultStartMinimizedToFalse() {
        var settings = new Settings();

        settings.StartMinimized.Should().BeFalse();
    }
}
