# Settings (AppSettings.cs + SettingsManager.cs)

Persisted as JSON via `SettingsManager` (path in `SettingsManager.FilePath`).

## AppSettings fields (key ones)
- `ActiveDays[7]` — bool array, Mon–Sun
- `WorkStart`, `WorkEnd`, `BreakStart`, `BreakEnd` — `TimeOnly`
- `IdleThresholdMinutes` — int
- `StopWhenUserReturns` — bool
- `UseScrollLock`, `UseMouseClick`, `UseArrowKeys` — bool (shared between Auto and Manual tabs)
- `FrequencyMinutes` — int (shared)
- `AutoTimeoutAction` / `ManualTimeoutAction` — `TimeoutAction` enum
- `ManualDurationMinutes`, `ManualDelayMinutes` — int
- `StartMinimized` — bool
- `DefaultMode` — string ("Auto" or "Manual")

## TimeoutAction enum values
`DoNothing`, `CloseApp`, `LockAndClose`, `ShutdownWindows`

## First-run behaviour
If settings file doesn't exist: startup is enabled automatically, defaults saved immediately.

## UI sync pattern
`_loading = true` guard used in `LoadSettingsToUI()` and cross-tab sync in `SaveFromManualUI()` to prevent save-on-change handlers firing during programmatic population.
