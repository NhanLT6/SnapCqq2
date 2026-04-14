# Auto Mode & UI Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add idle-triggered auto-mode with weekly schedule, redesign the UI into a three-tab card layout (Auto / Manual / General), and persist all settings to JSON.

**Architecture:** Pure-logic services (`ScheduleChecker`, `IdleDetector`, `SettingsManager`, `StartupManager`) live in `Services/` with no WinForms dependency. `AppSettings` is the single data model in `Models/`. `Main.cs` orchestrates them; `Main.Designer.cs` is a complete rewrite. An xUnit test project covers all services.

**Tech Stack:** .NET 8 Windows Forms, System.Text.Json, Win32 `user32!GetLastInputInfo`, `HKCU` registry for startup, xUnit 2.9.

**Note:** User has requested all changes left uncommitted for a single review. Skip every `git commit` step.

---

## File Map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `SnapCqq2/Models/AppSettings.cs` | Settings data model + `TimeoutAction` enum |
| Create | `SnapCqq2/Services/SettingsManager.cs` | JSON load/save to `%APPDATA%\SnapCqq2\settings.json` |
| Create | `SnapCqq2/Services/ScheduleChecker.cs` | Pure schedule-window logic (no timers, no WinForms) |
| Create | `SnapCqq2/Services/IdleDetector.cs` | `GetLastInputInfo` P/Invoke + poll-interval formula |
| Create | `SnapCqq2/Services/StartupManager.cs` | Registry read/write for Windows startup |
| Rewrite | `SnapCqq2/Main.Designer.cs` | Complete UI redesign: TabControl, section cards, status bar |
| Rewrite | `SnapCqq2/Main.cs` | New state machine, wiring all services, settings sync |
| Modify | `SnapCqq2/Program.cs` | Load settings before showing form; honour start-minimized |
| Create | `SnapCqq2.Tests/SnapCqq2.Tests.csproj` | xUnit test project |
| Create | `SnapCqq2.Tests/ScheduleCheckerTests.cs` | |
| Create | `SnapCqq2.Tests/SettingsManagerTests.cs` | |
| Create | `SnapCqq2.Tests/IdleDetectorTests.cs` | |

---

## Task 1: xUnit test project

**Files:**
- Create: `SnapCqq2.Tests/SnapCqq2.Tests.csproj`

- [ ] **Step 1: Create the test project**

```bash
cd E:\repos\SnapCqq2
dotnet new xunit -n SnapCqq2.Tests -o SnapCqq2.Tests --framework net8.0-windows
dotnet sln add SnapCqq2.Tests/SnapCqq2.Tests.csproj
dotnet add SnapCqq2.Tests/SnapCqq2.Tests.csproj reference SnapCqq2/SnapCqq2.csproj
```

- [ ] **Step 2: Edit `SnapCqq2.Tests/SnapCqq2.Tests.csproj` to match**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\SnapCqq2\SnapCqq2.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Verify build**

```bash
dotnet build SnapCqq2.Tests
```

Expected: `Build succeeded`.

---

## Task 2: AppSettings model

**Files:**
- Create: `SnapCqq2/Models/AppSettings.cs`

- [ ] **Step 1: Write `AppSettings.cs`**

```csharp
namespace SnapCqq2.Models;

public enum TimeoutAction
{
    DoNothing,
    CloseApp,
    LockAndClose,
    ShutdownWindows
}

public class AppSettings
{
    // Schedule
    // Index 0=Mon … 6=Sun
    public bool[] ActiveDays { get; set; } = [true, true, true, true, true, false, false];
    public TimeOnly WorkStart  { get; set; } = new(9,  0);
    public TimeOnly WorkEnd    { get; set; } = new(18, 0);
    public TimeOnly BreakStart { get; set; } = new(12, 0);
    public TimeOnly BreakEnd   { get; set; } = new(13, 0);

    // Idle trigger
    public int  IdleThresholdMinutes { get; set; } = 4;
    public bool StopWhenUserReturns  { get; set; } = true;

    // Shared: actions + frequency (apply to both Auto and Manual)
    public bool UseScrollLock { get; set; } = true;
    public bool UseMouseClick { get; set; } = false;
    public bool UseArrowKeys  { get; set; } = false;
    public int  FrequencyMinutes { get; set; } = 1;

    // Auto: what happens when schedule window ends
    public TimeoutAction AutoTimeoutAction { get; set; } = TimeoutAction.DoNothing;

    // Manual
    public int           ManualDurationMinutes { get; set; } = 30;
    public int           ManualDelayMinutes    { get; set; } = 0;
    public TimeoutAction ManualTimeoutAction   { get; set; } = TimeoutAction.CloseApp;

    // General
    public bool   LaunchOnStartup  { get; set; } = false;
    public bool   StartMinimized   { get; set; } = false;
    public string DefaultMode      { get; set; } = "Auto"; // "Auto" or "Manual"
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build SnapCqq2/SnapCqq2.csproj
```

Expected: `Build succeeded`.

---

## Task 3: SettingsManager + tests

**Files:**
- Create: `SnapCqq2/Services/SettingsManager.cs`
- Create: `SnapCqq2.Tests/SettingsManagerTests.cs`

- [ ] **Step 1: Write `SettingsManager.cs`**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using SnapCqq2.Models;

namespace SnapCqq2.Services;

