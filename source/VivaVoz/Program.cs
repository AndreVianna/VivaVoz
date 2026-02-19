namespace VivaVoz;

internal static class Program {
    [STAThread]
    public static void Main(string[] args) {
        LoggingService.Configure();
        Log.Information("Application Starting");

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => {
            if (eventArgs.ExceptionObject is Exception exception) {
                Log.Fatal(exception, "Unhandled exception");
            }
            else {
                Log.Fatal("Unhandled exception: {ExceptionObject}", eventArgs.ExceptionObject);
            }
        };

        try {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception) {
            Log.Fatal(exception, "Application terminated unexpectedly");
            throw;
        }
        finally {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
