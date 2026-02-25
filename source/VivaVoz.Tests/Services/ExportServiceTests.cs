using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class ExportServiceTests : IDisposable {
    private readonly string _tempDir;

    public ExportServiceTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-export-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort */ }

        GC.SuppressFinalize(this);
    }

    // ========== ExportTextAsync tests ==========

    [Fact]
    public async Task ExportTextAsync_WithValidArgs_ShouldCreateFileWithTranscript() {
        var service = new ExportService();
        var destPath = Path.Combine(_tempDir, "transcript.txt");

        await service.ExportTextAsync("Hello, World!", destPath);

        File.Exists(destPath).Should().BeTrue();
        (await File.ReadAllTextAsync(destPath)).Should().Be("Hello, World!");
    }

    [Fact]
    public async Task ExportTextAsync_WithMarkdownExtension_ShouldCreateFile() {
        var service = new ExportService();
        var destPath = Path.Combine(_tempDir, "transcript.md");

        await service.ExportTextAsync("# Title\n\nContent", destPath);

        File.Exists(destPath).Should().BeTrue();
        (await File.ReadAllTextAsync(destPath)).Should().Be("# Title\n\nContent");
    }

    [Fact]
    public async Task ExportTextAsync_WithNullTranscript_ShouldCreateEmptyFile() {
        var service = new ExportService();
        var destPath = Path.Combine(_tempDir, "empty.txt");

        await service.ExportTextAsync(null!, destPath);

        File.Exists(destPath).Should().BeTrue();
        (await File.ReadAllTextAsync(destPath)).Should().BeEmpty();
    }

    [Fact]
    public async Task ExportTextAsync_WithNullDestinationPath_ShouldThrow() {
        var service = new ExportService();

        var act = async () => await service.ExportTextAsync("text", null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExportTextAsync_WhenDestinationDirectoryMissing_ShouldCreateIt() {
        var service = new ExportService();
        var subDir = Path.Combine(_tempDir, "subdir", "nested");
        var destPath = Path.Combine(subDir, "out.txt");

        await service.ExportTextAsync("text", destPath);

        File.Exists(destPath).Should().BeTrue();
    }

    [Fact]
    public async Task ExportTextAsync_ShouldOverwriteExistingFile() {
        var service = new ExportService();
        var destPath = Path.Combine(_tempDir, "overwrite.txt");
        await File.WriteAllTextAsync(destPath, "old content");

        await service.ExportTextAsync("new content", destPath);

        (await File.ReadAllTextAsync(destPath)).Should().Be("new content");
    }

    // ========== ExportAudioAsync tests ==========

    [Fact]
    public async Task ExportAudioAsync_WithValidSource_ShouldCopyFile() {
        var service = new ExportService();
        var sourcePath = Path.Combine(_tempDir, "source.wav");
        var destPath = Path.Combine(_tempDir, "export.wav");
        await File.WriteAllBytesAsync(sourcePath, [0x52, 0x49, 0x46, 0x46]); // RIFF header bytes

        await service.ExportAudioAsync(sourcePath, destPath);

        File.Exists(destPath).Should().BeTrue();
        (await File.ReadAllBytesAsync(destPath)).Should().Equal([0x52, 0x49, 0x46, 0x46]);
    }

    [Fact]
    public async Task ExportAudioAsync_WhenSourceNotFound_ShouldThrowFileNotFoundException() {
        var service = new ExportService();
        var sourcePath = Path.Combine(_tempDir, "nonexistent.wav");
        var destPath = Path.Combine(_tempDir, "out.wav");

        var act = async () => await service.ExportAudioAsync(sourcePath, destPath);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task ExportAudioAsync_WithNullSourcePath_ShouldThrow() {
        var service = new ExportService();

        var act = async () => await service.ExportAudioAsync(null!, Path.Combine(_tempDir, "out.wav"));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExportAudioAsync_WithNullDestinationPath_ShouldThrow() {
        var service = new ExportService();
        var sourcePath = Path.Combine(_tempDir, "source.wav");
        await File.WriteAllBytesAsync(sourcePath, []);

        var act = async () => await service.ExportAudioAsync(sourcePath, null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExportAudioAsync_WhenDestinationDirectoryMissing_ShouldCreateIt() {
        var service = new ExportService();
        var sourcePath = Path.Combine(_tempDir, "source.wav");
        var subDir = Path.Combine(_tempDir, "audio", "exports");
        var destPath = Path.Combine(subDir, "out.wav");
        await File.WriteAllBytesAsync(sourcePath, [1, 2, 3]);

        await service.ExportAudioAsync(sourcePath, destPath);

        File.Exists(destPath).Should().BeTrue();
    }

    [Fact]
    public async Task ExportAudioAsync_ShouldOverwriteExistingFile() {
        var service = new ExportService();
        var sourcePath = Path.Combine(_tempDir, "source.wav");
        var destPath = Path.Combine(_tempDir, "existing.wav");
        await File.WriteAllBytesAsync(sourcePath, [1, 2, 3]);
        await File.WriteAllBytesAsync(destPath, [9, 9, 9]);

        await service.ExportAudioAsync(sourcePath, destPath);

        (await File.ReadAllBytesAsync(destPath)).Should().Equal([1, 2, 3]);
    }
}