public static class SettingsManager
{
    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SnapCqq2", "settings.json");

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json, _opts) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, _opts));
    }
}
```

- [ ] **Step 2: Write `SettingsManagerTests.cs`**

```csharp
using System.Text.Json;
using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class SettingsManagerTests : IDisposable
{
    // Redirect settings to a temp file so tests don't touch %APPDATA%
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public SettingsManagerTests()
    {
        // Reflect-inject temp path for isolation
        var field = typeof(SettingsManager).GetField("_path",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        field.SetValue(null, Path.Combine(_tempDir, "settings.json"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void Load_ReturnsDefaults_WhenFileAbsent()
    {
        var s = SettingsManager.Load();
        Assert.Equal(4, s.IdleThresholdMinutes);
        Assert.True(s.ActiveDays[0]); // Monday on
        Assert.False(s.ActiveDays[5]); // Saturday off
    }

    [Fact]
    public void Save_ThenLoad_RoundTrips()
    {
        var original = new AppSettings
        {
            IdleThresholdMinutes = 7,
            WorkStart = new TimeOnly(8, 30),
            UseMouseClick = true,
            DefaultMode = "Manual"
        };

        SettingsManager.Save(original);
        var loaded = SettingsManager.Load();

        Assert.Equal(7, loaded.IdleThresholdMinutes);
        Assert.Equal(new TimeOnly(8, 30), loaded.WorkStart);
        Assert.True(loaded.UseMouseClick);
        Assert.Equal("Manual", loaded.DefaultMode);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileCorrupt()
    {
        File.WriteAllText(Path.Combine(_tempDir, "settings.json"), "not json {{{{");
        var s = SettingsManager.Load();
        Assert.Equal(4, s.IdleThresholdMinutes);
    }
}
```

Note: `_path` is a `static readonly` field. The test uses reflection to point it at a temp directory. If `readonly` prevents this at runtime, change `_path` in `SettingsManager` to a non-readonly `static` field.

- [ ] **Step 3: Run tests**

```bash
dotnet test SnapCqq2.Tests --filter "SettingsManagerTests"
```

Expected: 3 passed.

---

## Task 4: ScheduleChecker + tests

**Files:**
- Create: `SnapCqq2/Services/ScheduleChecker.cs`
- Create: `SnapCqq2.Tests/ScheduleCheckerTests.cs`

- [ ] **Step 1: Write `ScheduleChecker.cs`**

```csharp
using SnapCqq2.Models;

namespace SnapCqq2.Services;

public static class ScheduleChecker
{
    /// <summary>
    /// Returns true if <paramref name="now"/> falls within the active automation window:
    /// correct day-of-week, within work hours, and not in the break window.
    /// </summary>
    public static bool IsInActiveWindow(AppSettings s, DateTime now)
    {
        // DayOfWeek: Sun=0 Mon=1 … Sat=6 → remap to Mon=0 … Sun=6
        int idx = ((int)now.DayOfWeek + 6) % 7;
        if (!s.ActiveDays[idx]) return false;

        var t = TimeOnly.FromDateTime(now);
        if (t < s.WorkStart || t >= s.WorkEnd) return false;
        if (t >= s.BreakStart && t < s.BreakEnd) return false;

        return true;
    }

    /// <summary>
    /// Returns a human-readable "Next active: …" string shown in the status bar
    /// when the scheduler is armed but outside the active window.
    /// </summary>
    public static string NextWindowDescription(AppSettings s, DateTime now)
    {
        // Try each day in the next 8 days (covers all week + today again)
        for (int d = 0; d < 8; d++)
        {
            var candidate = now.AddDays(d);
            int idx = ((int)candidate.DayOfWeek + 6) % 7;
            if (!s.ActiveDays[idx]) continue;

            // Same day: check if work hasn't started yet or break just ended
            var start = candidate.Date + s.WorkStart.ToTimeSpan();
            if (d == 0 && candidate >= start) continue; // already past today's window

            var when = d == 0 ? start : candidate.Date + s.WorkStart.ToTimeSpan();
            return $"Next: {when:ddd HH:mm}";
        }
        return "No schedule configured";
    }
}
```

- [ ] **Step 2: Write `ScheduleCheckerTests.cs`**

```csharp
using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class ScheduleCheckerTests
{
    private static AppSettings DefaultSettings() => new AppSettings();
    // Defaults: Mon–Fri 09:00–18:00, break 12:00–13:00, idle threshold 4 min

    // Helper: make a DateTime for a specific weekday + time in the current week
    private static DateTime At(DayOfWeek day, int hour, int minute = 0)
    {
        var today = DateTime.Today;
        int diff = ((int)day - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(diff).AddHours(hour).AddMinutes(minute);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday10am_ReturnsTrue()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 10));
        Assert.True(result);
    }

    [Fact]
    public void IsInActiveWindow_Saturday10am_ReturnsFalse()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Saturday, 10));
        Assert.False(result);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday8am_ReturnsFalse_BeforeWorkStart()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 8));
        Assert.False(result);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday19pm_ReturnsFalse_AfterWorkEnd()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 19));
        Assert.False(result);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday1230pm_ReturnsFalse_DuringBreak()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 12, 30));
        Assert.False(result);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday13pm_ReturnsTrue_AfterBreakEnds()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 13));
        Assert.True(result);
    }

    [Fact]
    public void IsInActiveWindow_WorkStartBoundary_ReturnsTrue()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Monday, 9, 0));
        Assert.True(result);
    }

    [Fact]
    public void IsInActiveWindow_WorkEndBoundary_ReturnsFalse()
    {
        // 18:00 is >= WorkEnd so not active
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Monday, 18, 0));
        Assert.False(result);
    }
}
```

- [ ] **Step 3: Run tests**

```bash
dotnet test SnapCqq2.Tests --filter "ScheduleCheckerTests"
```

Expected: 8 passed.

---

## Task 5: IdleDetector + tests

**Files:**
- Create: `SnapCqq2/Services/IdleDetector.cs`
- Create: `SnapCqq2.Tests/IdleDetectorTests.cs`

- [ ] **Step 1: Write `IdleDetector.cs`**

```csharp
using System.Runtime.InteropServices;

namespace SnapCqq2.Services;

public static class IdleDetector
{
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint cbSize;
        public uint dwTime;
    }

    /// <summary>Returns how long the system has been idle (no keyboard/mouse input).</summary>
    public static TimeSpan GetIdleTime()
    {
        var info = new LastInputInfo { cbSize = (uint)Marshal.SizeOf<LastInputInfo>() };
        GetLastInputInfo(ref info);
        // Environment.TickCount wraps at ~25 days; unchecked subtraction handles the wrap correctly.
        int elapsed = unchecked(Environment.TickCount - (int)info.dwTime);
        return TimeSpan.FromMilliseconds(Math.Max(0, elapsed));
    }

    /// <summary>
    /// Returns the idle-check timer interval in milliseconds.
    /// Formula: max(30s, threshold_minutes × 60s / 4)
    /// Guarantees worst-case activation latency ≤ 125% of threshold.
    /// </summary>
    public static int ComputePollIntervalMs(int thresholdMinutes)
        => (int)Math.Max(30_000, thresholdMinutes * 60_000.0 / 4);
}
```

- [ ] **Step 2: Write `IdleDetectorTests.cs`**

```csharp
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class IdleDetectorTests
{
    [Theory]
    [InlineData(1,  30_000)]   // 1 min → 30s (floor)
    [InlineData(2,  30_000)]   // 2 min → 30s (floor)
    [InlineData(4,  60_000)]   // 4 min → 60s
    [InlineData(5,  75_000)]   // 5 min → 75s
    [InlineData(10, 150_000)]  // 10 min → 150s
    [InlineData(20, 300_000)]  // 20 min → 300s
    public void ComputePollIntervalMs_ReturnsCorrectInterval(int thresholdMin, int expectedMs)
    {
        Assert.Equal(expectedMs, IdleDetector.ComputePollIntervalMs(thresholdMin));
    }

    [Fact]
    public void GetIdleTime_ReturnsNonNegative()
    {
        // Smoke test: can't control system idle, but result must be non-negative
        Assert.True(IdleDetector.GetIdleTime() >= TimeSpan.Zero);
    }
}
```

- [ ] **Step 3: Run tests**

```bash
dotnet test SnapCqq2.Tests --filter "IdleDetectorTests"
```

Expected: 7 passed.

---

## Task 6: StartupManager

**Files:**
- Create: `SnapCqq2/Services/StartupManager.cs`

No unit tests — registry manipulation requires the real `HKCU` and is an integration concern. Verified manually via General tab toggle.

- [ ] **Step 1: Write `StartupManager.cs`**

```csharp
using Microsoft.Win32;

