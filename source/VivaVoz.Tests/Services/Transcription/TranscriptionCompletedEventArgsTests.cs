using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class TranscriptionCompletedEventArgsTests {
    [Fact]
    public void Succeeded_ShouldSetSuccessToTrue() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Hello", "en", "tiny");

        args.Success.Should().BeTrue();
    }

    [Fact]
    public void Succeeded_ShouldSetRecordingId() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Hello", "en", "tiny");

        args.RecordingId.Should().Be(id);
    }

    [Fact]
    public void Succeeded_ShouldSetTranscriptText() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Hello world", "en", "tiny");

        args.Transcript.Should().Be("Hello world");
    }

    [Fact]
    public void Succeeded_ShouldSetDetectedLanguage() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Bonjour", "fr", "small");

        args.DetectedLanguage.Should().Be("fr");
    }

    [Fact]
    public void Succeeded_ShouldSetModelUsed() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Hello", "en", "medium");

        args.ModelUsed.Should().Be("medium");
    }

    [Fact]
    public void Succeeded_ShouldSetErrorMessageToNull() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Succeeded(id, "Hello", "en", "tiny");

        args.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldSetSuccessToFalse() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "Some error");

        args.Success.Should().BeFalse();
    }

    [Fact]
    public void Failed_ShouldSetRecordingId() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "Some error");

        args.RecordingId.Should().Be(id);
    }

    [Fact]
    public void Failed_ShouldSetErrorMessage() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "Whisper model not found");

        args.ErrorMessage.Should().Be("Whisper model not found");
    }

    [Fact]
    public void Failed_ShouldSetTranscriptToNull() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "error");

        args.Transcript.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldSetDetectedLanguageToNull() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "error");

        args.DetectedLanguage.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldSetModelUsedToNull() {
        var id = Guid.NewGuid();

        var args = TranscriptionCompletedEventArgs.Failed(id, "error");

        args.ModelUsed.Should().BeNull();
    }
}
