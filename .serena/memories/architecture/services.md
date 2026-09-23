# Services

## IdleDetector (IdleDetector.cs)
- `GetIdleTime()` — P/Invokes `GetLastInputInfo`, returns `TimeSpan`. Handles TickCount 25-day wrap via unchecked subtraction.
- `ComputePollIntervalMs(thresholdMinutes)` — returns `max(30000, threshold_min * 60000 / 4)`. Guarantees ≤ 125% worst-case latency.

## ScheduleChecker (ScheduleChecker.cs)
- `IsInActiveWindow(settings, now)` — returns true if current time is in an active work window (respects ActiveDays, WorkStart/End, excludes BreakStart/End)
- `IsOnBreak`, `NextWindowStart` (work start or break end), `WorkEndToday` (null if passed / inactive day)

## StartupManager (StartupManager.cs)
- Manages Windows Registry run-on-startup entry
- `Enable()`, `IsEnabled()`, `Sync(bool)` — used by first-run logic and settings checkbox
- Also writes `Explorer\StartupApproved\Run` (first byte even=enabled, odd=disabled). A Task Manager "disabled" flag there silently overrides the Run entry; `IsEnabled()` checks both, `Enable()` clears it.
- `RefreshPath()` — on non-first launch, re-points existing Run entry at current exe (Debug vs published path)

## SettingsManager (SettingsManager.cs)
- `FilePath` — static property, path to JSON settings file
- `Load()` / `Save(settings)` — JSON serialization

## SettingsSummary (SettingsSummary.cs)
- One-line text for collapsed rows and status: `Days`, `Schedule`, `Actions`, `EndAction`, `Duration`, `Countdown`, `IsDisruptive`