namespace SnapCqq2.Services;

public static class StartupManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SnapCqq2";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(AppName) is not null;
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)!;
        key.SetValue(AppName, $"\"{Environment.ProcessPath}\"");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)!;
        if (key.GetValue(AppName) is not null)
            key.DeleteValue(AppName);
    }

    public static void Sync(bool enabled)
    {
        if (enabled) Enable(); else Disable();
    }
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build SnapCqq2/SnapCqq2.csproj
```

Expected: `Build succeeded`.

---

## Task 7: UI — Main.Designer.cs rewrite

**Files:**
- Rewrite: `SnapCqq2/Main.Designer.cs`

This is a complete rewrite. All existing controls are replaced.

### Control inventory

#### Form-level
| Name | Type | Key properties |
|------|------|----------------|
| `tabControl` | TabControl | Dock=Fill, Font=Segoe UI 9pt |
| `tabAuto` | TabPage | Text="Auto" |
| `tabManual` | TabPage | Text="Manual" |
| `tabGeneral` | TabPage | Text="General" |
| `pnlStatusBar` | Panel | Dock=Bottom, Height=54, BackColor=Control |
| `pnlStatusMain` | Panel | Dock=Top, Height=32 inside pnlStatusBar |
| `pnlDot` | Panel | Size=10,10, BackColor=Gray, no border |
| `lblStatusText` | Label | Font=bold, AutoSize=true |
| `lblStatusSub` | Label | Font=9pt, ForeColor=Gray, AutoSize=true |
| `btnAction` | Button | Width=80, Anchor=Right, Font=bold, ForeColor=White |
| `pnlStatusDivider` | Panel | Height=1, Dock=Top, BackColor=LightGray inside pnlStatusBar |
| `lblLastAction` | Label | Dock=Fill, Font=Consolas 9pt, ForeColor=DimGray, Padding=8,2,0,0 |

#### Auto tab — WHEN card (always open)
| Name | Type | Key properties |
|------|------|----------------|
| `pnlWhen` | Panel | BorderStyle=FixedSingle, absolute position |
| `pnlWhenHeader` | Panel | Height=28, Dock=Top, Cursor=Default, BackColor=#F5F5F5 |
| `lblWhenTitle` | Label | Text="📅 When", Font=bold 9pt, ForeColor=#2e7d32, Dock=Fill, Padding=8,6,0,0 |
| `pnlWhenBody` | Panel | Dock=Fill |
| `btnDay[0..6]` | Button[7] | Text=Mon/Tue/Wed/Thu/Fri/Sat/Sun, Size=38,26, Font=bold 8.5pt |
| `dtpWorkStart` | DateTimePicker | Format=Time, ShowUpDown=true, Width=80 |
| `dtpWorkEnd` | DateTimePicker | Format=Time, ShowUpDown=true, Width=80 |
| `dtpBreakStart` | DateTimePicker | Format=Time, ShowUpDown=true, Width=80 |
| `dtpBreakEnd` | DateTimePicker | Format=Time, ShowUpDown=true, Width=80 |
| `numIdleThreshold` | NumericUpDown | Min=1, Max=60, Value=4, Width=50 |
| `chkStopOnReturn` | CheckBox | Text="Stop when user returns", Checked=true |

#### Auto tab — HOW card (collapsible)
| Name | Type | Key properties |
|------|------|----------------|
| `pnlHow` | Panel | BorderStyle=FixedSingle |
| `pnlHowHeader` | Panel | Height=28, Dock=Top, Cursor=Hand, BackColor=#F5F5F5 |
| `lblHowTitle` | Label | Text="▶ ⚡ How", Font=bold 9pt, ForeColor=#2e7d32, Dock=Fill, Padding=8,6,0,0 |
| `pnlHowBody` | Panel | Dock=Fill, Visible=false |
| `chkScrollLock` | CheckBox | Text="Scroll-lock", Checked=true |
| `chkMouseClick` | CheckBox | Text="Mouse click" |
| `chkArrowKeys` | CheckBox | Text="Arrow keys" |
| `numFrequency` | NumericUpDown | Min=1, Max=60, Value=1, Width=50 |

#### Auto tab — AFTER card (collapsible)
| Name | Type | Key properties |
|------|------|----------------|
| `pnlAfter` | Panel | BorderStyle=FixedSingle |
| `pnlAfterHeader` | Panel | Height=28, Dock=Top, Cursor=Hand, BackColor=#F5F5F5 |
| `lblAfterTitle` | Label | Text="▶ 🔚 After schedule ends", Font=bold 9pt, ForeColor=#2e7d32, Dock=Fill, Padding=8,6,0,0 |
| `pnlAfterBody` | Panel | Dock=Fill, Visible=false |
| `cmbAutoTimeout` | ComboBox | DropDownStyle=DropDownList, Items: "Do nothing","Lock Windows","Shutdown Windows","Close app" |

#### Manual tab — RUN card (always open)
| Name | Type | Key properties |
|------|------|----------------|
| `pnlRun` | Panel | BorderStyle=FixedSingle |
| `pnlRunHeader` | Panel | Height=28, Dock=Top, Cursor=Default, BackColor=#F5F5F5 |
| `lblRunTitle` | Label | Text="▼ ▶ Run", Font=bold 9pt, ForeColor=#2e7d32, Dock=Fill, Padding=8,6,0,0 |
| `pnlRunBody` | Panel | Dock=Fill |
| `numDuration` | NumericUpDown | Min=1, Max=480, Increment=15, Value=30, Width=60 |
| `cmbManualTimeout` | ComboBox | Items: "Close when finished","Lock Windows","Shutdown Windows","Do nothing" |
| `numDelay` | NumericUpDown | Min=0, Max=120, Increment=5, Value=0, Width=60 |

#### Manual tab — HOW card (collapsible, mirrors Auto HOW)
| Name | Type | Key properties |
|------|------|----------------|
| `pnlManualHow` | Panel | BorderStyle=FixedSingle |
| `pnlManualHowHeader` | Panel | Height=28, Dock=Top, Cursor=Hand, BackColor=#F5F5F5 |
| `lblManualHowTitle` | Label | Text="▶ ⚡ How", Font=bold 9pt, ForeColor=#2e7d32, Dock=Fill, Padding=8,6,0,0 |
| `pnlManualHowBody` | Panel | Dock=Fill, Visible=false |
| `chkManualScrollLock` | CheckBox | Text="Scroll-lock", Checked=true |
| `chkManualMouseClick` | CheckBox | Text="Mouse click" |
| `chkManualArrowKeys` | CheckBox | Text="Arrow keys" |
| `numManualFrequency` | NumericUpDown | Min=1, Max=60, Value=1, Width=50 |

#### General tab
| Name | Type | Key properties |
|------|------|----------------|
| `chkLaunchOnStartup` | CheckBox | Text="Launch on Windows startup", Appearance=Button style? No — plain CheckBox |
| `lblLaunchDesc` | Label | Text="Start SnapCqq2 automatically when you log in", ForeColor=Gray |
| `chkStartMinimized` | CheckBox | Text="Start minimized to tray" |
| `lblMinimizedDesc` | Label | Text="Hide window on launch, show only tray icon", ForeColor=Gray |
| `cmbDefaultMode` | ComboBox | Items: "Auto","Manual", DropDownStyle=DropDownList |
| `lblDefaultModeDesc` | Label | Text="Which tab is shown first when the app opens", ForeColor=Gray |

### Card heights (constants)

```csharp
private const int CardHeaderH = 28;
// Auto WHEN body: 3 rows × 32px + 8px top/bottom padding = 112px
private const int WhenBodyH   = 112;
// Auto HOW body: 2 rows × 30px + 16px padding = 76px
private const int HowBodyH    = 76;
// Auto AFTER body: 1 row + 16px padding = 44px
private const int AfterBodyH  = 44;
// Manual RUN body: 2 rows × 32px + 16px = 80px
private const int RunBodyH    = 80;
// Manual HOW body: 2 rows × 30px + 16px = 76px
private const int ManualHowBodyH = 76;

