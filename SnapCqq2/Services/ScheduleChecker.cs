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
    /// Returns a human-readable "Next: day HH:mm" string for the status bar
    /// when the scheduler is armed but currently outside the active window.
    /// </summary>
    public static string NextWindowDescription(AppSettings s, DateTime now)
    {
        for (int d = 0; d < 8; d++)
        {
            var candidate = now.AddDays(d);
            int idx = ((int)candidate.DayOfWeek + 6) % 7;
            if (!s.ActiveDays[idx]) continue;

            var start = candidate.Date + s.WorkStart.ToTimeSpan();
            if (d == 0 && now >= start) continue; // today's window already started (or is in break)

            return $"Next: {start:ddd HH:mm}";
        }
        return "No schedule configured";
    }
}
