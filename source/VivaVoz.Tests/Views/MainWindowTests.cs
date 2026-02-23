using AwesomeAssertions;
using NSubstitute;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Views;
using Xunit;

namespace VivaVoz.Tests.Views;

public class MainWindowTests {
    // ========== MainWindow.ShouldMinimizeToTray ==========

    [Fact]
    public void ShouldMinimizeToTray_WithNullService_ShouldReturnFalse() {
        var result = MainWindow.ShouldMinimizeToTray(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldMinimizeToTray_WithNullCurrentSettings_ShouldReturnFalse() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns((Settings?)null);

        var result = MainWindow.ShouldMinimizeToTray(service);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldMinimizeToTray_WhenMinimizeToTrayIsFalse_ShouldReturnFalse() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings { MinimizeToTray = false });

        var result = MainWindow.ShouldMinimizeToTray(service);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldMinimizeToTray_WhenMinimizeToTrayIsTrue_ShouldReturnTrue() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings { MinimizeToTray = true });

        var result = MainWindow.ShouldMinimizeToTray(service);

        result.Should().BeTrue();
    }
}
