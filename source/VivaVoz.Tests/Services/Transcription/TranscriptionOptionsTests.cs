using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class TranscriptionOptionsTests {
    [Fact]
    public void DefaultOptions_ShouldHaveNullLanguageAndModelId() {
        var options = new TranscriptionOptions();

        options.Language.Should().BeNull();
        options.ModelId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithLanguage_ShouldSetLanguage() {
        var options = new TranscriptionOptions(Language: "en");

        options.Language.Should().Be("en");
        options.ModelId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithModelId_ShouldSetModelId() {
        var options = new TranscriptionOptions(ModelId: "tiny");

        options.Language.Should().BeNull();
        options.ModelId.Should().Be("tiny");
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAll() {
        var options = new TranscriptionOptions(Language: "pt", ModelId: "base");

        options.Language.Should().Be("pt");
        options.ModelId.Should().Be("base");
    }

    [Fact]
    public void Equality_ShouldWorkForRecordType() {
        var a = new TranscriptionOptions(Language: "en", ModelId: "tiny");
        var b = new TranscriptionOptions(Language: "en", ModelId: "tiny");

        a.Should().Be(b);
    }
}
