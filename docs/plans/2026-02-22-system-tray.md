# System Tray Integration Implementation Plan (TASK-1.15)

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a persistent system tray icon with context menu, state-reactive icons, minimize-to-tray behavior, and a transcription notification tooltip.

**Architecture:** A new `TrayService` (programmatic Avalonia `TrayIcon`) is created in `App.axaml.cs` alongside existing services. It subscribes to recorder and transcription events to update icon state. `MainWindow` receives `ISettingsService` via constructor and overrides `OnClosing` to hide instead of close when `MinimizeToTray=true`. Two new boolean settings fields are added with an EF Core migration.

**Tech Stack:** .NET 10, Avalonia UI (TrayIcon, NativeMenu), CommunityToolkit.Mvvm, EF Core + SQLite (manual migration), xUnit + NSubstitute + AwesomeAssertions.

**Notes:**
- EF Core CLI tools are NOT installed. Create migration files manually following the pattern in `source/VivaVoz/Migrations/20260222000001_AddPendingTranscriptionStatus.*`.
- Tests use in-memory SQLite with `EnsureCreated()` — they don't run migrations, so new Settings columns are automatically picked up once `AppDbContext.OnModelCreating` is updated.
- `TrayService` is UI-bound (Avalonia `TrayIcon`) and must be marked `[ExcludeFromCodeCoverage]`. Testable logic is extracted into pure static helpers.
- Test command: `dotnet test source/VivaVoz.Tests`
- Build command: `dotnet build source/VivaVoz`

---

## Task 1: Settings Model — Add MinimizeToTray and StartMinimized

**Files:**
- Test: `source/VivaVoz.Tests/Models/SettingsTests.cs`
- Modify: `source/VivaVoz/Models/Settings.cs`
- Modify: `source/VivaVoz/Data/AppDbContext.cs`
- Modify: `source/VivaVoz/Services/SettingsService.cs`

### Step 1: Write failing tests for new Settings defaults

Append to `source/VivaVoz.Tests/Models/SettingsTests.cs` (inside the `SettingsTests` class, before the closing `}`):

```csharp
[Fact]
public void NewSettings_ShouldDefaultMinimizeToTrayToTrue() {
    var settings = new Settings();

    settings.MinimizeToTray.Should().BeTrue();
}

[Fact]
public void NewSettings_ShouldDefaultStartMinimizedToFalse() {
    var settings = new Settings();

    settings.StartMinimized.Should().BeFalse();
}
```

### Step 2: Run tests to verify they fail

```bash
dotnet test source/VivaVoz.Tests --filter "MinimizeToTray|StartMinimized"
```

Expected: 2 FAIL — `Settings` has no such properties.

### Step 3: Add fields to Settings model

In `source/VivaVoz/Models/Settings.cs`, add after `public bool AutoUpdate { get; set; }`:

```csharp
public bool MinimizeToTray { get; set; } = true;
public bool StartMinimized { get; set; } = false;
```

### Step 4: Add EF column config in AppDbContext

In `source/VivaVoz/Data/AppDbContext.cs`, inside `OnModelCreating`, after the `settings.Property(s => s.AutoUpdate)...` block:

```csharp
settings.Property(s => s.MinimizeToTray)
    .IsRequired()
    .HasDefaultValue(true);
settings.Property(s => s.StartMinimized)
    .IsRequired()
    .HasDefaultValue(false);
```

### Step 5: Update SettingsService defaults

In `source/VivaVoz/Services/SettingsService.cs`, update `CreateDefaults()` to include the new fields:

```csharp
private static Settings CreateDefaults() => new() {
    WhisperModelSize = "tiny",
    StoragePath = GetDefaultStoragePath(),
    Theme = "System",
    Language = "auto",
    ExportFormat = "MP3",
    HotkeyConfig = string.Empty,
    AudioInputDevice = null,
    AutoUpdate = false,
    MinimizeToTray = true,
    StartMinimized = false
};
```

### Step 6: Run tests to verify they pass

```bash
dotnet test source/VivaVoz.Tests --filter "MinimizeToTray|StartMinimized"
```

Expected: 2 PASS

### Step 7: Run full suite to confirm no regressions

