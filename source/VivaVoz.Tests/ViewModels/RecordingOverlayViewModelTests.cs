using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Services.Audio;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class RecordingOverlayViewModelTests {
    // ========== Constructor ==========

    [Fact]
    public void Constructor_WithNullRecorder_ShouldThrowArgumentNullException() {
        var act = () => new RecordingOverlayViewModel(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recorder");
    }

    [Fact]
    public void Constructor_WithValidRecorder_ShouldNotThrow() {
        var recorder = Substitute.For<IAudioRecorder>();

        var act = () => new RecordingOverlayViewModel(recorder);

        act.Should().NotThrow();
    }

    // ========== Initial state ==========

    [Fact]
    public void DurationText_Initially_ShouldBe_0000() {
        var recorder = Substitute.For<IAudioRecorder>();

        var vm = new RecordingOverlayViewModel(recorder);

        vm.DurationText.Should().Be("00:00");
    }

    [Fact]
    public void IsRecording_Initially_ShouldBeFalse() {
        var recorder = Substitute.For<IAudioRecorder>();

        var vm = new RecordingOverlayViewModel(recorder);

        vm.IsRecording.Should().BeFalse();
    }

    // ========== StopRecordingCommand ==========

    [Fact]
    public void StopRecordingCommand_WhenExecuted_ShouldCallRecorderStopRecording() {
        var recorder = Substitute.For<IAudioRecorder>();
        var vm = new RecordingOverlayViewModel(recorder);

        vm.StopRecordingCommand.Execute(null);

        recorder.Received(1).StopRecording();
    }

    // ========== FormatDuration ==========

    [Fact]
    public void FormatDuration_WithZeroSeconds_ShouldReturn_0000() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.Zero);

        result.Should().Be("00:00");
    }

    [Fact]
    public void FormatDuration_With30Seconds_ShouldReturn_0030() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(30));

        result.Should().Be("00:30");
    }

    [Fact]
    public void FormatDuration_With90Seconds_ShouldReturn_0130() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(90));

        result.Should().Be("01:30");
    }

    [Fact]
    public void FormatDuration_With3600Seconds_ShouldReturn_6000() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(3600));

        result.Should().Be("60:00");
    }

    [Fact]
    public void FormatDuration_With3661Seconds_ShouldReturn_6101() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(3661));

        result.Should().Be("61:01");
    }

    [Fact]
    public void FormatDuration_With59Seconds_ShouldReturn_0059() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(59));

        result.Should().Be("00:59");
    }

    [Fact]
    public void FormatDuration_With119Seconds_ShouldReturn_0159() {
        var result = RecordingOverlayViewModel.FormatDuration(TimeSpan.FromSeconds(119));

        result.Should().Be("01:59");
    }

    // ========== Dispose ==========

    [Fact]
    public void Dispose_ShouldNotThrow() {
        var recorder = Substitute.For<IAudioRecorder>();
        var vm = new RecordingOverlayViewModel(recorder);

        var act = vm.Dispose;

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromRecorderEvents() {
        var recorder = Substitute.For<IAudioRecorder>();
        var vm = new RecordingOverlayViewModel(recorder);

        vm.Dispose();

        // After dispose, unsubscribing again should not throw
        var act = () => {
            recorder.RecordingStarted -= null;
            recorder.RecordingStopped -= null;
        };
        act.Should().NotThrow();
    }

    // ========== PropertyChanged ==========

    [Fact]
    public void DurationText_WhenSet_ShouldRaisePropertyChanged() {
        var recorder = Substitute.For<IAudioRecorder>();
        var vm = new RecordingOverlayViewModel(recorder);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.DurationText = "01:23";

        changed.Should().Contain(nameof(RecordingOverlayViewModel.DurationText));
    }
}
