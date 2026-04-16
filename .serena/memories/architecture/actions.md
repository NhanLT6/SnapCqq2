# Simulated Input Actions (Main.cs — #region Automation actions)

`PerformRandomAction()` runs on every timer tick. Teams keepalive always fires first (if enabled), then one random system-input action is picked from the enabled pool.

## Actions
| Setting flag | Method | Dispatch |
|---|---|---|
| `UseScrollLock` | `PerformScrollLock` | Random pool |
| `UseMouseClick` | `PerformMouseClick` | Random pool |
| `UseArrowKeys` | `PerformArrowKey` | Random pool |
| `UseTeamsKeepAlive` | `PerformTeamsKeepAlive` | **Always, every tick** |

All system-input actions use `WindowsInput.InputSimulator` → `SendInput` Win32 API.
`SendInput` updates global `GetLastInputInfo` but only delivers window messages to the foreground window.

## Why Teams keepalive is dispatched differently
Teams (2023+) tracks presence via its own window message queue, not solely `GetLastInputInfo`. If Teams keepalive were pooled randomly with the other 3 actions, it would fire on average every ~4 minutes at 1-minute frequency — close enough to Teams' idle threshold that it sometimes loses the race. Firing it every tick guarantees it beats Teams' idle timer regardless of frequency setting.

## Teams keepalive mechanism (`Services/TeamsKeepAlive.cs`)
- `FindTeamsWindow()` — scans processes named `ms-teams` or `Teams`, returns `MainWindowHandle`
- `PostKeepAlive(hwnd)` — posts `WM_MOUSEMOVE` to the center of Teams' client area; no-ops safely when `hwnd == IntPtr.Zero`
- `TrySendKeepAlive()` — convenience: find + post in one call
- Cost: one `PostMessage` per tick (negligible — Teams processes thousands of `WM_MOUSEMOVE`/sec from real mouse movement)
