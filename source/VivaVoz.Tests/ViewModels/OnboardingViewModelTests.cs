using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class OnboardingViewModelTests {

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (ISettingsService settings, IModelManager models, IAudioRecorder recorder, ITranscriptionEngine engine)
        CreateDependencies(Settings? settings = null) {
        var settingsService = Substitute.For<ISettingsService>();
        settingsService.Current.Returns(settings ?? new Settings { HotkeyConfig = "Ctrl+Shift+R" });

        var modelManager = Substitute.For<IModelManager>();
        modelManager.GetAvailableModelIds().Returns(["tiny", "base", "small"]);

        var recorder = Substitute.For<IAudioRecorder>();
        var engine = Substitute.For<ITranscriptionEngine>();

        return (settingsService, modelManager, recorder, engine);
    }

    private static OnboardingViewModel CreateVm(Settings? settings = null) {
        var (s, m, r, e) = CreateDependencies(settings);
        return new OnboardingViewModel(s, m, r, e);
    }

    // ── Constructor null-guard tests ──────────────────────────────────────────

    [Fact]
    public void Constructor_WithNullSettingsService_ShouldThrow() {
        var (_, m, r, e) = CreateDependencies();

        var act = () => new OnboardingViewModel(null!, m, r, e);

        act.Should().Throw<ArgumentNullException>().WithParameterName("settingsService");
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrow() {
        var (s, _, r, e) = CreateDependencies();

        var act = () => new OnboardingViewModel(s, null!, r, e);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelManager");
    }

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrow() {
        var (s, m, _, e) = CreateDependencies();

        var act = () => new OnboardingViewModel(s, m, null!, e);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithNullTranscriptionEngine_ShouldThrow() {
        var (s, m, r, _) = CreateDependencies();

        var act = () => new OnboardingViewModel(s, m, r, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("transcriptionEngine");
    }

    [Fact]
    public void Constructor_WithValidArgs_ShouldNotThrow() {
        var act = () => CreateVm();

        act.Should().NotThrow();
    }

    // ── Initial state tests ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldStartOnStep1() {
        var vm = CreateVm();

        vm.CurrentStep.Should().Be(0);
        vm.IsStep1.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldLoadHotkeyFromSettings() {
        var vm = CreateVm(new Settings { HotkeyConfig = "Ctrl+Alt+V" });

        vm.HotkeyConfig.Should().Be("Ctrl+Alt+V");
    }

    [Fact]
    public void Constructor_ShouldBuildModelList() {
        var vm = CreateVm();

        vm.Models.Should().HaveCount(3);
    }

    [Fact]
    public void Constructor_IsFirstStep_ShouldBeTrue() {
        var vm = CreateVm();

        vm.IsFirstStep.Should().BeTrue();
    }

    [Fact]
    public void Constructor_IsLastStep_ShouldBeFalse() {
        var vm = CreateVm();

        vm.IsLastStep.Should().BeFalse();
    }

    [Fact]
    public void Constructor_CanGoNext_ShouldBeTrue() {
        var vm = CreateVm();

        vm.CanGoNext.Should().BeTrue();
    }

    [Fact]
    public void Constructor_CanGoPrevious_ShouldBeFalse() {
        var vm = CreateVm();

        vm.CanGoPrevious.Should().BeFalse();
    }

    [Fact]
    public void Constructor_CurrentStepDisplay_ShouldBeOne() {
        var vm = CreateVm();

        vm.CurrentStepDisplay.Should().Be(1);
    }

    // ── Navigation tests ──────────────────────────────────────────────────────

    [Fact]
    public void NextCommand_ShouldAdvanceStep() {
        var vm = CreateVm();

        vm.NextCommand.Execute(null);

        vm.CurrentStep.Should().Be(1);
    }

    [Fact]
    public void NextCommand_WhenOnStep2_ShouldShowStep3() {
        var vm = CreateVm();
        vm.NextCommand.Execute(null); // → step 2

        vm.NextCommand.Execute(null); // → step 3

        vm.IsStep3.Should().BeTrue();
    }

    [Fact]
    public void NextCommand_WhenOnLastStep_CanGoNextShouldBeFalse() {
        var vm = CreateVm();
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null); // → step 4 (index 3)

        vm.CanGoNext.Should().BeFalse();
    }

    [Fact]
    public void PreviousCommand_WhenOnStep2_ShouldGoBackToStep1() {
        var vm = CreateVm();
        vm.NextCommand.Execute(null); // → step 2

        vm.PreviousCommand.Execute(null);

        vm.CurrentStep.Should().Be(0);
        vm.IsStep1.Should().BeTrue();
    }

    [Fact]
    public void PreviousCommand_WhenOnStep1_ShouldNotGoBelow0() {
        var vm = CreateVm();

        // PreviousCommand is disabled on step 1, so it should not change state
        vm.CanGoPrevious.Should().BeFalse();
        vm.CurrentStep.Should().Be(0);
    }

    [Fact]
    public void Step_WhenAdvancedToStep4_IsLastStepShouldBeTrue() {
        var vm = CreateVm();
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null);

        vm.IsLastStep.Should().BeTrue();
        vm.IsStep4.Should().BeTrue();
    }

    [Fact]
    public void Step_Transitions_IsStepPropertiesShouldReflectCurrentStep() {
        var vm = CreateVm();

        vm.IsStep1.Should().BeTrue();
        vm.IsStep2.Should().BeFalse();
        vm.IsStep3.Should().BeFalse();
        vm.IsStep4.Should().BeFalse();

        vm.NextCommand.Execute(null);

        vm.IsStep1.Should().BeFalse();
        vm.IsStep2.Should().BeTrue();
        vm.IsStep3.Should().BeFalse();
        vm.IsStep4.Should().BeFalse();

        vm.NextCommand.Execute(null);

        vm.IsStep1.Should().BeFalse();
        vm.IsStep2.Should().BeFalse();
        vm.IsStep3.Should().BeTrue();
        vm.IsStep4.Should().BeFalse();

        vm.NextCommand.Execute(null);

        vm.IsStep1.Should().BeFalse();
        vm.IsStep2.Should().BeFalse();
        vm.IsStep3.Should().BeFalse();
        vm.IsStep4.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    public void CurrentStepDisplay_ShouldAlwaysBeCurrentStepPlusOne(int step, int expectedDisplay) {
        var vm = CreateVm();
        for (var i = 0; i < step; i++)
            vm.NextCommand.Execute(null);

        vm.CurrentStepDisplay.Should().Be(expectedDisplay);
    }

    // ── Hotkey tests ──────────────────────────────────────────────────────────

    [Fact]
    public void HotkeyDisplayText_WhenNotListening_ShouldFormatHotkey() {
        var vm = CreateVm(new Settings { HotkeyConfig = "Ctrl+Shift+R" });

        vm.HotkeyDisplayText.Should().Be("Ctrl + Shift + R");
    }

    [Fact]
    public void HotkeyDisplayText_WhenListening_ShouldShowPrompt() {
        var vm = CreateVm();

        vm.StartSetHotkeyCommand.Execute(null);

        vm.HotkeyDisplayText.Should().Be("Press combination...");
    }

    [Fact]
    public void HotkeyDisplayText_WhenHotkeyEmpty_ShouldShowDefault() {
        var vm = CreateVm(new Settings { HotkeyConfig = "" });

        vm.HotkeyDisplayText.Should().Be("Ctrl + Shift + R");
    }

    [Fact]
    public void AcceptHotkeyCapture_ShouldUpdateHotkeyAndStopListening() {
        var vm = CreateVm();
        vm.StartSetHotkeyCommand.Execute(null);
        vm.IsListeningForHotkey.Should().BeTrue();

        vm.AcceptHotkeyCapture("Ctrl+Alt+R");

        vm.HotkeyConfig.Should().Be("Ctrl+Alt+R");
        vm.IsListeningForHotkey.Should().BeFalse();
    }

    // ── Finish command tests ──────────────────────────────────────────────────

    [Fact]
    public async Task FinishCommand_ShouldSetHasCompletedOnboardingToTrue() {
        Settings? savedSettings = null;
        var (settingsService, models, recorder, engine) = CreateDependencies();
        settingsService
            .When(s => s.SaveSettingsAsync(Arg.Any<Settings>()))
            .Do(ci => savedSettings = ci.Arg<Settings>());

        var vm = new OnboardingViewModel(settingsService, models, recorder, engine);

        // Navigate to last step then finish
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null);
        vm.NextCommand.Execute(null);
        await vm.FinishCommand.ExecuteAsync(null);

        savedSettings.Should().NotBeNull();
        savedSettings!.HasCompletedOnboarding.Should().BeTrue();
    }

    [Fact]
    public async Task FinishCommand_ShouldPersistHotkeyConfig() {
        Settings? savedSettings = null;
        var (settingsService, models, recorder, engine) = CreateDependencies();
        settingsService
            .When(s => s.SaveSettingsAsync(Arg.Any<Settings>()))
            .Do(ci => savedSettings = ci.Arg<Settings>());

        var vm = new OnboardingViewModel(settingsService, models, recorder, engine);
        vm.AcceptHotkeyCapture("Ctrl+Alt+V");

        await vm.FinishCommand.ExecuteAsync(null);

        savedSettings!.HotkeyConfig.Should().Be("Ctrl+Alt+V");
    }

    [Fact]
    public async Task FinishCommand_ShouldRaiseWizardCompletedEvent() {
        var (settingsService, models, recorder, engine) = CreateDependencies();
        var vm = new OnboardingViewModel(settingsService, models, recorder, engine);

        var eventRaised = false;
        vm.WizardCompleted += (_, _) => eventRaised = true;

        await vm.FinishCommand.ExecuteAsync(null);

        eventRaised.Should().BeTrue();
    }

    [Fact]
    public async Task FinishCommand_ShouldCallSaveSettingsAsync() {
        var (settingsService, models, recorder, engine) = CreateDependencies();
        var vm = new OnboardingViewModel(settingsService, models, recorder, engine);

        await vm.FinishCommand.ExecuteAsync(null);

        await settingsService.Received(1).SaveSettingsAsync(Arg.Any<Settings>());
    }

    // ── IsListeningForHotkey tests ────────────────────────────────────────────

    [Fact]
    public void StartSetHotkeyCommand_ShouldSetIsListeningToTrue() {
        var vm = CreateVm();

        vm.StartSetHotkeyCommand.Execute(null);

        vm.IsListeningForHotkey.Should().BeTrue();
    }

    // ── Cleanup tests ─────────────────────────────────────────────────────────

    [Fact]
    public void Cleanup_ShouldNotThrow() {
        var vm = CreateVm();

        var act = vm.Cleanup;

        act.Should().NotThrow();
    }
}
