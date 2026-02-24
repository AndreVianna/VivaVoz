using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class GlobalHotkeyServiceTests {
    // ── Construction ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldNotThrow() {
        var act = () => new GlobalHotkeyService();

        act.Should().NotThrow();
    }

    // ── Initial state ──────────────────────────────────────────────────────────

    [Fact]
    public void IsRegistered_WhenNew_ShouldBeFalse() {
        var service = new GlobalHotkeyService();

        service.IsRegistered.Should().BeFalse();
    }

    [Fact]
    public void IsRecording_WhenNew_ShouldBeFalse() {
        var service = new GlobalHotkeyService();

        service.IsRecording.Should().BeFalse();
    }

    // ── Events ─────────────────────────────────────────────────────────────────

    [Fact]
    public void RecordingStartRequested_ShouldBeSubscribable() {
        var service = new GlobalHotkeyService();
        var raised = false;

        service.RecordingStartRequested += (_, _) => raised = true;

        raised.Should().BeFalse();
    }

    [Fact]
    public void RecordingStopRequested_ShouldBeSubscribable() {
        var service = new GlobalHotkeyService();
        var raised = false;

        service.RecordingStopRequested += (_, _) => raised = true;

        raised.Should().BeFalse();
    }

    // ── TryRegister guard clause ───────────────────────────────────────────────

    [Fact]
    public void TryRegister_WithNullConfig_ShouldReturnFalse() {
        var service = new GlobalHotkeyService();

        var result = service.TryRegister(null, "Toggle");

        result.Should().BeFalse();
    }

    [Fact]
    public void TryRegister_WithNullConfig_ShouldNotThrow() {
        var service = new GlobalHotkeyService();

        var act = () => service.TryRegister(null, "Toggle");

        act.Should().NotThrow();
    }

    [Fact]
    public void TryRegister_WithNullConfig_ShouldLeaveIsRegisteredFalse() {
        var service = new GlobalHotkeyService();

        service.TryRegister(null, "Toggle");

        service.IsRegistered.Should().BeFalse();
    }

    // ── Unregister guard clause ────────────────────────────────────────────────

    [Fact]
    public void Unregister_WhenNotRegistered_ShouldNotThrow() {
        var service = new GlobalHotkeyService();

        var act = service.Unregister;

        act.Should().NotThrow();
    }

    // ── Dispose ────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_WhenNotRegistered_ShouldNotThrow() {
        var service = new GlobalHotkeyService();

        var act = service.Dispose;

        act.Should().NotThrow();
    }

    // ── Toggle mode: HandleHotkeyDown ──────────────────────────────────────────

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenNotRecording_ShouldFireRecordingStartRequested() {
        var service = CreateToggleService();
        var startFired = false;
        service.RecordingStartRequested += (_, _) => startFired = true;

        service.HandleHotkeyDown();

        startFired.Should().BeTrue();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenNotRecording_ShouldNotFireRecordingStopRequested() {
        var service = CreateToggleService();
        var stopFired = false;
        service.RecordingStopRequested += (_, _) => stopFired = true;

        service.HandleHotkeyDown();

        stopFired.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenNotRecording_ShouldSetIsRecordingTrue() {
        var service = CreateToggleService();

        service.HandleHotkeyDown();

        service.IsRecording.Should().BeTrue();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenRecording_ShouldFireRecordingStopRequested() {
        var service = CreateToggleService();
        service.HandleHotkeyDown(); // start recording
        var stopFired = false;
        service.RecordingStopRequested += (_, _) => stopFired = true;

        service.HandleHotkeyDown(); // stop recording

        stopFired.Should().BeTrue();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenRecording_ShouldNotFireRecordingStartRequested() {
        var service = CreateToggleService();
        service.HandleHotkeyDown(); // start recording
        var startFired = false;
        service.RecordingStartRequested += (_, _) => startFired = true;

        service.HandleHotkeyDown(); // stop recording

        startFired.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleModeWhenRecording_ShouldSetIsRecordingFalse() {
        var service = CreateToggleService();
        service.HandleHotkeyDown(); // start recording

        service.HandleHotkeyDown(); // stop recording

        service.IsRecording.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyDown_InToggleMode_CalledThreeTimes_ShouldAlternateStartStop() {
        var service = CreateToggleService();
        var events = new List<string>();
        service.RecordingStartRequested += (_, _) => events.Add("start");
        service.RecordingStopRequested += (_, _) => events.Add("stop");

        service.HandleHotkeyDown(); // start
        service.HandleHotkeyDown(); // stop
        service.HandleHotkeyDown(); // start again

        events.Should().ContainInOrder("start", "stop", "start");
    }

    // ── Toggle mode: HandleHotkeyUp ────────────────────────────────────────────

    [Fact]
    public void HandleHotkeyUp_InToggleMode_ShouldNotFireRecordingStartRequested() {
        var service = CreateToggleService();
        var startFired = false;
        service.RecordingStartRequested += (_, _) => startFired = true;

        service.HandleHotkeyUp();

        startFired.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyUp_InToggleMode_ShouldNotFireRecordingStopRequested() {
        var service = CreateToggleService();
        var stopFired = false;
        service.RecordingStopRequested += (_, _) => stopFired = true;

        service.HandleHotkeyUp();

        stopFired.Should().BeFalse();
    }

    // ── Push-to-Talk mode: HandleHotkeyDown ───────────────────────────────────

    [Fact]
    public void HandleHotkeyDown_InPushToTalkMode_ShouldFireRecordingStartRequested() {
        var service = CreatePushToTalkService();
        var startFired = false;
        service.RecordingStartRequested += (_, _) => startFired = true;

        service.HandleHotkeyDown();

        startFired.Should().BeTrue();
    }

    [Fact]
    public void HandleHotkeyDown_InPushToTalkMode_ShouldNotFireRecordingStopRequested() {
        var service = CreatePushToTalkService();
        var stopFired = false;
        service.RecordingStopRequested += (_, _) => stopFired = true;

        service.HandleHotkeyDown();

        stopFired.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyDown_InPushToTalkMode_ShouldSetIsRecordingTrue() {
        var service = CreatePushToTalkService();

        service.HandleHotkeyDown();

        service.IsRecording.Should().BeTrue();
    }

    // ── Push-to-Talk mode: HandleHotkeyUp ─────────────────────────────────────

    [Fact]
    public void HandleHotkeyUp_InPushToTalkMode_ShouldFireRecordingStopRequested() {
        var service = CreatePushToTalkService();
        service.HandleHotkeyDown(); // press
        var stopFired = false;
        service.RecordingStopRequested += (_, _) => stopFired = true;

        service.HandleHotkeyUp(); // release

        stopFired.Should().BeTrue();
    }

    [Fact]
    public void HandleHotkeyUp_InPushToTalkMode_ShouldNotFireRecordingStartRequested() {
        var service = CreatePushToTalkService();
        service.HandleHotkeyDown(); // press
        var startFired = false;
        service.RecordingStartRequested += (_, _) => startFired = true;

        service.HandleHotkeyUp(); // release

        startFired.Should().BeFalse();
    }

    [Fact]
    public void HandleHotkeyUp_InPushToTalkMode_ShouldSetIsRecordingFalse() {
        var service = CreatePushToTalkService();
        service.HandleHotkeyDown(); // press

        service.HandleHotkeyUp(); // release

        service.IsRecording.Should().BeFalse();
    }

    // ── Mode switching ─────────────────────────────────────────────────────────

    [Fact]
    public void HandleHotkeyDown_WithUnknownMode_ShouldNotThrow() {
        var service = new GlobalHotkeyService {
            Mode = "Unknown"
        };

        var act = service.HandleHotkeyDown;

        act.Should().NotThrow();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static GlobalHotkeyService CreateToggleService() {
        var service = new GlobalHotkeyService {
            Mode = "Toggle"
        };
        return service;
    }

    private static GlobalHotkeyService CreatePushToTalkService() {
        var service = new GlobalHotkeyService {
            Mode = "Push-to-Talk"
        };
        return service;
    }
}
