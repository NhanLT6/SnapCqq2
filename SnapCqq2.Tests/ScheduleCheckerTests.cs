using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class ScheduleCheckerTests
{
    private static AppSettings DefaultSettings() => new AppSettings();
    // Defaults: Mon–Fri 09:00–18:00, break 12:00–13:00

    // Returns a DateTime for the next occurrence of a given DayOfWeek at a specific time
    private static DateTime At(DayOfWeek day, int hour, int minute = 0)
    {
        var today = DateTime.Today;
        int diff = ((int)day - (int)today.DayOfWeek + 7) % 7;
        // Ensure we always pick a future or same day (not today-in-the-past)
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
    public void IsInActiveWindow_Wednesday1230_ReturnsFalse_DuringBreak()
    {
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Wednesday, 12, 30));
        Assert.False(result);
    }

    [Fact]
    public void IsInActiveWindow_Wednesday13_ReturnsTrue_AfterBreakEnds()
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
        // 18:00 is >= WorkEnd so not in active window
        var result = ScheduleChecker.IsInActiveWindow(DefaultSettings(), At(DayOfWeek.Monday, 18, 0));
        Assert.False(result);
    }

    [Fact]
    public void NextWindowStart_DuringBreak_ReturnsBreakEnd()
    {
        var now = At(DayOfWeek.Wednesday, 12, 30);
        Assert.Equal(now.Date.AddHours(13), ScheduleChecker.NextWindowStart(DefaultSettings(), now));
    }

    [Fact]
    public void NextWindowStart_FridayEvening_ReturnsMondayMorning()
    {
        var now = At(DayOfWeek.Friday, 19);
        Assert.Equal(now.Date.AddDays(3).AddHours(9), ScheduleChecker.NextWindowStart(DefaultSettings(), now));
    }

    [Fact]
    public void NextWindowStart_NoActiveDays_ReturnsNull()
    {
        var s = new AppSettings { ActiveDays = new bool[7] };
        Assert.Null(ScheduleChecker.NextWindowStart(s, At(DayOfWeek.Monday, 10)));
    }

    [Fact]
    public void IsOnBreak_TrueOnlyDuringBreakOnActiveDay()
    {
        Assert.True(ScheduleChecker.IsOnBreak(DefaultSettings(), At(DayOfWeek.Wednesday, 12, 15)));
        Assert.False(ScheduleChecker.IsOnBreak(DefaultSettings(), At(DayOfWeek.Wednesday, 13)));
        Assert.False(ScheduleChecker.IsOnBreak(DefaultSettings(), At(DayOfWeek.Saturday, 12, 15)));
    }

    [Fact]
    public void WorkEndToday_BeforeEnd_ReturnsTodaysEnd()
    {
        var now = At(DayOfWeek.Tuesday, 17, 59);
        Assert.Equal(now.Date.AddHours(18), ScheduleChecker.WorkEndToday(DefaultSettings(), now));
    }

    [Fact]
    public void WorkEndToday_AfterEndOrInactiveDay_ReturnsNull()
    {
        Assert.Null(ScheduleChecker.WorkEndToday(DefaultSettings(), At(DayOfWeek.Tuesday, 18)));
        Assert.Null(ScheduleChecker.WorkEndToday(DefaultSettings(), At(DayOfWeek.Sunday, 10)));
    }
}
