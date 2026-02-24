namespace VivaVoz;

[ExcludeFromCodeCoverage]
public partial class App : Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted() {
        InitializeFileSystem();
        var dbContext = InitializeDatabase();
        await RecoverOrphanedTranscriptionsAsync(dbContext);
        var settingsService = new SettingsService(() => new AppDbContext());
        await settingsService.LoadSettingsAsync();
        var themeService = new ThemeService();
        themeService.ApplyTheme(settingsService.Current?.Theme ?? "System");
        var recorderService = new AudioRecorderService();
        var audioPlayerService = new AudioPlayerService();
        var modelManager = new WhisperModelManager();
        var modelService = new WhisperModelService(modelManager, new System.Net.Http.HttpClient());
        var whisperEngine = new WhisperTranscriptionEngine(modelManager);
        var transcriptionManager = new TranscriptionManager(whisperEngine, () => new AppDbContext(), modelService, settingsService);
        var clipboardService = new ClipboardService();
        var recordingService = new RecordingService(() => new AppDbContext());
        var dialogService = new DialogService();
        var exportService = new ExportService();
        var crashRecoveryService = new CrashRecoveryService();
        var notificationService = new NotificationService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var mainWindow = new MainWindow(settingsService) {
                DataContext = new MainViewModel(recorderService, audioPlayerService, dbContext, transcriptionManager, clipboardService, settingsService, modelService, recordingService, dialogService, exportService, crashRecoveryService, notificationService)
            };

            var overlayViewModel = new RecordingOverlayViewModel(recorderService);
            var overlayWindow = new RecordingOverlayWindow(settingsService) { DataContext = overlayViewModel };

            overlayViewModel.PropertyChanged += (_, e) => {
                if (e.PropertyName != nameof(RecordingOverlayViewModel.IsRecording))
                    return;
                if (overlayViewModel.IsRecording)
                    overlayWindow.ShowOverlay();
                else
                    overlayWindow.Hide();
            };

            var trayService = new TrayService(desktop, recorderService, transcriptionManager);
            trayService.Initialize();

            desktop.MainWindow = mainWindow;

            if (settingsService.Current?.StartMinimized == true) {
                mainWindow.ShowInTaskbar = false;
            }

            desktop.ShutdownRequested += (_, _) => trayService.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializeFileSystem() {
        var fileSystemService = new FileSystemService();
        FileSystemService.EnsureAppDirectories();
    }

    private static AppDbContext InitializeDatabase() {
        var dbContext = new AppDbContext();
        dbContext.Database.Migrate();
        return dbContext;
    }

    private static async Task RecoverOrphanedTranscriptionsAsync(AppDbContext dbContext) {
        var orphaned = await dbContext.Recordings
            .Where(r => r.Status == RecordingStatus.Transcribing)
            .ToListAsync();

        if (orphaned.Count == 0)
            return;

        foreach (var recording in orphaned) {
            recording.Status = RecordingStatus.PendingTranscription;
        }

        await dbContext.SaveChangesAsync();
        Log.Information("[App] Reset {Count} orphaned Transcribing recording(s) to PendingTranscription.", orphaned.Count);
    }
}
