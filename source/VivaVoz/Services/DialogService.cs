using Avalonia.Platform.Storage;

namespace VivaVoz.Services;

/// <summary>
/// Avalonia-backed dialog service. Shows native-style confirmation and file-picker dialogs.
/// </summary>
[ExcludeFromCodeCoverage]
public class DialogService : IDialogService {
    /// <inheritdoc />
    public async Task<string?> ShowSaveFileDialogAsync(
        string title,
        string suggestedFileName,
        string defaultExtension,
        string filterName,
        string[] filterPatterns) {
        var mainWindow = GetMainWindow();
        if (mainWindow is null)
            return null;

        var topLevel = TopLevel.GetTopLevel(mainWindow);
        if (topLevel is null)
            return null;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = defaultExtension,
            FileTypeChoices = [new FilePickerFileType(filterName) { Patterns = filterPatterns }]
        });

        return file?.TryGetLocalPath();
    }

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

    private static Window? GetMainWindow() => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
}
