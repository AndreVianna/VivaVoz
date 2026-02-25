namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class MainWindow : Window {
    private readonly ISettingsService? _settingsService;

    public MainWindow() => InitializeComponent();

    public MainWindow(ISettingsService settingsService) : this() => _settingsService = settingsService;

    public static bool ShouldMinimizeToTray(ISettingsService? settingsService)
        => settingsService?.Current?.MinimizeToTray == true;

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (ShouldMinimizeToTray(_settingsService)) {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
