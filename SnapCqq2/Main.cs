using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;
using SnapCqq2.Models;
using SnapCqq2.Services;

namespace SnapCqq2;

public partial class Main : Form
{
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    // ── Card height constants ──────────────────────────────────────
    private const int CardHeaderH    = 28;
    private const int WhenBodyH      = 104;
    private const int HowBodyH       = 83;
    private const int AfterBodyH     = 39;
    private const int RunBodyH       = 70;
    private const int ManualHowBodyH = 83;
    private const int CardGap        = 6;
    private const int TabPadding     = 8;

    // ── State ──────────────────────────────────────────────────────
    private enum AutoState   { Disabled, Armed, Running }
    private enum ManualState { Idle, WaitingToStart, Running }

    private AutoState   _autoState   = AutoState.Disabled;
    private ManualState _manualState = ManualState.Idle;

    private bool _howExpanded       = false;
    private bool _afterExpanded     = false;
    private bool _manualHowExpanded = false;

    private DateTime _manualEndTime;
    private DateTime _manualDelayEnd;

    private bool _loading = false; // suppresses save-on-change handlers during UI population
    private readonly bool _startMinimized;

    // ── Services ───────────────────────────────────────────────────
    private AppSettings _settings = new();
    private readonly InputSimulator _simulator = new();
    private readonly Random _rng = new();

    // ── Tray ───────────────────────────────────────────────────────
    private NotifyIcon      _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;

    // ────────────────────────────────────────────────────────────────
    #region Init

    public Main(bool startMinimized = false)
    {
        _startMinimized = startMinimized;
        InitializeComponent();
        BuildUI();
        SetupTrayIcon();
        _settings = LoadSettings();
        LoadSettingsToUI();
        RelayoutAutoCards();
        WireEvents();
        UpdateStatusBar();
    }

    private void WireEvents()
    {
        // Card toggles — wire both the header panel AND the label inside it,
        // because WinForms labels consume click events and don't propagate to parent.
        pnlHowHeader.Click       += (_, _) => ToggleCard(ref _howExpanded,       pnlHowBody,       lblHowTitle,       "⚡ How",                RelayoutAutoCards);
        lblHowTitle.Click        += (_, _) => ToggleCard(ref _howExpanded,       pnlHowBody,       lblHowTitle,       "⚡ How",                RelayoutAutoCards);
        pnlAfterHeader.Click     += (_, _) => ToggleCard(ref _afterExpanded,     pnlAfterBody,     lblAfterTitle,     "🔚 After schedule ends", RelayoutAutoCards);
        lblAfterTitle.Click      += (_, _) => ToggleCard(ref _afterExpanded,     pnlAfterBody,     lblAfterTitle,     "🔚 After schedule ends", RelayoutAutoCards);
        pnlManualHowHeader.Click += (_, _) => ToggleCard(ref _manualHowExpanded, pnlManualHowBody, lblManualHowTitle, "⚡ How",                RelayoutManualCards);
        lblManualHowTitle.Click  += (_, _) => ToggleCard(ref _manualHowExpanded, pnlManualHowBody, lblManualHowTitle, "⚡ How",                RelayoutManualCards);

        // Tab switch
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

        // Action button (Enable/Disable or Start/Stop)
        btnAction.Click += BtnAction_Click;

        // Auto settings
        for (int i = 0; i < 7; i++) btnDay[i].Click += DayBtn_Click;
        dtpWorkStart.ValueChanged          += AutoSetting_Changed;
        dtpWorkEnd.ValueChanged            += AutoSetting_Changed;
        dtpBreakStart.ValueChanged         += AutoSetting_Changed;
        dtpBreakEnd.ValueChanged           += AutoSetting_Changed;
        numIdleThreshold.ValueChanged      += AutoSetting_Changed;
        chkStopOnReturn.CheckedChanged     += AutoSetting_Changed;
        chkScrollLock.CheckedChanged       += AutoSetting_Changed;
        chkMouseClick.CheckedChanged       += AutoSetting_Changed;
        chkArrowKeys.CheckedChanged        += AutoSetting_Changed;
        chkTeamsKeepAlive.CheckedChanged   += AutoSetting_Changed;
        chkMouseMove.CheckedChanged        += AutoSetting_Changed;
        numFrequency.ValueChanged          += AutoSetting_Changed;
        cmbAutoTimeout.SelectedIndexChanged += AutoSetting_Changed;

        // Manual settings
        numDuration.ValueChanged              += ManualSetting_Changed;
        numDelay.ValueChanged                 += ManualSetting_Changed;
        cmbManualTimeout.SelectedIndexChanged += ManualSetting_Changed;
        chkManualScrollLock.CheckedChanged         += ManualSetting_Changed;
        chkManualMouseClick.CheckedChanged         += ManualSetting_Changed;
        chkManualArrowKeys.CheckedChanged          += ManualSetting_Changed;
        chkManualTeamsKeepAlive.CheckedChanged     += ManualSetting_Changed;
        chkManualMouseMove.CheckedChanged          += ManualSetting_Changed;
        numManualFrequency.ValueChanged       += ManualSetting_Changed;

        // General settings
        chkLaunchOnStartup.CheckedChanged   += GeneralSetting_Changed;
        cmbDefaultMode.SelectedIndexChanged += GeneralSetting_Changed;

        // Tray
        _trayIcon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowForm(); };

