using AwesomeAssertions;

using VivaVoz.Helpers;

using Xunit;

namespace VivaVoz.Tests.Helpers;

public class LanguageHelperTests {
    [Fact]
    public void GetDisplayName_WithPt_ShouldReturnPortuguese() {
        var result = LanguageHelper.GetDisplayName("pt");

        result.Should().Be("Portuguese");
    }

    [Fact]
    public void GetDisplayName_WithEn_ShouldReturnEnglish() {
        var result = LanguageHelper.GetDisplayName("en");

        result.Should().Be("English");
    }

    [Fact]
    public void GetDisplayName_WithFr_ShouldReturnFrench() {
        var result = LanguageHelper.GetDisplayName("fr");

        result.Should().Be("French");
    }

    [Fact]
    public void GetDisplayName_WithNull_ShouldReturnUnknown() {
        var result = LanguageHelper.GetDisplayName(null);

        result.Should().Be("Unknown");
    }

    [Fact]
    public void GetDisplayName_WithEmpty_ShouldReturnUnknown() {
        var result = LanguageHelper.GetDisplayName(string.Empty);

        result.Should().Be("Unknown");
    }

    [Fact]
    public void GetDisplayName_WithAuto_ShouldReturnAutoDetected() {
        var result = LanguageHelper.GetDisplayName("auto");

        result.Should().Be("Auto-detected");
    }

    [Fact]
    public void GetDisplayName_WithInvalidCode_ShouldReturnCodeAsIs() {
        var result = LanguageHelper.GetDisplayName("not/a/valid/code");

        result.Should().Be("not/a/valid/code");
    }
}
