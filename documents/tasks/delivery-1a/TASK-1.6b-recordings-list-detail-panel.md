# Task 1.6b: Recordings List + Detail Panel

**Goal:** Implement the UI components for displaying the list of recordings and the detailed view for a selected recording.
**Part of:** Delivery 1a

## Context
With the shell in place, we need to populate it. The primary interaction flow is selecting a recording from the list on the left to view its details on the right. This task builds those two panels and wires up the selection logic.

## Requirements

### Functional
- **Left Panel (Recordings List):**
  - Display a scrollable list of recordings.
  - Each item must show: Date/Time, Duration, and a brief preview (e.g., first few words of transcript).
  - List should be sorted by Date (Newest first).
  - Selection changes the content of the right panel.
- **Right Panel (Detail View):**
  - Display the full details of the selected recording.
  - If no recording is selected, show a placeholder (e.g., "Select a recording to view details").
  - Includes placeholders for future audio player and full transcript text.

### Technical
- **Components:** `ListBox` or `ItemsControl` with `ItemTemplate`.
- **Binding:** `ItemsSource="{Binding Recordings}"`, `SelectedItem="{Binding SelectedRecording}"`.
- **ViewModels:**
  - `RecordingsListViewModel`
  - `RecordingDetailViewModel` (or just `RecordingViewModel` if simple enough for MVP)
- **Data Source:** For this task, use dummy data/mock service until the real database is wired up in Task 1.7.

### File Path Conventions
- Views: `/home/andre/projects/VivaVoz/source/VivaVoz/Views/RecordingsListView.axaml`, `/home/andre/projects/VivaVoz/source/VivaVoz/Views/RecordingDetailView.axaml`
- ViewModels: `/home/andre/projects/VivaVoz/source/VivaVoz/ViewModels/RecordingsListViewModel.cs`

## Acceptance Criteria (Verification Steps)

- [ ] **List Population (Mock Data)**
  - Launch the application with mock data enabled (3 records).
  - Verify the list contains 3 items.
  - Verify the list is sorted by Date (Newest first).
- [ ] **Item Selection**
  - Click on the first item in the recordings list.
  - Verify the detail panel updates to show the selected recording's details.
  - Verify the "No Selection" placeholder is hidden.
- [ ] **No Selection State**
  - Launch the application (no item pre-selected).
  - Verify the detail panel displays a message like "Select a recording to view details".

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
- **RecordingsListViewModel (or MainViewModel recordings):** Verify recordings collection is initialized (not null). Verify `SelectedRecording` is null by default. Verify setting `SelectedRecording` raises `PropertyChanged`. Verify recordings are sorted by `CreatedAt` descending (newest first) when loaded.
- **Mock data:** Verify mock/seed data produces exactly 3 recordings with distinct dates.
- **Minimum:** 4 tests with specific value assertions.
