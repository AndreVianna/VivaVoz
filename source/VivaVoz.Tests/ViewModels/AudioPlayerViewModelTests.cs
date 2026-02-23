using AwesomeAssertions;

using NSubstitute;

using VivaVoz.Models;
using VivaVoz.Services.Audio;
using VivaVoz.ViewModels;

using Xunit;

namespace VivaVoz.Tests.ViewModels;

public class AudioPlayerViewModelTests {
    [Fact]
    public void Constructor_WithNullAudioPlayer_ShouldThrowArgumentNullException() {
        var act = () => new AudioPlayerViewModel(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("audioPlayer");
    }

    [Fact]
    public void IsPlaying_WhenNewInstance_ShouldBeFalse() {
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new AudioPlayerViewModel(player);

        viewModel.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void HasAudio_WhenNewInstance_ShouldBeFalse() {
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new AudioPlayerViewModel(player);

        viewModel.HasAudio.Should().BeFalse();
    }

    [Fact]
    public void CurrentPosition_WhenNewInstance_ShouldBeZero() {
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new AudioPlayerViewModel(player);

        viewModel.CurrentPosition.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void TotalDuration_WhenNewInstance_ShouldBeZero() {
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new AudioPlayerViewModel(player);

        viewModel.TotalDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void PlayPauseLabel_WhenNotPlaying_ShouldBePlay() {
        var player = Substitute.For<IAudioPlayer>();

        var viewModel = new AudioPlayerViewModel(player);

        viewModel.PlayPauseLabel.Should().Be("Play");
    }

    [Fact]
    public void PlayPauseLabel_WhenPlaying_ShouldBePause() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            IsPlaying = true
        };

        viewModel.PlayPauseLabel.Should().Be("Pause");
    }

    [Fact]
    public void OnIsPlayingChanged_ShouldRaisePlayPauseLabelPropertyChanged() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player);
        var changed = new List<string>();
        viewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName is not null)
                changed.Add(args.PropertyName);
        };

        viewModel.IsPlaying = true;

        changed.Should().Contain(nameof(AudioPlayerViewModel.PlayPauseLabel));
    }

    [Fact]
    public void StopCommand_WhenExecuted_ShouldResetStateAndCallPlayerStop() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            IsPlaying = true,
            CurrentPosition = TimeSpan.FromSeconds(5),
            TotalDuration = TimeSpan.FromSeconds(10),
            Progress = 0.5
        };

        viewModel.StopCommand.Execute(null);

        viewModel.IsPlaying.Should().BeFalse();
        viewModel.CurrentPosition.Should().Be(TimeSpan.Zero);
        viewModel.Progress.Should().Be(0);
        player.Received(1).Stop();
    }

    [Fact]
    public void TogglePlayPauseCommand_WhenNoAudioLoaded_ShouldNotCallPlayer() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player);

        viewModel.TogglePlayPauseCommand.Execute(null);

        player.DidNotReceiveWithAnyArgs().Play(default!);
        player.DidNotReceive().Pause();
    }

    [Fact]
    public void TogglePlayPauseCommand_WhenAudioLoadedAndNotPlaying_ShouldCallPlay() {
        var player = Substitute.For<IAudioPlayer>();
        player.IsPlaying.Returns(false, true);
        player.TotalDuration.Returns(TimeSpan.FromSeconds(30));
        var viewModel = new AudioPlayerViewModel(player);

        // Create a temp file to make HasAudio true
        var tempFile = Path.GetTempFileName();
        try {
            var recording = new Recording {
                Id = Guid.NewGuid(),
                Title = "Test",
                AudioFileName = tempFile,
                Status = RecordingStatus.Complete,
                Language = "en",
                Duration = TimeSpan.FromSeconds(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WhisperModel = "tiny",
                FileSize = 100
            };
            viewModel.LoadRecording(recording);

            viewModel.TogglePlayPauseCommand.Execute(null);

            player.Received(1).Play(tempFile);
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void TogglePlayPauseCommand_WhenAudioLoadedAndPlaying_ShouldCallPause() {
        var player = Substitute.For<IAudioPlayer>();
        player.IsPlaying.Returns(true);
        var viewModel = new AudioPlayerViewModel(player);

        var tempFile = Path.GetTempFileName();
        try {
            var recording = new Recording {
                Id = Guid.NewGuid(),
                Title = "Test",
                AudioFileName = tempFile,
                Status = RecordingStatus.Complete,
                Language = "en",
                Duration = TimeSpan.FromSeconds(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WhisperModel = "tiny",
                FileSize = 100
            };
            viewModel.LoadRecording(recording);

            viewModel.TogglePlayPauseCommand.Execute(null);

            player.Received(1).Pause();
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadRecording_WithNull_ShouldClearAudioState() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            HasAudio = true,
            TotalDuration = TimeSpan.FromSeconds(10)
        };

        viewModel.LoadRecording(null);

        viewModel.HasAudio.Should().BeFalse();
        viewModel.TotalDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void LoadRecording_WithValidRecording_ShouldSetTotalDuration() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player);

        var tempFile = Path.GetTempFileName();
        try {
            var recording = new Recording {
                Id = Guid.NewGuid(),
                Title = "Test",
                AudioFileName = tempFile,
                Status = RecordingStatus.Complete,
                Language = "en",
                Duration = TimeSpan.FromSeconds(42),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WhisperModel = "tiny",
                FileSize = 100
            };

            viewModel.LoadRecording(recording);

            viewModel.TotalDuration.Should().Be(TimeSpan.FromSeconds(42));
            viewModel.HasAudio.Should().BeTrue();
            viewModel.CurrentPosition.Should().Be(TimeSpan.Zero);
            viewModel.Progress.Should().Be(0);
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadRecording_WithNonExistentFile_ShouldSetHasAudioFalse() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player);

        var recording = new Recording {
            Id = Guid.NewGuid(),
            Title = "Test",
            AudioFileName = "/nonexistent/path/file.wav",
            Status = RecordingStatus.Complete,
            Language = "en",
            Duration = TimeSpan.FromSeconds(10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            WhisperModel = "tiny",
            FileSize = 100
        };

        viewModel.LoadRecording(recording);

        viewModel.HasAudio.Should().BeFalse();
    }

    [Fact]
    public void LoadRecording_ShouldStopCurrentPlaybackFirst() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            IsPlaying = true
        };

        viewModel.LoadRecording(null);

        player.Received(1).Stop();
        viewModel.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void TogglePlayPauseCommand_WhenPlaySucceeds_ShouldUpdateTotalDurationFromPlayer() {
        var player = Substitute.For<IAudioPlayer>();
        player.IsPlaying.Returns(false, true);
        player.TotalDuration.Returns(TimeSpan.FromSeconds(60));
        var viewModel = new AudioPlayerViewModel(player);

        var tempFile = Path.GetTempFileName();
        try {
            var recording = new Recording {
                Id = Guid.NewGuid(),
                Title = "Test",
                AudioFileName = tempFile,
                Status = RecordingStatus.Complete,
                Language = "en",
                Duration = TimeSpan.FromSeconds(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WhisperModel = "tiny",
                FileSize = 100
            };
            viewModel.LoadRecording(recording);

            viewModel.TogglePlayPauseCommand.Execute(null);

            viewModel.TotalDuration.Should().Be(TimeSpan.FromSeconds(60));
            viewModel.IsPlaying.Should().BeTrue();
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void TogglePlayPauseCommand_WhenPaused_ShouldSetIsPlayingFalse() {
        var player = Substitute.For<IAudioPlayer>();
        player.IsPlaying.Returns(true);
        var viewModel = new AudioPlayerViewModel(player);

        var tempFile = Path.GetTempFileName();
        try {
            var recording = new Recording {
                Id = Guid.NewGuid(),
                Title = "Test",
                AudioFileName = tempFile,
                Status = RecordingStatus.Complete,
                Language = "en",
                Duration = TimeSpan.FromSeconds(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                WhisperModel = "tiny",
                FileSize = 100
            };
            viewModel.LoadRecording(recording);

            viewModel.TogglePlayPauseCommand.Execute(null);

            viewModel.IsPlaying.Should().BeFalse();
        }
        finally {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Progress_WhenChangedWithNoAudio_ShouldNotCallSeek() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            Progress = 0.5
        };

        player.DidNotReceiveWithAnyArgs().Seek(default);
    }

    [Fact]
    public void Progress_WhenChangedWithAudioAndDuration_ShouldCallSeek() {
        var player = Substitute.For<IAudioPlayer>();
        player.CurrentPosition.Returns(TimeSpan.FromSeconds(15));
        var viewModel = new AudioPlayerViewModel(player) {
            HasAudio = true,
            TotalDuration = TimeSpan.FromSeconds(30),
            Progress = 0.5
        };

        player.Received(1).Seek(Arg.Is<TimeSpan>(t => Math.Abs(t.TotalSeconds - 15) < 0.1));
    }

    [Fact]
    public void Progress_WhenChangedWithZeroDuration_ShouldNotCallSeek() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player) {
            HasAudio = true,
            TotalDuration = TimeSpan.Zero,
            Progress = 0.5
        };

        player.DidNotReceiveWithAnyArgs().Seek(default);
    }

    [Fact]
    public void LoadRecording_WithRelativePath_ShouldResolveAgainstAudioDirectory() {
        var player = Substitute.For<IAudioPlayer>();
        var viewModel = new AudioPlayerViewModel(player);
        var recording = new Recording {
            Id = Guid.NewGuid(),
            Title = "Test",
            AudioFileName = "relative/file.wav",
            Status = RecordingStatus.Complete,
            Language = "en",
            Duration = TimeSpan.FromSeconds(10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            WhisperModel = "tiny",
            FileSize = 100
        };

        viewModel.LoadRecording(recording);

        viewModel.HasAudio.Should().BeFalse();
        viewModel.TotalDuration.Should().Be(TimeSpan.FromSeconds(10));
    }
}
