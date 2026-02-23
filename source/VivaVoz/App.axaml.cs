using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;
using VivaVoz.Views;

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
        var recorderService = new AudioRecorderService();
        var audioPlayerService = new AudioPlayerService();
        var modelManager = new WhisperModelManager();
        var whisperEngine = new WhisperTranscriptionEngine(modelManager);
        var transcriptionManager = new TranscriptionManager(whisperEngine, () => new AppDbContext());
        var clipboardService = new ClipboardService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var mainWindow = new MainWindow(settingsService) {
                DataContext = new MainViewModel(recorderService, audioPlayerService, dbContext, transcriptionManager, clipboardService, settingsService)
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

        if (orphaned.Count == 0) return;

        foreach (var recording in orphaned) {
            recording.Status = RecordingStatus.PendingTranscription;
        }

        await dbContext.SaveChangesAsync();
        Log.Information("[App] Reset {Count} orphaned Transcribing recording(s) to PendingTranscription.", orphaned.Count);
    }
}
