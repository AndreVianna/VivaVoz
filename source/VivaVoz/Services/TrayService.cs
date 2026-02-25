using Avalonia.Platform;

namespace VivaVoz.Services;

public class TrayService(
    IClassicDesktopStyleApplicationLifetime desktop,
    IAudioRecorder recorder,
    ITranscriptionManager transcriptionManager) : ITrayService {
    private readonly IClassicDesktopStyleApplicationLifetime _desktop = desktop ?? throw new ArgumentNullException(nameof(desktop));
    private readonly IAudioRecorder _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
    private readonly ITranscriptionManager _transcriptionManager = transcriptionManager ?? throw new ArgumentNullException(nameof(transcriptionManager));
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _toggleRecordingItem;
    private int _activeTranscriptions;
    private WindowIcon? _idleIcon;
    private WindowIcon? _recordingIcon;
    private WindowIcon? _transcribingIcon;
    private WindowIcon? _readyIcon;
    private CancellationTokenSource? _revertCts;

    private const string IdleIconUri = "avares://VivaVoz/Assets/TrayIcons/tray-idle.ico";
    private const string RecordingIconUri = "avares://VivaVoz/Assets/TrayIcons/tray-recording.ico";
    private const string TranscribingIconUri = "avares://VivaVoz/Assets/TrayIcons/tray-transcribing.ico";
    private const string ReadyIconUri = "avares://VivaVoz/Assets/TrayIcons/tray-ready.ico";

    /// <summary>
    /// The current tray icon state. Exposed as <c>internal</c> for unit testing.
    /// </summary>
    internal TrayIconState CurrentState { get; private set; } = TrayIconState.Idle;

    /// <summary>
    /// The number of in-flight transcriptions. Exposed as <c>internal</c> for unit testing.
    /// </summary>
    internal int ActiveTranscriptions => _activeTranscriptions;

    [ExcludeFromCodeCoverage(Justification = "Requires Avalonia platform and AssetLoader at runtime.")]
    public void Initialize() {
        _idleIcon = LoadIcon(IdleIconUri);
        _recordingIcon = LoadIcon(RecordingIconUri);
        _transcribingIcon = LoadIcon(TranscribingIconUri);
        _readyIcon = LoadIcon(ReadyIconUri);

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
            Icon = _idleIcon,
            ToolTipText = "VivaVoz",
            Menu = menu,
            IsVisible = true
        };
        _trayIcon.Clicked += (_, _) => ToggleMainWindowVisibility();

        _recorder.RecordingStarted += OnRecordingStarted;
        _recorder.RecordingStopped += OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted += OnTranscriptionCompleted;

        Log.Information("[TrayService] Tray icon initialized.");
    }

    public void SetState(TrayIconState state) {
        CancelPendingRevert();
        ApplyState(state);
    }

    public void SetStateTemporary(TrayIconState state, TimeSpan duration) {
        CancelPendingRevert();
        ApplyState(state);

        var cts = new CancellationTokenSource();
        _revertCts = cts;

        _ = RevertToIdleAsync(duration, cts.Token);
    }

    public void ShowTranscriptionComplete(string? transcript) {
        if (_trayIcon is null)
            return;
        if (!ShouldShowTranscriptionNotification(_desktop.MainWindow))
            return;

        _trayIcon.ToolTipText = FormatTooltipText(transcript);
        Log.Information("[TrayService] Transcription complete notification shown.");
    }

    public void Dispose() {
        _recorder.RecordingStarted -= OnRecordingStarted;
        _recorder.RecordingStopped -= OnRecordingStopped;
        _transcriptionManager.TranscriptionCompleted -= OnTranscriptionCompleted;
        CancelPendingRevert();
        _trayIcon?.Dispose();
        _trayIcon = null;
        _idleIcon = null;
        _recordingIcon = null;
        _transcribingIcon = null;
        _readyIcon = null;
        GC.SuppressFinalize(this);
    }

    // ── Internal logic handlers (testable without Avalonia) ────────────────────

    /// <summary>
    /// Handles recording started: transitions state to <see cref="TrayIconState.Recording"/>.
    /// Exposed as <c>internal</c> for unit testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal void HandleRecordingStarted() => SetState(TrayIconState.Recording);

    /// <summary>
    /// Handles recording stopped: increments in-flight transcriptions and transitions
    /// state to <see cref="TrayIconState.Transcribing"/>.
    /// Exposed as <c>internal</c> for unit testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal void HandleRecordingStopped() {
        Interlocked.Increment(ref _activeTranscriptions);
        SetState(TrayIconState.Transcribing);
    }

    /// <summary>
    /// Handles transcription completion: decrements in-flight counter.
    /// On success with no remaining transcriptions, temporarily shows
    /// <see cref="TrayIconState.Ready"/> before reverting to
    /// <see cref="TrayIconState.Idle"/> after 3 seconds.
    /// Exposed as <c>internal</c> for unit testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal void HandleTranscriptionCompleted(bool success, string? transcript) {
        var remaining = Interlocked.Decrement(ref _activeTranscriptions);

        if (remaining <= 0) {
            _activeTranscriptions = 0;
            if (success) {
                SetStateTemporary(TrayIconState.Ready, TimeSpan.FromSeconds(3));
            }
            else {
                SetState(TrayIconState.Idle);
            }
        }

        if (success) {
            ShowTranscriptionComplete(transcript);
        }
    }

    /// <summary>
    /// Returns <c>true</c> when the main window is hidden and a tray notification
    /// (tooltip update) should be shown. Exposed as <c>internal</c> for unit testing.
    /// </summary>
    internal static bool ShouldShowTranscriptionNotification(Window? window)
        => window?.IsVisible == false;

    // ── Avalonia event handlers (excluded from code coverage) ─────────────────

    [ExcludeFromCodeCoverage(Justification = "Dispatches to UI thread; tested via HandleRecordingStarted.")]
    private void OnRecordingStarted(object? sender, EventArgs e)
        => Avalonia.Threading.Dispatcher.UIThread.Post(HandleRecordingStarted);

    [ExcludeFromCodeCoverage(Justification = "Dispatches to UI thread; tested via HandleRecordingStopped.")]
    private void OnRecordingStopped(object? sender, AudioRecordingStoppedEventArgs e)
        => Avalonia.Threading.Dispatcher.UIThread.Post(HandleRecordingStopped);

    [ExcludeFromCodeCoverage(Justification = "Dispatches to UI thread; tested via HandleTranscriptionCompleted.")]
    private void OnTranscriptionCompleted(object? sender, TranscriptionCompletedEventArgs e) {
        var success = e.Success;
        var transcript = e.Transcript;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => HandleTranscriptionCompleted(success, transcript));
    }

    [ExcludeFromCodeCoverage(Justification = "UI event handler; touches Avalonia recorder/controls.")]
    private void OnToggleRecordingClicked(object? sender, EventArgs e) {
        if (CurrentState == TrayIconState.Recording) {
            _recorder.StopRecording();
        }
        else {
            try {
                _recorder.StartRecording();
            }
            catch (Exception ex) {
                Log.Error(ex, "[TrayService] Failed to start recording from tray.");
            }
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Requires live Avalonia Window.")]
    private void ShowMainWindow() {
        var window = _desktop.MainWindow;
        if (window is null)
            return;

        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    [ExcludeFromCodeCoverage(Justification = "Requires live Avalonia Window.")]
    private void ToggleMainWindowVisibility() {
        var window = _desktop.MainWindow;
        if (window is null)
            return;

        if (window.IsVisible)
            window.Hide();
        else
            ShowMainWindow();
    }

    [ExcludeFromCodeCoverage(Justification = "Requires Avalonia AssetLoader at runtime.")]
    private static WindowIcon LoadIcon(string avaloniaUri) {
        var uri = new Uri(avaloniaUri);
        using var stream = AssetLoader.Open(uri);
        return new WindowIcon(stream);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private void ApplyState(TrayIconState state) {
        CurrentState = state;
        if (_trayIcon is null)
            return;

        // Avalonia UI mutations must happen on the UI thread.
        // Post is safe to call from any thread; if we're already on the UI thread this is a no-op queue.
        Avalonia.Threading.Dispatcher.UIThread.Post(() => ApplyTrayIconUI(state));
    }

    [ExcludeFromCodeCoverage(Justification = "Requires live Avalonia TrayIcon and NativeMenu at runtime.")]
    private void ApplyTrayIconUI(TrayIconState state) {
        _trayIcon!.Icon = state switch {
            TrayIconState.Recording => _recordingIcon,
            TrayIconState.Transcribing => _transcribingIcon,
            TrayIconState.Ready => _readyIcon,
            _ => _idleIcon,
        };
        _trayIcon.ToolTipText = GetTooltipForState(state);

        _toggleRecordingItem?.Header = state == TrayIconState.Recording
                ? "Stop Recording"
                : "Start Recording";

        Log.Debug("[TrayService] State changed to {State}.", state);
    }

    private void CancelPendingRevert() {
        var cts = _revertCts;
        _revertCts = null;
        cts?.Cancel();
        cts?.Dispose();
    }

    private async Task RevertToIdleAsync(TimeSpan duration, CancellationToken token) {
        try {
            await Task.Delay(duration, token);
            // Dispose the CTS only if it hasn't been replaced by a newer call.
            if (_revertCts?.Token == token) {
                var cts = _revertCts;
                _revertCts = null;
                cts?.Dispose();
            }
            // ApplyState updates CurrentState on this thread-pool thread, then
            // dispatches Avalonia UI mutations to the UI thread internally.
            ApplyState(TrayIconState.Idle);
        }
        catch (OperationCanceledException) {
            // Cancelled by a subsequent state change — intentional.
        }
    }

    // ── Testable static helpers ────────────────────────────────────────────────

    public static string FormatTooltipText(string? transcript) => string.IsNullOrWhiteSpace(transcript)
            ? "VivaVoz — No speech detected."
            : transcript.Length <= 30 ? $"VivaVoz — {transcript}" : $"VivaVoz — {transcript[..30]}...";

    public static string GetTooltipForState(TrayIconState state) => state switch {
        TrayIconState.Recording => "VivaVoz — Recording...",
        TrayIconState.Transcribing => "VivaVoz — Transcribing...",
        TrayIconState.Ready => "VivaVoz — Transcript ready!",
        _ => "VivaVoz"
    };
}
