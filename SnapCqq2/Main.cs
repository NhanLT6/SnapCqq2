using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;

namespace SnapCqq2;

public partial class Main : Form
{
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    private enum TimeoutAction
    {
        CloseApp,
        LockAndClose,
        ShutdownWindows,
        DoNothing
    }

    private enum AppState { Idle, WaitingToStart, Running }

    private readonly Random _rng = new();
    private readonly InputSimulator _simulator = new();

    private AppState _state = AppState.Idle;
    private DateTime _endTime;
    private DateTime _delayEndTime;

    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;

    private bool _optionsExpanded = false;
    private const int CollapsedHeight = 125;
    private const int ExpandedHeight = 235;

    #region Constructor

    public Main()
    {
        InitializeComponent();
        SetupTrayIcon();
        ValidateActionSelection();
    }

    #endregion

    #region Tray Icon Setup

    private void SetupTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Show", null, TrayMenuShow_Click);
        _trayMenu.Items.Add("Exit", null, TrayMenuExit_Click);

        _trayIcon = new NotifyIcon
        {
            Icon = this.Icon,
            Text = "SnapCqq2",
            ContextMenuStrip = _trayMenu,
            Visible = false
        };
        _trayIcon.MouseClick += TrayIcon_MouseClick;
    }

    #endregion

    #region Event Handlers

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_state == AppState.Running)
        {
            StopAutomation();
            TransitionTo(AppState.Idle);
        }
        else if (_state == AppState.Idle)
        {
            TransitionTo(numDelay.Value > 0 ? AppState.WaitingToStart : AppState.Running);
        }
    }

    private void btnStartNow_Click(object sender, EventArgs e)
    {
        TransitionTo(AppState.Running);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        TransitionTo(AppState.Idle);
    }

    private void ActionCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        ValidateActionSelection();
    }

    private void actionTimer_Tick(object sender, EventArgs e)
    {
        if (DateTime.Now >= _endTime)
        {
            var action = (TimeoutAction)cmbTimeoutAction.SelectedIndex;
            StopAutomation(action);
            if (action != TimeoutAction.CloseApp && action != TimeoutAction.LockAndClose)
                TransitionTo(AppState.Idle);
            return;
        }
        PerformRandomAction();
    }

    private void updateTimer_Tick(object sender, EventArgs e)
    {
        if (_state != AppState.Running) return;

        var remaining = _endTime - DateTime.Now;
        var ts = remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        var display = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        lblRemainingTime.Text = $"Time remaining: {display}";

        var trayText = $"Running - {display} remaining";
        _trayIcon.Text = trayText.Length > 63 ? trayText[..63] : trayText;
    }

    private void delayTimer_Tick(object sender, EventArgs e)
    {
        var remaining = _delayEndTime - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            TransitionTo(AppState.Running);
            return;
        }
        var display = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        var trayText = $"Starting in {display}";
        _trayIcon.Text = trayText.Length > 63 ? trayText[..63] : trayText;
    }

    private void lblToggleOptions_Click(object sender, EventArgs e)
    {
        SetOptionsExpanded(!_optionsExpanded);
    }

    private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ShowForm();
    }

    private void TrayMenuShow_Click(object? sender, EventArgs e) => ShowForm();

    private void TrayMenuExit_Click(object? sender, EventArgs e) => Application.Exit();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized && _state != AppState.Idle)
            HideToTray();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        actionTimer?.Dispose();
        updateTimer?.Dispose();
        delayTimer?.Dispose();
        _trayIcon?.Dispose();
        base.OnFormClosed(e);
    }

    #endregion

    #region State Machine

    private void TransitionTo(AppState newState)
    {
        _state = newState;

        switch (newState)
        {
            case AppState.Idle:
                actionTimer.Stop();
                updateTimer.Stop();
                delayTimer.Stop();
                SetConfigEnabled(true);
                ShowForm();
                break;

            case AppState.WaitingToStart:
                _delayEndTime = DateTime.Now.AddMinutes((double)numDelay.Value);
                SetConfigEnabled(false);
                delayTimer.Start();
                HideToTray();
                break;

            case AppState.Running:
                delayTimer.Stop();
                _endTime = DateTime.Now.AddMinutes((double)numDuration.Value);
                actionTimer.Interval = (int)((double)numFrequency.Value * 60 * 1_000);
                actionTimer.Start();
                updateTimer.Start();
                HideToTray();
                break;
        }

        UpdateUIForState();
        ValidateActionSelection();
    }

    private void UpdateUIForState()
    {
        bool isWaiting = _state == AppState.WaitingToStart;
        bool isRunning = _state == AppState.Running;

        btnStartStop.Visible = !isWaiting;
        btnStartStop.Text = isRunning ? "Stop" : "Start";
        btnStartStop.BackColor = isRunning
            ? System.Drawing.Color.FromArgb(183, 28, 28)
            : System.Drawing.Color.FromArgb(46, 125, 50);

        btnStartNow.Visible = isWaiting;
        btnCancel.Visible = isWaiting;

        switch (_state)
        {
            case AppState.Idle:
                lblRemainingTime.Text = "Start app to see remaining time";
                lblStatus.Text = "Idle";
                break;
            case AppState.WaitingToStart:
                lblRemainingTime.Text = "Waiting to start…";
                lblStatus.Text = $"Will start at {_delayEndTime:T}";
                break;
            case AppState.Running:
                lblStatus.Text = $"Running… (stops at {_endTime:T})";
                break;
        }
    }

    #endregion

    #region Automation Control

    private void StopAutomation(TimeoutAction action = TimeoutAction.DoNothing)
    {
        actionTimer.Stop();
        updateTimer.Stop();

        switch (action)
        {
            case TimeoutAction.CloseApp:
                Close();
                break;
            case TimeoutAction.LockAndClose:
                LockWorkStation();
                Close();
                break;
            case TimeoutAction.ShutdownWindows:
                System.Diagnostics.Process.Start("shutdown", "/s /t 0");
                break;
        }
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

        actions[_rng.Next(actions.Count)]();
    }

    private void PerformMouseClick()
    {
        _simulator.Mouse.LeftButtonClick();
        lblStatus.Text = $"[{DateTime.Now:T}] Mouse click";
    }

    private void PerformScrollLock()
    {
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

    #region Tray Helpers

    private void HideToTray()
    {
        _trayIcon.Visible = true;
        Hide();
    }

    private void ShowForm()
    {
        Show();
        WindowState = FormWindowState.Normal;
        BringToFront();
        _trayIcon.Visible = false;
    }

    #endregion

    #region UI Helpers

    private void SetOptionsExpanded(bool expanded)
    {
        _optionsExpanded = expanded;
        lblToggleOptions.Text = expanded ? "▼ Options" : "▶ Options";
        pnlOptions.Visible = expanded;
        int newHeight = expanded ? ExpandedHeight : CollapsedHeight;
        pnlStatus.Top = newHeight - 48;
        ClientSize = new System.Drawing.Size(ClientSize.Width, newHeight);
    }

    private void ValidateActionSelection()
    {
        var anyActionSelected = chkMouseClick.Checked || chkScrollLock.Checked || chkArrowKeys.Checked;
        btnStartStop.Enabled = _state == AppState.Running || (anyActionSelected && _state == AppState.Idle);
    }

    private void SetConfigEnabled(bool enabled)
    {
        numDuration.Enabled = enabled;
        numFrequency.Enabled = enabled;
        numDelay.Enabled = enabled;
        cmbTimeoutAction.Enabled = enabled;
        chkMouseClick.Enabled = enabled;
        chkScrollLock.Enabled = enabled;
        chkArrowKeys.Enabled = enabled;
    }

    #endregion
}
