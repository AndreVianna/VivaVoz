using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class SettingsViewModelTests {
    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullSettingsService_ShouldThrowArgumentNullException() {
        var recorder = Substitute.For<IAudioRecorder>();
        var modelManager = Substitute.For<IModelManager>();
        var themeService = Substitute.For<IThemeService>();

        var act = () => new SettingsViewModel(null!, recorder, modelManager, themeService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("settingsService");
    }

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrowArgumentNullException() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings());
        var modelManager = Substitute.For<IModelManager>();
        var themeService = Substitute.For<IThemeService>();

        var act = () => new SettingsViewModel(service, null!, modelManager, themeService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrowArgumentNullException() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings());
        var recorder = Substitute.For<IAudioRecorder>();
        var themeService = Substitute.For<IThemeService>();

        var act = () => new SettingsViewModel(service, recorder, null!, themeService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelManager");
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();

        var act = () => new SettingsViewModel(service, recorder, modelManager, themeService);

        act.Should().NotThrow();
    }

    // ========== Initialization from settings ==========

    [Fact]
    public void Constructor_ShouldInitializeWhisperModelSizeFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.WhisperModelSize = "base");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.WhisperModelSize.Should().Be("base");
    }

    [Fact]
    public void Constructor_ShouldInitializeAudioInputDeviceFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.AudioInputDevice = "USB Mic");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.AudioInputDevice.Should().Be("USB Mic");
    }

    [Fact]
    public void Constructor_ShouldInitializeStoragePathFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.StoragePath = "/custom/path");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.StoragePath.Should().Be("/custom/path");
    }

    [Fact]
    public void Constructor_ShouldInitializeExportFormatFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.ExportFormat = "WAV");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.ExportFormat.Should().Be("WAV");
    }

    [Fact]
    public void Constructor_ShouldInitializeThemeFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.Theme = "Dark");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.Theme.Should().Be("Dark");
    }

    [Fact]
    public void Constructor_ShouldInitializeLanguageFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.Language = "en");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.Language.Should().Be("en");
    }

    [Fact]
    public void Constructor_ShouldInitializeRunAtStartupFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.RunAtStartup = true);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.RunAtStartup.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldInitializeMinimizeToTrayFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.MinimizeToTray = false);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.MinimizeToTray.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldInitializeHotkeyConfigFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.HotkeyConfig = "Ctrl+Alt+R");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.HotkeyConfig.Should().Be("Ctrl+Alt+R");
    }

    [Fact]
    public void Constructor_ShouldInitializeRecordingModeFromSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies(s => s.RecordingMode = "Push-to-Talk");

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.RecordingMode.Should().Be("Push-to-Talk");
    }

    [Fact]
    public void Constructor_WhenCurrentSettingsIsNull_ShouldUseDefaults() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns((Settings?)null);
        var recorder = Substitute.For<IAudioRecorder>();
        recorder.GetAvailableDevices().Returns([]);
        var modelManager = Substitute.For<IModelManager>();
        modelManager.GetAvailableModelIds().Returns([]);
        var themeService = Substitute.For<IThemeService>();

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.WhisperModelSize.Should().Be("tiny");
        vm.Theme.Should().Be("System");
        vm.Language.Should().Be("auto");
    }

    // ========== AvailableDevices ==========

    [Fact]
    public void Constructor_ShouldPopulateAvailableDevicesFromRecorder() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        recorder.GetAvailableDevices().Returns(["Mic A", "Mic B"]);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.AvailableDevices.Should().Equal("Mic A", "Mic B");
    }

    [Fact]
    public void Constructor_WhenNoDevices_ShouldHaveEmptyAvailableDevices() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        recorder.GetAvailableDevices().Returns([]);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);
        vm.AvailableDevices.Should().BeEmpty();
    }

    // ========== Models collection ==========

    [Fact]
    public void Constructor_ShouldPopulateModelsFromModelManager() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns(["tiny", "base", "small"]);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.Models.Should().HaveCount(3);
        vm.Models.Select(m => m.ModelId).Should().Equal("tiny", "base", "small");
    }

    [Fact]
    public void Constructor_WhenNoModels_ShouldHaveEmptyModelsCollection() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns([]);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.Models.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetIsInstalledOnModelItemsFromModelManager() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns(["tiny", "base"]);
        modelManager.IsModelDownloaded("tiny").Returns(true);
        modelManager.IsModelDownloaded("base").Returns(false);

        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        vm.Models.First(m => m.ModelId == "tiny").IsInstalled.Should().BeTrue();
        vm.Models.First(m => m.ModelId == "base").IsInstalled.Should().BeFalse();
    }

    // ========== Static option lists ==========

    [Fact]
    public void AvailableThemes_ShouldContainSystemLightDark() {
        SettingsViewModel.AvailableThemes.Should().Contain("System");
        SettingsViewModel.AvailableThemes.Should().Contain("Light");
        SettingsViewModel.AvailableThemes.Should().Contain("Dark");
    }

    [Fact]
    public void AvailableModels_ShouldContainWhisperSizes() {
        SettingsViewModel.AvailableModels.Should().Contain("tiny");
        SettingsViewModel.AvailableModels.Should().Contain("base");
        SettingsViewModel.AvailableModels.Should().Contain("large");
    }

    [Fact]
    public void AvailableExportFormats_ShouldContainExpectedFormats() {
        SettingsViewModel.AvailableExportFormats.Should().Contain("MP3");
        SettingsViewModel.AvailableExportFormats.Should().Contain("WAV");
        SettingsViewModel.AvailableExportFormats.Should().Contain("OGG");
    }

    [Fact]
    public void AvailableRecordingModes_ShouldContainToggleAndPushToTalk() {
        SettingsViewModel.AvailableRecordingModes.Should().Contain("Toggle");
        SettingsViewModel.AvailableRecordingModes.Should().Contain("Push-to-Talk");
    }

    [Fact]
    public void AvailableLanguages_ShouldContainAutoAndCommonLanguages() {
        SettingsViewModel.AvailableLanguages.Should().Contain("auto");
        SettingsViewModel.AvailableLanguages.Should().Contain("en");
    }

    // ========== Property change triggers save ==========

    [Fact]
    public void WhisperModelSize_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            WhisperModelSize = "large"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.WhisperModelSize == "large"));
    }

    [Fact]
    public void AudioInputDevice_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            AudioInputDevice = "USB Mic"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.AudioInputDevice == "USB Mic"));
    }

    [Fact]
    public void StoragePath_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            StoragePath = "/new/path"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.StoragePath == "/new/path"));
    }

    [Fact]
    public void ExportFormat_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            ExportFormat = "OGG"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.ExportFormat == "OGG"));
    }

    [Fact]
    public void Theme_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            Theme = "Dark"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.Theme == "Dark"));
    }

    [Fact]
    public void Language_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            Language = "fr"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.Language == "fr"));
    }

    [Fact]
    public void RunAtStartup_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            RunAtStartup = true
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.RunAtStartup));
    }

    [Fact]
    public void MinimizeToTray_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            MinimizeToTray = false
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => !s.MinimizeToTray));
    }

    [Fact]
    public void HotkeyConfig_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            HotkeyConfig = "Ctrl+Shift+R"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.HotkeyConfig == "Ctrl+Shift+R"));
    }

    [Fact]
    public void RecordingMode_WhenChanged_ShouldCallSaveSettings() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService) {
            RecordingMode = "Push-to-Talk"
        };

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.RecordingMode == "Push-to-Talk"));
    }

    // ========== No save during initialization ==========

    [Fact]
    public void Constructor_ShouldNotCallSaveSettingsAsyncDuringInitialization() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();

        _ = new SettingsViewModel(service, recorder, modelManager, themeService);

        service.DidNotReceive().SaveSettingsAsync(Arg.Any<Settings>());
    }

    // ========== PropertyChanged notifications ==========

    [Fact]
    public void WhisperModelSize_WhenChanged_ShouldRaisePropertyChanged() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        var changed = new List<string>();
        vm.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        vm.WhisperModelSize = "medium";

        changed.Should().Contain(nameof(SettingsViewModel.WhisperModelSize));
    }

    [Fact]
    public void Theme_WhenChanged_ShouldRaisePropertyChanged() {
        var (service, recorder, modelManager, themeService) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager, themeService);

        var changed = new List<string>();
        vm.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        vm.Theme = "Light";

        changed.Should().Contain(nameof(SettingsViewModel.Theme));
    }

    // ========== Helper methods ==========

    private static (ISettingsService service, IAudioRecorder recorder, IModelManager modelManager, IThemeService themeService) CreateDependencies(
        Action<Settings>? configure = null) {
        var settings = new Settings();
        configure?.Invoke(settings);

        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(settings);
        service.SaveSettingsAsync(Arg.Any<Settings>()).Returns(Task.CompletedTask);

        var recorder = Substitute.For<IAudioRecorder>();
        recorder.GetAvailableDevices().Returns([]);

        var modelManager = Substitute.For<IModelManager>();
        modelManager.GetAvailableModelIds().Returns([]);

        var themeService = Substitute.For<IThemeService>();

        return (service, recorder, modelManager, themeService);
    }
}
