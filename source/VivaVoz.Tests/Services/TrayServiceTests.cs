using AwesomeAssertions;
using VivaVoz.Services;
using Xunit;

namespace VivaVoz.Tests.Services;

public class TrayServiceTests {
    // ========== TrayService.FormatTooltipText ==========

    [Fact]
    public void FormatTooltipText_WithNullTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(null);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithEmptyTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(string.Empty);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithShortTranscript_ShouldReturnFullText() {
        var result = TrayService.FormatTooltipText("Hello world");

        result.Should().Be("VivaVoz — Hello world");
    }

    [Fact]
    public void FormatTooltipText_WithLongTranscript_ShouldTruncateTo30Chars() {
        var transcript = "This is a very long transcript that should be truncated";

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be("VivaVoz — This is a very long transcript...");
    }

    [Fact]
    public void FormatTooltipText_WithExactly30CharTranscript_ShouldNotTruncate() {
        var transcript = new string('a', 30); // exactly 30 chars

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be($"VivaVoz — {transcript}");
        result.Should().NotContain("...");
    }

    [Fact]
    public void FormatTooltipText_With31CharTranscript_ShouldTruncateWithEllipsis() {
        var transcript = new string('a', 31); // boundary: one over the limit

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be($"VivaVoz — {new string('a', 30)}...");
    }

    [Fact]
    public void FormatTooltipText_WithWhitespaceOnly_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText("   ");

        result.Should().Be("VivaVoz — No speech detected.");
    }

    // ========== TrayService.GetTooltipForState ==========

    [Fact]
    public void GetTooltipForState_WhenIdle_ShouldReturnIdleText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Idle);

        result.Should().Be("VivaVoz");
    }

    [Fact]
    public void GetTooltipForState_WhenRecording_ShouldReturnRecordingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Recording);

        result.Should().Be("VivaVoz — Recording...");
    }

    [Fact]
    public void GetTooltipForState_WhenTranscribing_ShouldReturnTranscribingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Transcribing);

        result.Should().Be("VivaVoz — Transcribing...");
    }

    [Fact]
    public void GetTooltipForState_WithInvalidEnumValue_ShouldReturnDefault() {
        var invalidState = (TrayIconState)999;

        var result = TrayService.GetTooltipForState(invalidState);

        result.Should().Be("VivaVoz");
    }
}
