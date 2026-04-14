# SnapCqq2 — Auto Mode & UI Redesign

**Date:** 2026-04-14  
**Status:** Approved

---

## Overview

SnapCqq2 currently requires the user to manually start and stop automation. This redesign adds:

1. **Auto mode** — idle detection triggers automation within a configured schedule, no manual intervention needed
2. **Redesigned UI** — three-tab layout (Auto / Manual / General) with collapsible section cards
3. **Windows startup integration** — app launches automatically at login

The existing Manual mode is preserved unchanged in behaviour but restyled to match the new UI.

---

## UI Structure

### Tab layout: Auto | Manual | General

**Auto tab** — default tab on launch (configurable)

Three collapsible section cards:

| Card | Chevron default | Contains |
|------|-----------------|----------|
| 📅 **When** | Always expanded | Days, work hours, break, idle threshold |
| ⚡ **How** | Collapsed | Actions (scroll-lock / mouse click / arrow keys), frequency |
| 🔚 **After schedule ends** | Collapsed | Timeout action (do nothing / lock / shutdown / close app) |

All card headers use the same green colour (`#2e7d32`). Clicking a collapsed header expands it; clicking again collapses it.

**Manual tab** — preserves current behaviour, restyled

| Card | Chevron default | Contains |
|------|-----------------|----------|
| ▶ **Run** | Always expanded | Duration (min), delay (min), timeout action |
| ⚡ **How** | Collapsed | Actions, frequency |

**General tab** — app-wide settings

| Setting | Control | Default |
|---------|---------|---------|
| Launch on Windows startup | Toggle switch | On |
| Start minimized to tray | Toggle switch | On |
| Default mode on launch | Dropdown (Auto / Manual) | Auto |

### Status bar (shared across all tabs)

```
[● Status text — detail]          [Enable / Disable]
[HH:MM:SS AM] Last action log
```

- Left: coloured dot (green = active, amber = monitoring/waiting, grey = idle) + status text + sub-text
- Right: **Enable** (green) or **Disable** (red) button — replaces the old large Start/Stop button
- Second row: last action log line in monospace

In Manual tab the button label is **Start** / **Stop** instead of Enable / Disable.

---

## Auto Mode — Behaviour

### Idle detection

- Poll `GetLastInputInfo` (Win32) every 30 seconds
- If idle duration ≥ configured threshold → start automation
- If user becomes active again AND "Stop when user returns" is checked → stop automation
- Idle threshold is configurable (default 4 min, min 1 min)

### Schedule enforcement

The scheduler checks whether the current moment is within the active window before starting or continuing automation:

- **Active window** = within a configured day of the week AND within work hours AND NOT within break time
- Outside the active window: automation does not run even if the user is idle
- Entering the active window mid-day: normal idle detection resumes immediately
- Status bar shows "Outside schedule · Next: [day] [time]" when out of window

### Schedule configuration

- **Days**: Mon–Sun toggle buttons (pill style). Green = selected, grey = not selected. Mon–Fri selected by default.
- **Work hours**: Start time, End time (time pickers, 15-min increments)
- **Break**: Break start, Break end (time pickers). Break must fall between work start and end.
- **Idle threshold**: numeric spinner (minutes)
- **Stop when user returns**: checkbox (default: checked)

### State machine (Auto mode)

```
Disabled → [Enable clicked] → Armed (monitoring)
Armed → [enters schedule + user idle ≥ threshold] → Running (automating)
Running → [user active + "stop on return" on] → Armed (monitoring)
Running → [schedule ends] → Armed (monitoring) → [timeout action applied if configured]
Armed → [Disable clicked] → Disabled
```

---

## Manual Mode — Behaviour (unchanged)

- **Duration**: how long to run (minutes)
- **Delay**: wait before starting (0 = immediate)
- **Timeout action**: what to do when duration expires
- Start/Stop button in status bar
- Delay state shows "Starting in HH:MM:SS" in tray; "Start Now" and "Cancel" replace the button

---

## General Tab — Behaviour

### Launch on Windows startup

- Writes/removes a registry key: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\SnapCqq2`
- Value: path to the executable
- Toggle reflects actual registry state on load

### Start minimized to tray

- On launch, if enabled: skip `Show()`, go straight to `HideToTray()`
- Saved to user settings (JSON or app config)

### Default mode on launch

- Selects which tab is active when the form opens
- Saved to user settings

---

## Settings Persistence

All configuration is persisted to a JSON settings file at:
`%APPDATA%\SnapCqq2\settings.json`

Includes: schedule (days, hours, break), idle threshold, stop-on-return, actions, frequency, timeout action, delay, duration, General tab settings.

Loaded on startup, saved immediately on any change (ensures no config loss on crash or forced close).

---

## Performance & Lightweight Contract

This app must never measurably affect Windows performance. The following rules are binding:

### Timer discipline

| Timer | Interval | Runs when |
|-------|----------|-----------|
| `idleCheckTimer` | 30 s | Auto mode armed only (not when disabled, not in Manual mode) |
| `actionTimer` | N min (user config) | Actively automating only |
| `updateTimer` | 1 s | Actively automating only — **must be stopped during monitoring** |
| `delayTimer` | 1 s | Manual delay countdown only |

The 1-second `updateTimer` must **not** run during Auto monitoring state. There is nothing to update in the status bar that changes every second while the user is active. It starts only when automation fires, and stops the moment automation pauses.

### `GetLastInputInfo` cost

`GetLastInputInfo` reads a single DWORD from kernel shared memory. It does not cross a privilege boundary, causes no I/O, and has no scheduling side-effects. Even at the minimum 30-second interval the overhead is immeasurable (< 1 ms/hour of CPU time total).

### Dynamic poll interval

Rather than a fixed interval, `idleCheckTimer` uses an interval derived from the configured idle threshold:

```
poll_interval = max(30s, threshold_minutes × 60 / 4)
```

| Threshold | Poll interval | Worst-case latency |
|-----------|--------------|-------------------|
| 1 min     | 30 s         | +30 s             |
| 2 min     | 30 s         | +30 s             |
| 4 min     | 60 s         | +60 s             |
| 10 min    | 150 s        | +150 s            |

The interval is recalculated once when the scheduler is armed (not on every tick). Implementation is a single expression:

```csharp
idleCheckTimer.Interval = (int)Math.Max(30_000, (double)idleThresholdMinutes * 60_000 / 4);
```

This guarantees worst-case activation latency ≤ 125% of the threshold, uses fewer timer ticks for large thresholds, and is correct down to the minimum threshold of 1 minute.

### No background threads

All timers use `System.Windows.Forms.Timer`, which runs on the UI thread and fires `WM_TIMER` messages. No `Thread`, `Task`, `BackgroundWorker`, or `ThreadPool` usage is introduced. This eliminates contention, context-switch overhead, and GC pressure from thread stacks.

### Tray-only state

When hidden to tray, the form is not rendered. The only live resources are:
- The `NotifyIcon` (a Win32 tray handle)
- One 30-second timer message per poll cycle (Auto armed) or nothing (Disabled / Manual idle)

### Settings I/O

Settings are written to disk only when a value changes (not on a timer). The JSON file is small (< 1 KB). No polling of the settings file occurs.

---

## Out of Scope

- Multiple schedule profiles
- Per-day custom hours
- Network/remote triggers
- Any UI other than Windows Forms