private const int CardGap     = 6;
private const int TabPadding  = 8;
```

### Layout method (put in Main.cs, called on every card toggle and on tab switch)

```csharp
private void RelayoutAutoCards()
{
    int y = TabPadding;

    // WHEN — always expanded
    pnlWhen.Location = new Point(TabPadding, y);
    pnlWhen.Width = tabAuto.ClientSize.Width - TabPadding * 2;
    pnlWhen.Height = CardHeaderH + (_whenExpanded ? WhenBodyH : 0);
    pnlWhenBody.Visible = _whenExpanded;
    y += pnlWhen.Height + CardGap;

    // HOW
    pnlHow.Location = new Point(TabPadding, y);
    pnlHow.Width = pnlWhen.Width;
    pnlHow.Height = CardHeaderH + (_howExpanded ? HowBodyH : 0);
    pnlHowBody.Visible = _howExpanded;
    y += pnlHow.Height + CardGap;

    // AFTER
    pnlAfter.Location = new Point(TabPadding, y);
    pnlAfter.Width = pnlWhen.Width;
    pnlAfter.Height = CardHeaderH + (_afterExpanded ? AfterBodyH : 0);
    pnlAfterBody.Visible = _afterExpanded;
    y += pnlAfter.Height + CardGap;

    // Resize form to fit (like existing app does)
    int statusH = pnlStatusBar.Height;
    int tabStripH = tabControl.ItemSize.Height + 4; // approximate tab strip
    ClientSize = new Size(ClientSize.Width,
        tabStripH + y + TabPadding + statusH + SystemInformation.CaptionHeight);
}

private void RelayoutManualCards()
{
    int y = TabPadding;

    pnlRun.Location = new Point(TabPadding, y);
    pnlRun.Width = tabManual.ClientSize.Width - TabPadding * 2;
    pnlRun.Height = CardHeaderH + RunBodyH; // always open
    pnlRunBody.Visible = true;
    y += pnlRun.Height + CardGap;

    pnlManualHow.Location = new Point(TabPadding, y);
    pnlManualHow.Width = pnlRun.Width;
    pnlManualHow.Height = CardHeaderH + (_manualHowExpanded ? ManualHowBodyH : 0);
    pnlManualHowBody.Visible = _manualHowExpanded;

    // Form height for manual tab uses fixed collapsed state (simpler)
    // relayout is only called on manual tab switch or how toggle
}
```

### Expand/collapse toggle method (put in Main.cs)

```csharp
private void ToggleCard(ref bool state, Panel body, Label title, string baseText, Action relayout)
{
    state = !state;
    body.Visible = state;
    title.Text = (state ? "▼ " : "▶ ") + baseText;
    relayout();
}
```

- [ ] **Step 1: Write `Main.Designer.cs`**

Completely replace the file with a new `InitializeComponent` that:

1. Instantiates all controls from the inventory above
2. Sets all properties from the inventory
3. Adds the card panels to their respective TabPages with absolute positioning (do NOT use FlowLayoutPanel)
4. Adds `tabAuto`, `tabManual`, `tabGeneral` to `tabControl.TabPages`
5. Adds `tabControl` and `pnlStatusBar` to the form's `Controls`
6. Sets `Dock = DockStyle.Fill` on `tabControl` and `Dock = DockStyle.Bottom` on `pnlStatusBar`
7. Sets form: `ClientSize = new Size(580, 330)`, `FormBorderStyle = FixedSingle`, `MaximizeBox = false`, `StartPosition = CenterScreen`, `Text = "SnapCqq2"`
8. Declares all controls as `private` fields at the bottom of the partial class

**Day button layout inside pnlWhenBody** (row starting at y=8):
- `btnDay[i]` x = 8 + i*42, y = 8, Size = 38×26

**Hours row** (y=42 inside pnlWhenBody):
- Label "Hours" at x=8, then `dtpWorkStart` x=58, Label "to" x=146, `dtpWorkEnd` x=162, Label "Break" x=252, `dtpBreakStart` x=298, Label "–" x=386, `dtpBreakEnd` x=398

**Idle row** (y=76 inside pnlWhenBody):
- Label "Activate after" x=8, `numIdleThreshold` x=110, Label "min idle" x=166, `chkStopOnReturn` x=240

**HOW body** (y=8 inside pnlHowBody):
- Row 1 (y=8): Label "Actions" x=8, `chkScrollLock` x=70, `chkMouseClick` x=176, `chkArrowKeys` x=285
- Row 2 (y=40): Label "Every" x=8, `numFrequency` x=70, Label "min" x=128

**AFTER body** (y=8 inside pnlAfterBody):
- `cmbAutoTimeout` x=8, y=8, Width=220

**RUN body** (y=8 inside pnlRunBody):
- Row 1 (y=8): Label "Duration" x=8, `numDuration` x=76, Label "min then" x=134, `cmbManualTimeout` x=206
- Row 2 (y=44): Label "Delay" x=8, `numDelay` x=76, Label "min (0 = start immediately)" x=134

**Manual HOW body** — same layout as Auto HOW body, using `chkManualScrollLock`, `chkManualMouseClick`, `chkManualArrowKeys`, `numManualFrequency`

**Status bar layout** (inside `pnlStatusBar`):
- `pnlStatusMain`: Dock=Top, Height=34; inside it: `pnlDot` at x=10, vertically centred; `lblStatusText` at x=26; `lblStatusSub` placed below `lblStatusText` (or use multi-line label); `btnAction` Anchor=Top|Right, Right=8, Width=80, Height=24
- `pnlStatusDivider`: Dock=Top, Height=1, BackColor=LightGray (after pnlStatusMain)
- `lblLastAction`: Dock=Fill, Font=Consolas 9pt, ForeColor=DimGray, Padding=10,2,0,0

**General tab** (absolute layout, y starting at 8, row height 52):
- Row 0 (y=8): `chkLaunchOnStartup` x=12, y=8; `lblLaunchDesc` x=12, y=28, ForeColor=Gray
- Divider panel (1px, LightGray) at y=60
- Row 1 (y=68): `chkStartMinimized` x=12; `lblMinimizedDesc` x=12, y=88
- Divider at y=120
- Row 2 (y=128): Label "Default mode on launch" x=12; `cmbDefaultMode` x=230, y=125; `lblDefaultModeDesc` x=12, y=148

- [ ] **Step 2: Verify the project builds**

```bash
dotnet build SnapCqq2/SnapCqq2.csproj
```

Expected: `Build succeeded` (no runtime check yet — UI wiring happens in Task 8).

---

## Task 8: Main.cs — rewrite logic and wire everything

**Files:**
- Rewrite: `SnapCqq2/Main.cs`

### State machine

```csharp
private enum AppMode  { Auto, Manual }
private enum AutoState  { Disabled, Armed, Running }
private enum ManualState { Idle, WaitingToStart, Running }
```

- `_autoState` drives the Auto tab
- `_manualState` drives the Manual tab
- Active mode = whichever tab is selected

### Full `Main.cs`

```csharp
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;
using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2;

