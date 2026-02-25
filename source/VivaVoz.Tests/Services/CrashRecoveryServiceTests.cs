using System.Text.Json;

using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class CrashRecoveryServiceTests : IDisposable {
    private readonly string _tempDir;
    private readonly string _markerPath;

    public CrashRecoveryServiceTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-recovery-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _markerPath = Path.Combine(_tempDir, "in-progress.json");
    }

    public void Dispose() {
        try {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best effort */ }

        GC.SuppressFinalize(this);
    }

    // ========== HasOrphan tests ==========

    [Fact]
    public void HasOrphan_WhenNoMarkerExists_ShouldReturnFalse() {
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan().Should().BeFalse();
    }

    [Fact]
    public void HasOrphan_WhenMarkerExistsAndAudioFileExists_ShouldReturnTrue() {
        var audioPath = CreateTempAudioFile();
        WriteMarker(audioPath);
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan().Should().BeTrue();
    }

    [Fact]
    public void HasOrphan_WhenMarkerExistsButAudioFileMissing_ShouldReturnFalse() {
        WriteMarker("/nonexistent/path.wav");
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan().Should().BeFalse();
    }

    [Fact]
    public void HasOrphan_WhenMarkerIsCorrupt_ShouldReturnFalse() {
        File.WriteAllText(_markerPath, "{{not valid json}}");
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan().Should().BeFalse();
    }

    [Fact]
    public void HasOrphan_WhenMarkerIsCorrupt_ShouldAutoDismissMarker() {
        File.WriteAllText(_markerPath, "{{not valid json}}");
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan();

        File.Exists(_markerPath).Should().BeFalse();
    }

    [Fact]
    public void HasOrphan_WhenMarkerHasNoFilePath_ShouldAutoDismissMarker() {
        File.WriteAllText(_markerPath, "{\"StartedAt\":\"2024-01-01T00:00:00Z\"}");
        var service = new CrashRecoveryService(_markerPath);

        service.HasOrphan();

        File.Exists(_markerPath).Should().BeFalse();
    }

    // ========== GetOrphanPath tests ==========

    [Fact]
    public void GetOrphanPath_WhenNoMarker_ShouldReturnNull() {
        var service = new CrashRecoveryService(_markerPath);

        service.GetOrphanPath().Should().BeNull();
    }

    [Fact]
    public void GetOrphanPath_WhenValidMarker_ShouldReturnFilePath() {
        const string audioPath = "/some/audio/path.wav";
        WriteMarker(audioPath);
        var service = new CrashRecoveryService(_markerPath);

        service.GetOrphanPath().Should().Be(audioPath);
    }

    [Fact]
    public void GetOrphanPath_WhenCorruptMarker_ShouldReturnNull() {
        File.WriteAllText(_markerPath, "not-json");
        var service = new CrashRecoveryService(_markerPath);

        service.GetOrphanPath().Should().BeNull();
    }

    [Fact]
    public void GetOrphanPath_WhenMarkerHasNoFilePath_ShouldReturnNull() {
        File.WriteAllText(_markerPath, "{\"StartedAt\":\"2024-01-01T00:00:00Z\"}");
        var service = new CrashRecoveryService(_markerPath);

        service.GetOrphanPath().Should().BeNull();
    }

    // ========== Dismiss tests ==========

    [Fact]
    public void Dismiss_WhenMarkerExists_ShouldDeleteIt() {
        WriteMarker("/some/path.wav");
        var service = new CrashRecoveryService(_markerPath);

        service.Dismiss();

        File.Exists(_markerPath).Should().BeFalse();
    }

    [Fact]
    public void Dismiss_WhenNoMarkerExists_ShouldNotThrow() {
        var service = new CrashRecoveryService(_markerPath);

        var act = service.Dismiss;

        act.Should().NotThrow();
    }

    [Fact]
    public void Dismiss_AfterDismiss_HasOrphanShouldReturnFalse() {
        var audioPath = CreateTempAudioFile();
        WriteMarker(audioPath);
        var service = new CrashRecoveryService(_markerPath);

        service.Dismiss();

        service.HasOrphan().Should().BeFalse();
    }

    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullMarkerPath_ShouldUseDefaultPath() {
        var act = () => new CrashRecoveryService();

        act.Should().NotThrow();
    }

    // ========== Helper methods ==========

    private string CreateTempAudioFile() {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid()}.wav");
        File.WriteAllBytes(path, [0x52, 0x49, 0x46, 0x46]);
        return path;
    }

    private void WriteMarker(string filePath) {
        var json = JsonSerializer.Serialize(new { FilePath = filePath, StartedAt = DateTime.UtcNow });
        File.WriteAllText(_markerPath, json);
    }
}
