using AwesomeAssertions;

using VivaVoz.Services;

using Xunit;

namespace VivaVoz.Tests.Services;

public class FileSystemServiceTests {
    [Fact]
    public void EnsureAppDirectories_ShouldNotThrow() {
        var act = FileSystemService.EnsureAppDirectories;

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureAppDirectories_WhenCalledTwice_ShouldBeIdempotent() {
        FileSystemService.EnsureAppDirectories();

        var act = FileSystemService.EnsureAppDirectories;

        act.Should().NotThrow();
    }
}
