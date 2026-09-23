namespace SnapCqq2.Models;

public enum TimeoutAction
{
    DoNothing,
    CloseApp,        // legacy (old Manual mode) — treated as DoNothing
    LockAndClose,
    ShutdownWindows
}

public class AppSettings
{
    // Schedule — index 0=Mon … 6=Sun
    public bool[] ActiveDays { get; set; } = [true, true, true, true, true, false, false];
    public TimeOnly WorkStart  { get; set; } = new(9,  0);
    public TimeOnly WorkEnd    { get; set; } = new(18, 0);
    public TimeOnly BreakStart { get; set; } = new(12, 0);
    public TimeOnly BreakEnd   { get; set; } = new(13, 0);

    // Idle trigger
    public int  IdleThresholdMinutes { get; set; } = 4;
    public bool StopWhenUserReturns  { get; set; } = true;

    // Actions + frequency (shared by schedule and quick runs)
    public bool UseScrollLock      { get; set; } = true;
    public bool UseMouseClick      { get; set; } = false;
    public bool UseArrowKeys       { get; set; } = false;
    public bool UseTeamsKeepAlive  { get; set; } = false;
    public bool UseMouseMove       { get; set; } = false;
    public int  FrequencyMinutes   { get; set; } = 1;

    // Schedule on/off — survives restarts
    public bool AutoEnabled { get; set; } = true;

    // What happens at the end of work hours if the user is away
    public TimeoutAction AutoTimeoutAction { get; set; } = TimeoutAction.DoNothing;
    public DateOnly?     EndActionSkippedOn { get; set; }

    // Quick run ("Run now") — last used duration (0 = until stopped) and what happens after
    public int           QuickRunMinutes   { get; set; } = 60;
    public TimeoutAction QuickRunEndAction { get; set; } = TimeoutAction.DoNothing;

    // General
    public bool LaunchOnStartup { get; set; } = true;
}
