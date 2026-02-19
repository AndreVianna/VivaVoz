namespace VivaVoz.Services;

public static class LoggingService {
    private const string _outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static void Configure() {
        Directory.CreateDirectory(FilePaths.LogsDirectory);

        var logFilePath = Path.Combine(FilePaths.LogsDirectory, "vivavoz-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: logFilePath,
                outputTemplate: _outputTemplate,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31)
            .CreateLogger();
    }
}
