using System.Net;
using System.Security.Cryptography;

using AwesomeAssertions;

using VivaVoz.Services.Transcription;

using Xunit;

namespace VivaVoz.Tests.Services.Transcription;

public class WhisperModelServiceTests : IDisposable {
    private readonly string _tempDir;
    private readonly WhisperModelManager _modelManager;

    public WhisperModelServiceTests() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vivavoz-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _modelManager = new WhisperModelManager(_tempDir);
    }

    public void Dispose() {
        try {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best effort cleanup */ }

        GC.SuppressFinalize(this);
    }

    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrow() {
        var act = () => new WhisperModelService(null!, new HttpClient());

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelManager");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrow() {
        var act = () => new WhisperModelService(_modelManager, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow() {
        var act = () => new WhisperModelService(_modelManager, new HttpClient());

        act.Should().NotThrow();
    }

    // ========== GetAvailableModelIds ==========

    [Fact]
    public void GetAvailableModelIds_ShouldReturnExpectedModels() {
        var service = CreateService();

        var ids = service.GetAvailableModelIds();

        ids.Should().Contain("tiny");
        ids.Should().Contain("base");
        ids.Should().Contain("small");
        ids.Should().Contain("medium");
        ids.Should().Contain("large-v3");
    }

    // ========== IsModelDownloaded ==========

    [Fact]
    public void IsModelDownloaded_WhenFileNotPresent_ShouldReturnFalse() {
        var service = CreateService();

        service.IsModelDownloaded("tiny").Should().BeFalse();
    }

    [Fact]
    public void IsModelDownloaded_WhenFilePresent_ShouldReturnTrue() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");
        var service = CreateService();

        service.IsModelDownloaded("tiny").Should().BeTrue();
    }

    // ========== DeleteModel ==========

    [Fact]
    public void DeleteModel_WithNullOrEmptyId_ShouldThrow() {
        var service = CreateService();

        var act = () => service.DeleteModel(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeleteModel_WhenModelInstalled_ShouldRemoveFile() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");
        var service = CreateService();

        service.DeleteModel("tiny");

        File.Exists(modelPath).Should().BeFalse();
    }

    [Fact]
    public void DeleteModel_WhenModelNotInstalled_ShouldNotThrow() {
        var service = CreateService();

        var act = () => service.DeleteModel("tiny");

        act.Should().NotThrow();
    }

    // ========== DownloadModelAsync ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DownloadModelAsync_WithNullOrEmptyId_ShouldThrow(string? modelId) {
        var service = CreateService();

        var act = () => service.DownloadModelAsync(modelId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DownloadModelAsync_WhenModelAlreadyInstalled_ShouldNotDownload() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");

        var handler = new TrackingHttpMessageHandler(CreateFakeContent("fake data"));
        var service = CreateService(handler);

        await service.DownloadModelAsync("tiny");

        handler.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task DownloadModelAsync_WhenModelAlreadyInstalled_ShouldReportFullProgress() {
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.WriteAllText(modelPath, "fake model data");
        var service = CreateService();

        double? reportedProgress = null;
        var progress = new Progress<double>(p => reportedProgress = p);

        await service.DownloadModelAsync("tiny", progress);
        await Task.Delay(50); // let Progress<T> dispatch complete

        reportedProgress.Should().Be(1.0);
    }

    [Fact]
    public async Task DownloadModelAsync_ShouldWriteModelFileToModelsDirectory() {
        var fakeContent = new byte[] { 1, 2, 3, 4, 5 };
        var handler = new TrackingHttpMessageHandler(fakeContent);
        var service = CreateService(handler);

        await service.DownloadModelAsync("tiny");

        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.Exists(modelPath).Should().BeTrue();
        File.ReadAllBytes(modelPath).Should().Equal(fakeContent);
    }

    [Fact]
    public async Task DownloadModelAsync_ShouldReportProgressDuringDownload() {
        var fakeContent = new byte[10000]; // 10 KB to ensure multiple progress reports
        Random.Shared.NextBytes(fakeContent);
        var handler = new TrackingHttpMessageHandler(fakeContent);
        var service = CreateService(handler);

        var progressValues = new List<double>();
        var progress = new Progress<double>(progressValues.Add);

        await service.DownloadModelAsync("tiny", progress);
        await Task.Delay(100); // let Progress<T> dispatch

        progressValues.Should().NotBeEmpty();
        progressValues[^1].Should().Be(1.0);
    }

    [Fact]
    public async Task DownloadModelAsync_ShouldNotLeaveTempFileOnSuccess() {
        var fakeContent = new byte[] { 1, 2, 3 };
        var handler = new TrackingHttpMessageHandler(fakeContent);
        var service = CreateService(handler);

        await service.DownloadModelAsync("tiny");

        var tempPath = Path.Combine(_tempDir, "ggml-tiny.bin.tmp");
        File.Exists(tempPath).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadModelAsync_WhenCancelled_ShouldCleanUpTempFile() {
        using var cts = new CancellationTokenSource();
        var handler = new SlowHttpMessageHandler();
        var service = CreateService(handler);

        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        var act = () => service.DownloadModelAsync("tiny", null, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        var tempPath = Path.Combine(_tempDir, "ggml-tiny.bin.tmp");
        File.Exists(tempPath).Should().BeFalse();
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.Exists(modelPath).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadModelAsync_WhenHttpFails_ShouldCleanUpTempFile() {
        var handler = new FailingHttpMessageHandler();
        var service = CreateService(handler);

        var act = () => service.DownloadModelAsync("tiny");

        await act.Should().ThrowAsync<HttpRequestException>();

        var tempPath = Path.Combine(_tempDir, "ggml-tiny.bin.tmp");
        File.Exists(tempPath).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadModelAsync_ShouldCreateModelsDirectoryIfMissing() {
        var deepDir = Path.Combine(_tempDir, "nested", "models");
        var deepModelManager = new WhisperModelManager(deepDir);
        var handler = new TrackingHttpMessageHandler([1, 2, 3]);
        var service = new WhisperModelService(deepModelManager, new HttpClient(handler));

        await service.DownloadModelAsync("tiny");

        Directory.Exists(deepDir).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadModelAsync_WithValidHashInRegistry_ShouldValidateSha256() {
        var fakeContent = new byte[] { 1, 2, 3, 4, 5 };
        var correctHash = ComputeSha256(fakeContent);
        var handler = new TrackingHttpMessageHandler(fakeContent);
        var expectedHashes = new Dictionary<string, string> { ["tiny"] = correctHash };
        var service = new WhisperModelService(_modelManager, new HttpClient(handler), expectedHashes);

        var act = () => service.DownloadModelAsync("tiny");

        await act.Should().NotThrowAsync();

        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.Exists(modelPath).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadModelAsync_WithWrongHashInRegistry_ShouldThrowAndCleanUp() {
        var fakeContent = new byte[] { 1, 2, 3, 4, 5 };
        const string wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";
        var handler = new TrackingHttpMessageHandler(fakeContent);
        var expectedHashes = new Dictionary<string, string> { ["tiny"] = wrongHash };
        var service = new WhisperModelService(_modelManager, new HttpClient(handler), expectedHashes);

        var act = () => service.DownloadModelAsync("tiny");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SHA256 hash mismatch*");

        var tempPath = Path.Combine(_tempDir, "ggml-tiny.bin.tmp");
        File.Exists(tempPath).Should().BeFalse();
        var modelPath = Path.Combine(_tempDir, "ggml-tiny.bin");
        File.Exists(modelPath).Should().BeFalse();
    }

    // ========== Helpers ==========

    private WhisperModelService CreateService(HttpMessageHandler? handler = null) {
        var httpClient = handler is not null
            ? new HttpClient(handler)
            : new HttpClient(new TrackingHttpMessageHandler([1, 2, 3]));
        return new WhisperModelService(_modelManager, httpClient);
    }

    private static byte[] CreateFakeContent(string content) => System.Text.Encoding.UTF8.GetBytes(content);

    private static string ComputeSha256(byte[] data) {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TrackingHttpMessageHandler(byte[] content) : HttpMessageHandler {
        private readonly byte[] _content = content;
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
            CallCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new ByteArrayContent(_content)
            };
            response.Content.Headers.ContentLength = _content.Length;
            return Task.FromResult(response);
        }
    }

    private sealed class SlowHttpMessageHandler : HttpMessageHandler {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
            await Task.Delay(Timeout.Infinite, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class FailingHttpMessageHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return Task.FromResult(response);
        }
    }
}
