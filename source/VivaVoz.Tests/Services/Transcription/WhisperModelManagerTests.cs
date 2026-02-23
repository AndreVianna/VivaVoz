using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class WhisperModelManagerTests : IDisposable {
    private readonly string _tempDir;
    private readonly WhisperModelManager _manager;

    public WhisperModelManagerTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _manager = new WhisperModelManager(_tempDir);
    }

    public void Dispose() {
        try {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best effort cleanup */ }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ModelsDirectory_ShouldReturnConfiguredPath() => _manager.ModelsDirectory.Should().Be(_tempDir);

    [Fact]
    public void DefaultConstructor_ShouldUseAppModelsDirectory() {
        var manager = new WhisperModelManager();

        manager.ModelsDirectory.Should().Be(VivaVoz.Constants.FilePaths.ModelsDirectory);
    }

    [Fact]
    public void GetModelPath_WithValidId_ShouldReturnExpectedPath() {
        var path = _manager.GetModelPath("tiny");

        path.Should().Be(Path.Combine(_tempDir, "ggml-tiny.bin"));
    }

    [Fact]
    public void GetModelPath_WithBaseId_ShouldReturnExpectedPath() {
        var path = _manager.GetModelPath("base");

        path.Should().Be(Path.Combine(_tempDir, "ggml-base.bin"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetModelPath_WithNullOrEmptyId_ShouldThrow(string? modelId) {
        var act = () => _manager.GetModelPath(modelId!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsModelDownloaded_WhenModelNotPresent_ShouldReturnFalse() => _manager.IsModelDownloaded("tiny").Should().BeFalse();

    [Fact]
    public void IsModelDownloaded_WhenModelFileExists_ShouldReturnTrue() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");

        _manager.IsModelDownloaded("tiny").Should().BeTrue();
    }

    [Fact]
    public void GetAvailableModelIds_ShouldContainExpectedModels() {
        var ids = WhisperModelManager.GetAvailableModelIds();

        ids.Should().Contain("tiny");
        ids.Should().Contain("base");
        ids.Should().Contain("small");
        ids.Should().Contain("medium");
        ids.Should().Contain("large-v3");
    }

    [Fact]
    public async Task EnsureModelAsync_WhenModelAlreadyExists_ShouldReturnExistingPath() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        await File.WriteAllTextAsync(modelPath, "fake model data");

        var result = await _manager.EnsureModelAsync("tiny");

        result.Should().Be(modelPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EnsureModelAsync_WithNullOrEmptyId_ShouldThrow(string? modelId) {
        var act = () => _manager.EnsureModelAsync(modelId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnsureModelAsync_WithUnknownModelId_ShouldThrowArgumentException() {
        var act = () => _manager.EnsureModelAsync("nonexistent-model");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Unknown model id*");
    }
}
