using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class AboutViewModelTests {
    // ========== Constructor (service overload) tests ==========

    [Fact]
    public void Constructor_WithNullSettingsService_ShouldThrow() {
        var act = () => new AboutViewModel((ISettingsService)null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("settingsService");
    }

    [Fact]
    public void Constructor_WithValidSettingsService_ShouldNotThrow() {
        var service = CreateService();

        var act = () => new AboutViewModel(service);

        act.Should().NotThrow();
    }

    // ========== Static properties ==========

    [Fact]
    public void AppName_ShouldBeVivaVoz() {
        var vm = CreateViewModel();

        vm.AppName.Should().Be("VivaVoz");
    }

    [Fact]
    public void Tagline_ShouldBeCorrect() {
        var vm = CreateViewModel();

        vm.Tagline.Should().Be("Your voice, alive.");
    }

    [Fact]
    public void Credits_ShouldMentionAndreVianna() {
        var vm = CreateViewModel();

        vm.Credits.Should().Contain("Andre Vianna");
    }

    [Fact]
    public void GitHubUrl_ShouldPointToRepo() {
        var vm = CreateViewModel();

        vm.GitHubUrl.Should().StartWith("https://github.com/AndreVianna/VivaVoz");
    }

    [Fact]
    public void IssuesUrl_ShouldPointToIssues() {
        var vm = CreateViewModel();

        vm.IssuesUrl.Should().Contain("issues");
        vm.IssuesUrl.Should().StartWith("https://github.com");
    }

    // ========== Version display tests ==========

    [Fact]
    public void AppVersion_WhenPassedExplicitly_ShouldBeUsed() {
        var vm = new AboutViewModel("2.3.4", "Ctrl+Shift+R");

        vm.AppVersion.Should().Be("2.3.4");
    }

    [Fact]
    public void AppVersion_WhenPassedNull_ShouldFallbackToDefaultString() {
        var vm = new AboutViewModel(null!, "Ctrl+Shift+R");

        vm.AppVersion.Should().NotBeNull();
    }

    [Fact]
    public void AppVersion_FromSettingsService_ShouldBeValidSemver() {
        var service = CreateService();

        var vm = new AboutViewModel(service);

        vm.AppVersion.Should().NotBeNullOrWhiteSpace();
        // Should be parseable as a version (e.g. "0.0.0" or "1.2.3")
        Version.TryParse(vm.AppVersion, out _).Should().BeTrue();
    }

    // ========== Hotkey display tests ==========

    [Fact]
    public void HotkeyDisplay_WhenHotkeyIsConfigured_ShouldFormatWithSpaces() {
        var vm = new AboutViewModel("1.0.0", "Ctrl+Shift+R");

        vm.HotkeyDisplay.Should().Be("Ctrl + Shift + R");
    }

    [Fact]
    public void HotkeyDisplay_WhenHotkeyIsEmpty_ShouldUseDefault() {
        var vm = new AboutViewModel("1.0.0", string.Empty);

        // Default is Ctrl+Shift+R formatted with spaces
        vm.HotkeyDisplay.Should().Contain("Ctrl");
        vm.HotkeyDisplay.Should().Contain("Shift");
        vm.HotkeyDisplay.Should().Contain("R");
    }

    [Fact]
    public void HotkeyDisplay_WhenHotkeyIsNull_ShouldUseDefault() {
        var vm = new AboutViewModel("1.0.0", null!);

        vm.HotkeyDisplay.Should().Contain("Ctrl");
        vm.HotkeyDisplay.Should().Contain("Shift");
    }

    [Fact]
    public void HotkeyDisplay_FromSettingsService_WithCustomHotkey_ShouldFormatCorrectly() {
        var service = CreateService(s => s.HotkeyConfig = "Alt+F4");

        var vm = new AboutViewModel(service);

        vm.HotkeyDisplay.Should().Be("Alt + F4");
    }

    [Fact]
    public void HotkeyDisplay_FromSettingsService_WithNoSettings_ShouldUseDefault() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns((Settings?)null);

        var vm = new AboutViewModel(service);

        vm.HotkeyDisplay.Should().NotBeNullOrWhiteSpace();
    }

    // ========== Helper methods ==========

    private static AboutViewModel CreateViewModel(string version = "1.0.0", string hotkey = "Ctrl+Shift+R")
        => new(version, hotkey);

    private static ISettingsService CreateService(Action<Settings>? configure = null) {
        var service = Substitute.For<ISettingsService>();
        var settings = new Settings { HotkeyConfig = "Ctrl+Shift+R" };
        configure?.Invoke(settings);
        service.Current.Returns(settings);
        return service;
    }
}
