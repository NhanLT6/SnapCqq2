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
}
