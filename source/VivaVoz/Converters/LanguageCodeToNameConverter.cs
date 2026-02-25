using Avalonia.Data.Converters;

using VivaVoz.Helpers;

namespace VivaVoz.Converters;

public class LanguageCodeToNameConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        LanguageHelper.GetDisplayName(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