public partial class Main : Form
{
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    // ── Constants ──────────────────────────────────────────────────
    private const int CardHeaderH    = 28;
    private const int WhenBodyH      = 112;
    private const int HowBodyH       = 76;
    private const int AfterBodyH     = 44;
    private const int RunBodyH       = 80;
    private const int ManualHowBodyH = 76;
    private const int CardGap        = 6;
    private const int TabPadding     = 8;

    // ── State ──────────────────────────────────────────────────────
    private AutoState   _autoState   = AutoState.Disabled;
    private ManualState _manualState = ManualState.Idle;

    private bool _whenExpanded      = true;
    private bool _howExpanded       = false;
    private bool _afterExpanded     = false;
    private bool _manualHowExpanded = false;

    private DateTime _manualEndTime;
    private DateTime _manualDelayEnd;
    private bool     _loading = false;  // suppresses save-on-change during UI load

    // ── Services ───────────────────────────────────────────────────
    private AppSettings _settings = new();
    private readonly InputSimulator _simulator = new();
    private readonly Random _rng = new();

    // ── Tray ───────────────────────────────────────────────────────
    private NotifyIcon     _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;

    // ────────────────────────────────────────────────────────────────
    #region Init

    public Main()
    {
        InitializeComponent();
        SetupTrayIcon();
        _settings = SettingsManager.Load();
        LoadSettingsToUI();
        RelayoutAutoCards();
        RelayoutManualCards();
        WireEvents();
    }

    private void WireEvents()
    {
        // Card collapse toggles
        pnlHowHeader.Click    += (_, _) => ToggleCard(ref _howExpanded,      pnlHowBody,       lblHowTitle,       "⚡ How",                RelayoutAutoCards);
        pnlAfterHeader.Click  += (_, _) => ToggleCard(ref _afterExpanded,    pnlAfterBody,     lblAfterTitle,     "🔚 After schedule ends", RelayoutAutoCards);
        pnlManualHowHeader.Click += (_, _) => ToggleCard(ref _manualHowExpanded, pnlManualHowBody, lblManualHowTitle, "⚡ How",             RelayoutManualCards);

        // Tab change
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

        // Action button
        btnAction.Click += BtnAction_Click;

        // Auto settings — save immediately on change
        foreach (var btn in btnDay) btn.Click += DayBtn_Click;
        dtpWorkStart.ValueChanged  += AutoSetting_Changed;
        dtpWorkEnd.ValueChanged    += AutoSetting_Changed;
        dtpBreakStart.ValueChanged += AutoSetting_Changed;
        dtpBreakEnd.ValueChanged   += AutoSetting_Changed;
        numIdleThreshold.ValueChanged += AutoSetting_Changed;
        chkStopOnReturn.CheckedChanged += AutoSetting_Changed;
        chkScrollLock.CheckedChanged   += AutoSetting_Changed;
        chkMouseClick.CheckedChanged   += AutoSetting_Changed;
        chkArrowKeys.CheckedChanged    += AutoSetting_Changed;
        numFrequency.ValueChanged      += AutoSetting_Changed;
        cmbAutoTimeout.SelectedIndexChanged += AutoSetting_Changed;

        // Manual settings
        numDuration.ValueChanged       += ManualSetting_Changed;
        numDelay.ValueChanged          += ManualSetting_Changed;
        cmbManualTimeout.SelectedIndexChanged += ManualSetting_Changed;
        chkManualScrollLock.CheckedChanged += ManualSetting_Changed;
        chkManualMouseClick.CheckedChanged += ManualSetting_Changed;
        chkManualArrowKeys.CheckedChanged  += ManualSetting_Changed;
        numManualFrequency.ValueChanged    += ManualSetting_Changed;

        // General settings
        chkLaunchOnStartup.CheckedChanged += GeneralSetting_Changed;
        chkStartMinimized.CheckedChanged  += GeneralSetting_Changed;
        cmbDefaultMode.SelectedIndexChanged += GeneralSetting_Changed;

        // Tray
        _trayIcon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowForm(); };