```bash
dotnet test source/VivaVoz.Tests
```

Expected: all previously passing tests still pass.

### Step 8: Commit

```bash
git add source/VivaVoz/Models/Settings.cs \
        source/VivaVoz/Data/AppDbContext.cs \
        source/VivaVoz/Services/SettingsService.cs \
        source/VivaVoz.Tests/Models/SettingsTests.cs
git commit -m "feat: add MinimizeToTray and StartMinimized settings fields (TASK-1.15)"
```

---

## Task 2: EF Core Migration — AddTraySettings (manual)

**Files:**
- Create: `source/VivaVoz/Migrations/20260222120000_AddTraySettings.cs`
- Create: `source/VivaVoz/Migrations/20260222120000_AddTraySettings.Designer.cs`
- Modify: `source/VivaVoz/Migrations/AppDbContextModelSnapshot.cs`

No tests for migrations themselves — correctness is validated by `dotnet build`.

### Step 1: Create migration Up/Down file

Create `source/VivaVoz/Migrations/20260222120000_AddTraySettings.cs`:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace VivaVoz.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddTraySettings : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.AddColumn<bool>(
            name: "MinimizeToTray",
            table: "Settings",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<bool>(
            name: "StartMinimized",
            table: "Settings",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropColumn(name: "MinimizeToTray", table: "Settings");
        migrationBuilder.DropColumn(name: "StartMinimized", table: "Settings");
    }
}
```

### Step 2: Create Designer.cs file

Create `source/VivaVoz/Migrations/20260222120000_AddTraySettings.Designer.cs`.

Copy from `20260222000001_AddPendingTranscriptionStatus.Designer.cs` and update:
- Change `[Migration("20260222000001_AddPendingTranscriptionStatus")]` → `[Migration("20260222120000_AddTraySettings")]`
- Change `partial class AddPendingTranscriptionStatus` → `partial class AddTraySettings`
- In the `BuildTargetModel` Settings entity block, add after the `AutoUpdate` property block:

```csharp
b.Property<bool>("MinimizeToTray")
    .ValueGeneratedOnAdd()
    .HasColumnType("INTEGER")
    .HasDefaultValue(true);

b.Property<bool>("StartMinimized")
    .ValueGeneratedOnAdd()
    .HasColumnType("INTEGER")
    .HasDefaultValue(false);
```

The full Settings entity block in the Designer should look like:

```csharp
modelBuilder.Entity("VivaVoz.Models.Settings", b =>
    {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER");

        b.Property<string>("AudioInputDevice")
            .HasColumnType("TEXT");

        b.Property<bool>("AutoUpdate")
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasDefaultValue(false);

        b.Property<string>("ExportFormat")
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT")
            .HasDefaultValue("MP3");

        b.Property<string>("HotkeyConfig")
            .IsRequired()
            .HasColumnType("TEXT");

        b.Property<string>("Language")
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT")
            .HasDefaultValue("auto");

        b.Property<bool>("MinimizeToTray")
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasDefaultValue(true);

        b.Property<bool>("StartMinimized")
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasDefaultValue(false);

        b.Property<string>("StoragePath")
            .IsRequired()
            .HasColumnType("TEXT");

        b.Property<string>("Theme")
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT")
            .HasDefaultValue("System");

        b.Property<string>("WhisperModelSize")
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("TEXT")
            .HasDefaultValue("tiny");

        b.HasKey("Id");

        b.ToTable("Settings", (string)null);
    });
```

### Step 3: Update AppDbContextModelSnapshot.cs

In `source/VivaVoz/Migrations/AppDbContextModelSnapshot.cs`, inside the Settings entity block, add the two new properties after `AutoUpdate`:

```csharp
b.Property<bool>("MinimizeToTray")
    .ValueGeneratedOnAdd()
    .HasColumnType("INTEGER")
    .HasDefaultValue(true);

b.Property<bool>("StartMinimized")
    .ValueGeneratedOnAdd()
    .HasColumnType("INTEGER")
    .HasDefaultValue(false);
