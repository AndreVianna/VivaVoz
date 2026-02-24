using AwesomeAssertions;

using VivaVoz.Services.Audio;

using Xunit;

namespace VivaVoz.Tests.Services.Audio;

public class AudioPlayerServiceTests {
    [Fact]
    public void IsPlaying_WhenNewInstance_ShouldBeFalse() {
        var service = new AudioPlayerService();

        service.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void CurrentPosition_WhenNewInstance_ShouldBeZero() {
        var service = new AudioPlayerService();

        service.CurrentPosition.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void TotalDuration_WhenNewInstance_ShouldBeZero() {
        var service = new AudioPlayerService();

        service.TotalDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Stop_WhenNotPlaying_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = service.Stop;

        act.Should().NotThrow();
    }

    [Fact]
    public void Pause_WhenNotPlaying_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = service.Pause;

        act.Should().NotThrow();
    }

    [Fact]
    public void Play_WithEmptyPath_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = () => service.Play(string.Empty);

        act.Should().NotThrow();
    }

    [Fact]
    public void Play_WithWhitespacePath_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = () => service.Play("   ");

        act.Should().NotThrow();
    }

    [Fact]
    public void Play_WithNonExistentFile_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = () => service.Play(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid()}.wav"));

        act.Should().NotThrow();
    }

    [Fact]
    public void Play_WithNonExistentFile_ShouldNotSetIsPlaying() {
        var service = new AudioPlayerService();

        service.Play(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid()}.wav"));

        service.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void Seek_WhenNoReaderInitialized_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = () => service.Seek(TimeSpan.FromSeconds(5));

        act.Should().NotThrow();
    }

    [Fact]
    public void Seek_WithNegativePosition_ShouldNotThrow() {
        var service = new AudioPlayerService();

        var act = () => service.Seek(TimeSpan.FromSeconds(-1));

        act.Should().NotThrow();
    }

    [Fact]
    public void Stop_WhenCalledMultipleTimes_ShouldNotThrow() {
        var service = new AudioPlayerService();

        service.Stop();
        var act = service.Stop;

        act.Should().NotThrow();
    }

    [Fact]
    public void PlaybackStopped_Event_ShouldBeSubscribable() {
        var service = new AudioPlayerService();
        var eventRaised = false;

        service.PlaybackStopped += (_, _) => eventRaised = true;

        eventRaised.Should().BeFalse();
    }
}
