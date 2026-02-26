using Avalonia.Input;

namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (DataContext is not SettingsViewModel vm || !vm.IsListeningForHotkey)
            return;

        // Ignore modifier-only key presses — wait for an actual key
        if (e.Key is Key.LeftShift or Key.RightShift
                  or Key.LeftCtrl or Key.RightCtrl
                  or Key.LeftAlt or Key.RightAlt
                  or Key.LWin or Key.RWin
                  or Key.None) {
            return;
        }

        var parts = new List<string>(4);
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (e.KeyModifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Win");
        parts.Add(e.Key.ToString());

        vm.AcceptHotkeyCapture(string.Join("+", parts));
        e.Handled = true;
    }
}
