using AwesomeAssertions;

using VivaVoz.Constants;

using Xunit;

namespace VivaVoz.Tests.Constants;

public class FilePathsTests {
    [Fact]
    public void AppDataDirectory_ShouldEndWithVivaVoz() {
        var directoryName = Path.GetFileName(FilePaths.AppDataDirectory.TrimEnd(Path.DirectorySeparatorChar));

        directoryName.Should().BeEquivalentTo("VivaVoz");
    }

    [Fact]
    public void SubDirectories_ShouldBeUnderAppDataDirectory() {
        var root = Path.GetFullPath(FilePaths.AppDataDirectory);

        Path.GetFullPath(FilePaths.DataDirectory).Should().StartWith(root);
        Path.GetFullPath(FilePaths.AudioDirectory).Should().StartWith(root);
        Path.GetFullPath(FilePaths.ModelsDirectory).Should().StartWith(root);
        Path.GetFullPath(FilePaths.LogsDirectory).Should().StartWith(root);
    }

    [Fact]
    public void SubDirectories_ShouldHaveExpectedFolderNames() {
        Path.GetFileName(FilePaths.DataDirectory).Should().Be("data");
        Path.GetFileName(FilePaths.AudioDirectory).Should().Be("audio");
        Path.GetFileName(FilePaths.ModelsDirectory).Should().Be("models");
        Path.GetFileName(FilePaths.LogsDirectory).Should().Be("logs");
    }
}