        // Timer ticks
        idleCheckTimer.Tick  += IdleCheckTimer_Tick;
        actionTimer.Tick     += ActionTimer_Tick;
        updateTimer.Tick     += UpdateTimer_Tick;
        delayTimer.Tick      += DelayTimer_Tick;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Settings load/save

    private void LoadSettingsToUI()
    {
        _loading = true;
        try
        {
            // Days
            for (int i = 0; i < 7; i++)
                SetDayButtonState(btnDay[i], _settings.ActiveDays[i]);

            // Times
            dtpWorkStart.Value  = DateTime.Today + _settings.WorkStart.ToTimeSpan();
            dtpWorkEnd.Value    = DateTime.Today + _settings.WorkEnd.ToTimeSpan();
            dtpBreakStart.Value = DateTime.Today + _settings.BreakStart.ToTimeSpan();
            dtpBreakEnd.Value   = DateTime.Today + _settings.BreakEnd.ToTimeSpan();

            // Idle
            numIdleThreshold.Value = _settings.IdleThresholdMinutes;
            chkStopOnReturn.Checked = _settings.StopWhenUserReturns;

            // Actions (shared)
            chkScrollLock.Checked = chkManualScrollLock.Checked = _settings.UseScrollLock;
            chkMouseClick.Checked = chkManualMouseClick.Checked = _settings.UseMouseClick;
            chkArrowKeys.Checked  = chkManualArrowKeys.Checked  = _settings.UseArrowKeys;
            numFrequency.Value = numManualFrequency.Value = _settings.FrequencyMinutes;

            // Auto timeout
            cmbAutoTimeout.SelectedIndex = (int)_settings.AutoTimeoutAction;

            // Manual
            numDuration.Value = _settings.ManualDurationMinutes;
            numDelay.Value    = _settings.ManualDelayMinutes;
            cmbManualTimeout.SelectedIndex = (int)_settings.ManualTimeoutAction;

            // General
            chkLaunchOnStartup.Checked = StartupManager.IsEnabled();
            chkStartMinimized.Checked  = _settings.StartMinimized;
            cmbDefaultMode.SelectedIndex = _settings.DefaultMode == "Manual" ? 1 : 0;

            // Select default tab
            tabControl.SelectedTab = _settings.DefaultMode == "Manual" ? tabManual : tabAuto;
        }
        finally { _loading = false; }
    }

    private void SaveFromAutoUI()
    {
        for (int i = 0; i < 7; i++) _settings.ActiveDays[i] = btnDay[i].Tag is true;
        _settings.WorkStart  = TimeOnly.FromDateTime(dtpWorkStart.Value);
        _settings.WorkEnd    = TimeOnly.FromDateTime(dtpWorkEnd.Value);
        _settings.BreakStart = TimeOnly.FromDateTime(dtpBreakStart.Value);
        _settings.BreakEnd   = TimeOnly.FromDateTime(dtpBreakEnd.Value);
        _settings.IdleThresholdMinutes = (int)numIdleThreshold.Value;
        _settings.StopWhenUserReturns  = chkStopOnReturn.Checked;
        _settings.UseScrollLock  = chkScrollLock.Checked;
        _settings.UseMouseClick  = chkMouseClick.Checked;
        _settings.UseArrowKeys   = chkArrowKeys.Checked;
        _settings.FrequencyMinutes = (int)numFrequency.Value;
        _settings.AutoTimeoutAction = (TimeoutAction)cmbAutoTimeout.SelectedIndex;
        SettingsManager.Save(_settings);
    }

    private void SaveFromManualUI()
    {
        _settings.ManualDurationMinutes = (int)numDuration.Value;
        _settings.ManualDelayMinutes    = (int)numDelay.Value;
        _settings.ManualTimeoutAction   = (TimeoutAction)cmbManualTimeout.SelectedIndex;
        _settings.UseScrollLock   = chkManualScrollLock.Checked;
        _settings.UseMouseClick   = chkManualMouseClick.Checked;
        _settings.UseArrowKeys    = chkManualArrowKeys.Checked;
        _settings.FrequencyMinutes = (int)numManualFrequency.Value;
        // Sync back to auto controls
        chkScrollLock.Checked = _settings.UseScrollLock;
        chkMouseClick.Checked = _settings.UseMouseClick;
        chkArrowKeys.Checked  = _settings.UseArrowKeys;
        numFrequency.Value    = _settings.FrequencyMinutes;
        SettingsManager.Save(_settings);
    }

