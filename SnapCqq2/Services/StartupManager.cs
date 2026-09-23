using Microsoft.Win32;

namespace SnapCqq2.Services;

public static class StartupManager
{
    private const string RunKey      = @"Software\Microsoft\Windows\CurrentVersion\Run";
    // Windows keeps a separate on/off flag per Run entry here (Task Manager / Settings > Startup apps).
    // A disabled flag silently overrides the Run entry, so both must agree for the app to launch at logon.
    private const string ApprovedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string AppName     = "SnapCqq2";

    private static string Command => $"\"{Environment.ProcessPath}\" --minimized";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(AppName) is not null && IsApproved();
    }

    public static void Enable()
    {
        using (var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true))
            key.SetValue(AppName, Command);

        // Clear any "disabled" flag left by Task Manager. First byte even = enabled, odd = disabled.
        using var approved = Registry.CurrentUser.CreateSubKey(ApprovedKey, writable: true);
        approved.SetValue(AppName, new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key?.GetValue(AppName) is not null)
            key.DeleteValue(AppName);
    }

    public static void Sync(bool enabled)
    {
        if (enabled) Enable(); else Disable();
    }

    /// <summary>
    /// Re-points an existing Run entry at the current exe, so moving the app or switching
    /// between Debug and published builds doesn't leave startup launching a stale path.
    /// Does not touch the approval flag — a user's Task Manager choice is respected.
    /// </summary>
    public static void RefreshPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key?.GetValue(AppName) is string current && current != Command)
            key.SetValue(AppName, Command);
    }

    private static bool IsApproved()
    {
        using var key = Registry.CurrentUser.OpenSubKey(ApprovedKey, writable: false);
        return key?.GetValue(AppName) is not byte[] { Length: > 0 } flag || (flag[0] & 1) == 0;
    }
}