```

### Step 4: Build to verify migration compiles

```bash
dotnet build source/VivaVoz
```

Expected: Build succeeded, 0 errors.

### Step 5: Commit

```bash
git add source/VivaVoz/Migrations/
git commit -m "feat: add EF migration AddTraySettings (TASK-1.15)"
```

---

## Task 3: TrayIconState Enum and ITrayService Interface

**Files:**
- Create: `source/VivaVoz/Services/ITrayService.cs`
- Test: `source/VivaVoz.Tests/Services/TrayServiceTests.cs`

### Step 1: Write failing test for TrayIconState enum

Create `source/VivaVoz.Tests/Services/TrayServiceTests.cs`:

```csharp
using AwesomeAssertions;
using VivaVoz.Services;
using Xunit;

namespace VivaVoz.Tests.Services;

public class TrayServiceTests {
    // ========== TrayIconState enum ==========

    [Fact]
    public void TrayIconState_ShouldHaveIdleValue() {
        var state = TrayIconState.Idle;

        state.Should().Be(TrayIconState.Idle);
    }

    [Fact]
    public void TrayIconState_ShouldHaveRecordingValue() {
        var state = TrayIconState.Recording;

        state.Should().Be(TrayIconState.Recording);
    }

    [Fact]
    public void TrayIconState_ShouldHaveTranscribingValue() {
        var state = TrayIconState.Transcribing;

        state.Should().Be(TrayIconState.Transcribing);
    }

    [Fact]
    public void TrayIconState_IdleShouldNotEqualRecording() {
        TrayIconState.Idle.Should().NotBe(TrayIconState.Recording);
    }

    // ========== TrayService.FormatTooltipText ==========

    [Fact]
    public void FormatTooltipText_WithNullTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(null);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithEmptyTranscript_ShouldReturnDefaultText() {
        var result = TrayService.FormatTooltipText(string.Empty);

        result.Should().Be("VivaVoz — No speech detected.");
    }

    [Fact]
    public void FormatTooltipText_WithShortTranscript_ShouldReturnFullText() {
        var result = TrayService.FormatTooltipText("Hello world");

        result.Should().Be("VivaVoz — Hello world");
    }

    [Fact]
    public void FormatTooltipText_WithLongTranscript_ShouldTruncateTo30Chars() {
        var transcript = "This is a very long transcript that should be truncated";

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be("VivaVoz — This is a very long tran...");
    }

    [Fact]
    public void FormatTooltipText_WithExactly30CharTranscript_ShouldNotTruncate() {
        var transcript = new string('a', 30); // exactly 30 chars

        var result = TrayService.FormatTooltipText(transcript);

        result.Should().Be($"VivaVoz — {transcript}");
        result.Should().NotContain("...");
    }

    // ========== TrayService.GetTooltipForState ==========

    [Fact]
    public void GetTooltipForState_WhenIdle_ShouldReturnIdleText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Idle);

        result.Should().Be("VivaVoz");
    }

    [Fact]
    public void GetTooltipForState_WhenRecording_ShouldReturnRecordingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Recording);

        result.Should().Be("VivaVoz — Recording...");
    }

    [Fact]
    public void GetTooltipForState_WhenTranscribing_ShouldReturnTranscribingText() {
        var result = TrayService.GetTooltipForState(TrayIconState.Transcribing);

        result.Should().Be("VivaVoz — Transcribing...");
    }
}
```

### Step 2: Run tests to verify they fail

```bash
dotnet test source/VivaVoz.Tests --filter "TrayService"
```

Expected: Build error — `TrayIconState` and `TrayService` don't exist yet.

### Step 3: Create ITrayService and TrayIconState

Create `source/VivaVoz/Services/ITrayService.cs`:

```csharp
namespace VivaVoz.Services;

public enum TrayIconState {
    Idle,
    Recording,
    Transcribing
}

public interface ITrayService : IDisposable {
    void SetState(TrayIconState state);
    void ShowTranscriptionComplete(string? transcript);
}
```

### Step 4: Create stub TrayService with testable static helpers

Create `source/VivaVoz/Services/TrayService.cs`:

```csharp
using System.Diagnostics.CodeAnalysis;

namespace VivaVoz.Services;

