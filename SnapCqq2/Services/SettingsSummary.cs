using SnapCqq2.Models;

namespace SnapCqq2.Services;

/// <summary>
/// One-line, human-readable descriptions of settings for the collapsed rows, status area and tray.
/// </summary>
public static class SettingsSummary
{
    private static readonly string[] DayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    /// <summary>"Mon–Fri", "Mon, Wed, Fri", "Every day"… Runs of 3+ days collapse into a range.</summary>
    public static string Days(bool[] active)
    {
        int count = active.Count(a => a);
        if (count == 7) return "Every day";
        if (count == 0) return "No days";

        var parts = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if (!active[i]) continue;
            int j = i;
            while (j + 1 < 7 && active[j + 1]) j++;
            if (j - i >= 2)      parts.Add($"{DayNames[i]}–{DayNames[j]}");
            else if (j == i + 1) parts.Add($"{DayNames[i]}, {DayNames[j]}");
            else                 parts.Add(DayNames[i]);
            i = j;
        }
        return string.Join(", ", parts);
    }

    public static string Schedule(AppSettings s)
    {
        var text = $"{Days(s.ActiveDays)} {s.WorkStart:HH:mm}–{s.WorkEnd:HH:mm}";
        if (s.BreakEnd > s.BreakStart) text += $", break {s.BreakStart:HH:mm}–{s.BreakEnd:HH:mm}";
        return $"{text} · {s.IdleThresholdMinutes} min idle";
    }

    public static List<string> ActionNames(AppSettings s)
    {
        var names = new List<string>();
        if (s.UseMouseMove)      names.Add("Mouse move");
        if (s.UseTeamsKeepAlive) names.Add("Teams");
        if (s.UseScrollLock)     names.Add("Scroll lock");
        if (s.UseMouseClick)     names.Add("Mouse click");
        if (s.UseArrowKeys)      names.Add("Arrow keys");
        return names;
    }

    public static string Actions(AppSettings s)
    {
        var names = ActionNames(s);
        return names.Count == 0
            ? "Nothing selected"
            : $"{string.Join(", ", names)} · every {s.FrequencyMinutes} min";
    }

    public static bool IsDisruptive(TimeoutAction a) =>
        a is TimeoutAction.LockAndClose or TimeoutAction.ShutdownWindows;

    public static string EndAction(TimeoutAction a) => a switch
    {
        TimeoutAction.LockAndClose    => "Lock Windows",
        TimeoutAction.ShutdownWindows => "Shut down Windows",
        _                             => "Do nothing"
    };

    /// <summary>Short noun for badges: "Shutdown", "Lock".</summary>
    public static string EndActionNoun(TimeoutAction a) =>
        a == TimeoutAction.ShutdownWindows ? "Shutdown" : "Lock";

    /// <summary>"30 min", "1 h", "1 h 30 min", or "until stopped" for 0.</summary>
    public static string Duration(int minutes)
    {
        if (minutes <= 0) return "until stopped";
        if (minutes < 60) return $"{minutes} min";
        return minutes % 60 == 0 ? $"{minutes / 60} h" : $"{minutes / 60} h {minutes % 60} min";
    }

    /// <summary>"4:05" under an hour, "2h 07m" above.</summary>
    public static string Countdown(TimeSpan t)
    {
        if (t < TimeSpan.Zero) t = TimeSpan.Zero;
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}h {t.Minutes:00}m"
            : $"{t.Minutes}:{t.Seconds:00}";
    }
}
