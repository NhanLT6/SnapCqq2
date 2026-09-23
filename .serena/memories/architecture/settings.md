# Settings (AppSettings.cs + SettingsManager.cs)

Persisted as JSON via `SettingsManager` (%APPDATA%\SnapCqq2\settings.json). Unknown keys (old Manual-mode fields, DefaultMode) are ignored on load.

## AppSettings fields
- `ActiveDays[7]` (Mon–Sun), `WorkStart`, `WorkEnd`, `BreakStart`, `BreakEnd` — `TimeOnly`
- `IdleThresholdMinutes`, `StopWhenUserReturns`
- `UseMouseMove`, `UseTeamsKeepAlive`, `UseScrollLock`, `UseMouseClick`, `UseArrowKeys`, `FrequencyMinutes`
- `AutoEnabled` — schedule on/off
- `AutoTimeoutAction` — end-of-work-hours action; `EndActionSkippedOn` (DateOnly?) — "Skip today"
- `QuickRunMinutes` (0 = until stopped), `QuickRunEndAction`
- `LaunchOnStartup` (legacy; `StartupManager` is the source of truth)

## TimeoutAction enum
`DoNothing`, `CloseApp` (legacy, treated as DoNothing), `LockAndClose`, `ShutdownWindows`

## UI binding
`LoadSettingsToUI` / `SaveFromUI` / `Setting_Changed` (guarded by `_loading`). Collapsed row text comes from `SettingsSummary`.

## First-run behaviour
If settings file doesn't exist: startup is enabled automatically, defaults saved immediately.
