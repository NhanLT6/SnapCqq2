namespace SnapCqq2.Models;

public enum TimeoutAction
{
    DoNothing,
    CloseApp,
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

    // Shared: actions + frequency (apply to both Auto and Manual modes)
    public bool UseScrollLock    { get; set; } = true;
    public bool UseMouseClick    { get; set; } = false;
    public bool UseArrowKeys     { get; set; } = false;
    public int  FrequencyMinutes { get; set; } = 1;

    // Auto: what happens when the schedule window ends
    public TimeoutAction AutoTimeoutAction { get; set; } = TimeoutAction.DoNothing;

    // Manual
    public int           ManualDurationMinutes { get; set; } = 30;
    public int           ManualDelayMinutes    { get; set; } = 0;
    public TimeoutAction ManualTimeoutAction   { get; set; } = TimeoutAction.CloseApp;

    // General
    public bool   LaunchOnStartup { get; set; } = false;
    public bool   StartMinimized  { get; set; } = false;
    public string DefaultMode     { get; set; } = "Auto"; // "Auto" or "Manual"
}
