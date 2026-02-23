using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;

namespace VivaVoz.Services;

[ExcludeFromCodeCoverage]
public class TrayService : ITrayService {
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly IAudioRecorder _recorder;
    private readonly ITranscriptionManager _transcriptionManager;
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _toggleRecordingItem;
    private TrayIconState _currentState = TrayIconState.Idle;
    private int _activeTranscriptions;

    private const string IdleIconUri = "avares://VivaVoz/Assets/vivavoz-mono-16x16.png";
    private const string ActiveIconUri = "avares://VivaVoz/Assets/vivavoz-16x16.png";

    public TrayService(
        IClassicDesktopStyleApplicationLifetime desktop,
        IAudioRecorder recorder,
        ITranscriptionManager transcriptionManager) {
        _desktop = desktop;
        _recorder = recorder;
        _transcriptionManager = transcriptionManager;
    }

    public void Initialize() {
        _toggleRecordingItem = new NativeMenuItem { Header = "Start Recording" };
        _toggleRecordingItem.Click += OnToggleRecordingClicked;

        var openItem = new NativeMenuItem { Header = "Open VivaVoz" };
        openItem.Click += (_, _) => ShowMainWindow();

        var settingsItem = new NativeMenuItem { Header = "Settings" };
        settingsItem.Click += (_, _) => ShowMainWindow();

        var exitItem = new NativeMenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => _desktop.Shutdown();

        var menu = new NativeMenu();
        menu.Items.Add(_toggleRecordingItem);
        menu.Items.Add(openItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(exitItem);

        _trayIcon = new TrayIcon {
            Icon = LoadIcon(IdleIconUri),
            ToolTipText = "VivaVoz",
            Menu = menu,
            IsVisible = true
        };
        _trayIcon.Clicked += (_, _) => ShowMainWindow();

        _recorder.RecordingStopped += OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted += OnTranscriptionCompleted;

        Log.Information("[TrayService] Tray icon initialized.");
    }

    public void SetState(TrayIconState state) {
        if (_trayIcon is null) return;
        _currentState = state;

        _trayIcon.Icon = LoadIcon(state == TrayIconState.Idle ? IdleIconUri : ActiveIconUri);
        _trayIcon.ToolTipText = GetTooltipForState(state);

        if (_toggleRecordingItem is not null) {
            _toggleRecordingItem.Header = state == TrayIconState.Recording
                ? "Stop Recording"
                : "Start Recording";
        }

        Log.Debug("[TrayService] State changed to {State}.", state);
    }

    public void ShowTranscriptionComplete(string? transcript) {
        if (_trayIcon is null) return;

        var window = _desktop.MainWindow;
        if (window is null || window.IsVisible) return;

        _trayIcon.ToolTipText = FormatTooltipText(transcript);
        Log.Information("[TrayService] Transcription complete notification shown.");
    }

    public void Dispose() {
        _recorder.RecordingStopped -= OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted -= OnTranscriptionCompleted;
        _trayIcon?.Dispose();
        _trayIcon = null;
    }

    private void OnToggleRecordingClicked(object? sender, EventArgs e) {
        if (_currentState == TrayIconState.Recording) {
            _recorder.StopRecording();
        }
        else {
            try {
                _recorder.StartRecording();
                SetState(TrayIconState.Recording);
            }
            catch (Exception ex) {
                Log.Error(ex, "[TrayService] Failed to start recording from tray.");
            }
        }
    }

    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e) {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => {
            Interlocked.Increment(ref _activeTranscriptions);
            SetState(TrayIconState.Transcribing);
        });
    }

    private void OnTranscriptionCompleted(object? sender, TranscriptionCompletedEventArgs e) {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => {
            var remaining = Interlocked.Decrement(ref _activeTranscriptions);

            if (remaining <= 0) {
                _activeTranscriptions = 0;
                SetState(TrayIconState.Idle);
            }

            if (e.Success) {
                ShowTranscriptionComplete(e.Transcript);
            }
        });
    }

    private void ShowMainWindow() {
        var window = _desktop.MainWindow;
        if (window is null) return;

        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private static WindowIcon LoadIcon(string avaloniaUri) {
        var uri = new Uri(avaloniaUri);
        using var stream = AssetLoader.Open(uri);
        return new WindowIcon(new Bitmap(stream));
    }

    // Testable static helpers
    public static string FormatTooltipText(string? transcript) {
        if (string.IsNullOrEmpty(transcript))
            return "VivaVoz — No speech detected.";

        if (transcript.Length <= 30)
            return $"VivaVoz — {transcript}";

        return $"VivaVoz — {transcript[..30]}...";
    }

    public static string GetTooltipForState(TrayIconState state) => state switch {
        TrayIconState.Recording => "VivaVoz — Recording...",
        TrayIconState.Transcribing => "VivaVoz — Transcribing...",
        _ => "VivaVoz"
    };
}
