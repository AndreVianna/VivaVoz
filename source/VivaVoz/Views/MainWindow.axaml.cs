using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using VivaVoz.Services;

namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class MainWindow : Window {
    private ISettingsService? _settingsService;

    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(ISettingsService settingsService) : this() {
        _settingsService = settingsService;
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (_settingsService?.Current?.MinimizeToTray == true) {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
