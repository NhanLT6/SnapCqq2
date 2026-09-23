# State (Main.cs)

Single screen, no tabs. One `engineTimer` drives everything via `RefreshNow()` → `Step()` → `UpdateView()` → `ScheduleNextTick()`.

## State fields
- `_settings.AutoEnabled` — schedule on/off (persisted)
- `_acting` — schedule mode: user idle ≥ threshold inside work hours, actions firing
- `_pausedUntil` — temporary pause (1 h / until tomorrow), in memory
- `_quickRun` (`QuickRunState(StartAt, EndAt?)`) — "Run now": ignores schedule and idle; EndAt null = until stopped
- `_shutdownAt` — pending `shutdown /s /t 60` (cancel = `shutdown /a`)

## View (derived each tick by `CurrentView`, never stored)
Priority: ShutdownPending > QuickRunWaiting/QuickRun > Off > Paused > Acting > Watching > OnBreak > OutsideSchedule.
`Describe(view)` returns a `Status(Title, Chip, Detail, Primary, Tone)`. Title = mode ("Schedule" = old Auto, "Run now" = old Manual, or "Shutdown"); the colored chip is the status (Tone: Active green, Armed blue, Stopped grey, Alert red) for at-a-glance reading.

## Split button (primary action per view)
Main button always acts on the mode named in the state text: Off→Turn on · any schedule-on view→Pause 1 h · Paused→Resume · QuickRun→Stop · ShutdownPending→Cancel shutdown. Starting Run now is only in ▾ / tray, under a "Run now, ignoring schedule" heading.
▾ menu and tray menu are rebuilt on open by `FillMenu(items, forTray)`. ▾ is hidden when empty (`HasMenuItems`); the button group keeps a fixed width. Tray `Opening` must set `e.Cancel = false` — WinForms pre-cancels menus that are empty when opening starts.
Tray icon = app icon at small size (no status dot); state is in the tooltip.

## End-of-day action
Fires only when work hours end (`WorkEndToday` crossing between ticks) while `_acting` and the user hasn't returned, and not skipped today (`EndActionSkippedOn`). Never at break start.
Quick run end uses `QuickRunEndAction` (DoNothing = back to schedule).

## Self-input detection
`UserReturned` ignores input within 2 s after our own SendInput (`_lastSimulatedInput`), otherwise every simulated action would look like the user returning.

## Timer interval (lightweight contract)
1 s while the window is visible or a shutdown is pending; otherwise `IdleDetector.ComputePollIntervalMs(threshold)` or sooner if a deadline (next action, run start/end, pause end, work end) is due. Minimum 1 s.
