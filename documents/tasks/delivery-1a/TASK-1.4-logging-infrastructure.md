# Task 1.4: Logging Infrastructure

**Goal:** Implement a robust file-based logging system to capture application events, errors, and debugging information.
**Part of:** Delivery 1a

## Context
Since VivaVoz runs locally, diagnosing issues requires detailed logs. This task sets up the logging infrastructure, ensuring that errors, warnings, and informational events are recorded to a file for troubleshooting.

## Requirements

### Functional
- Logs must be written to `%LOCALAPPDATA%/VivaVoz/logs/vivavoz-{yyyyMMdd}.log`.
- Log entries must include timestamp, severity level, source context, and message.
- Logs should be rotated daily (or weekly as per PRD preference, but daily is often better for debugging; let's stick to daily rolling for finer granularity unless strictly weekly mandated. PRD says "Rotated weekly", so let's configure `RollingInterval.Day` but mention weekly rotation preference if critical. Actually, "Rotated weekly" usually implies keeping N weeks of logs. Let's use `RollingInterval.Day` and retain files for 30 days to cover a month).
- **Severity Levels:**
  - `Debug`: Detailed flow (optional in release).
  - `Information`: App start, recording start/stop.
  - `Warning`: Non-critical issues (e.g., config missing, using defaults).
  - `Error`: Exceptions, crashes.
  - `Fatal`: Unrecoverable errors.

### Technical
- **Library:** Serilog (with `Serilog.Sinks.File`).
- **Configuration:** Code-based configuration in `App.axaml.cs` or `Program.cs`.
- **Output Template:** `"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`
- **Retention:** `retainedFileCountLimit: 31` (keep ~1 month).

### File Path Conventions
- Logger Setup: `/home/andre/projects/VivaVoz/source/VivaVoz/Services/LoggingService.cs` (or directly in `Program.cs`)

## Acceptance Criteria (Verification Steps)

- [ ] **Log File Creation**
  - Launch the application.
  - Navigate to `%LOCALAPPDATA%/VivaVoz/logs/`.
  - Verify a file named `vivavoz-{yyyyMMdd}.log` exists (matching today's date).
- [ ] **Log Content Verification**
  - Open the log file.
  - Verify it contains an `[INF]` entry with "Application Starting".
  - Verify the entry includes a valid ISO-8601 timestamp.
- [ ] **Error Logging**
  - Trigger a test exception or force an error condition.
  - Open the log file.
  - Verify a new entry exists with level `[ERR]` or `[FAT]`.
  - Verify the stack trace is included in the log entry.

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
- **LoggingService.ConfigureLogging():** Verify calling `ConfigureLogging()` does not throw. Verify `Log.Logger` is configured (not the default silent logger) after calling `ConfigureLogging()`.
- **LoggingService.CloseAndFlush():** Verify calling `CloseAndFlush()` does not throw.
- **Log output format:** Verify that logging an Information message produces output containing the expected severity tag and message text (use a `StringWriter` or in-memory sink).
- **Minimum:** 3 tests with specific assertions.
