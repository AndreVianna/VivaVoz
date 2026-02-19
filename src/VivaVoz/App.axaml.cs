using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using VivaVoz.Data;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.ViewModels;
using VivaVoz.Views;

namespace VivaVoz;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeFileSystem();
        var dbContext = InitializeDatabase();
        var recorderService = new AudioRecorderService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(recorderService, dbContext)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializeFileSystem()
    {
        var fileSystemService = new FileSystemService();
        fileSystemService.EnsureAppDirectories();
    }

    private static AppDbContext InitializeDatabase()
    {
        var dbContext = new AppDbContext();
        dbContext.Database.Migrate();
        return dbContext;
    }
}
