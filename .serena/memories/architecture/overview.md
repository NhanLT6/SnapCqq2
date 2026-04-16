# SnapCqq2 — Project Overview

A Windows Forms (.NET 8) tray application that simulates user activity to prevent Teams/PC idle status.

## Solution structure
- `SnapCqq2/` — main app (WinForms, net8.0-windows)
- `SnapCqq2.Tests/` — xUnit test project

## Key files
- `SnapCqq2/Main.cs` — core state machine, timers, action dispatch
- `SnapCqq2/Main.UI.cs` — UI construction helpers
- `SnapCqq2/Main.Designer.cs` — designer-generated control declarations
- `SnapCqq2/Program.cs` — entry point
- `SnapCqq2/Models/AppSettings.cs` — settings model
- `SnapCqq2/Services/SettingsManager.cs` — JSON load/save
- `SnapCqq2/Services/ScheduleChecker.cs` — work/break window logic
- `SnapCqq2/Services/IdleDetector.cs` — GetLastInputInfo wrapper + poll interval formula
- `SnapCqq2/Services/StartupManager.cs` — Windows startup registry entry

## Key dependency
- `WindowsInput` (InputSimulator) — used to send synthetic keyboard/mouse input via SendInput
