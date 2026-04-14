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

    /// <summary>
    /// Returns how long the system has been idle (no keyboard or mouse input).
    /// Uses unchecked subtraction to handle the ~25-day TickCount wrap correctly.
    /// </summary>
    public static TimeSpan GetIdleTime()
    {
        var info = new LastInputInfo { cbSize = (uint)Marshal.SizeOf<LastInputInfo>() };
        GetLastInputInfo(ref info);
        int elapsed = unchecked(Environment.TickCount - (int)info.dwTime);
        return TimeSpan.FromMilliseconds(Math.Max(0, elapsed));
    }

    /// <summary>
    /// Returns the idle-check timer interval in milliseconds.
    /// Formula: max(30 s, threshold_minutes × 60 s / 4)
    /// Guarantees worst-case activation latency ≤ 125 % of the configured threshold.
    /// </summary>
    public static int ComputePollIntervalMs(int thresholdMinutes)
        => (int)Math.Max(30_000, thresholdMinutes * 60_000.0 / 4);
}
