using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class TranscriptionResultTests {
    [Fact]
    public void Constructor_WithRequiredParameters_ShouldSetProperties() {
        var duration = TimeSpan.FromSeconds(2.5);
        var result = new TranscriptionResult(
            Text: "Hello world",
            DetectedLanguage: "en",
            Duration: duration,
            ModelUsed: "tiny"
        );

        result.Text.Should().Be("Hello world");
        result.DetectedLanguage.Should().Be("en");
        result.Duration.Should().Be(duration);
        result.ModelUsed.Should().Be("tiny");
        result.Confidence.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithConfidence_ShouldSetConfidence() {
        var result = new TranscriptionResult(
            Text: "test",
            DetectedLanguage: "en",
            Duration: TimeSpan.FromSeconds(1),
            ModelUsed: "tiny",
            Confidence: 0.95f
        );

        result.Confidence.Should().Be(0.95f);
    }

    [Fact]
    public void Equality_ShouldWorkForRecordType() {
        var duration = TimeSpan.FromSeconds(1);
        var a = new TranscriptionResult("Hi", "en", duration, "tiny");
        var b = new TranscriptionResult("Hi", "en", duration, "tiny");

        a.Should().Be(b);
    }
}
