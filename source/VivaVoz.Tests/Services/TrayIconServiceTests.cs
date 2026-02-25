using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class TrayIconServiceTests {
    // ========== Construction ==========

    [Fact]
    public void Constructor_WithNoCallback_ShouldNotThrow() {
        var act = () => new TrayIconService();

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithCallback_ShouldNotThrow() {
        var act = () => new TrayIconService(_ => { });

        act.Should().NotThrow();
    }

    // ========== Initial state ==========

    [Fact]
    public void CurrentState_WhenNew_ShouldBeIdle() {
        var service = new TrayIconService();

        service.CurrentState.Should().Be(AppState.Idle);
    }

    // ========== SetState ==========

    [Fact]
    public void SetState_WithRecording_ShouldUpdateCurrentState() {
        var service = new TrayIconService();

        service.SetState(AppState.Recording);

        service.CurrentState.Should().Be(AppState.Recording);
    }

    [Fact]
    public void SetState_WithTranscribing_ShouldUpdateCurrentState() {
        var service = new TrayIconService();

        service.SetState(AppState.Transcribing);

        service.CurrentState.Should().Be(AppState.Transcribing);
    }

    [Fact]
    public void SetState_WithIdle_ShouldUseIdleState() {
        var service = new TrayIconService();
        service.SetState(AppState.Recording);

        service.SetState(AppState.Idle);

        service.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void SetState_WithReady_ShouldUpdateCurrentState() {
        var service = new TrayIconService();

        service.SetState(AppState.Ready);

        service.CurrentState.Should().Be(AppState.Ready);
    }

    [Fact]
    public void SetState_ShouldInvokeCallback() {
        var received = new List<AppState>();
        var service = new TrayIconService(state => received.Add(state));

        service.SetState(AppState.Recording);
        service.SetState(AppState.Transcribing);

        received.Should().Equal(AppState.Recording, AppState.Transcribing);
    }

    // ========== SetStateTemporary ==========

    [Fact]
    public void SetStateTemporary_ShouldImmediatelySetState() {
        var service = new TrayIconService();

        service.SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(10));

        service.CurrentState.Should().Be(AppState.Ready);
    }

    [Fact]
    public void SetStateTemporary_ShouldInvokeCallbackImmediately() {
        var received = new List<AppState>();
        var service = new TrayIconService(state => received.Add(state));

        service.SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(10));

        received.Should().ContainSingle().Which.Should().Be(AppState.Ready);
    }

    [Fact]
    public async Task SetStateTemporary_WithReady_ShouldRevertToIdleAfterDuration() {
        var service = new TrayIconService();

        service.SetStateTemporary(AppState.Ready, TimeSpan.FromMilliseconds(30));

        // Should be Ready immediately
        service.CurrentState.Should().Be(AppState.Ready);

        // After duration elapses it should have reverted.
        // Use 500 ms to avoid false failures under thread-pool load during full suite runs.
        await Task.Delay(500);
        service.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void SetState_WhenCalledDuringTemporary_ShouldCancelRevert() {
        var service = new TrayIconService();

        // Start a long-lived temporary state
        service.SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(60));
        service.CurrentState.Should().Be(AppState.Ready);

        // Override with a permanent state change
        service.SetState(AppState.Recording);

        service.CurrentState.Should().Be(AppState.Recording);
    }

    [Fact]
    public async Task SetStateTemporary_WhenCalledDuringExisting_ShouldCancelPreviousRevertAndStartNew() {
        var service = new TrayIconService();

        // First temporary — would revert in 60 seconds
        service.SetStateTemporary(AppState.Ready, TimeSpan.FromSeconds(60));

        // Second temporary — very short
        service.SetStateTemporary(AppState.Ready, TimeSpan.FromMilliseconds(30));

        await Task.Delay(150);

        // Should have reverted from the SECOND timer (not the first)
        service.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public async Task SetState_CalledDuringTemporary_ShouldPreventsRevertToIdle() {
        var service = new TrayIconService();

        service.SetStateTemporary(AppState.Ready, TimeSpan.FromMilliseconds(30));

        // Immediately override — cancel the revert
        service.SetState(AppState.Recording);

        await Task.Delay(150);

        // The temporary revert was cancelled, so we stay in Recording
        service.CurrentState.Should().Be(AppState.Recording);
    }
}
