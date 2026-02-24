namespace VivaVoz.Models;

/// <summary>
/// Represents a system-wide hotkey combination (modifier keys + virtual key).
/// Used to configure the global recording hotkey in the application settings.
/// </summary>
public class HotkeyConfig {
    /// <summary>Alt modifier key flag (MOD_ALT = 0x0001).</summary>
    public const uint ModAlt = 0x0001;

    /// <summary>Control modifier key flag (MOD_CONTROL = 0x0002).</summary>
    public const uint ModControl = 0x0002;

    /// <summary>Shift modifier key flag (MOD_SHIFT = 0x0004).</summary>
    public const uint ModShift = 0x0004;

    /// <summary>Windows key modifier flag (MOD_WIN = 0x0008).</summary>
    public const uint ModWin = 0x0008;

    /// <summary>Bit-mask of modifier keys (Alt, Ctrl, Shift, Win).</summary>
    public uint Modifiers { get; init; }

    /// <summary>Virtual-key code (e.g. <c>'R'</c> = 0x52).</summary>
    public uint VirtualKey { get; init; }

    /// <summary>The default hotkey: Ctrl+Shift+R.</summary>
    public static HotkeyConfig Default => new() {
        Modifiers = ModControl | ModShift,
        VirtualKey = 'R'
    };

    /// <summary>
    /// Parses a hotkey string such as <c>"Ctrl+Shift+R"</c> into a <see cref="HotkeyConfig"/>.
    /// Returns <see langword="null"/> if the string is empty, whitespace, or contains no
    /// recognisable key (letter or digit).
    /// </summary>
    /// <param name="config">The hotkey string to parse.</param>
    public static HotkeyConfig? Parse(string? config) {
        if (string.IsNullOrWhiteSpace(config))
            return null;

        var parts = config.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        uint modifiers = 0;
        uint? virtualKey = null;

        foreach (var part in parts) {
            switch (part.ToUpperInvariant()) {
                case "CTRL":
                case "CONTROL":
                    modifiers |= ModControl;
                    break;
                case "ALT":
                    modifiers |= ModAlt;
                    break;
                case "SHIFT":
                    modifiers |= ModShift;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= ModWin;
                    break;
                default:
                    if (part.Length == 1 && char.IsLetterOrDigit(part[0]))
                        virtualKey = char.ToUpperInvariant(part[0]);
                    break;
            }
        }

        return virtualKey.HasValue
            ? new HotkeyConfig { Modifiers = modifiers, VirtualKey = virtualKey.Value }
            : null;
    }

    /// <summary>
    /// Returns a human-readable string such as <c>"Ctrl+Shift+R"</c>.
    /// The format is compatible with <see cref="Parse"/>.
    /// </summary>
    public override string ToString() {
        var parts = new List<string>(5);

        if ((Modifiers & ModControl) != 0)
            parts.Add("Ctrl");
        if ((Modifiers & ModAlt) != 0)
            parts.Add("Alt");
        if ((Modifiers & ModShift) != 0)
            parts.Add("Shift");
        if ((Modifiers & ModWin) != 0)
            parts.Add("Win");

        parts.Add(((char)VirtualKey).ToString());

        return string.Join("+", parts);
    }
}
