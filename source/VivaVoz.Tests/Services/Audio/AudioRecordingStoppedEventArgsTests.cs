using AwesomeAssertions;

using VivaVoz.Services.Audio;

using Xunit;

namespace VivaVoz.Tests.Services.Audio;

public class AudioRecordingStoppedEventArgsTests {
    [Fact]
    public void FilePath_ShouldReturnValuePassedToConstructor() {
        var args = new AudioRecordingStoppedEventArgs("path.wav", TimeSpan.FromSeconds(1));

        args.FilePath.Should().Be("path.wav");
    }

    [Fact]
    public void Duration_ShouldReturnValuePassedToConstructor() {
        var duration = TimeSpan.FromSeconds(5);

        var args = new AudioRecordingStoppedEventArgs("path.wav", duration);

        args.Duration.Should().Be(duration);
    }

    [Fact]
    public void Instance_ShouldInheritFromEventArgs() {
        var args = new AudioRecordingStoppedEventArgs("path.wav", TimeSpan.FromSeconds(1));

        args.Should().BeAssignableTo<EventArgs>();
    }
}
