using Microsoft.Win32;

namespace SnapCqq2.Services;

public static class StartupManager
{
    private const string RunKey  = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SnapCqq2";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(AppName) is not null;
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.SetValue(AppName, $"\"{Environment.ProcessPath}\" --minimized");
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
}
