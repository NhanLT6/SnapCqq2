namespace SnapCqq2;

public partial class Main : Form
{
    private readonly Random _rng = new();

    /// one wrapper per form
    private readonly WindowsInput.InputSimulator _simulator = new();

    /// when to stop
    private DateTime _endTime;

    /// façade flag
    private bool _isRunning;

    public Main()
    {
        InitializeComponent();
    }

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_isRunning) StopAutomation();
        else StartAutomation();
    }

    private void actionTimer_Tick(object sender, EventArgs e)
    {
        // 1. Run the action
        PerformRandomAction();

        // 2. Check whether the allotted time is over
        if (DateTime.Now >= _endTime)
            StopAutomation(chkAutoClose.Checked); // may close the form
    }

    private void StartAutomation()
    {
        // compute when to finish
        var minutes = (double)numDuration.Value;
        _endTime = DateTime.Now.AddMinutes(minutes);

        // configure interval
        var seconds = (int)numInterval.Value;
        actionTimer.Interval = seconds * 1_000;

        // (optional) disable config controls while running
        SetConfigEnabled(false);

        // run!
        _isRunning = true;
        btnStartStop.Text = "Stop";
        lblStatus.Text = $"Running… (will stop at {_endTime:T})";
        actionTimer.Start();
    }

    private void StopAutomation(bool closeApp = false)
    {
        actionTimer.Stop();
        _isRunning = false;
        btnStartStop.Text = "Start";
        lblStatus.Text = closeApp ? "Closing…" : "Stopped";
        SetConfigEnabled(true);

        if (closeApp) Close(); // kills the form (and app) gracefully
    }

    private void SetConfigEnabled(bool enabled)
    {
        numDuration.Enabled = enabled;
        numInterval.Enabled = enabled;
        chkAutoClose.Enabled = enabled;
    }

    private void PerformRandomAction()
    {
        // Decide: 0 = mouse click, 1 = key press
        if (_rng.Next(2) == 0)
        {
            // ------------ Mouse left click ------------
            _simulator.Mouse.LeftButtonClick();
            lblStatus.Text = $"[{DateTime.Now:T}] Click";
        }
        else
        {
            // ------------- Key press ------------------
            // Choose one of: Up, Down, Left, Right
            WindowsInput.Native.VirtualKeyCode[] arrows =
            {
                WindowsInput.Native.VirtualKeyCode.UP, WindowsInput.Native.VirtualKeyCode.DOWN,
                WindowsInput.Native.VirtualKeyCode.LEFT, WindowsInput.Native.VirtualKeyCode.RIGHT
            };
            var key = arrows[_rng.Next(arrows.Length)];

            _simulator.Keyboard.KeyPress(key);
            lblStatus.Text = $"[{DateTime.Now:T}] Key: {key}";
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        actionTimer?.Dispose();
        base.OnFormClosed(e);
    }
}