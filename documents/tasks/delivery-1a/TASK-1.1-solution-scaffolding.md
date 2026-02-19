# Task 1.1: Solution Scaffolding

**Goal:** Initialize the VivaVoz solution with .NET 10 and Avalonia UI, establishing the project structure and MVVM pattern.
**Part of:** Delivery 1a

## Context
This is the genesis of the project. We need a clean, compilable solution that serves as the foundation for all future development. It sets up the UI framework (Avalonia) and the architectural pattern (MVVM) that will separate our view logic from our business logic.

## Requirements

### Functional
- The application must compile and run on Windows.
- The application must display a default "Welcome to VivaVoz" message or blank window confirming the UI stack is loaded.
- The solution must include a test project referenced by the main project.

### Technical
- **Framework:** .NET 10
- **UI Framework:** Avalonia UI (latest stable)
- **Theme:** Fluent Theme (Dark Mode default for now, or system)
- **Pattern:** MVVM (Model-View-ViewModel) using `CommunityToolkit.Mvvm`
- **Project Name:** `VivaVoz`
- **Solution File:** `VivaVoz.sln`
- **Projects:**
  - `VivaVoz` (Main App)
  - `VivaVoz.Core` (Business Logic/Models - optional if we want strict separation, but for MVP a single project or strictly folder-separated is fine. Let's stick to a single project with folders `Models`, `ViewModels`, `Views`, `Services` for simplicity unless specified otherwise. *Correction:* Standard Avalonia templates often use a separate `.Desktop` project, but a single project target is fine for a dedicated desktop app. Let's follow standard `avalonia.xplat` or similar template structure but stripped for Windows-primary.)
  - `VivaVoz.Tests` (xUnit)

### File Path Conventions
- Solution root: `/home/andre/projects/VivaVoz/source/`
- Main project: `/home/andre/projects/VivaVoz/source/VivaVoz/`
- Tests: `/home/andre/projects/VivaVoz/source/VivaVoz.Tests/`

## Acceptance Criteria (Verification Steps)

### Verify Solution Build
- Run `dotnet build` in the solution root.
- Verify that the build succeeds with 0 errors.
- Confirm that output artifacts are generated in `bin/Debug/net10.0`.

### Verify Application Launch
- Launch the application executable.
- Verify that a window titled "VivaVoz" appears.
- Confirm that the window applies the Fluent theme (modern look).

### Verify Test Project Linkage
- Run `dotnet test` for the `VivaVoz.Tests` project.
- Verify that the test runner executes successfully.
- Confirm that it reports 0 failed tests (even if 0 tests run).

> **Note:** This is the only task where a placeholder test is acceptable. All subsequent tasks must produce meaningful unit tests for any service or model classes they create.