        // Timers
        idleCheckTimer.Tick += IdleCheckTimer_Tick;
        actionTimer.Tick    += ActionTimer_Tick;
        updateTimer.Tick    += UpdateTimer_Tick;
        delayTimer.Tick     += DelayTimer_Tick;
    }

    private static AppSettings LoadSettings()
    {
        bool isFirstRun = !File.Exists(SettingsManager.FilePath);
        var settings = SettingsManager.Load();

        if (isFirstRun)
        {
            StartupManager.Enable();
            SettingsManager.Save(settings); // persist defaults so next launch isn't treated as first run
        }

        return settings;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Settings load/save

    private void LoadSettingsToUI()
    {
        _loading = true;
        try
        {
            for (int i = 0; i < 7; i++)
                SetDayButtonState(btnDay[i], _settings.ActiveDays[i]);

            dtpWorkStart.Value  = DateTime.Today + _settings.WorkStart.ToTimeSpan();
            dtpWorkEnd.Value    = DateTime.Today + _settings.WorkEnd.ToTimeSpan();
            dtpBreakStart.Value = DateTime.Today + _settings.BreakStart.ToTimeSpan();
            dtpBreakEnd.Value   = DateTime.Today + _settings.BreakEnd.ToTimeSpan();

            numIdleThreshold.Value  = _settings.IdleThresholdMinutes;
            chkStopOnReturn.Checked = _settings.StopWhenUserReturns;

            // Actions — shared; populate both Auto and Manual HOW cards
            chkScrollLock.Checked         = chkManualScrollLock.Checked         = _settings.UseScrollLock;
            chkMouseClick.Checked         = chkManualMouseClick.Checked         = _settings.UseMouseClick;
            chkArrowKeys.Checked          = chkManualArrowKeys.Checked          = _settings.UseArrowKeys;
            chkTeamsKeepAlive.Checked     = chkManualTeamsKeepAlive.Checked     = _settings.UseTeamsKeepAlive;
            chkMouseMove.Checked          = chkManualMouseMove.Checked          = _settings.UseMouseMove;
            numFrequency.Value = numManualFrequency.Value = _settings.FrequencyMinutes;

            cmbAutoTimeout.SelectedIndex = _settings.AutoTimeoutAction switch
            {
                TimeoutAction.LockAndClose    => 1,
                TimeoutAction.ShutdownWindows => 2,
                _                             => 0  // DoNothing (CloseApp also falls back to DoNothing)
            };
            numDuration.Value               = _settings.ManualDurationMinutes;
            numDelay.Value                  = _settings.ManualDelayMinutes;
            cmbManualTimeout.SelectedIndex = _settings.ManualTimeoutAction switch
            {
                TimeoutAction.LockAndClose    => 1,
                TimeoutAction.ShutdownWindows => 2,
                TimeoutAction.DoNothing       => 3,
                _                             => 0  // CloseApp = "Close when finished"
            };

            chkLaunchOnStartup.Checked   = StartupManager.IsEnabled();
            cmbDefaultMode.SelectedIndex = _settings.DefaultMode == "Manual" ? 1 : 0;

            tabControl.SelectedTab = _settings.DefaultMode == "Manual" ? tabManual : tabAuto;
        }
        finally
        {
            _loading = false;
        }
    }

    private void SaveFromAutoUI()
    {
        for (int i = 0; i < 7; i++)
            _settings.ActiveDays[i] = btnDay[i].Tag is true;

        _settings.WorkStart  = TimeOnly.FromDateTime(dtpWorkStart.Value);
        _settings.WorkEnd    = TimeOnly.FromDateTime(dtpWorkEnd.Value);
        _settings.BreakStart = TimeOnly.FromDateTime(dtpBreakStart.Value);
        _settings.BreakEnd   = TimeOnly.FromDateTime(dtpBreakEnd.Value);

        _settings.IdleThresholdMinutes = (int)numIdleThreshold.Value;
        _settings.StopWhenUserReturns  = chkStopOnReturn.Checked;
        _settings.UseScrollLock        = chkScrollLock.Checked;
        _settings.UseMouseClick        = chkMouseClick.Checked;
        _settings.UseArrowKeys         = chkArrowKeys.Checked;
        _settings.UseTeamsKeepAlive    = chkTeamsKeepAlive.Checked;
        _settings.UseMouseMove         = chkMouseMove.Checked;
        _settings.FrequencyMinutes     = (int)numFrequency.Value;
        _settings.AutoTimeoutAction = cmbAutoTimeout.SelectedIndex switch
        {
            1 => TimeoutAction.LockAndClose,
            2 => TimeoutAction.ShutdownWindows,
            _ => TimeoutAction.DoNothing
        };

        SettingsManager.Save(_settings);
    }

    private void SaveFromManualUI()
    {
        _settings.ManualDurationMinutes = (int)numDuration.Value;
        _settings.ManualDelayMinutes    = (int)numDelay.Value;
        _settings.ManualTimeoutAction = cmbManualTimeout.SelectedIndex switch
        {
            1 => TimeoutAction.LockAndClose,
            2 => TimeoutAction.ShutdownWindows,
            3 => TimeoutAction.DoNothing,
            _ => TimeoutAction.CloseApp  // 0 = "Close when finished"
        };
        _settings.UseScrollLock         = chkManualScrollLock.Checked;
        _settings.UseMouseClick         = chkManualMouseClick.Checked;
        _settings.UseArrowKeys          = chkManualArrowKeys.Checked;
        _settings.UseTeamsKeepAlive     = chkManualTeamsKeepAlive.Checked;
        _settings.UseMouseMove          = chkManualMouseMove.Checked;
        _settings.FrequencyMinutes      = (int)numManualFrequency.Value;

        // Sync shared action settings back to Auto HOW controls
        _loading = true;
        chkScrollLock.Checked         = _settings.UseScrollLock;
        chkMouseClick.Checked         = _settings.UseMouseClick;
        chkArrowKeys.Checked          = _settings.UseArrowKeys;
        chkTeamsKeepAlive.Checked     = _settings.UseTeamsKeepAlive;
        chkMouseMove.Checked          = _settings.UseMouseMove;
        numFrequency.Value            = _settings.FrequencyMinutes;
        _loading = false;

        SettingsManager.Save(_settings);
    }

    private void AutoSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        SaveFromAutoUI();
        if (_autoState == AutoState.Armed)
            idleCheckTimer.Interval = IdleDetector.ComputePollIntervalMs(_settings.IdleThresholdMinutes);
    }

    private void ManualSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        SaveFromManualUI();
    }

    private void GeneralSetting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        StartupManager.Sync(chkLaunchOnStartup.Checked);
        _settings.DefaultMode = cmbDefaultMode.SelectedIndex == 1 ? "Manual" : "Auto";
        SettingsManager.Save(_settings);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Day buttons

    private void DayBtn_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        bool current = btn.Tag is true;
        SetDayButtonState(btn, !current);
        if (!_loading) SaveFromAutoUI();
    }

    private static void SetDayButtonState(Button btn, bool active)
    {
        btn.Tag       = active;
        btn.BackColor = active ? Color.FromArgb(46, 125, 50) : Color.FromArgb(228, 228, 228);
        btn.ForeColor = active ? Color.White : Color.FromArgb(85, 85, 85);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Card layout

    private void ToggleCard(ref bool state, Panel body, Label title, string baseText, Action relayout)
    {
        state      = !state;
        body.Visible = state;
        title.Text = (state ? "▼ " : "▶ ") + baseText;
        relayout();
    }

    private void RelayoutAutoCards()
    {
        int y = TabPadding;
        int w = tabAuto.ClientSize.Width - TabPadding * 2;
        if (w <= 0) w = 548; // fallback before form is shown

        pnlWhen.SetBounds(TabPadding, y, w, CardHeaderH + WhenBodyH);
        y += pnlWhen.Height + CardGap;

        pnlHow.SetBounds(TabPadding, y, w, CardHeaderH + (_howExpanded ? HowBodyH : 0));
        y += pnlHow.Height + CardGap;

        pnlAfter.SetBounds(TabPadding, y, w, CardHeaderH + (_afterExpanded ? AfterBodyH : 0));
        y += pnlAfter.Height + TabPadding;

        ResizeFormToContent(y);
    }

    private void RelayoutManualCards()
    {
        int y = TabPadding;
        int w = tabManual.ClientSize.Width - TabPadding * 2;
        if (w <= 0) w = 548;

        pnlRun.SetBounds(TabPadding, y, w, CardHeaderH + RunBodyH);
        y += pnlRun.Height + CardGap;

        pnlManualHow.SetBounds(TabPadding, y, w, CardHeaderH + (_manualHowExpanded ? ManualHowBodyH : 0));
        y += pnlManualHow.Height + TabPadding;

        ResizeFormToContent(y);
    }

    private void ResizeFormToContent(int tabContentHeight)
    {
        // tabControl.Top is 0 (Dock=Fill minus Dock=Bottom status bar)
        // The tab strip occupies some pixels above the tab page client area.
        // We measure it accurately once the form is visible; use a constant until then.
        int tabStripH = _tabStripH > 0 ? _tabStripH : 28;
        ClientSize = new Size(ClientSize.Width, tabStripH + tabContentHeight + pnlStatusBar.Height);
    }

    private int _tabStripH = 0;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Measure actual tab strip height now that the form is rendered
        _tabStripH = tabControl.Height - tabAuto.Height;
        // Fix action button position: Anchor=Right is computed from the panel's size
        // at InitializeComponent time (before layout), which may be wrong.
        // Reposition explicitly once the panel has its real width.
        btnAction.Left = pnlStatusMain.ClientSize.Width - btnAction.Width - 8;
        // Re-layout with accurate measurement
        if (tabControl.SelectedTab == tabAuto)   RelayoutAutoCards();
        if (tabControl.SelectedTab == tabManual) RelayoutManualCards();
    }

    private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabAuto)   RelayoutAutoCards();
        if (tabControl.SelectedTab == tabManual) RelayoutManualCards();
        UpdateStatusBar();
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Auto mode state machine

    private void BtnAction_Click(object? sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabAuto)
        {
            if (_autoState == AutoState.Disabled) ArmAuto();
            else DisarmAuto();
        }
        else if (tabControl.SelectedTab == tabManual)
        {
            if (_manualState == ManualState.Idle) StartManual();
            else StopManual();
        }
    }

    private void ArmAuto()
    {
        _autoState = AutoState.Armed;
        idleCheckTimer.Interval = IdleDetector.ComputePollIntervalMs(_settings.IdleThresholdMinutes);
        idleCheckTimer.Start();
        UpdateStatusBar();
        var trayStatus = ScheduleChecker.IsInActiveWindow(_settings, DateTime.Now)
            ? "Armed · Watching for idle"
            : ScheduleChecker.NextWindowDescription(_settings, DateTime.Now);
        _trayIcon.Text = Truncate(trayStatus, 63);
        HideToTray();
    }

    private void DisarmAuto()
    {
        if (_autoState == AutoState.Running) StopAutomation();
        _autoState = AutoState.Disabled;
        idleCheckTimer.Stop();
        UpdateStatusBar();
        ShowForm();
    }

    private void IdleCheckTimer_Tick(object? sender, EventArgs e)
    {
        if (!ScheduleChecker.IsInActiveWindow(_settings, DateTime.Now))
        {
            if (_autoState == AutoState.Running)
            {
                StopAutomation();
                ApplyAutoTimeout();
                _autoState = AutoState.Armed;
                UpdateStatusBar();
            }
            var desc = ScheduleChecker.NextWindowDescription(_settings, DateTime.Now);
            _trayIcon.Text = Truncate($"Armed · {desc}", 63);
            return;
        }

        var idle      = IdleDetector.GetIdleTime();
        var threshold = TimeSpan.FromMinutes(_settings.IdleThresholdMinutes);

        if (_autoState == AutoState.Armed && idle >= threshold)
        {
            _autoState = AutoState.Running;
            StartAutomation();
            UpdateStatusBar();
        }
        else if (_autoState == AutoState.Running && _settings.StopWhenUserReturns && idle < threshold)
        {
            StopAutomation();
            _autoState = AutoState.Armed;
            _trayIcon.Text = Truncate("Armed · Watching for idle", 63);
            UpdateStatusBar();
        }
        else if (_autoState == AutoState.Armed)
        {
            _trayIcon.Text = Truncate("Armed · Watching for idle", 63);
        }
    }

    private void ApplyAutoTimeout()
    {
        switch (_settings.AutoTimeoutAction)
        {
            case TimeoutAction.LockAndClose:    LockWorkStation(); break;
            case TimeoutAction.ShutdownWindows: System.Diagnostics.Process.Start("shutdown", "/s /t 0"); break;
            // DoNothing and CloseApp (legacy): stay armed, do nothing
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Manual mode state machine

    private void StartManual()
    {
        if (_settings.ManualDelayMinutes > 0)
        {
            _manualState    = ManualState.WaitingToStart;
            _manualDelayEnd = DateTime.Now.AddMinutes(_settings.ManualDelayMinutes);
            delayTimer.Start();
        }
        else
        {
            _manualState = ManualState.Running;
            BeginManualRun();
        }
        UpdateStatusBar();
        HideToTray();
    }

    private void StopManual(TimeoutAction action = TimeoutAction.DoNothing)
    {
        actionTimer.Stop();
        updateTimer.Stop();
        delayTimer.Stop();
        _manualState = ManualState.Idle;
        UpdateStatusBar();

        switch (action)
        {
            case TimeoutAction.CloseApp:        Close(); return;
            case TimeoutAction.LockAndClose:    LockWorkStation(); Close(); return;
            case TimeoutAction.ShutdownWindows: System.Diagnostics.Process.Start("shutdown", "/s /t 0"); return;
        }
        ShowForm();
    }

    private void BeginManualRun()
    {
        _manualEndTime         = DateTime.Now.AddMinutes(_settings.ManualDurationMinutes);
        actionTimer.Interval   = _settings.FrequencyMinutes * 60_000;
        actionTimer.Start();
        updateTimer.Start();
    }

    private void DelayTimer_Tick(object? sender, EventArgs e)
    {
        if (DateTime.Now >= _manualDelayEnd)
        {
            delayTimer.Stop();
            _manualState = ManualState.Running;
            BeginManualRun();
            UpdateStatusBar();
        }
        else
        {
            var rem = _manualDelayEnd - DateTime.Now;
            _trayIcon.Text = Truncate($"Starting in {rem:hh\\:mm\\:ss}", 63);
        }
    }

    private void ActionTimer_Tick(object? sender, EventArgs e)
    {
        if (_manualState == ManualState.Running && DateTime.Now >= _manualEndTime)
        {
            var action = _settings.ManualTimeoutAction;
            StopManual(action);
            return;
        }
        PerformRandomAction();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (_manualState != ManualState.Running) return;
        var rem = _manualEndTime - DateTime.Now;
        var ts  = rem.TotalSeconds > 0 ? rem : TimeSpan.Zero;
        lblStatusText.Text = $"Running — {ts:hh\\:mm\\:ss} remaining";
        _trayIcon.Text     = Truncate(lblStatusText.Text, 63);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Automation actions

    private void StartAutomation()
    {
        PerformRandomAction(); // act immediately — don't make the user wait a full frequency cycle
        actionTimer.Interval = _settings.FrequencyMinutes * 60_000;
        actionTimer.Start();
        _trayIcon.Text = Truncate("Active — automating", 63);
        // updateTimer intentionally NOT started — no countdown in auto mode
    }

    private void StopAutomation()
    {
        actionTimer.Stop();
        updateTimer.Stop(); // defensive
    }

    private void PerformRandomAction()
    {
        // Teams keepalive always fires every tick when enabled — it must beat Teams'
        // own idle timer reliably, so it cannot be left to random selection.
        if (_settings.UseTeamsKeepAlive) PerformTeamsKeepAlive();

        var actions = new List<Action>();
        if (_settings.UseScrollLock) actions.Add(PerformScrollLock);
        if (_settings.UseMouseClick) actions.Add(PerformMouseClick);
        if (_settings.UseArrowKeys)  actions.Add(PerformArrowKey);
        if (_settings.UseMouseMove)  actions.Add(PerformMouseMove);
        if (actions.Count > 0) actions[_rng.Next(actions.Count)]();
    }

    private void PerformScrollLock()
    {
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        LogAction("Scroll-lock toggle");
    }

    private void PerformMouseClick()
    {
        _simulator.Mouse.LeftButtonClick();
        LogAction("Mouse click");
    }

    private void PerformArrowKey()
    {
        VirtualKeyCode[] arrows = [VirtualKeyCode.UP, VirtualKeyCode.DOWN, VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT];
        var key = arrows[_rng.Next(arrows.Length)];
        _simulator.Keyboard.KeyPress(key);
        LogAction($"Arrow key: {key}");
    }

    private void PerformMouseMove()
    {
        // 2px jitter and return — net zero displacement, no on-screen indicator
        _simulator.Mouse.MoveMouseBy(2, 0);
        _simulator.Mouse.MoveMouseBy(-2, 0);
        LogAction("Mouse move");
    }

    private void PerformTeamsKeepAlive()
    {
        bool sent = TeamsKeepAlive.TrySendKeepAlive();
        LogAction(sent ? "Teams keepalive sent" : "Teams keepalive — Teams not found");
    }

    private void LogAction(string msg) =>
        lblLastAction.Text = $"[{DateTime.Now:T}] {msg}";

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Status bar

    private void UpdateStatusBar()
    {
        if (tabControl.SelectedTab == tabAuto)
        {
            switch (_autoState)
            {
                case AutoState.Disabled:
                    SetStatus(Color.Gray,
                        "Disabled", "Click Enable to arm the scheduler",
                        "Enable", Color.FromArgb(46, 125, 50));
                    break;

                case AutoState.Armed:
                    var inWindow = ScheduleChecker.IsInActiveWindow(_settings, DateTime.Now);
                    var sub = inWindow
                        ? "Within schedule — watching for idle"
                        : ScheduleChecker.NextWindowDescription(_settings, DateTime.Now);
                    SetStatus(Color.FromArgb(245, 127, 23), "Monitoring", sub,
                        "Disable", Color.FromArgb(183, 28, 28));
                    break;

                case AutoState.Running:
                    SetStatus(Color.FromArgb(46, 125, 50), "Active — automating", "User is idle",
                        "Disable", Color.FromArgb(183, 28, 28));
                    break;
            }
        }
        else if (tabControl.SelectedTab == tabManual)
        {
            switch (_manualState)
            {
                case ManualState.Idle:
                    SetStatus(Color.Gray, "Idle", "Ready to start",
                        "Start", Color.FromArgb(46, 125, 50));
                    break;

                case ManualState.WaitingToStart:
                    SetStatus(Color.FromArgb(245, 127, 23), "Waiting",
                        $"Starting at {_manualDelayEnd:T}",
                        "Stop", Color.FromArgb(183, 28, 28));
                    break;

                case ManualState.Running:
                    SetStatus(Color.FromArgb(46, 125, 50), "Running",
                        $"Until {_manualEndTime:T}",
                        "Stop", Color.FromArgb(183, 28, 28));
                    break;
            }
        }
        else
        {
            SetStatus(Color.Gray, string.Empty, string.Empty, string.Empty, Color.Gray);
        }
    }

    private void SetStatus(Color dotColor, string text, string sub, string btnLabel, Color btnColor)
    {
        lblDot.ForeColor    = dotColor;
        lblStatusText.Text  = text;
        lblStatusSub.Text   = sub;
        btnAction.Text      = btnLabel;
        btnAction.BackColor = btnColor;
        btnAction.Visible   = btnLabel.Length > 0;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Tray & window lifecycle

    private void SetupTrayIcon()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Show", null, (_, _) => ShowForm());
        _trayMenu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Icon             = Icon,
            Text             = "SnapCqq2",
            ContextMenuStrip = _trayMenu,
            Visible          = false
        };
    }

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

    protected override void SetVisibleCore(bool value)
    {
        // Launched via Windows startup (--minimized flag): suppress window on first show.
        // Auto mode: arm immediately so the app is working from boot.
        // Manual mode: show the window so the user can configure and start a run.
        if (!IsHandleCreated && _startMinimized && _settings.DefaultMode == "Auto")
        {
            CreateHandle();
            _trayIcon.Visible = true;
            base.SetVisibleCore(false);
            ArmAuto();
            return;
        }

        base.SetVisibleCore(value);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        bool autoArmed  = _autoState != AutoState.Disabled;
        bool manualBusy = _manualState != ManualState.Idle;
        if (WindowState == FormWindowState.Minimized && (autoArmed || manualBusy))
            HideToTray();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        idleCheckTimer?.Dispose();
        actionTimer?.Dispose();
        updateTimer?.Dispose();
        delayTimer?.Dispose();
        _trayIcon?.Dispose();
        base.OnFormClosed(e);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Helpers

    private static string Truncate(string s, int max) =>
        s.Length > max ? s[..max] : s;

    #endregion
}
