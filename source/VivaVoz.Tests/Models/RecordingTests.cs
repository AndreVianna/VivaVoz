using AwesomeAssertions;

using VivaVoz.Models;

using Xunit;

namespace VivaVoz.Tests.Models;

public class RecordingTests {
    [Fact]
    public void NewRecording_ShouldDefaultStatusToRecording() {
        var recording = new Recording();

        recording.Status.Should().Be(RecordingStatus.Recording);
    }

    [Fact]
    public void NewRecording_ShouldDefaultLanguageToAuto() {
        var recording = new Recording();

        recording.Language.Should().Be("auto");
    }

    [Fact]
    public void NewRecording_ShouldDefaultLanguageCodeToUnknown() {
        var recording = new Recording();

        recording.LanguageCode.Should().Be("unknown");
    }

    [Fact]
    public void Recording_WhenAllPropertiesAreSet_ShouldRetainValues() {
        var now = DateTime.UtcNow;
        var duration = TimeSpan.FromSeconds(42);
        var id = Guid.NewGuid();

        var recording = new Recording {
            Id = id,
            Title = "Test Title",
            AudioFileName = "file.wav",
            Transcript = "Transcript",
            Status = RecordingStatus.Complete,
            Language = "en",
            LanguageCode = "en",
            Duration = duration,
            CreatedAt = now,
            UpdatedAt = now.AddMinutes(1),
            WhisperModel = "tiny",
            FileSize = 1234
        };

        recording.Id.Should().Be(id);
        recording.Title.Should().Be("Test Title");
        recording.AudioFileName.Should().Be("file.wav");
        recording.Transcript.Should().Be("Transcript");
        recording.Status.Should().Be(RecordingStatus.Complete);
        recording.Language.Should().Be("en");
        recording.LanguageCode.Should().Be("en");
        recording.Duration.Should().Be(duration);
        recording.CreatedAt.Should().Be(now);
        recording.UpdatedAt.Should().Be(now.AddMinutes(1));
        recording.WhisperModel.Should().Be("tiny");
        recording.FileSize.Should().Be(1234);
    }
}
