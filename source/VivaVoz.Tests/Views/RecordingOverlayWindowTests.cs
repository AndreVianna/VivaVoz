using Avalonia;

using AwesomeAssertions;

using VivaVoz.Views;

using Xunit;

namespace VivaVoz.Tests.Views;

public class RecordingOverlayWindowTests {
    // ========== ComputeDefaultPosition ==========

    [Fact]
    public void ComputeDefaultPosition_ShouldCenterHorizontally() {
        var workArea = new PixelRect(0, 0, 1920, 1080);

        var result = RecordingOverlayWindow.ComputeDefaultPosition(workArea, 220, 52);

        result.X.Should().Be((1920 - 220) / 2);
    }

    [Fact]
    public void ComputeDefaultPosition_ShouldPlaceNearBottom() {
        var workArea = new PixelRect(0, 0, 1920, 1080);

        var result = RecordingOverlayWindow.ComputeDefaultPosition(workArea, 220, 52);

        result.Y.Should().Be(1080 - 52 - 40);
    }

    [Fact]
    public void ComputeDefaultPosition_WithOffset_ShouldIncludeWorkAreaOffset() {
        var workArea = new PixelRect(100, 50, 1920, 1080);

        var result = RecordingOverlayWindow.ComputeDefaultPosition(workArea, 220, 52);

        result.X.Should().Be(100 + ((1920 - 220) / 2));
        result.Y.Should().Be(50 + 1080 - 52 - 40);
    }

    [Fact]
    public void ComputeDefaultPosition_WithNarrowScreen_ShouldStillCenter() {
        var workArea = new PixelRect(0, 0, 320, 480);

        var result = RecordingOverlayWindow.ComputeDefaultPosition(workArea, 220, 52);

        result.X.Should().Be((320 - 220) / 2);
        result.Y.Should().Be(480 - 52 - 40);
    }
}
