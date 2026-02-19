namespace VivaVoz.Constants;

public static class FilePaths {
    public static readonly string AppDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VivaVoz");

    public static readonly string DataDirectory = Path.Combine(AppDataDirectory, "data");
    public static readonly string AudioDirectory = Path.Combine(AppDataDirectory, "audio");
    public static readonly string ModelsDirectory = Path.Combine(AppDataDirectory, "models");
    public static readonly string LogsDirectory = Path.Combine(AppDataDirectory, "logs");
}
