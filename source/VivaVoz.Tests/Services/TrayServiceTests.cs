using Avalonia.Controls.ApplicationLifetimes;

using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services;

public class TrayServiceTests {
    // ========== TrayService.FormatTooltipText ==========

    [Fact]
    public void FormatTooltipText_WithNullTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(null);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithEmptyTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(string.Empty);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithShortTranscript_ShouldReturnFullText() {
        var result = TrayService.FormatTooltipText("Hello world");

        result.Should().Be("VivaVoz — Hello world");
    }

    [Fact]
    public void FormatTooltipText_WithLongTranscript_ShouldTruncateTo30Chars() {
        const string transcript = "This is a very long transcript that should be truncated";

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be("VivaVoz — This is a very long transcript...");
    }

    [Fact]
    public void FormatTooltipText_WithExactly30CharTranscript_ShouldNotTruncate() {
        var transcript = new string('a', 30); // exactly 30 chars

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be($"VivaVoz — {transcript}");
        result.Should().NotContain("...");
    }

    [Fact]
    public void FormatTooltipText_With31CharTranscript_ShouldTruncateWithEllipsis() {
        var transcript = new string('a', 31); // boundary: one over the limit

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be($"VivaVoz — {new string('a', 30)}...");
    }

    [Fact]
    public void FormatTooltipText_WithWhitespaceOnly_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText("   ");

