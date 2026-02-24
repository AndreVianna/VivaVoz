namespace VivaVoz.Services;

/// <summary>
/// Avalonia-backed dialog service. Shows a native-style confirmation window.
/// </summary>
[ExcludeFromCodeCoverage]
public class DialogService : IDialogService {
    /// <inheritdoc />
    public Task<bool> ShowConfirmAsync(string title, string message) {
        var tcs = new TaskCompletionSource<bool>();

        var yesButton = new Button { Content = "Yes" };
        var noButton = new Button { Content = "No" };

        var window = new Window {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 320,
            Content = new StackPanel {
                Spacing = 16,
                Margin = new Thickness(24),
                Children = {
                    new TextBlock {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        MaxWidth = 400
                    },
                    new StackPanel {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { noButton, yesButton }
                    }
                }
            }
        };

        yesButton.Click += (_, _) => {
            tcs.TrySetResult(true);
            window.Close();
        };
        noButton.Click += (_, _) => {
            tcs.TrySetResult(false);
            window.Close();
        };
        window.Closed += (_, _) => tcs.TrySetResult(false);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null) {
            _ = window.ShowDialog(desktop.MainWindow);
        }
        else {
            window.Show();
        }

        return tcs.Task;
    }
}
