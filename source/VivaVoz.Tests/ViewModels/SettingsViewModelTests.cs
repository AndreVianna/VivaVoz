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

        var act = () => new SettingsViewModel(null!, recorder, modelManager);

        act.Should().Throw<ArgumentNullException>().WithParameterName("settingsService");
    }

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrowArgumentNullException() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings());
        var modelManager = Substitute.For<IModelManager>();

        var act = () => new SettingsViewModel(service, null!, modelManager);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrowArgumentNullException() {
        var service = Substitute.For<ISettingsService>();
        service.Current.Returns(new Settings());
        var recorder = Substitute.For<IAudioRecorder>();

        var act = () => new SettingsViewModel(service, recorder, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelManager");
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow() {
        var (service, recorder, modelManager) = CreateDependencies();

        var act = () => new SettingsViewModel(service, recorder, modelManager);

        act.Should().NotThrow();
    }

    // ========== Initialization from settings ==========

    [Fact]
    public void Constructor_ShouldInitializeWhisperModelSizeFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.WhisperModelSize = "base");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.WhisperModelSize.Should().Be("base");
    }

    [Fact]
    public void Constructor_ShouldInitializeAudioInputDeviceFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.AudioInputDevice = "USB Mic");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.AudioInputDevice.Should().Be("USB Mic");
    }

    [Fact]
    public void Constructor_ShouldInitializeStoragePathFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.StoragePath = "/custom/path");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.StoragePath.Should().Be("/custom/path");
    }

    [Fact]
    public void Constructor_ShouldInitializeExportFormatFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.ExportFormat = "WAV");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.ExportFormat.Should().Be("WAV");
    }

    [Fact]
    public void Constructor_ShouldInitializeThemeFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.Theme = "Dark");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Theme.Should().Be("Dark");
    }

    [Fact]
    public void Constructor_ShouldInitializeLanguageFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.Language = "en");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Language.Should().Be("en");
    }

    [Fact]
    public void Constructor_ShouldInitializeRunAtStartupFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.RunAtStartup = true);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.RunAtStartup.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldInitializeMinimizeToTrayFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.MinimizeToTray = false);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.MinimizeToTray.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldInitializeHotkeyConfigFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.HotkeyConfig = "Ctrl+Alt+R");

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.HotkeyConfig.Should().Be("Ctrl+Alt+R");
    }

    [Fact]
    public void Constructor_ShouldInitializeRecordingModeFromSettings() {
        var (service, recorder, modelManager) = CreateDependencies(s => s.RecordingMode = "Push-to-Talk");

        var vm = new SettingsViewModel(service, recorder, modelManager);

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

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.WhisperModelSize.Should().Be("tiny");
        vm.Theme.Should().Be("System");
        vm.Language.Should().Be("auto");
    }

    // ========== AvailableDevices ==========

    [Fact]
    public void Constructor_ShouldPopulateAvailableDevicesFromRecorder() {
        var (service, recorder, modelManager) = CreateDependencies();
        recorder.GetAvailableDevices().Returns(["Mic A", "Mic B"]);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.AvailableDevices.Should().Equal("Mic A", "Mic B");
    }

    [Fact]
    public void Constructor_WhenNoDevices_ShouldHaveEmptyAvailableDevices() {
        var (service, recorder, modelManager) = CreateDependencies();
        recorder.GetAvailableDevices().Returns([]);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.AvailableDevices.Should().BeEmpty();
    }

    // ========== Models collection ==========

    [Fact]
    public void Constructor_ShouldPopulateModelsFromModelManager() {
        var (service, recorder, modelManager) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns(["tiny", "base", "small"]);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Models.Should().HaveCount(3);
        vm.Models.Select(m => m.ModelId).Should().Equal("tiny", "base", "small");
    }

    [Fact]
    public void Constructor_WhenNoModels_ShouldHaveEmptyModelsCollection() {
        var (service, recorder, modelManager) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns([]);

        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Models.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetIsInstalledOnModelItemsFromModelManager() {
        var (service, recorder, modelManager) = CreateDependencies();
        modelManager.GetAvailableModelIds().Returns(["tiny", "base"]);
        modelManager.IsModelDownloaded("tiny").Returns(true);
        modelManager.IsModelDownloaded("base").Returns(false);

        var vm = new SettingsViewModel(service, recorder, modelManager);

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
    public void WhisperModelSize_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.WhisperModelSize = "large";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.WhisperModelSize == "large"));
    }

    [Fact]
    public void AudioInputDevice_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.AudioInputDevice = "USB Mic";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.AudioInputDevice == "USB Mic"));
    }

    [Fact]
    public void StoragePath_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.StoragePath = "/new/path";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.StoragePath == "/new/path"));
    }

    [Fact]
    public void ExportFormat_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.ExportFormat = "OGG";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.ExportFormat == "OGG"));
    }

    [Fact]
    public void Theme_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Theme = "Dark";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.Theme == "Dark"));
    }

    [Fact]
    public void Language_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.Language = "fr";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.Language == "fr"));
    }

    [Fact]
    public void RunAtStartup_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.RunAtStartup = true;

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.RunAtStartup == true));
    }

    [Fact]
    public void MinimizeToTray_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.MinimizeToTray = false;

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.MinimizeToTray == false));
    }

    [Fact]
    public void HotkeyConfig_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.HotkeyConfig = "Ctrl+Shift+R";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.HotkeyConfig == "Ctrl+Shift+R"));
    }

    [Fact]
    public void RecordingMode_WhenChanged_ShouldCallSaveSettingsAsync() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        vm.RecordingMode = "Push-to-Talk";

        service.Received(1).SaveSettingsAsync(Arg.Is<Settings>(s => s.RecordingMode == "Push-to-Talk"));
    }

    // ========== No save during initialization ==========

    [Fact]
    public void Constructor_ShouldNotCallSaveSettingsAsyncDuringInitialization() {
        var (service, recorder, modelManager) = CreateDependencies();

        _ = new SettingsViewModel(service, recorder, modelManager);

        service.DidNotReceive().SaveSettingsAsync(Arg.Any<Settings>());
    }

    // ========== PropertyChanged notifications ==========

    [Fact]
    public void WhisperModelSize_WhenChanged_ShouldRaisePropertyChanged() {
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

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
        var (service, recorder, modelManager) = CreateDependencies();
        var vm = new SettingsViewModel(service, recorder, modelManager);

        var changed = new List<string>();
        vm.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        vm.Theme = "Light";

        changed.Should().Contain(nameof(SettingsViewModel.Theme));
    }

    // ========== Helper methods ==========

    private static (ISettingsService service, IAudioRecorder recorder, IModelManager modelManager) CreateDependencies(
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

        return (service, recorder, modelManager);
    }
}