[ExcludeFromCodeCoverage]
public class TrayService : ITrayService {
    public void SetState(TrayIconState state) { }
    public void ShowTranscriptionComplete(string? transcript) { }
    public void Dispose() { }

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
```

### Step 5: Run tests to verify they pass

```bash
dotnet test source/VivaVoz.Tests --filter "TrayService"
```

Expected: all TrayService tests PASS.

### Step 6: Run full suite

```bash
dotnet test source/VivaVoz.Tests
```

Expected: all tests pass.

### Step 7: Commit

```bash
git add source/VivaVoz/Services/ITrayService.cs \
        source/VivaVoz/Services/TrayService.cs \
        source/VivaVoz.Tests/Services/TrayServiceTests.cs
git commit -m "feat: add TrayIconState enum, ITrayService, and TrayService helpers (TASK-1.15)"
```

---

## Task 4: Implement TrayService — Full Avalonia TrayIcon

**Files:**
- Modify: `source/VivaVoz/Services/TrayService.cs`

This is UI-bound Avalonia code. It cannot be unit tested and is already `[ExcludeFromCodeCoverage]`.

### Step 1: Replace the stub TrayService implementation

Replace the entire content of `source/VivaVoz/Services/TrayService.cs` with:

```csharp
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

        var menu = new NativeMenu();
        menu.Items.Add(_toggleRecordingItem);
        menu.Items.Add(openItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(new NativeMenuItem { Header = "Exit" });
        // wire Exit separately to access the item
        ((NativeMenuItem)menu.Items[^1]).Click += (_, _) => _desktop.Shutdown();

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

    // Testable static helpers (used by unit tests via TrayServiceTests)
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
```

### Step 2: Build to verify

```bash
dotnet build source/VivaVoz
```

Expected: Build succeeded, 0 errors.

### Step 3: Run tests (static helpers must still pass)

```bash
dotnet test source/VivaVoz.Tests
```

Expected: all tests pass.

### Step 4: Commit

```bash
git add source/VivaVoz/Services/TrayService.cs
git commit -m "feat: implement full TrayService with Avalonia TrayIcon (TASK-1.15)"
```

---

## Task 5: Update MainWindow to Support Minimize-to-Tray

**Files:**
- Modify: `source/VivaVoz/Views/MainWindow.axaml.cs`

No new tests — `MainWindow` is UI-bound and already `[ExcludeFromCodeCoverage]`. Behavior is tested manually.

### Step 1: Update MainWindow code-behind

Replace the entire content of `source/VivaVoz/Views/MainWindow.axaml.cs` with:

```csharp
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using VivaVoz.Services;

namespace VivaVoz.Views;

[ExcludeFromCodeCoverage]
public partial class MainWindow : Window {
    private readonly ISettingsService _settingsService;

    public MainWindow(ISettingsService settingsService) {
        _settingsService = settingsService;
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (_settingsService.Current?.MinimizeToTray == true) {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
```

### Step 2: Build to verify

```bash
dotnet build source/VivaVoz
```

Expected: Build succeeded. (App.axaml.cs will show a compile error about `new MainWindow()` — fix that in Task 6.)

Actually, if the build fails because `App.axaml.cs` still passes no constructor argument, that is expected. Proceed to Task 6 immediately.

### Step 3: Commit after Task 6 builds cleanly (do not commit yet)

---

## Task 6: Wire TrayService and StartMinimized in App.axaml.cs

**Files:**
- Modify: `source/VivaVoz/App.axaml.cs`

### Step 1: Replace App.axaml.cs content

Replace the entire content of `source/VivaVoz/App.axaml.cs` with:

```csharp
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VivaVoz.Data;
using VivaVoz.Models;
using VivaVoz.Services;
using VivaVoz.Services.Audio;
using VivaVoz.Services.Transcription;
using VivaVoz.ViewModels;
using VivaVoz.Views;

namespace VivaVoz;

[ExcludeFromCodeCoverage]
public partial class App : Application {
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted() {
        InitializeFileSystem();
        var dbContext = InitializeDatabase();
        await RecoverOrphanedTranscriptionsAsync(dbContext);
        var settingsService = new SettingsService(() => new AppDbContext());
        await settingsService.LoadSettingsAsync();
        var recorderService = new AudioRecorderService();
        var audioPlayerService = new AudioPlayerService();
        var modelManager = new WhisperModelManager();
        var whisperEngine = new WhisperTranscriptionEngine(modelManager);
        var transcriptionManager = new TranscriptionManager(whisperEngine, () => new AppDbContext());
        var clipboardService = new ClipboardService();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var mainWindow = new MainWindow(settingsService) {
                DataContext = new MainViewModel(recorderService, audioPlayerService, dbContext, transcriptionManager, clipboardService, settingsService)
            };

            var trayService = new TrayService(desktop, recorderService, transcriptionManager);
            trayService.Initialize();

            desktop.MainWindow = mainWindow;

            if (settingsService.Current?.StartMinimized == true) {
                mainWindow.ShowInTaskbar = false;
                // Window stays hidden; tray icon is visible. Show() called on first tray click.
            }

            desktop.ShutdownRequested += (_, _) => trayService.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializeFileSystem() {
        var fileSystemService = new FileSystemService();
        FileSystemService.EnsureAppDirectories();
    }

    private static AppDbContext InitializeDatabase() {
        var dbContext = new AppDbContext();
        dbContext.Database.Migrate();
        return dbContext;
    }

    private static async Task RecoverOrphanedTranscriptionsAsync(AppDbContext dbContext) {
        var orphaned = await dbContext.Recordings
            .Where(r => r.Status == RecordingStatus.Transcribing)
            .ToListAsync();

        if (orphaned.Count == 0) return;

        foreach (var recording in orphaned) {
            recording.Status = RecordingStatus.PendingTranscription;
        }

        await dbContext.SaveChangesAsync();
        Log.Information("[App] Reset {Count} orphaned Transcribing recording(s) to PendingTranscription.", orphaned.Count);
    }
}
```

### Step 2: Build to verify everything compiles

```bash
dotnet build source/VivaVoz
```

Expected: Build succeeded, 0 errors.

### Step 3: Run full test suite

```bash
dotnet test source/VivaVoz.Tests
```

Expected: all tests pass (230+ tests, 0 failures).

### Step 4: Commit both Task 5 and Task 6 together

```bash
git add source/VivaVoz/Views/MainWindow.axaml.cs \
        source/VivaVoz/App.axaml.cs
git commit -m "feat: wire TrayService into App, add minimize-to-tray window behavior (TASK-1.15)"
```

---

## Task 7: Final Verification and Completion

### Step 1: Full build

```bash
dotnet build source/VivaVoz
```

Expected: Build succeeded, 0 errors, 0 warnings about the new code.

### Step 2: Full test run

```bash
dotnet test source/VivaVoz.Tests
```

Expected: all tests pass.

### Step 3: Signal completion

```bash
openclaw system event --text 'Done: TASK-1.15 System Tray implemented'
```

---

## Acceptance Criteria Checklist

- [ ] `Settings.MinimizeToTray` defaults to `true`
- [ ] `Settings.StartMinimized` defaults to `false`
- [ ] EF migration `AddTraySettings` adds both columns
- [ ] `TrayIconState` enum: Idle, Recording, Transcribing
- [ ] `ITrayService` interface with `SetState`, `ShowTranscriptionComplete`, `Dispose`
- [ ] `TrayService.FormatTooltipText(null)` → "VivaVoz — No speech detected."
- [ ] `TrayService.FormatTooltipText("Hello world")` → "VivaVoz — Hello world"
- [ ] `TrayService.FormatTooltipText(51-char string)` → truncated to 30 + "..."
- [ ] `TrayService.GetTooltipForState(Idle)` → "VivaVoz"
- [ ] `TrayService.GetTooltipForState(Recording)` → "VivaVoz — Recording..."
- [ ] `TrayService.GetTooltipForState(Transcribing)` → "VivaVoz — Transcribing..."
- [ ] `MainWindow` receives `ISettingsService` via constructor
- [ ] Closing the window hides it (when MinimizeToTray=true)
- [ ] Tray icon visible when app is running
- [ ] Context menu: Start/Stop Recording, Open VivaVoz, Settings, separator, Exit
- [ ] Tray icon changes between mono (idle) and color (recording/transcribing)
- [ ] All 230+ existing tests still pass
