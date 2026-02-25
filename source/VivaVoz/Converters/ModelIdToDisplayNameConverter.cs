using System.Globalization;

using Avalonia.Data.Converters;

namespace VivaVoz.Converters;

public class ModelIdToDisplayNameConverter : IValueConverter {
    public static readonly ModelIdToDisplayNameConverter Instance = new();

    private static readonly Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase) {
        ["tiny"] = "Tiny",
        ["base"] = "Base",
        ["small"] = "Small",
        ["medium"] = "Medium",
        ["large-v3"] = "Large v3",
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string id && _displayNames.TryGetValue(id, out var name) ? name : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
