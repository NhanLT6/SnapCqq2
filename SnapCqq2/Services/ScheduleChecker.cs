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
        if (!IsActiveDay(s, now)) return false;

        var t = TimeOnly.FromDateTime(now);
        if (t < s.WorkStart || t >= s.WorkEnd) return false;
        if (t >= s.BreakStart && t < s.BreakEnd) return false;

        return true;
    }

    /// <summary>True during the break on an active day, inside work hours.</summary>
    public static bool IsOnBreak(AppSettings s, DateTime now)
    {
        if (!IsActiveDay(s, now)) return false;
        var t = TimeOnly.FromDateTime(now);
        return t >= s.WorkStart && t < s.WorkEnd && t >= s.BreakStart && t < s.BreakEnd;
    }

    /// <summary>
    /// Next moment the schedule becomes active (work start or break end), or null if no day is active.
    /// </summary>
    public static DateTime? NextWindowStart(AppSettings s, DateTime now)
    {
        for (int d = 0; d < 8; d++)
        {
            var date = now.Date.AddDays(d);
            if (!IsActiveDay(s, date)) continue;

            foreach (var t in new[] { s.WorkStart, s.BreakEnd })
            {
                var candidate = date + t.ToTimeSpan();
                if (candidate > now && IsInActiveWindow(s, candidate)) return candidate;
            }
        }
        return null;
    }

    /// <summary>End of today's work hours if today is active and it hasn't passed yet; otherwise null.</summary>
    public static DateTime? WorkEndToday(AppSettings s, DateTime now)
    {
        if (!IsActiveDay(s, now)) return null;
        var end = now.Date + s.WorkEnd.ToTimeSpan();
        return now < end ? end : null;
    }

    // DayOfWeek: Sun=0 Mon=1 … Sat=6 → remap to Mon=0 … Sun=6
    private static bool IsActiveDay(AppSettings s, DateTime date) =>
        s.ActiveDays[((int)date.DayOfWeek + 6) % 7];
}
