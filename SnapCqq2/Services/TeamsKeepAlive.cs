using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SnapCqq2.Services;

public static class TeamsKeepAlive
{
    private static readonly string[] ProcessNames = ["ms-teams", "Teams"];
    private const uint WM_MOUSEMOVE = 0x0200;

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>
    /// Finds the main window handle of a running Teams process.
    /// Returns IntPtr.Zero if Teams is not running or has no visible window.
    /// </summary>
    public static IntPtr FindTeamsWindow()
    {
        foreach (var name in ProcessNames)
        {
            foreach (var p in Process.GetProcessesByName(name))
                if (p.MainWindowHandle != IntPtr.Zero)
                    return p.MainWindowHandle;
        }
        return IntPtr.Zero;
    }

    /// <summary>
    /// Posts a WM_MOUSEMOVE to the given window handle, resetting Teams' internal
    /// activity timer without changing focus or producing visible effects.
    /// Returns false if the handle is zero (Teams not found).
    /// </summary>
    public static bool PostKeepAlive(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;
        GetClientRect(hwnd, out var rect);
        int x = (rect.Right - rect.Left) / 2;
        int y = (rect.Bottom - rect.Top) / 2;
        var lParam = (IntPtr)((y << 16) | (x & 0xFFFF));
        PostMessage(hwnd, WM_MOUSEMOVE, IntPtr.Zero, lParam);
        return true;
    }

    /// <summary>
    /// Convenience: find Teams window and post a keepalive in one call.
    /// Returns true if the message was sent, false if Teams was not found.
    /// </summary>
    public static bool TrySendKeepAlive()
        => PostKeepAlive(FindTeamsWindow());
}
