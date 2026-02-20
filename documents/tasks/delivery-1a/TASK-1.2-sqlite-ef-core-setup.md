# Task 1.2: SQLite + EF Core Setup

**Goal:** Establish the data layer using Entity Framework Core and SQLite, including the initial schema for Recordings and Settings.
**Part of:** Delivery 1a

## Context
VivaVoz requires a local database to manage recording metadata and user preferences. This task sets up the ORM (EF Core) and creates the initial schema, ensuring the database file is generated and migrated automatically on application startup.

## Requirements

### Functional
- The application must verify and create the database file `vivavoz.db` on startup if it does not exist.
- The database schema must include tables for `Recordings` and `Settings`.
- Pending migrations must be applied automatically at launch.

### Technical
- **Framework:** EF Core 10 (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Database Location:** `%LOCALAPPDATA%/VivaVoz/data/vivavoz.db` (Ensure directory exists before creating context)
- **Entities:**
  - `Recording`
    - `Id` (Guid, PK)
    - `Title` (string, max 200)
    - `AudioFileName` (string, relative path)
    - `Transcript` (string, nullable)
    - `Status` (enum: Recording, Transcribing, Complete, Failed)
    - `Language` (string, default "auto")
    - `Duration` (TimeSpan)
    - `CreatedAt` (DateTime UTC)
    - `UpdatedAt` (DateTime UTC)
    - `WhisperModel` (string, e.g., "tiny", "base")
    - `FileSize` (long, bytes)
  - `Settings` (Singleton pattern typically, or Id=1)
    - `Id` (int, PK)
    - `HotkeyConfig` (string, JSON or specific format)
    - `WhisperModelSize` (string, default "tiny")
    - `AudioInputDevice` (string, nullable)
    - `StoragePath` (string, default `%LOCALAPPDATA%/VivaVoz`)
    - `ExportFormat` (string, default "MP3")
    - `Theme` (string, default "System")
    - `Language` (string, default "auto")
    - `AutoUpdate` (bool, default false)

### File Path Conventions
- Context: `/home/andre/projects/VivaVoz/source/VivaVoz/Data/AppDbContext.cs`
- Models: `/home/andre/projects/VivaVoz/source/VivaVoz/Models/Recording.cs`, `/home/andre/projects/VivaVoz/source/VivaVoz/Models/Settings.cs`

## Acceptance Criteria (Verification Steps)

### Verify Database Creation
- Ensure the application is not running and the directory `%LOCALAPPDATA%/VivaVoz/data/` is empty.
- Launch the application.
- Verify that a file named `vivavoz.db` is created in that directory.
- Confirm that the file size is greater than 0 bytes.

### Verify Schema
- Inspect the generated `vivavoz.db` database using a SQLite tool.
- Verify that a table named `Recordings` exists with columns: Id, Title, AudioFileName, CreatedAt.
- Verify that a table named `Settings` exists.

### Verify Initial Migration
- Delete the database if it exists.
- Run the application.
- Verify that the `InitialCreate` migration is applied successfully.
- Confirm that no exceptions regarding missing tables are thrown.

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
- **Recording model:** Verify default values (Status = Recording, Language = "auto", CreatedAt = UTC now). Verify all required properties are settable.
- **Settings model:** Verify default values (WhisperModelSize = "tiny", ExportFormat = "MP3", Theme = "System", AutoUpdate = false). Verify StoragePath defaults to `%LOCALAPPDATA%/VivaVoz`.
- **DatabaseInitializer:** Verify `Initialize()` calls `EnsureCreated()` on the context (use in-memory SQLite or mock). Verify a default Settings row is seeded if none exists.
- **Minimum:** 6 tests, all with specific value assertions (no `Assert.True(true)`).
