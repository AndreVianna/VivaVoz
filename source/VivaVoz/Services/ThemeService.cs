using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Styling;

namespace VivaVoz.Services;

[ExcludeFromCodeCoverage]
public class ThemeService : IThemeService {
    public void ApplyTheme(string theme) {
        if (Application.Current is null) return;
        Application.Current.RequestedThemeVariant = theme switch {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }
}
