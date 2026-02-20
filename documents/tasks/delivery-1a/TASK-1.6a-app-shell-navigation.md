# Task 1.6a: App Shell + Navigation

**Goal:** Establish the main application window and basic navigation structure using Avalonia UI and the Fluent theme.
**Part of:** Delivery 1a

## Context
This task builds the skeleton of the application. It creates the primary window and defines the split-view layout that will hold our list of recordings on the left and the detail/transcript view on the right.

## Requirements

### Functional
- The application must launch into a main window titled "VivaVoz".
- The layout must use a split-pane approach:
  - Left panel (fixed width or resizable, e.g., 300px min) for the recordings list.
  - Right panel (flexible, takes remaining space) for the detail view.
- The UI must use the Fluent theme (modern look).
- Basic navigation or content switching mechanism should be in place (e.g., changing the detail view based on list selection).

### Technical
- **Framework:** Avalonia UI.
- **Components:** `SplitView` or `Grid` with two columns (`ColumnDefinitions="300,*"`).
- **MVVM:** `MainViewModel` controlling the current view state.
- **Theme:** `FluentTheme` applied in `App.axaml`.

### File Path Conventions
- Main Window: `/home/andre/projects/VivaVoz/source/VivaVoz/Views/MainWindow.axaml`
- Main ViewModel: `/home/andre/projects/VivaVoz/source/VivaVoz/ViewModels/MainViewModel.cs`

## Acceptance Criteria (Verification Steps)

- [ ] **Main Window Layout**
  - Launch the application.
  - Verify the main window title is "VivaVoz".
  - Resize the window horizontally.
  - Verify the left panel maintains its width (or min-width).
  - Verify the right panel expands/contracts to fill the remaining space.
- [ ] **Theme Application**
  - Observe the UI elements (buttons, window chrome).
  - Verify Fluent design language is applied (rounded corners, standard Avalonia Fluent controls).

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
- **MainViewModel:** Verify `MainViewModel` can be instantiated without throwing. Verify initial state properties (e.g., `SelectedRecording` is null, `IsRecording` is false).
- **Minimum:** 2 tests. UI layout verification is manual (Avalonia headless testing is optional for MVP).
