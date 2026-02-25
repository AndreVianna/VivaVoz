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

        var hotkeyService = new GlobalHotkeyService();
        var parsedHotkey = HotkeyConfig.Parse(settingsService.Current?.HotkeyConfig);
        hotkeyService.TryRegister(parsedHotkey ?? HotkeyConfig.Default, settingsService.Current?.RecordingMode ?? "Toggle");

        var updateChecker = new GitHubUpdateChecker(new System.Net.Http.HttpClient());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            // Create TrayIconService with a deferred callback to TrayService (assigned below).
            // The callback maps AppState → TrayIconState so MainViewModel state transitions
            // drive the actual tray icon without coupling the ViewModel to Avalonia.
            ITrayService? trayService = null;
            var trayIconService = new TrayIconService(appState => {
                if (trayService is null) return;
                trayService.SetState(appState switch {
                    AppState.Recording => TrayIconState.Recording,
                    AppState.Transcribing => TrayIconState.Transcribing,
                    AppState.Ready => TrayIconState.Ready,
                    _ => TrayIconState.Idle,
                });
            });

            var mainWindow = new MainWindow(settingsService) {
                DataContext = new MainViewModel(recorderService, audioPlayerService, dbContext, transcriptionManager, clipboardService, settingsService, modelService, recordingService, dialogService, exportService, crashRecoveryService, notificationService, trayIconService: trayIconService, hotkeyService: hotkeyService, updateChecker: updateChecker)
            };

            // Show the onboarding wizard on first launch
            if (!(settingsService.Current?.HasCompletedOnboarding ?? false)) {
                mainWindow.Opened += async (_, _) => {
                    var onboardingVm = new OnboardingViewModel(
                        settingsService, modelService, recorderService, whisperEngine);
                    var onboardingWindow = new OnboardingWindow { DataContext = onboardingVm };
                    onboardingVm.WizardCompleted += (_, _) => onboardingWindow.Close();
                    await onboardingWindow.ShowDialog(mainWindow);

                    // Re-register hotkey in case user changed it during onboarding
                    var updatedHotkey = HotkeyConfig.Parse(settingsService.Current?.HotkeyConfig);
                    hotkeyService.TryRegister(
                        updatedHotkey ?? HotkeyConfig.Default,
                        settingsService.Current?.RecordingMode ?? "Toggle");
                };
            }

            // Check for updates on startup (non-blocking)
            if (settingsService.Current?.CheckForUpdatesOnStartup == true) {
                mainWindow.Opened += async (_, _) => {
                    try {
                        var info = await updateChecker.CheckForUpdateAsync();
                        if (info is not null)
                            ShowUpdateAvailableDialog(mainWindow, info);
                    }
                    catch {
                        // Silently ignore update check errors
                    }
                };
            }

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

            trayService = new TrayService(desktop, recorderService, transcriptionManager);
            trayService.Initialize();

            desktop.MainWindow = mainWindow;

            if (settingsService.Current?.StartMinimized == true) {
                mainWindow.ShowInTaskbar = false;
            }

            desktop.ShutdownRequested += (_, _) => {
                trayService?.Dispose();
                hotkeyService.Dispose();
            };
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

    private static void ShowUpdateAvailableDialog(Window owner, UpdateInfo info) {
        var downloadButton = new Button { Content = "Download", Margin = new Avalonia.Thickness(0, 0, 8, 0) };
        var dismissButton = new Button { Content = "Dismiss" };

        var window = new Window {
            Title = "Update Available",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 340,
            Content = new StackPanel {
                Spacing = 16,
                Margin = new Avalonia.Thickness(24),
                Children = {
                    new TextBlock {
                        Text = $"VivaVoz v{info.Version} is available!",
                        FontWeight = Avalonia.Media.FontWeight.SemiBold,
                        FontSize = 16
                    },
                    new StackPanel {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Children = { downloadButton, dismissButton }
                    }
                }
            }
        };

        downloadButton.Click += (_, _) => {
            try {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(info.DownloadUrl) { UseShellExecute = true });
            }
            catch { /* best-effort */ }
            window.Close();
        };
        dismissButton.Click += (_, _) => window.Close();

        window.Show(owner);
    }
}