        result.Should().Be("VivaVoz — No speech detected.");
    }

    // ========== TrayService.GetTooltipForState ==========

    [Fact]
    public void GetTooltipForState_WhenIdle_ShouldReturnIdleText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Idle);

        result.Should().Be("VivaVoz");
    }

    [Fact]
    public void GetTooltipForState_WhenRecording_ShouldReturnRecordingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Recording);

        result.Should().Be("VivaVoz — Recording...");
    }

    [Fact]
    public void GetTooltipForState_WhenTranscribing_ShouldReturnTranscribingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Transcribing);

        result.Should().Be("VivaVoz — Transcribing...");
    }

    [Fact]
    public void GetTooltipForState_WithInvalidEnumValue_ShouldReturnDefault() {
        const TrayIconState invalidState = (TrayIconState)999;

        var result = TrayService.GetTooltipForState(invalidState);

        result.Should().Be("VivaVoz");
    }

    // ========== Construction ==========

    [Fact]
    public void Constructor_WithNullDesktop_ShouldThrowArgumentNullException() {
        var recorder = Substitute.For<IAudioRecorder>();
        var transcriptionManager = Substitute.For<ITranscriptionManager>();

        var act = () => new TrayService(null!, recorder, transcriptionManager);

        act.Should().Throw<ArgumentNullException>().WithParameterName("desktop");
    }

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrowArgumentNullException() {
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        var transcriptionManager = Substitute.For<ITranscriptionManager>();

        var act = () => new TrayService(desktop, null!, transcriptionManager);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithNullTranscriptionManager_ShouldThrowArgumentNullException() {
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        var recorder = Substitute.For<IAudioRecorder>();

        var act = () => new TrayService(desktop, recorder, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("transcriptionManager");
    }

    [Fact]
    public void Constructor_WithValidArgs_ShouldNotThrow() {
        var act = () => CreateTrayService();

        act.Should().NotThrow();
    }

    // ========== Initial state ==========

    [Fact]
    public void CurrentState_WhenNew_ShouldBeIdle() {
        var service = CreateTrayService();

        service.CurrentState.Should().Be(TrayIconState.Idle);
    }

    [Fact]
    public void ActiveTranscriptions_WhenNew_ShouldBeZero() {
        var service = CreateTrayService();

        service.ActiveTranscriptions.Should().Be(0);
    }

    // ========== HandleRecordingStarted ==========

    [Fact]
    public void HandleRecordingStarted_ShouldSetStateToRecording() {
        var service = CreateTrayService();

        service.HandleRecordingStarted();

        service.CurrentState.Should().Be(TrayIconState.Recording);
    }

    [Fact]
    public void HandleRecordingStarted_ShouldNotThrow() {
        var service = CreateTrayService();

        var act = service.HandleRecordingStarted;

        act.Should().NotThrow();
    }

    // ========== HandleRecordingStopped ==========

    [Fact]
    public void HandleRecordingStopped_ShouldSetStateToTranscribing() {
        var service = CreateTrayService();

        service.HandleRecordingStopped();

        service.CurrentState.Should().Be(TrayIconState.Transcribing);
    }

    [Fact]
    public void HandleRecordingStopped_ShouldIncrementActiveTranscriptions() {
        var service = CreateTrayService();

        service.HandleRecordingStopped();

        service.ActiveTranscriptions.Should().Be(1);
    }

    [Fact]
    public void HandleRecordingStopped_CalledTwice_ShouldCountTwoActiveTranscriptions() {
        var service = CreateTrayService();

        service.HandleRecordingStopped();
        service.HandleRecordingStopped();

        service.ActiveTranscriptions.Should().Be(2);
    }

    // ========== HandleTranscriptionCompleted ==========

    [Fact]
    public void HandleTranscriptionCompleted_WhenLastTranscription_ShouldSetStateToIdle() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1

        service.HandleTranscriptionCompleted(false, null); // remaining = 0

        service.CurrentState.Should().Be(TrayIconState.Idle);
    }

    [Fact]
    public void HandleTranscriptionCompleted_WhenLastTranscription_ShouldResetActiveTranscriptionsToZero() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1

        service.HandleTranscriptionCompleted(false, null); // remaining = 0

        service.ActiveTranscriptions.Should().Be(0);
    }

    [Fact]
    public void HandleTranscriptionCompleted_WhenMoreTranscriptionsActive_ShouldRemainTranscribing() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1
        service.HandleRecordingStopped(); // active = 2

        service.HandleTranscriptionCompleted(false, null); // remaining = 1

        service.CurrentState.Should().Be(TrayIconState.Transcribing);
    }

    [Fact]
    public void HandleTranscriptionCompleted_WhenMoreTranscriptionsActive_ShouldDecrementActiveTranscriptions() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1
        service.HandleRecordingStopped(); // active = 2

        service.HandleTranscriptionCompleted(false, null); // remaining = 1

        service.ActiveTranscriptions.Should().Be(1);
    }

    [Fact]
    public void HandleTranscriptionCompleted_WithSuccess_ShouldNotThrow() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1

        var act = () => service.HandleTranscriptionCompleted(true, "Hello world");

        act.Should().NotThrow();
    }

    [Fact]
    public void HandleTranscriptionCompleted_WithFailure_ShouldNotThrow() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1

        var act = () => service.HandleTranscriptionCompleted(false, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void HandleTranscriptionCompleted_SequenceOfRecordingsAndTranscriptions_WithSuccess_ShouldSetStateToReady() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1
        service.HandleRecordingStopped(); // active = 2
        service.HandleTranscriptionCompleted(true, "first"); // remaining = 1 — stays Transcribing
        service.HandleTranscriptionCompleted(true, "second"); // remaining = 0 — goes to Ready (then reverts after 3s)

        // Immediately after completion, state should be Ready (timer will revert to Idle in 3 seconds)
        service.CurrentState.Should().Be(TrayIconState.Ready);
        service.ActiveTranscriptions.Should().Be(0);
    }

    [Fact]
    public void HandleTranscriptionCompleted_SequenceWithFailure_ShouldReturnToIdle() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1
        service.HandleTranscriptionCompleted(false, null); // remaining = 0, failure → Idle

        service.CurrentState.Should().Be(TrayIconState.Idle);
        service.ActiveTranscriptions.Should().Be(0);
    }

    // ========== HandleTranscriptionCompleted — Ready state ==========

    [Fact]
    public void HandleTranscriptionCompleted_WhenSuccessAndLastTranscription_ShouldSetStateToReady() {
        var service = CreateTrayService();
        service.HandleRecordingStopped(); // active = 1

        service.HandleTranscriptionCompleted(true, "hello"); // success, remaining = 0

        service.CurrentState.Should().Be(TrayIconState.Ready);
    }

    // ========== SetState — Ready ==========

    [Fact]
    public void SetState_WithReady_ShouldSetReadyState() {
        var service = CreateTrayService();

        service.SetState(TrayIconState.Ready);

        service.CurrentState.Should().Be(TrayIconState.Ready);
    }

    // ========== SetStateTemporary ==========

    [Fact]
    public void SetStateTemporary_ShouldImmediatelySetState() {
        var service = CreateTrayService();

        service.SetStateTemporary(TrayIconState.Ready, TimeSpan.FromSeconds(60));

        service.CurrentState.Should().Be(TrayIconState.Ready);
    }

    [Fact]
    public async Task SetStateTemporary_ShouldRevertToIdleAfterDuration() {
        var service = CreateTrayService();

        service.SetStateTemporary(TrayIconState.Ready, TimeSpan.FromMilliseconds(30));
        service.CurrentState.Should().Be(TrayIconState.Ready);

        await Task.Delay(150);

        service.CurrentState.Should().Be(TrayIconState.Idle);
    }

    // ========== GetTooltipForState — Ready ==========

    [Fact]
    public void GetTooltipForState_WhenReady_ShouldReturnReadyText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Ready);

        result.Should().Be("VivaVoz — Transcript ready!");
    }

    // ========== ShouldShowTranscriptionNotification ==========

    [Fact]
    public void ShouldShowTranscriptionNotification_WithNullWindow_ShouldReturnFalse() {
        var result = TrayService.ShouldShowTranscriptionNotification(null);

        result.Should().BeFalse();
    }

    // ========== Dispose ==========

    [Fact]
    public void Dispose_ShouldNotThrow() {
        var service = CreateTrayService();

        var act = service.Dispose;

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldNotThrow() {
        var service = CreateTrayService();
        service.Dispose();

        var act = service.Dispose;

        act.Should().NotThrow();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static TrayService CreateTrayService() {
        var desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        var recorder = Substitute.For<IAudioRecorder>();
        var transcriptionManager = Substitute.For<ITranscriptionManager>();
        return new TrayService(desktop, recorder, transcriptionManager);
    }
}
