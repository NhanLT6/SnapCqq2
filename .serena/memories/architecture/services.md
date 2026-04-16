# Services

## IdleDetector (IdleDetector.cs)
- `GetIdleTime()` — P/Invokes `GetLastInputInfo`, returns `TimeSpan`. Handles TickCount 25-day wrap via unchecked subtraction.
- `ComputePollIntervalMs(thresholdMinutes)` — returns `max(30000, threshold_min * 60000 / 4)`. Guarantees ≤ 125% worst-case latency.

## ScheduleChecker (ScheduleChecker.cs)
- `IsInActiveWindow(settings, now)` — returns true if current time is in an active work window (respects ActiveDays, WorkStart/End, excludes BreakStart/End)
- `NextWindowDescription(settings, now)` — human-readable string for tray tooltip

## StartupManager (StartupManager.cs)
- Manages Windows Registry run-on-startup entry
- `Enable()`, `IsEnabled()`, `Sync(bool)` — used by first-run logic and settings checkbox

## SettingsManager (SettingsManager.cs)
- `FilePath` — static property, path to JSON settings file
- `Load()` / `Save(settings)` — JSON serialization
