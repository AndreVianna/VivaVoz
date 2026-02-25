namespace VivaVoz.Helpers;

public static class LanguageHelper {
    public static string GetDisplayName(string? isoCode) {
        if (string.IsNullOrEmpty(isoCode))
            return "Unknown";
        if (isoCode == "auto")
            return "Auto-detected";
        try {
            return CultureInfo.GetCultureInfo(isoCode).EnglishName;
        } catch {
            return isoCode;
        }
    }
}
