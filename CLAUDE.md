# SnapCqq2

Windows tray app (.NET 8 WinForms) that simulates user activity to prevent idle status on Teams and Windows.

## Serena memories

This project uses Serena memories as a persistent knowledge base. They live in `.serena/memories/` and are loaded automatically when Serena is active. Always read them at the start of a session before exploring the codebase.

| Memory | Contents |
|---|---|
| `architecture/overview` | Solution structure, key files, dependencies |
| `architecture/state-machines` | Auto/Manual state machines, all 4 timers |
| `architecture/actions` | Action dispatch logic, Teams keepalive design |
| `architecture/settings` | AppSettings fields, TimeoutAction enum, UI sync patterns |
| `architecture/services` | IdleDetector, ScheduleChecker, StartupManager, SettingsManager, TeamsKeepAlive |

## Development

```
dotnet build SnapCqq2/SnapCqq2.csproj
dotnet test SnapCqq2.Tests/SnapCqq2.Tests.csproj
```

If the app is running and locks the Debug exe, use `-c Release` for tests.

## Conventions

- Commit messages: short single-line only
- New services go in `SnapCqq2/Services/`
- New settings: add to `AppSettings`, load in `LoadSettingsToUI`, save in `SaveFromUI`, wire `CheckedChanged`/`ValueChanged` to `Setting_Changed` in `WireEvents`, and update the row text in `SettingsSummary` if it should show when collapsed
- UI controls: declare in `Main.UI.cs` field list, instantiate in `BuildUI()`, add to the relevant row body's `Controls.AddRange`. Positions are 96-DPI units wrapped in `S()` (form uses `AutoScaleMode.None`)
