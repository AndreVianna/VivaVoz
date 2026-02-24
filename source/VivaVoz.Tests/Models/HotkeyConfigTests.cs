using AwesomeAssertions;

using VivaVoz.Models;

using Xunit;

namespace VivaVoz.Tests.Models;

public class HotkeyConfigTests {
    [Fact]
    public void Parse_WithNullString_ShouldReturnNull() {
        var result = HotkeyConfig.Parse(null);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WithEmptyString_ShouldReturnNull() {
        var result = HotkeyConfig.Parse(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WithWhitespaceString_ShouldReturnNull() {
        var result = HotkeyConfig.Parse("   ");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WithOnlyModifiers_ShouldReturnNull() {
        var result = HotkeyConfig.Parse("Ctrl+Shift");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_WithCtrlShiftR_ShouldReturnCorrectModifiers() {
        var result = HotkeyConfig.Parse("Ctrl+Shift+R");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModControl | HotkeyConfig.ModShift);
    }

    [Fact]
    public void Parse_WithCtrlShiftR_ShouldReturnCorrectVirtualKey() {
        var result = HotkeyConfig.Parse("Ctrl+Shift+R");

        result.Should().NotBeNull();
        result!.VirtualKey.Should().Be('R');
    }

    [Fact]
    public void Parse_WithControlModifierSpelling_ShouldParseCorrectly() {
        var result = HotkeyConfig.Parse("Control+Shift+R");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModControl | HotkeyConfig.ModShift);
    }

    [Fact]
    public void Parse_WithLowercaseModifiers_ShouldParseCorrectly() {
        var result = HotkeyConfig.Parse("ctrl+shift+r");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModControl | HotkeyConfig.ModShift);
        result!.VirtualKey.Should().Be('R');
    }

    [Fact]
    public void Parse_WithAltModifier_ShouldIncludeAltFlag() {
        var result = HotkeyConfig.Parse("Alt+F");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModAlt);
    }

    [Fact]
    public void Parse_WithWinModifier_ShouldIncludeWinFlag() {
        var result = HotkeyConfig.Parse("Win+D");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModWin);
    }

    [Fact]
    public void Parse_WithWindowsModifierSpelling_ShouldParseCorrectly() {
        var result = HotkeyConfig.Parse("Windows+D");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(HotkeyConfig.ModWin);
    }

    [Fact]
    public void Parse_WithSingleLetterKey_ShouldReturnConfigWithNoModifiers() {
        var result = HotkeyConfig.Parse("A");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(0u);
        result!.VirtualKey.Should().Be('A');
    }

    [Fact]
    public void Parse_WithDigitKey_ShouldReturnCorrectVirtualKey() {
        var result = HotkeyConfig.Parse("Ctrl+1");

        result.Should().NotBeNull();
        result!.VirtualKey.Should().Be('1');
    }

    [Fact]
    public void Parse_WithAllModifiers_ShouldReturnAllFlags() {
        var result = HotkeyConfig.Parse("Ctrl+Alt+Shift+Win+R");

        result.Should().NotBeNull();
        result!.Modifiers.Should().Be(
            HotkeyConfig.ModControl | HotkeyConfig.ModAlt | HotkeyConfig.ModShift | HotkeyConfig.ModWin);
    }

    [Fact]
    public void Default_ShouldHaveControlAndShiftModifiers() {
        var config = HotkeyConfig.Default;

        config.Modifiers.Should().Be(HotkeyConfig.ModControl | HotkeyConfig.ModShift);
    }

    [Fact]
    public void Default_ShouldHaveRAsVirtualKey() {
        var config = HotkeyConfig.Default;

        config.VirtualKey.Should().Be('R');
    }

    [Fact]
    public void ToString_WithCtrlShiftR_ShouldReturnReadableString() {
        var config = HotkeyConfig.Parse("Ctrl+Shift+R")!;

        var result = config.ToString();

        result.Should().Be("Ctrl+Shift+R");
    }

    [Fact]
    public void ToString_WithOnlyKey_ShouldReturnJustKey() {
        var config = HotkeyConfig.Parse("A")!;

        var result = config.ToString();

        result.Should().Be("A");
    }

    [Fact]
    public void ToString_WithAltAndKey_ShouldIncludeAlt() {
        var config = HotkeyConfig.Parse("Alt+F")!;

        var result = config.ToString();

        result.Should().Be("Alt+F");
    }

    [Fact]
    public void Parse_RoundTrip_ShouldPreserveConfig() {
        var original = HotkeyConfig.Default;
        var serialized = original.ToString();

        var parsed = HotkeyConfig.Parse(serialized);

        parsed.Should().NotBeNull();
        parsed!.Modifiers.Should().Be(original.Modifiers);
        parsed.VirtualKey.Should().Be(original.VirtualKey);
    }

    [Fact]
    public void ModControl_ShouldBe2() => HotkeyConfig.ModControl.Should().Be(0x0002u);

    [Fact]
    public void ModAlt_ShouldBe1() => HotkeyConfig.ModAlt.Should().Be(0x0001u);

    [Fact]
    public void ModShift_ShouldBe4() => HotkeyConfig.ModShift.Should().Be(0x0004u);

    [Fact]
    public void ModWin_ShouldBe8() => HotkeyConfig.ModWin.Should().Be(0x0008u);
}
