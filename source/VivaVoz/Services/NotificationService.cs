namespace VivaVoz.Services;

/// <summary>
/// Avalonia-backed notification service. Shows warning toasts, recoverable error dialogs,
/// and catastrophic error screens using code-constructed Avalonia windows.
/// </summary>
[ExcludeFromCodeCoverage]
public class NotificationService : INotificationService {
    /// <inheritdoc />
    public async Task ShowWarningAsync(string message) {
        var tcs = new TaskCompletionSource();
        var dismissButton = new Button { Content = "Dismiss" };

        var window = new Window {
            Title = "Warning",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 300,
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
                        Children = { dismissButton }
                    }
                }
            }
        };

        dismissButton.Click += (_, _) => { tcs.TrySetResult(); window.Close(); };
        window.Closed += (_, _) => tcs.TrySetResult();

        ShowWindow(window);

        // Auto-dismiss after 4 seconds
        _ = Task.Delay(4000).ContinueWith(_ => Dispatcher.UIThread.Post(() => {
            if (!tcs.Task.IsCompleted) {
                tcs.TrySetResult();
                window.Close();
            }
        }));

        await tcs.Task;
    }

    /// <inheritdoc />
    public Task<bool> ShowRecoverableErrorAsync(
        string title,
        string message,
        string primaryLabel = "Retry",
        string cancelLabel = "Cancel") {
        var tcs = new TaskCompletionSource<bool>();
        var primaryButton = new Button { Content = primaryLabel };
        var cancelButton = new Button { Content = cancelLabel };

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
                        Children = { cancelButton, primaryButton }
                    }
                }
            }
        };

        primaryButton.Click += (_, _) => { tcs.TrySetResult(true); window.Close(); };
        cancelButton.Click += (_, _) => { tcs.TrySetResult(false); window.Close(); };
        window.Closed += (_, _) => tcs.TrySetResult(false);

        ShowModalWindow(window);
        return tcs.Task;
    }

    /// <inheritdoc />
    public Task ShowCatastrophicErrorAsync(string title, string message) {
        var tcs = new TaskCompletionSource();
        var restartButton = new Button { Content = "Restart" };
        var dismissButton = new Button { Content = "Dismiss" };

        var window = new Window {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            MinWidth = 400,
            MaxWidth = 600,
            Content = new StackPanel {
                Spacing = 16,
                Margin = new Thickness(24),
                Children = {
                    new TextBlock {
                        Text = title,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        FontSize = 18
                    },
                    new TextBlock {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        MaxWidth = 550
                    },
                    new StackPanel {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { dismissButton, restartButton }
                    }
                }
            }
        };

        restartButton.Click += (_, _) => {
            tcs.TrySetResult();
            window.Close();
            Environment.Exit(1);
        };
        dismissButton.Click += (_, _) => { tcs.TrySetResult(); window.Close(); };
        window.Closed += (_, _) => tcs.TrySetResult();

        ShowModalWindow(window);
        return tcs.Task;
    }

    private static void ShowModalWindow(Window window) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null) {
            _ = window.ShowDialog(desktop.MainWindow);
        }
        else {
            window.Show();
        }
    }

    private static void ShowWindow(Window window) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null) {
            window.Show(desktop.MainWindow);
        }
        else {
            window.Show();
        }
    }
}
