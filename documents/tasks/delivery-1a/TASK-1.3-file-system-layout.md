# Task 1.3: File System Layout

**Goal:** Ensure the required directory structure for data, audio, models, and logs is created on application startup.
**Part of:** Delivery 1a

## Context
VivaVoz relies on a specific file system structure to store its database, audio recordings, Whisper models, and logs. This task ensures that the necessary folders are created automatically when the application runs, preventing runtime errors due to missing directories.

## Requirements

### Functional
- The application must verify and create the following directories within `%LOCALAPPDATA%/VivaVoz/` (or equivalent on Linux/macOS for dev):
  - `data/` (For SQLite database)
  - `audio/` (For raw WAV recordings)
  - `models/` (For Whisper model binaries)
  - `logs/` (For application logs)

### Technical
- **Path Resolution:** Use `Environment.SpecialFolder.LocalApplicationData` combined with `VivaVoz`.
- **Implementation:** Create a `FileSystemService` or equivalent startup logic.
- **Error Handling:** Log any `UnauthorizedAccessException` or `IOException` during directory creation (though rare in AppData).

### File Path Conventions
- Service: `/home/andre/projects/VivaVoz/source/VivaVoz/Services/FileSystemService.cs`
- Constants: `/home/andre/projects/VivaVoz/source/VivaVoz/Constants/FilePaths.cs`

## Acceptance Criteria (Verification Steps)

### Verify Directory Creation
- Ensure the directory `%LOCALAPPDATA%/VivaVoz` does not exist.
- Launch the application.
- Verify that the directory `%LOCALAPPDATA%/VivaVoz` is created.
- Confirm that the subdirectories `data`, `audio`, `models`, and `logs` exist inside it.

### Verify Directory Existence
- Ensure the directory `%LOCALAPPDATA%/VivaVoz` already exists with some subdirectories missing.
- Launch the application again.
- Verify that the missing subdirectories are created.
- Confirm that existing directories remain untouched.

### Unit Tests Required

**Testing Standards (apply to ALL tests in this task):**
- **Framework:** xUnit
- **Mocking:** NSubstitute (already in test project — do NOT use Moq or any other framework)
- **Assertions:** AwesomeAssertions (add NuGet package if not present — use fluent assertion syntax)
- **Naming:** GUTs (Good Unit Tests) — `MethodName_Scenario_ExpectedBehavior`
- **Structure:** Arrange-Act-Assert (AAA) pattern, clearly separated
- **Principles:** FIRST — Fast, Isolated, Repeatable, Self-validating, Timely
- **One logical assertion per test** — each test verifies a single behavior
Produce unit tests in `VivaVoz.Tests` covering:
- **FilePaths constants:** Verify `BaseDirectory` resolves to `{LocalAppData}/VivaVoz`. Verify `AudioDirectory`, `DataDirectory`, `ModelsDirectory`, `LogsDirectory` are subdirectories of `BaseDirectory`. Verify `DatabasePath` points to `data/vivavoz.db`.
- **FileSystemService.EnsureDirectoriesExist():** Verify all 4 directories are created when none exist (use a temp directory, not real AppData). Verify method is idempotent (calling twice doesn't throw). Verify missing subdirectories are created when parent exists.
- **Minimum:** 5 tests, all with specific path/existence assertions.
