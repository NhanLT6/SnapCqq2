using WindowsInput;
using WindowsInput.Native;

namespace SnapCqq2;

public partial class Main : Form
{
    private readonly Random _rng = new();

    private readonly InputSimulator _simulator = new();

    private DateTime _endTime;

    private bool _isRunning;

    #region Constructor

    public Main()
    {
        InitializeComponent();
        ValidateActionSelection();
    }

    #endregion

    #region Event Handlers

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_isRunning) StopAutomation();
        else StartAutomation();
    }

    private void ActionCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        ValidateActionSelection();
    }

    private void actionTimer_Tick(object sender, EventArgs e)
    {
        PerformRandomAction();

        if (DateTime.Now >= _endTime)
            StopAutomation(chkAutoClose.Checked);
    }

    private void updateTimer_Tick(object sender, EventArgs e)
    {
        if (!_isRunning) return;

        var remaining = _endTime - DateTime.Now;
        lblRemainingTime.Text = remaining.TotalSeconds > 0
            ? $"Time remaining: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}"
            : "Time remaining: 00:00:00";
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        actionTimer?.Dispose();
        updateTimer?.Dispose();
        base.OnFormClosed(e);
    }

    #endregion

    #region Automation Control

    private void StartAutomation()
    {
        var runFor = (double)numDuration.Value;
        _endTime = DateTime.Now.AddMinutes(runFor);

        // Convert frequency from minutes to milliseconds
        var frequencyMinutes = (double)numFrequency.Value;
        actionTimer.Interval = (int)(frequencyMinutes * 60 * 1_000);

        SetConfigEnabled(false);

        _isRunning = true;
        btnStartStop.Text = "Stop";
        lblStatus.Text = $"Running… (will stop at {_endTime:T})";
        actionTimer.Start();
        updateTimer.Start();
    }

    private void StopAutomation(bool closeApp = false)
    {
        actionTimer.Stop();
        updateTimer.Stop();
        _isRunning = false;
        btnStartStop.Text = "Start";
        lblStatus.Text = closeApp ? "Closing…" : "Stopped";
        lblRemainingTime.Text = "";
        SetConfigEnabled(true);

        if (closeApp) Close();
    }

    #endregion

    #region Action Execution

    private void PerformRandomAction()
    {
        var actions = new List<Action>();

        if (chkMouseClick.Checked)
            actions.Add(PerformMouseClick);

        if (chkScrollLock.Checked)
            actions.Add(PerformScrollLock);

        if (chkArrowKeys.Checked)
            actions.Add(PerformArrowKey);

        if (actions.Count == 0)
        {
            lblStatus.Text = $"[{DateTime.Now:T}] No actions enabled";
            return;
        }

        var selectedAction = actions[_rng.Next(actions.Count)];
        selectedAction();
    }

    private void PerformMouseClick()
    {
        _simulator.Mouse.LeftButtonClick();
        lblStatus.Text = $"[{DateTime.Now:T}] Mouse click";
    }

    private void PerformScrollLock()
    {
        // Press twice to toggle on then off (returns to original state)
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        lblStatus.Text = $"[{DateTime.Now:T}] Scroll-lock toggle";
    }

    private void PerformArrowKey()
    {
        VirtualKeyCode[] arrows =
        [
            VirtualKeyCode.UP,
            VirtualKeyCode.DOWN,
            VirtualKeyCode.LEFT,
            VirtualKeyCode.RIGHT
        ];

        var key = arrows[_rng.Next(arrows.Length)];
        _simulator.Keyboard.KeyPress(key);
        lblStatus.Text = $"[{DateTime.Now:T}] Arrow key: {key}";
    }

    #endregion

    #region Helper Methods

    private void ValidateActionSelection()
    {
        var anyActionSelected = chkMouseClick.Checked || chkScrollLock.Checked || chkArrowKeys.Checked;
        btnStartStop.Enabled = anyActionSelected;
    }

    private void SetConfigEnabled(bool enabled)
    {
        numDuration.Enabled = enabled;
        numFrequency.Enabled = enabled;
        chkAutoClose.Enabled = enabled;
        chkMouseClick.Enabled = enabled;
        chkScrollLock.Enabled = enabled;
        chkArrowKeys.Enabled = enabled;
    }

    #endregion
}