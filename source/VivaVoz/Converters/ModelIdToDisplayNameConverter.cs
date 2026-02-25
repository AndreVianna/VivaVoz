using System.Globalization;

using Avalonia.Data.Converters;

namespace VivaVoz.Converters;

public class ModelIdToDisplayNameConverter : IValueConverter {
    public static readonly ModelIdToDisplayNameConverter Instance = new();

    private static readonly Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase) {
        ["tiny"] = "Tiny (~75 MB)",
        ["base"] = "Base (~142 MB)",
        ["small"] = "Small (~466 MB)",
        ["medium"] = "Medium (~1.5 GB)",
        ["large-v3"] = "Large v3 (~2.9 GB)",
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string id && _displayNames.TryGetValue(id, out var name) ? name : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
