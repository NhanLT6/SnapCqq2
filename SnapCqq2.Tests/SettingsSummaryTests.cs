using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class SettingsSummaryTests
{
    [Theory]
    [InlineData(new[] { true, true, true, true, true, false, false },   "Mon–Fri")]
    [InlineData(new[] { true, true, true, true, true, true, true },     "Every day")]
    [InlineData(new[] { false, false, false, false, false, false, false }, "No days")]
    [InlineData(new[] { true, false, true, false, true, false, false }, "Mon, Wed, Fri")]
    [InlineData(new[] { true, true, false, false, false, true, true },  "Mon, Tue, Sat, Sun")]
    [InlineData(new[] { false, true, true, true, false, false, true },  "Tue–Thu, Sun")]
    public void Days_CollapsesRunsOfThreeOrMore(bool[] days, string expected)
    {
        Assert.Equal(expected, SettingsSummary.Days(days));
    }

    [Fact]
    public void Schedule_Defaults()
    {
        Assert.Equal("Mon–Fri 09:00–18:00, break 12:00–13:00 · 4 min idle", SettingsSummary.Schedule(new AppSettings()));
    }

    [Fact]
    public void Schedule_OmitsEmptyBreak()
    {
        var s = new AppSettings { BreakStart = new TimeOnly(12, 0), BreakEnd = new TimeOnly(12, 0) };
        Assert.Equal("Mon–Fri 09:00–18:00 · 4 min idle", SettingsSummary.Schedule(s));
    }

    [Fact]
    public void Actions_ListsEnabledWithFrequency()
    {
        var s = new AppSettings { UseScrollLock = false, UseMouseMove = true, UseTeamsKeepAlive = true, FrequencyMinutes = 2 };
        Assert.Equal("Mouse move, Teams · every 2 min", SettingsSummary.Actions(s));
    }

    [Fact]
    public void Actions_NothingSelected()
    {
        var s = new AppSettings { UseScrollLock = false };
        Assert.Equal("Nothing selected", SettingsSummary.Actions(s));
    }

    [Theory]
    [InlineData(0,   "until stopped")]
    [InlineData(30,  "30 min")]
    [InlineData(60,  "1 h")]
    [InlineData(90,  "1 h 30 min")]
    [InlineData(120, "2 h")]
    public void Duration_Formats(int minutes, string expected)
    {
        Assert.Equal(expected, SettingsSummary.Duration(minutes));
    }

    [Fact]
    public void Countdown_UnderAndOverAnHour()
    {
        Assert.Equal("4:05",   SettingsSummary.Countdown(TimeSpan.FromSeconds(245)));
        Assert.Equal("2h 07m", SettingsSummary.Countdown(TimeSpan.FromMinutes(127)));
        Assert.Equal("0:00",   SettingsSummary.Countdown(TimeSpan.FromSeconds(-5)));
    }

    [Fact]
    public void EndAction_OnlyLockAndShutdownAreDisruptive()
    {
        Assert.False(SettingsSummary.IsDisruptive(TimeoutAction.DoNothing));
        Assert.False(SettingsSummary.IsDisruptive(TimeoutAction.CloseApp));
        Assert.True(SettingsSummary.IsDisruptive(TimeoutAction.LockAndClose));
        Assert.True(SettingsSummary.IsDisruptive(TimeoutAction.ShutdownWindows));
    }
}
