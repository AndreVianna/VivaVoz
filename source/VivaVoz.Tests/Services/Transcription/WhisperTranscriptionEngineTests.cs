using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class WhisperTranscriptionEngineTests : IDisposable {
    private readonly string _tempDir;
    private readonly WhisperModelManager _modelManager;
    private readonly WhisperTranscriptionEngine _engine;

    public WhisperTranscriptionEngineTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _modelManager = new WhisperModelManager(_tempDir);
        _engine = new WhisperTranscriptionEngine(_modelManager);
    }

    public void Dispose() {
        _engine.Dispose();
        try {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best effort cleanup */ }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrow() {
        var act = () => new WhisperTranscriptionEngine(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SupportedLanguages_ShouldContainAutoAndCommonLanguages() {
        var languages = _engine.SupportedLanguages;

        languages.Should().Contain("auto");
        languages.Should().Contain("en");
        languages.Should().Contain("es");
        languages.Should().Contain("fr");
        languages.Should().Contain("pt");
        languages.Should().Contain("de");
        languages.Should().Contain("ja");
        languages.Should().Contain("zh");
    }

    [Fact]
    public void SupportedLanguages_ShouldNotBeEmpty() => _engine.SupportedLanguages.Should().NotBeEmpty();

    [Fact]
    public void IsAvailable_WhenNoModelDownloaded_ShouldBeFalse() => _engine.IsAvailable.Should().BeFalse();

    [Fact]
    public void IsAvailable_WhenTinyModelExists_ShouldBeTrue() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");

        _engine.IsAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TranscribeAsync_WithNullOrEmptyPath_ShouldThrow(string? path) {
        var options = new TranscriptionOptions();

        var act = () => _engine.TranscribeAsync(path!, options);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TranscribeAsync_WithNonExistentFile_ShouldThrowFileNotFoundException() {
        var nonExistentPath = Path.Combine(_tempDir, $"ghost-{Guid.NewGuid()}.wav");
        var options = new TranscriptionOptions();

        var act = () => _engine.TranscribeAsync(nonExistentPath, options);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task TranscribeAsync_WithNonExistentFile_ShouldIncludeFilePathInException() {
        var nonExistentPath = Path.Combine(_tempDir, "ghost.wav");
        var options = new TranscriptionOptions();

        try {
            await _engine.TranscribeAsync(nonExistentPath, options);
            Assert.Fail("Expected FileNotFoundException");
        }
        catch (FileNotFoundException ex) {
            ex.FileName.Should().Be(nonExistentPath);
            ex.Message.Should().Contain("ghost.wav");
        }
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow() {
        var engine = new WhisperTranscriptionEngine(_modelManager);

        engine.Dispose();
        var act = engine.Dispose;

        act.Should().NotThrow();
    }
}
