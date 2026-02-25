using System.Runtime.InteropServices;

namespace VivaVoz.Services;

/// <summary>
/// Windows-native implementation of <see cref="IHotkeyService"/> using
/// <c>user32.dll</c> P/Invoke (<c>RegisterHotKey</c> / <c>UnregisterHotKey</c>).
/// Runs a dedicated STA message-pump thread so the hotkey is independent of
/// the Avalonia UI thread.
/// </summary>
public sealed partial class GlobalHotkeyService : IHotkeyService {
    // ── Win32 constants ────────────────────────────────────────────────────────
    private const int WmHotKey = 0x0312;
    private const int WmQuit = 0x0012;
    private const int HotkeyId = 9001;

    // ── State ──────────────────────────────────────────────────────────────────
    private HotkeyConfig? _config;
    private Thread? _messageThread;

    /// <summary>
    /// The currently active recording mode: <c>"Toggle"</c> or <c>"Push-to-Talk"</c>.
    /// Settable internally and exposed as <c>internal</c> so unit tests can configure
    /// the service without going through the Win32 registration path.
    /// </summary>
    internal string Mode { get; set; } = "Toggle";

    // ── Public surface ─────────────────────────────────────────────────────────

    /// <inheritdoc />
    public event EventHandler? RecordingStartRequested;

    /// <inheritdoc />
    public event EventHandler? RecordingStopRequested;

    /// <inheritdoc />
    public bool IsRegistered { get; private set; }

    /// <summary>
    /// Tracks whether the service believes recording is currently active.
    /// Used by Toggle mode to alternate start/stop on successive key presses.
    /// Exposed as <c>internal</c> for unit-test assertions.
    /// </summary>
    internal bool IsRecording { get; private set; }

    // ── Registration ───────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool TryRegister(HotkeyConfig? config, string recordingMode) {
        if (config is null)
            return false;

        if (IsRegistered)
            Unregister();

        _config = config;
        Mode = recordingMode;
        IsRecording = false;

        if (!PlatformTryRegister(config)) {
            Log.Warning(
                "[GlobalHotkeyService] Failed to register hotkey {Config}. It may be in use by another application.",
                config);
            return false;
        }

        IsRegistered = true;
        Log.Information("[GlobalHotkeyService] Hotkey {Config} registered in {Mode} mode.", config, recordingMode);
        return true;
    }

    /// <inheritdoc />
    public void Unregister() {
        if (!IsRegistered)
            return;

        PlatformUnregister();
        IsRegistered = false;
        IsRecording = false;
        Log.Information("[GlobalHotkeyService] Hotkey unregistered.");
    }

    // ── Hotkey event handlers (pure logic, no P/Invoke) ────────────────────────

    /// <summary>
    /// Called by the Win32 message handler when the hotkey is pressed (key-down).
    /// In <b>Toggle</b> mode alternates start/stop. In <b>Push-to-Talk</b> mode triggers start.
    /// Exposed as <c>internal</c> for unit testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal void HandleHotkeyDown() {
        if (Mode == "Push-to-Talk") {
            IsRecording = true;
            RecordingStartRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Toggle mode
        if (IsRecording) {
            IsRecording = false;
            RecordingStopRequested?.Invoke(this, EventArgs.Empty);
        }
        else {
            IsRecording = true;
            RecordingStartRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Called by the low-level keyboard hook when the hotkey combination is released (key-up).
    /// Only meaningful in <b>Push-to-Talk</b> mode.
    /// Exposed as <c>internal</c> for unit testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal void HandleHotkeyUp() {
        if (Mode != "Push-to-Talk")
            return;

        IsRecording = false;
        RecordingStopRequested?.Invoke(this, EventArgs.Empty);
    }

    // ── IDisposable ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void Dispose() => Unregister();

    // ── Win32 plumbing (excluded from code coverage) ───────────────────────────

    [ExcludeFromCodeCoverage(Justification = "Requires Windows user32.dll at runtime.")]
    private bool PlatformTryRegister(HotkeyConfig config) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Log.Warning("[GlobalHotkeyService] Hotkey registration is only supported on Windows.");
            return false;
        }

        var registered = false;
        using var ready = new ManualResetEventSlim(false);

        _messageThread = new Thread(() => {
            registered = NativeMethods.RegisterHotKey(nint.Zero, HotkeyId, config.Modifiers, config.VirtualKey);
            ready.Set();

            if (!registered)
                return;

            while (NativeMethods.GetMessage(out var msg, nint.Zero, 0, 0)) {
                if (msg.message == WmHotKey && msg.wParam == HotkeyId)
                    HandleHotkeyDown();
                else if (msg.message == WmQuit)
                    break;

                NativeMethods.DispatchMessage(ref msg);
            }

            NativeMethods.UnregisterHotKey(nint.Zero, HotkeyId);
        }) {
            IsBackground = true,
            Name = "GlobalHotkeyMessageThread"
        };

        _messageThread.SetApartmentState(ApartmentState.STA);
        _messageThread.Start();
        ready.Wait();

        return registered;
    }

    [ExcludeFromCodeCoverage(Justification = "Requires Windows user32.dll at runtime.")]
    private void PlatformUnregister() {
        if (_messageThread?.IsAlive != true)
            return;

        // Post WM_QUIT to the message thread so its pump exits cleanly.
        NativeMethods.PostThreadMessage((uint)_messageThread.ManagedThreadId, WmQuit, nint.Zero, nint.Zero);
        _messageThread.Join(millisecondsTimeout: 500);
        _messageThread = null;
    }

    // ── P/Invoke declarations ──────────────────────────────────────────────────

    [ExcludeFromCodeCoverage(Justification = "Requires Windows user32.dll at runtime.")]
    private static partial class NativeMethods {
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG {
            public nint hwnd;
            public uint message;
            public nint wParam;
            public nint lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UnregisterHotKey(nint hWnd, int id);

        [LibraryImport("user32.dll", EntryPoint = "GetMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [LibraryImport("user32.dll", EntryPoint = "DispatchMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DispatchMessage(ref MSG lpmsg);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool PostThreadMessage(uint idThread, uint msg, nint wParam, nint lParam);
    }
}
