namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class AboutWindow : Window {
    public AboutWindow() => InitializeComponent();

    private void OnOkClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
