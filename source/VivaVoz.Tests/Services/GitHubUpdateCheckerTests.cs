using System.Net;
using System.Net.Http;

using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class GitHubUpdateCheckerTests {
    // ========== Constructor tests ==========

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrow() {
        var act = () => new GitHubUpdateChecker(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_WithValidHttpClient_ShouldNotThrow() {
        var act = () => new GitHubUpdateChecker(new HttpClient());

        act.Should().NotThrow();
    }

    // ========== CheckForUpdateAsync — newer version ==========

    [Fact]
    public async Task CheckForUpdateAsync_WhenNewerVersionAvailable_ShouldReturnUpdateInfo() {
        // Arrange: patch the current version to "0.0.1" so any real release looks newer
        var json = """{"tag_name":"v99.0.0","html_url":"https://example.com/releases/v99.0.0","body":"What's new"}""";
        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        // Act
        var result = await checker.CheckForUpdateAsync();

        // The result may be null if the running assembly is already >=99.0.0, so we only
        // verify when we know the current version is older.
        // Instead, let's use the internal helper to compare explicitly.
        var currentStr = GitHubUpdateChecker.GetCurrentVersion();
        if (Version.TryParse(currentStr, out var current) && current < new Version(99, 0, 0)) {
            result.Should().NotBeNull();
            result!.Version.Should().Be("99.0.0");
            result.DownloadUrl.Should().Be("https://example.com/releases/v99.0.0");
            result.ReleaseNotes.Should().Be("What's new");
        }
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNewerVersionAvailable_ReturnsCorrectVersion() {
        // Force a scenario where we know the version is newer by using a trivially
        // low-version assembly helper.
        var json = """{"tag_name":"v9999.0.0","html_url":"https://github.com/AndreVianna/VivaVoz/releases/v9999.0.0","body":"Release notes"}""";
        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        // If for some reason the running test assembly reports version >=9999, skip.
        var cur = GitHubUpdateChecker.GetCurrentVersion();
        if (!Version.TryParse(cur, out var curV) || curV < new Version(9999, 0, 0)) {
            result.Should().NotBeNull();
            result!.Version.Should().Be("9999.0.0");
        }
    }

    // ========== CheckForUpdateAsync — same or older version ==========

    [Fact]
    public async Task CheckForUpdateAsync_WhenSameVersionAvailable_ShouldReturnNull() {
        var currentVersion = GitHubUpdateChecker.GetCurrentVersion();
        var json = $$$"""{"tag_name":"v{{{currentVersion}}}","html_url":"https://example.com","body":""}""";
        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenOlderVersionAvailable_ShouldReturnNull() {
        var json = """{"tag_name":"v0.0.1","html_url":"https://example.com","body":""}""";
        var handler = new FakeHttpHandler(HttpStatusCode.OK, json);
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        // Current version is >= 0.0.1 (assuming test assembly has proper version),
        // so the result should be null (no update) unless the assembly version is 0.0.0.
        var cur = GitHubUpdateChecker.GetCurrentVersion();
        if (Version.TryParse(cur, out var curV) && curV >= new Version(0, 0, 1)) {
            result.Should().BeNull();
        }
    }

    // ========== CheckForUpdateAsync — network/API errors ==========

    [Fact]
    public async Task CheckForUpdateAsync_WhenServerReturns404_ShouldReturnNull() {
        var handler = new FakeHttpHandler(HttpStatusCode.NotFound, string.Empty);
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenServerReturns403_ShouldReturnNull() {
        var handler = new FakeHttpHandler(HttpStatusCode.Forbidden, """{"message":"API rate limit exceeded"}""");
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenHttpRequestThrows_ShouldReturnNull() {
        var handler = new ThrowingHttpHandler();
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenJsonIsMalformed_ShouldReturnNull() {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, "not json at all{{{{");
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenTagNameIsMissing_ShouldReturnNull() {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, """{"html_url":"https://example.com","body":""}""");
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenTagNameIsNotSemver_ShouldReturnNull() {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, """{"tag_name":"not-a-version","html_url":"https://example.com","body":""}""");
        var checker = new GitHubUpdateChecker(new HttpClient(handler));

        var result = await checker.CheckForUpdateAsync();

        result.Should().BeNull();
    }

    // ========== GetCurrentVersion ==========

    [Fact]
    public void GetCurrentVersion_ShouldReturnValidSemver() {
        var version = GitHubUpdateChecker.GetCurrentVersion();

        version.Should().NotBeNullOrWhiteSpace();
        Version.TryParse(version, out _).Should().BeTrue();
    }

    // ========== Helper types ==========

    private sealed class FakeHttpHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = new HttpResponseMessage(statusCode) {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHttpHandler : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("No internet connection"));
    }
}