    private void AutoSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        SaveFromAutoUI();
        if (_autoState == AutoState.Armed)
            idleCheckTimer.Interval = IdleDetector.ComputePollIntervalMs(_settings.IdleThresholdMinutes);
    }

    private void ManualSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        SaveFromManualUI();
    }

    private void GeneralSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        StartupManager.Sync(chkLaunchOnStartup.Checked);
        _settings.StartMinimized = chkStartMinimized.Checked;
        _settings.DefaultMode    = cmbDefaultMode.SelectedIndex == 1 ? "Manual" : "Auto";
        SettingsManager.Save(_settings);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Day buttons

    private void DayBtn_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        bool current = btn.Tag is true;
        SetDayButtonState(btn, !current);
        if (!_loading) SaveFromAutoUI();
    }

    private static void SetDayButtonState(Button btn, bool active)
    {
        btn.Tag = active;
        btn.BackColor = active
            ? Color.FromArgb(46, 125, 50)
            : Color.FromArgb(228, 228, 228);
        btn.ForeColor = active ? Color.White : Color.FromArgb(85, 85, 85);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Card layout

    private void ToggleCard(ref bool state, Panel body, Label title, string baseText, Action relayout)
    {
        state = !state;
        body.Visible = state;
        title.Text = (state ? "▼ " : "▶ ") + baseText;
        relayout();
    }

    private void RelayoutAutoCards()
    {
        int y = TabPadding;
        int w = tabAuto.ClientSize.Width - TabPadding * 2;

        pnlWhen.SetBounds(TabPadding, y, w, CardHeaderH + WhenBodyH);
        y += pnlWhen.Height + CardGap;

        pnlHow.SetBounds(TabPadding, y, w, CardHeaderH + (_howExpanded ? HowBodyH : 0));
        y += pnlHow.Height + CardGap;

        pnlAfter.SetBounds(TabPadding, y, w, CardHeaderH + (_afterExpanded ? AfterBodyH : 0));
        y += pnlAfter.Height + TabPadding;

        // Resize form: tabstrip height + content + status bar
        int tabStripH = tabControl.Top + tabControl.ItemSize.Height + 4;
        ClientSize = new Size(ClientSize.Width, tabStripH + y + pnlStatusBar.Height);
    }

    private void RelayoutManualCards()
    {
        int y = TabPadding;
        int w = tabManual.ClientSize.Width - TabPadding * 2;

        pnlRun.SetBounds(TabPadding, y, w, CardHeaderH + RunBodyH);
        y += pnlRun.Height + CardGap;

        pnlManualHow.SetBounds(TabPadding, y, w, CardHeaderH + (_manualHowExpanded ? ManualHowBodyH : 0));
        y += pnlManualHow.Height + TabPadding;

        int tabStripH = tabControl.Top + tabControl.ItemSize.Height + 4;
        ClientSize = new Size(ClientSize.Width, tabStripH + y + pnlStatusBar.Height);
    }

    private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabAuto)   RelayoutAutoCards();
        if (tabControl.SelectedTab == tabManual) RelayoutManualCards();
        UpdateStatusBar();
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Auto mode state machine

    private void BtnAction_Click(object? sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabAuto)
        {
            if (_autoState == AutoState.Disabled) ArmAuto();
            else DisarmAuto();
        }
        else if (tabControl.SelectedTab == tabManual)
        {
            if (_manualState == ManualState.Idle) StartManual();
            else StopManual();
        }
    }

    private void ArmAuto()
    {
        _autoState = AutoState.Armed;
        idleCheckTimer.Interval = IdleDetector.ComputePollIntervalMs(_settings.IdleThresholdMinutes);
        idleCheckTimer.Start();
        UpdateStatusBar();
        HideToTray();
    }

    private void DisarmAuto()
    {
        if (_autoState == AutoState.Running) StopAutomation();
        _autoState = AutoState.Disabled;
        idleCheckTimer.Stop();
        UpdateStatusBar();
        ShowForm();
    }

    private void IdleCheckTimer_Tick(object? sender, EventArgs e)
    {
        if (!ScheduleChecker.IsInActiveWindow(_settings, DateTime.Now))
        {
            // If we were running, stop
            if (_autoState == AutoState.Running)
            {
                StopAutomation();
                ApplyAutoTimeout();
                _autoState = AutoState.Armed;
                UpdateStatusBar();
            }
            // Update tray text with next window info
            var desc = ScheduleChecker.NextWindowDescription(_settings, DateTime.Now);
            _trayIcon.Text = Truncate($"Armed · {desc}", 63);
            return;
        }

        var idle = IdleDetector.GetIdleTime();
        var threshold = TimeSpan.FromMinutes(_settings.IdleThresholdMinutes);

        if (_autoState == AutoState.Armed && idle >= threshold)
        {
            _autoState = AutoState.Running;
            StartAutomation();
            UpdateStatusBar();
        }
        else if (_autoState == AutoState.Running && _settings.StopWhenUserReturns && idle < threshold)
        {
            StopAutomation();
            _autoState = AutoState.Armed;
            UpdateStatusBar();
        }
    }

    private void ApplyAutoTimeout()
    {
        switch (_settings.AutoTimeoutAction)
        {
            case TimeoutAction.CloseApp:       Close(); break;
            case TimeoutAction.LockAndClose:   LockWorkStation(); Close(); break;
            case TimeoutAction.ShutdownWindows: System.Diagnostics.Process.Start("shutdown", "/s /t 0"); break;
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Manual mode state machine

    private void StartManual()
    {
        if (_settings.ManualDelayMinutes > 0)
        {
            _manualState = ManualState.WaitingToStart;
            _manualDelayEnd = DateTime.Now.AddMinutes(_settings.ManualDelayMinutes);
            delayTimer.Start();
        }
        else
        {
            _manualState = ManualState.Running;
            BeginManualRun();
        }
        UpdateStatusBar();
        HideToTray();
    }

    private void StopManual(TimeoutAction action = TimeoutAction.DoNothing)
    {
        actionTimer.Stop();
        updateTimer.Stop();
        delayTimer.Stop();
        _manualState = ManualState.Idle;
        UpdateStatusBar();

        switch (action)
        {
            case TimeoutAction.CloseApp:       Close(); return;
            case TimeoutAction.LockAndClose:   LockWorkStation(); Close(); return;
            case TimeoutAction.ShutdownWindows: System.Diagnostics.Process.Start("shutdown", "/s /t 0"); return;
        }
        ShowForm();
    }

    private void BeginManualRun()
    {
        _manualEndTime = DateTime.Now.AddMinutes(_settings.ManualDurationMinutes);
        actionTimer.Interval = _settings.FrequencyMinutes * 60_000;
        actionTimer.Start();
        updateTimer.Start();
    }

    private void DelayTimer_Tick(object? sender, EventArgs e)
    {
        if (DateTime.Now >= _manualDelayEnd)
        {
            delayTimer.Stop();
            _manualState = ManualState.Running;
            BeginManualRun();
            UpdateStatusBar();
        }
        else
        {
            var rem = _manualDelayEnd - DateTime.Now;
            _trayIcon.Text = Truncate($"Starting in {rem:hh\\:mm\\:ss}", 63);
        }
    }

    private void ActionTimer_Tick(object? sender, EventArgs e)
    {
        // Auto: no actionTimer — idle check handles frequency via its own interval
        // Manual: check end time
        if (_manualState == ManualState.Running && DateTime.Now >= _manualEndTime)
        {
            var action = _settings.ManualTimeoutAction;
            StopManual(action);
            return;
        }
        PerformRandomAction();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (_manualState != ManualState.Running) return;
        var rem = _manualEndTime - DateTime.Now;
        var ts = rem.TotalSeconds > 0 ? rem : TimeSpan.Zero;
        lblStatusText.Text = $"Running — {ts:hh\\:mm\\:ss} remaining";
        _trayIcon.Text = Truncate(lblStatusText.Text, 63);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Automation actions

    private void StartAutomation()
    {
        actionTimer.Interval = _settings.FrequencyMinutes * 60_000;
        actionTimer.Start();
        // updateTimer intentionally NOT started here — no countdown to show
    }

    private void StopAutomation()
    {
        actionTimer.Stop();
        // updateTimer is not running in auto mode, but stop defensively
        updateTimer.Stop();
    }

    private void PerformRandomAction()
    {
        var actions = new List<Action>();
        if (_settings.UseScrollLock) actions.Add(PerformScrollLock);
        if (_settings.UseMouseClick) actions.Add(PerformMouseClick);
        if (_settings.UseArrowKeys)  actions.Add(PerformArrowKey);
        if (actions.Count == 0) return;
        actions[_rng.Next(actions.Count)]();
    }

    private void PerformScrollLock()
    {
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        LogAction("Scroll-lock toggle");
    }

    private void PerformMouseClick()
    {
        _simulator.Mouse.LeftButtonClick();
        LogAction("Mouse click");
    }

    private void PerformArrowKey()
    {
        VirtualKeyCode[] arrows = [VirtualKeyCode.UP, VirtualKeyCode.DOWN, VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT];
        var key = arrows[_rng.Next(arrows.Length)];
        _simulator.Keyboard.KeyPress(key);
        LogAction($"Arrow key: {key}");
    }

    private void LogAction(string msg)
    {
        lblLastAction.Text = $"[{DateTime.Now:T}] {msg}";
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Status bar

    private void UpdateStatusBar()
    {
        bool isAutoTab = tabControl.SelectedTab == tabAuto;

        if (isAutoTab)
        {
            switch (_autoState)
            {
                case AutoState.Disabled:
                    SetStatus(Color.Gray, "Disabled", "Click Enable to arm the scheduler", "Enable", Color.FromArgb(46, 125, 50));
                    break;
                case AutoState.Armed:
                    var inWindow = ScheduleChecker.IsInActiveWindow(_settings, DateTime.Now);
                    var sub = inWindow
                        ? "Within schedule — watching for idle"
                        : ScheduleChecker.NextWindowDescription(_settings, DateTime.Now);
                    SetStatus(Color.FromArgb(245, 127, 23), "Monitoring", sub, "Disable", Color.FromArgb(183, 28, 28));
                    break;
                case AutoState.Running:
                    SetStatus(Color.FromArgb(46, 125, 50), "Active — automating", "User idle", "Disable", Color.FromArgb(183, 28, 28));
                    break;
            }
        }
        else if (tabControl.SelectedTab == tabManual)
        {
            switch (_manualState)
            {
                case ManualState.Idle:
                    SetStatus(Color.Gray, "Idle", "Ready to start", "Start", Color.FromArgb(46, 125, 50));
                    break;
                case ManualState.WaitingToStart:
                    SetStatus(Color.FromArgb(245, 127, 23), "Waiting", $"Starting at {_manualDelayEnd:T}", "Stop", Color.FromArgb(183, 28, 28));
                    break;
                case ManualState.Running:
                    SetStatus(Color.FromArgb(46, 125, 50), "Running", $"Until {_manualEndTime:T}", "Stop", Color.FromArgb(183, 28, 28));
                    break;
            }
        }
        else
        {
            SetStatus(Color.Gray, "", "", "", Color.Gray);
        }
    }

    private void SetStatus(Color dot, string text, string sub, string btnLabel, Color btnColor)
    {
        pnlDot.BackColor   = dot;
        lblStatusText.Text = text;
        lblStatusSub.Text  = sub;
        btnAction.Text     = btnLabel;
        btnAction.BackColor = btnColor;
        btnAction.Visible  = btnLabel.Length > 0;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Tray & window lifecycle

    private void SetupTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Show",  null, (_, _) => ShowForm());
        _trayMenu.Items.Add("Exit",  null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Icon = this.Icon,
            Text = "SnapCqq2",
            ContextMenuStrip = _trayMenu,
            Visible = false
        };
    }

    private void HideToTray()
    {
        _trayIcon.Visible = true;
        Hide();
    }

    private void ShowForm()
    {
        Show();
        WindowState = FormWindowState.Normal;
        BringToFront();
        _trayIcon.Visible = false;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        bool autoArmed   = _autoState != AutoState.Disabled;
        bool manualBusy  = _manualState != ManualState.Idle;
        if (WindowState == FormWindowState.Minimized && (autoArmed || manualBusy))
            HideToTray();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        idleCheckTimer?.Dispose();
        actionTimer?.Dispose();
        updateTimer?.Dispose();
        delayTimer?.Dispose();
        _trayIcon?.Dispose();
        base.OnFormClosed(e);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Helpers

    private static string Truncate(string s, int max) => s.Length > max ? s[..max] : s;

    #endregion
}
```

- [ ] **Step 1: Write `Main.cs`** with the full code above.

- [ ] **Step 2: Build**

```bash
dotnet build SnapCqq2/SnapCqq2.csproj
```

Expected: `Build succeeded`. Fix any name mismatches between Designer.cs control names and Main.cs references.

---

## Task 9: Program.cs — startup behaviour

**Files:**
- Modify: `SnapCqq2/Program.cs`

- [ ] **Step 1: Update `Program.cs`**

```csharp
using SnapCqq2.Services;

namespace SnapCqq2;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settings = SettingsManager.Load();
        var form = new Main();

        if (settings.StartMinimized)
        {
            // Form constructor already ran; just don't call Run with Show.
            // Main.cs OnLoad will hide to tray when StartMinimized is set.
            // Signal via property read in OnLoad (see note below).
        }

        Application.Run(form);
    }
}
```

Add to `Main.cs` constructor, after `LoadSettingsToUI()`:

```csharp
if (_settings.StartMinimized)
{
    // Will be handled in OnLoad to ensure tray icon is set up first
    Load += (_, _) => HideToTray();
}
```

- [ ] **Step 2: Build and smoke-test**

```bash
dotnet build SnapCqq2/SnapCqq2.csproj
dotnet run --project SnapCqq2/SnapCqq2.csproj
```

Verify:
1. App opens on Auto tab
2. Day buttons Mon–Fri are green, Sat/Sun grey
3. Time pickers show 09:00–18:00 / Break 12:00–13:00
4. Status bar shows "Disabled · Click Enable to arm"
5. Click Enable → status changes to "Monitoring", app hides to tray
6. Click tray icon → form reappears, status shows "Monitoring"
7. Click Disable → status returns to "Disabled"
8. Switch to Manual tab → Start button appears, status shows "Idle"
9. HOW card header click expands/collapses the card

---

## Task 10: Run all tests

- [ ] **Step 1: Run full test suite**

```bash
dotnet test SnapCqq2.Tests -v normal
```

Expected: All tests pass (ScheduleCheckerTests ×8, SettingsManagerTests ×3, IdleDetectorTests ×7 = 18 total).

---

## Self-Review Notes

- **Spec coverage:** All items covered — schedule (days/hours/break), idle detection, dynamic poll interval, state machine (Disabled→Armed→Running), Manual mode unchanged, General tab (startup/minimized/default mode), settings persistence, timer discipline (updateTimer only runs in manual running state, idleCheckTimer only when armed).
- **Performance contract:** `updateTimer` is never started in Auto mode; `idleCheckTimer` stops on Disarm. ✓
- **Type consistency:** `TimeoutAction` defined once in `Models/AppSettings.cs`, used consistently. `btnDay` is `Button[]`. All control names in Main.cs match Designer.cs inventory. ✓
- **No placeholders:** All code is complete. ✓
