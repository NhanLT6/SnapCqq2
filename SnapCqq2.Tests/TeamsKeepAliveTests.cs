using SnapCqq2.Services;

namespace SnapCqq2.Tests;

public class TeamsKeepAliveTests
{
    [Fact]
    public void PostKeepAlive_ReturnsFalse_WhenHandleIsZero()
    {
        var result = TeamsKeepAlive.PostKeepAlive(IntPtr.Zero);
        Assert.False(result);
    }

    [Fact]
    public void FindTeamsWindow_DoesNotThrow()
    {
        var ex = Record.Exception(() => TeamsKeepAlive.FindTeamsWindow());
        Assert.Null(ex);
    }

    [Fact]
    public void TrySendKeepAlive_ReturnsFalse_WhenTeamsNotRunning()
    {
        // If Teams happens to be running, this test is skipped rather than failing.
        var hwnd = TeamsKeepAlive.FindTeamsWindow();
        if (hwnd != IntPtr.Zero) return; // Teams is open — can't assert false

        var result = TeamsKeepAlive.TrySendKeepAlive();
        Assert.False(result);
    }
}
