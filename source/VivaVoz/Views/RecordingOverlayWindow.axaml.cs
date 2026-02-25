using Avalonia.Input;

namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class RecordingOverlayWindow : Window {
    private readonly ISettingsService? _settingsService;
    private bool _positionInitialized;

    public RecordingOverlayWindow() => InitializeComponent();

    public RecordingOverlayWindow(ISettingsService settingsService) : this() {
        _settingsService = settingsService;
        PositionChanged += OnPositionChanged;
    }

    /// <summary>Shows the overlay without stealing keyboard focus from the active window.</summary>
    public void ShowOverlay() => Show();

    private void OnBorderPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        if (_positionInitialized)
            return;
        _positionInitialized = true;

        var settings = _settingsService?.Current;
        if (settings?.OverlayX is { } x && settings.OverlayY is { } y) {
            Position = new PixelPoint(x, y);
        }
        else {
            PlaceAtBottomCenter();
        }
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e) {
        if (!_positionInitialized)
            return;
        if (_settingsService?.Current is not { } settings)
            return;

        settings.OverlayX = Position.X;
        settings.OverlayY = Position.Y;
        _ = _settingsService.SaveSettingsAsync(settings);
    }

    private void PlaceAtBottomCenter() {
        var screen = Screens.Primary;
        if (screen is null)
            return;

        var workArea = screen.WorkingArea;
        var position = ComputeDefaultPosition(workArea, (int)Width, (int)Height);
        Position = position;
    }

    internal static PixelPoint ComputeDefaultPosition(PixelRect workArea, int windowWidth, int windowHeight) {
        var x = workArea.X + ((workArea.Width - windowWidth) / 2);
        var y = workArea.Y + workArea.Height - windowHeight - 40;
        return new PixelPoint(x, y);
    }
}
