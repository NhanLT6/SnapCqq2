using System.Diagnostics;
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

    private const int ShutdownGraceSeconds = 60;

    // ── State ──────────────────────────────────────────────────────
    // What the status area shows. Derived from the fields below on every tick — never stored.
    private enum View { Off, Watching, OnBreak, OutsideSchedule, Acting, Paused, QuickRunWaiting, QuickRun, ShutdownPending }

    // A "Run now" session: ignores schedule and idle. EndAt null = until stopped.
    private sealed record QuickRunState(DateTime StartAt, DateTime? EndAt);

    private bool            _acting;               // schedule mode: user is idle, actions firing
    private DateTime?       _pausedUntil;
    private QuickRunState?  _quickRun;
    private DateTime?       _shutdownAt;           // pending `shutdown /s /t 60`
    private DateTime        _nextActionAt;
    private DateTime        _lastTick = DateTime.Now;
    private DateTime        _lastSimulatedInput = DateTime.MinValue;

    private string   _lastActionText = "";
    private int      _actionsToday;
    private DateOnly _actionsDay;

    private bool _loading = false; // suppresses save-on-change handlers during UI population
    private bool _exiting;
    private bool _trayHintShown;
    private readonly bool _startMinimized;

    // ── Services ───────────────────────────────────────────────────
    private AppSettings _settings = new();
    private readonly InputSimulator _simulator = new();
    private readonly Random _rng = new();

    // ── Tray ───────────────────────────────────────────────────────
    private NotifyIcon       _trayIcon = null!;
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
        WireEvents();
        RefreshNow();
        engineTimer.Start();
    }

    private void WireEvents()
    {
        btnPrimary.Click += (_, _) => PrimaryAction();
        btnMenu.Click    += (_, _) =>
        {
            FillMenu(_actionMenu.Items, forTray: false);
            _actionMenu.Show(btnMenu, new Point(btnMenu.Width, btnMenu.Height), ToolStripDropDownDirection.BelowLeft);
        };
        lnkBadge.LinkClicked += (_, _) => ToggleSkipToday();

        // Schedule
        for (int i = 0; i < 7; i++) btnDay[i].Click += DayBtn_Click;
        dtpWorkStart.ValueChanged       += Setting_Changed;
        dtpWorkEnd.ValueChanged         += Setting_Changed;
        dtpBreakStart.ValueChanged      += Setting_Changed;
        dtpBreakEnd.ValueChanged        += Setting_Changed;
        numIdleThreshold.ValueChanged   += Setting_Changed;
        chkStopOnReturn.CheckedChanged  += Setting_Changed;

        // Actions
        chkMouseMove.CheckedChanged      += Setting_Changed;
        chkTeamsKeepAlive.CheckedChanged += Setting_Changed;
        chkScrollLock.CheckedChanged     += Setting_Changed;
        chkMouseClick.CheckedChanged     += Setting_Changed;
        chkArrowKeys.CheckedChanged      += Setting_Changed;
        numFrequency.ValueChanged        += Setting_Changed;

        // At end
        cmbEndAction.SelectedIndexChanged += Setting_Changed;

        // General
        chkLaunchOnStartup.CheckedChanged += (_, _) => { if (!_loading) StartupManager.Sync(chkLaunchOnStartup.Checked); };

        // Tray
        _trayIcon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowForm(); };
        _trayMenu.Opening    += (_, e) =>
        {
            FillMenu(_trayMenu.Items, forTray: true);
            e.Cancel = false; // WinForms pre-cancels menus that were empty when opening started
        };

        engineTimer.Tick += (_, _) => RefreshNow();
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
        else
        {
            StartupManager.RefreshPath();
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

            numIdleThreshold.Value  = Math.Clamp(_settings.IdleThresholdMinutes, 1, 60);
            chkStopOnReturn.Checked = _settings.StopWhenUserReturns;

            chkMouseMove.Checked      = _settings.UseMouseMove;
            chkTeamsKeepAlive.Checked = _settings.UseTeamsKeepAlive;
            chkScrollLock.Checked     = _settings.UseScrollLock;
            chkMouseClick.Checked     = _settings.UseMouseClick;
            chkArrowKeys.Checked      = _settings.UseArrowKeys;
            numFrequency.Value        = Math.Clamp(_settings.FrequencyMinutes, 1, 60);

            cmbEndAction.SelectedIndex = _settings.AutoTimeoutAction switch
            {
                TimeoutAction.LockAndClose    => 1,
                TimeoutAction.ShutdownWindows => 2,
                _                             => 0  // DoNothing (legacy CloseApp too)
            };

            chkLaunchOnStartup.Checked = StartupManager.IsEnabled();
        }
        finally
        {
            _loading = false;
        }
        UpdateSummaries();
    }

    private void SaveFromUI()
    {
        for (int i = 0; i < 7; i++)
            _settings.ActiveDays[i] = btnDay[i].Tag is true;

        _settings.WorkStart  = TimeOnly.FromDateTime(dtpWorkStart.Value);
        _settings.WorkEnd    = TimeOnly.FromDateTime(dtpWorkEnd.Value);
        _settings.BreakStart = TimeOnly.FromDateTime(dtpBreakStart.Value);
        _settings.BreakEnd   = TimeOnly.FromDateTime(dtpBreakEnd.Value);

        _settings.IdleThresholdMinutes = (int)numIdleThreshold.Value;
        _settings.StopWhenUserReturns  = chkStopOnReturn.Checked;

        _settings.UseMouseMove      = chkMouseMove.Checked;
        _settings.UseTeamsKeepAlive = chkTeamsKeepAlive.Checked;
        _settings.UseScrollLock     = chkScrollLock.Checked;
        _settings.UseMouseClick     = chkMouseClick.Checked;
        _settings.UseArrowKeys      = chkArrowKeys.Checked;
        _settings.FrequencyMinutes  = (int)numFrequency.Value;

        _settings.AutoTimeoutAction = cmbEndAction.SelectedIndex switch
        {
            1 => TimeoutAction.LockAndClose,
            2 => TimeoutAction.ShutdownWindows,
            _ => TimeoutAction.DoNothing
        };

        SettingsManager.Save(_settings);
    }

    private void Setting_Changed(object? sender, EventArgs e)
    {
        if (_loading) return;
        SaveFromUI();
        UpdateSummaries();
        RefreshNow();
    }

    private void UpdateSummaries()
    {
        lblScheduleSummary.Text = SettingsSummary.Schedule(_settings);
        lblActionsSummary.Text  = SettingsSummary.Actions(_settings);
        lblEndSummary.Text      = SettingsSummary.EndAction(_settings.AutoTimeoutAction);

        bool danger = SettingsSummary.IsDisruptive(_settings.AutoTimeoutAction);
        lblEndSummary.ForeColor = danger ? ColDanger : ColText;
        lblEndKey.ForeColor     = danger ? ColDanger : ColSecondary;
        lblEndIcon.ForeColor    = danger ? ColDanger : ColSecondary;
    }

    private void SaveSettings() => SettingsManager.Save(_settings);

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Day buttons

    private void DayBtn_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        SetDayButtonState(btn, btn.Tag is not true);
        Setting_Changed(sender, e);
    }

    private static void SetDayButtonState(Button btn, bool active)
    {
        btn.Tag       = active;
        btn.BackColor = active ? ColButton : ColDayOff;
        btn.ForeColor = active ? Color.White : ColSecondary;
        btn.FlatAppearance.MouseOverBackColor = active ? ColButtonHi : ColBorder;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Engine

    /// <summary>Advances state, redraws the status area and tray, and schedules the next tick.</summary>
    private void RefreshNow()
    {
        var now = DateTime.Now;
        Step(now);
        UpdateView(now);
        ScheduleNextTick(now);
    }

    private void Step(DateTime now)
    {
        if (_pausedUntil is { } p && now >= p) _pausedUntil = null;

        if (_quickRun is { } run)
        {
            if (now < run.StartAt) { }
            else if (run.EndAt is { } end && now >= end) FinishQuickRun();
            else if (now >= _nextActionAt) Act(now);
        }
        else if (_settings.AutoEnabled && _pausedUntil is null)
        {
            StepSchedule(now);
        }
        else
        {
            _acting = false;
        }

        _lastTick = now;
    }

    private void StepSchedule(DateTime now)
    {
        var idle      = IdleDetector.GetIdleTime();
        var threshold = TimeSpan.FromMinutes(_settings.IdleThresholdMinutes);

        // Work hours just ended while the user was away → end-of-day action.
        // Only at work end, never at break start.
        if (_acting && ScheduleChecker.WorkEndToday(_settings, _lastTick) is { } end && now >= end)
        {
            _acting = false;
            if (!UserReturned(idle, threshold) && _settings.EndActionSkippedOn != DateOnly.FromDateTime(now))
                ApplyEndAction(_settings.AutoTimeoutAction);
            return;
        }

        if (!ScheduleChecker.IsInActiveWindow(_settings, now))
        {
            _acting = false;
            return;
        }

        if (!_acting)
        {
            if (idle >= threshold) { _acting = true; Act(now); }
        }
        else if (_settings.StopWhenUserReturns && UserReturned(idle, threshold))
        {
            _acting = false;
        }
        else if (now >= _nextActionAt)
        {
            Act(now);
        }
    }

    // Our own SendInput actions reset the system idle clock too, so "idle < threshold" alone
    // would treat every simulated action as the user coming back. Only count input that
    // happened after our last simulated action.
    private bool UserReturned(TimeSpan idle, TimeSpan threshold)
    {
        if (idle >= threshold) return false;
        var lastInput = DateTime.Now - idle;
        return lastInput > _lastSimulatedInput.AddSeconds(2);
    }

    /// <summary>
    /// Every second while the window is open or a shutdown is counting down; otherwise only as
    /// often as needed — the idle-poll interval, or sooner if an action/deadline is due.
    /// </summary>
    private void ScheduleNextTick(DateTime now)
    {
        int ms = 1000;
        if (!Visible && _shutdownAt is null)
        {
            ms = IdleDetector.ComputePollIntervalMs(_settings.IdleThresholdMinutes);
            foreach (var due in UpcomingDeadlines())
            {
                var until = (int)Math.Ceiling((due - now).TotalMilliseconds);
                if (until > 0) ms = Math.Min(ms, until);
            }
            ms = Math.Max(1000, ms);
        }
        if (engineTimer.Interval != ms) engineTimer.Interval = ms;
    }

    private IEnumerable<DateTime> UpcomingDeadlines()
    {
        if (_acting || _quickRun is not null) yield return _nextActionAt;
        if (_quickRun is { } run)
        {
            yield return run.StartAt;
            if (run.EndAt is { } end) yield return end;
        }
        if (_pausedUntil is { } p) yield return p;
        if (_acting && ScheduleChecker.WorkEndToday(_settings, DateTime.Now) is { } workEnd) yield return workEnd;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Commands

    private void PrimaryAction()
    {
        switch (CurrentView(DateTime.Now))
        {
            case View.ShutdownPending: CancelShutdown(); break;
            case View.QuickRun:
            case View.QuickRunWaiting: StopQuickRun(); break;
            case View.Off:             TurnOn(); break;
            case View.Paused:          Resume(); break;
            default:                   PauseUntil(DateTime.Now.AddHours(1)); break; // any schedule-on state
        }
    }

    private void TurnOn()
    {
        _settings.AutoEnabled = true;
        SaveSettings();
        RefreshNow();
    }

    private void TurnOff()
    {
        _settings.AutoEnabled = false;
        _acting      = false;
        _pausedUntil = null;
        SaveSettings();
        RefreshNow();
    }

    private void PauseUntil(DateTime until)
    {
        _pausedUntil = until;
        _acting      = false;
        RefreshNow();
    }

    private void Resume()
    {
        _pausedUntil = null;
        RefreshNow();
    }

    private void StartQuickRun(int minutes, int delayMinutes = 0)
    {
        _settings.QuickRunMinutes = minutes;
        SaveSettings();

        var start = DateTime.Now.AddMinutes(delayMinutes);
        _quickRun     = new QuickRunState(start, minutes > 0 ? start.AddMinutes(minutes) : null);
        _nextActionAt = start;
        _acting       = false;
        RefreshNow();
        HideToTray();
    }

    private void ShowQuickRunDialog()
    {
        using var dlg = new QuickRunDialog(_settings.QuickRunMinutes, _settings.QuickRunEndAction, Icon);
        if (dlg.ShowDialog(Visible ? this : null) != DialogResult.OK) return;
        _settings.QuickRunEndAction = dlg.EndAction;
        StartQuickRun(dlg.DurationMinutes, dlg.DelayMinutes);
    }

    private void ExtendQuickRun(int minutes)
    {
        if (_quickRun is { EndAt: { } end } run)
            _quickRun = run with { EndAt = end.AddMinutes(minutes) };
        RefreshNow();
    }

    private void StopQuickRun()
    {
        _quickRun = null;
        RefreshNow();
    }

    private void FinishQuickRun()
    {
        _quickRun = null;
        ApplyEndAction(_settings.QuickRunEndAction);
    }

    private void SetQuickRunEndAction(TimeoutAction action)
    {
        _settings.QuickRunEndAction = action;
        SaveSettings();
        RefreshNow();
    }

    private void ToggleSkipToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        _settings.EndActionSkippedOn = _settings.EndActionSkippedOn == today ? null : today;
        SaveSettings();
        RefreshNow();
    }

    private void ApplyEndAction(TimeoutAction action)
    {
        switch (action)
        {
            case TimeoutAction.LockAndClose:    LockWorkStation(); break;
            case TimeoutAction.ShutdownWindows: StartShutdown();   break;
            // DoNothing / legacy CloseApp: back to the schedule
        }
    }

    // Windows' own countdown does the shutdown, so it still happens if this app is killed;
    // `shutdown /a` cancels it.
    private void StartShutdown()
    {
        RunShutdownExe($"/s /t {ShutdownGraceSeconds} /c \"SnapCqq2: shutting down. Open SnapCqq2 or run 'shutdown /a' to cancel.\"");
        _shutdownAt = DateTime.Now.AddSeconds(ShutdownGraceSeconds);
        _trayIcon.ShowBalloonTip(10_000, "Shutting down in 1 minute", "Click the SnapCqq2 tray icon to cancel.", ToolTipIcon.Warning);
    }

    private void CancelShutdown()
    {
        RunShutdownExe("/a");
        _shutdownAt = null;
        RefreshNow();
    }

    private static void RunShutdownExe(string args) =>
        Process.Start(new ProcessStartInfo("shutdown", args) { CreateNoWindow = true, UseShellExecute = false });

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Automation actions

    private void Act(DateTime now)
    {
        var done = PerformActions();
        _nextActionAt = now.AddMinutes(_settings.FrequencyMinutes);

        var today = DateOnly.FromDateTime(now);
        if (_actionsDay != today) { _actionsDay = today; _actionsToday = 0; }
        _actionsToday++;
        _lastActionText = $"Last {now:HH:mm} · {done}";
    }

    /// <summary>Runs Teams keepalive (every tick) plus one random input action. Returns what ran.</summary>
    private string PerformActions()
    {
        var done = new List<string>();

        // Teams keepalive always fires every tick when enabled — it must beat Teams'
        // own idle timer reliably, so it cannot be left to random selection.
        if (_settings.UseTeamsKeepAlive)
            done.Add(TeamsKeepAlive.TrySendKeepAlive() ? "Teams" : "Teams not found");

        var actions = new List<(string Name, Action Run)>();
        if (_settings.UseMouseMove)  actions.Add(("mouse move",  PerformMouseMove));
        if (_settings.UseScrollLock) actions.Add(("scroll lock", PerformScrollLock));
        if (_settings.UseMouseClick) actions.Add(("mouse click", PerformMouseClick));
        if (_settings.UseArrowKeys)  actions.Add(("arrow key",   PerformArrowKey));
        if (actions.Count > 0)
        {
            var (name, run) = actions[_rng.Next(actions.Count)];
            run();
            _lastSimulatedInput = DateTime.Now;
            done.Insert(0, name);
        }

        if (done.Count == 0) return "no actions selected";
        var text = string.Join(" + ", done);
        return char.ToUpper(text[0]) + text[1..];
    }

    private void PerformScrollLock()
    {
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
        _simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
    }

    private void PerformMouseClick() => _simulator.Mouse.LeftButtonClick();

    private void PerformArrowKey()
    {
        VirtualKeyCode[] arrows = [VirtualKeyCode.UP, VirtualKeyCode.DOWN, VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT];
        _simulator.Keyboard.KeyPress(arrows[_rng.Next(arrows.Length)]);
    }

    private void PerformMouseMove()
    {
        // 2px jitter and return — net zero displacement, no on-screen indicator
        _simulator.Mouse.MoveMouseBy(2, 0);
        _simulator.Mouse.MoveMouseBy(-2, 0);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Status view

    private View CurrentView(DateTime now)
    {
        if (_shutdownAt is not null)  return View.ShutdownPending;
        if (_quickRun is { } run)     return now < run.StartAt ? View.QuickRunWaiting : View.QuickRun;
        if (!_settings.AutoEnabled)   return View.Off;
        if (_pausedUntil is not null) return View.Paused;
        if (_acting)                  return View.Acting;
        if (ScheduleChecker.IsInActiveWindow(_settings, now)) return View.Watching;
        if (ScheduleChecker.IsOnBreak(_settings, now))        return View.OnBreak;
        return View.OutsideSchedule;
    }

    private enum Tone { Active, Armed, Stopped, Alert }

    private sealed record Status(string Title, string Chip, string Detail, string Primary, Tone Tone);

    private void UpdateView(DateTime now)
    {
        var view   = CurrentView(now);
        var status = Describe(view, now);

        _trayIcon.Text = Truncate($"SnapCqq2 · {status.Title}: {status.Chip}", 63);

        lblState.Text   = status.Title;
        lblDetail.Text  = status.Detail;
        btnPrimary.Text = status.Primary;
        lblChip.Text    = "● " + status.Chip;
        (lblChip.ForeColor, lblChip.BackColor) = status.Tone switch
        {
            Tone.Active => (ColActing, ColActingBg),
            Tone.Armed  => (ColArmed,  ColArmedBg),
            Tone.Alert  => (ColDanger, ColDangerBg),
            _           => (ColSecondary, ColHover)
        };

        var badge = DescribeBadge(view, now);
        if (badge is { } b)
        {
            lblBadge.Text      = b.Text;
            lblBadge.BackColor = b.Danger ? ColDangerBg : ColHover;
            lblBadge.ForeColor = b.Danger ? ColDanger : ColSecondary;
            lnkBadge.Text      = b.Link ?? "";
        }

        lblActivity.Text = _lastActionText.Length == 0
            ? "No actions yet"
            : $"{_lastActionText} · {_actionsToday} today";

        int heroHeight = pnlHero.Height;
        LayoutHero(badge is not null, badge?.Link is not null, HasMenuItems(view));
        if (pnlHero.Height != heroHeight) RelayoutRows();
    }

    /// <summary>
    /// Title = mode (Schedule / Run now), chip = its status. The main button always acts on
    /// the titled mode; switching to Run now lives in the ▾ menu.
    /// </summary>
    private Status Describe(View view, DateTime now)
    {
        string then = _settings.QuickRunEndAction switch
        {
            TimeoutAction.LockAndClose    => "then lock Windows",
            TimeoutAction.ShutdownWindows => "then shut down Windows",
            _ => _settings.AutoEnabled ? "then back to schedule" : "then off"
        };
        string Cd(DateTime t) => SettingsSummary.Countdown(t - now);

        switch (view)
        {
            case View.ShutdownPending:
                return new("Shutdown", $"In {Cd(_shutdownAt!.Value)}", "Cancel to keep working", "Cancel shutdown", Tone.Alert);

            case View.QuickRunWaiting:
                return new("Run now", "Starting", $"Starts in {Cd(_quickRun!.StartAt)} · {then}", "Stop", Tone.Armed);

            case View.QuickRun:
                return _quickRun!.EndAt is { } end
                    ? new("Run now", "Running", $"{Cd(end)} left · {then}", "Stop", Tone.Active)
                    : new("Run now", "Running", "Until you stop · ignoring schedule and idle", "Stop", Tone.Active);

            case View.Off:
                return new("Schedule", "Off", "Turn on to act when you're away in work hours", "Turn on", Tone.Stopped);

            case View.Paused:
                var until = _pausedUntil!.Value;
                return new("Schedule", "Paused",
                    (until.Date > now.Date ? "Until tomorrow" : $"Until {until:HH:mm}") + " · resumes by itself", "Resume", Tone.Stopped);

            case View.Acting:
                return new("Schedule", "Acting", $"You're away · next action in {Cd(_nextActionAt)}", "Pause 1 h", Tone.Active);

            case View.Watching:
                return new("Schedule", "Watching",
                    $"Acts after {_settings.IdleThresholdMinutes} min idle · until {_settings.WorkEnd:HH:mm}", "Pause 1 h", Tone.Armed);

            case View.OnBreak:
                return new("Schedule", "On break", $"Resumes {_settings.BreakEnd:HH:mm}", "Pause 1 h", Tone.Armed);

            default:
                var next = ScheduleChecker.NextWindowStart(_settings, now);
                return new("Schedule", "Outside hours",
                    next is { } n ? $"Next: {(n.Date == now.Date ? "today" : n.ToString("ddd"))} {n:HH:mm}" : "No active days",
                    "Pause 1 h", Tone.Armed);
        }
    }

    private (string Text, string? Link, bool Danger)? DescribeBadge(View view, DateTime now)
    {
        if (view is View.QuickRun or View.QuickRunWaiting)
        {
            return SettingsSummary.IsDisruptive(_settings.QuickRunEndAction) && _quickRun?.EndAt is not null
                ? ($"{SettingsSummary.EndActionNoun(_settings.QuickRunEndAction)} when the run ends", null, true)
                : null;
        }

        if (view is View.Off or View.ShutdownPending) return null;
        if (!SettingsSummary.IsDisruptive(_settings.AutoTimeoutAction)) return null;
        if (ScheduleChecker.WorkEndToday(_settings, now) is not { } end) return null;

        var noun = SettingsSummary.EndActionNoun(_settings.AutoTimeoutAction);
        return _settings.EndActionSkippedOn == DateOnly.FromDateTime(now)
            ? ($"{noun} skipped today", "Undo", false)
            : ($"{noun} at {end:HH:mm} if you're away", "Skip today", true);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Menus

    /// <summary>
    /// Fills the ▾ menu (secondary actions) or the tray menu (status + primary + secondary + app items).
    /// Both are rebuilt on open so they always match the current state.
    /// </summary>
    private void FillMenu(ToolStripItemCollection items, bool forTray)
    {
        items.Clear();
        var now  = DateTime.Now;
        var view = CurrentView(now);
        var status = Describe(view, now);

        if (forTray)
        {
            items.Add(new ToolStripMenuItem($"{status.Title}: {status.Chip}") { Enabled = false });
            if (DescribeBadge(view, now) is { } badge)
                items.Add(new ToolStripMenuItem(badge.Text) { Enabled = false });
            items.Add(new ToolStripSeparator());
            items.Add(Item(status.Primary, PrimaryAction));
        }

        switch (view)
        {
            case View.ShutdownPending:
                break;

            case View.QuickRun:
            case View.QuickRunWaiting:
                if (_quickRun?.EndAt is not null)
                {
                    items.Add(Item("Add 15 min", () => ExtendQuickRun(15)));
                    items.Add(Item("Add 1 h",    () => ExtendQuickRun(60)));
                    items.Add(new ToolStripSeparator());
                    items.Add(new ToolStripMenuItem("When done") { Enabled = false });
                    items.Add(Radio(_settings.AutoEnabled ? "Back to schedule" : "Nothing", TimeoutAction.DoNothing));
                    items.Add(Radio("Lock Windows",      TimeoutAction.LockAndClose));
                    items.Add(Radio("Shut down Windows", TimeoutAction.ShutdownWindows));
                }
                break;

            case View.Off:
                AddRunItems(items);
                break;

            case View.Paused:
                items.Add(Item("Turn off schedule", TurnOff));
                items.Add(new ToolStripSeparator());
                AddRunItems(items);
                break;

            default: // Watching, Acting, OnBreak, OutsideSchedule — primary is "Pause 1 h"
                items.Add(Item("Pause until tomorrow", () => PauseUntil(DateTime.Today.AddDays(1))));
                items.Add(Item("Turn off schedule", TurnOff));
                items.Add(new ToolStripSeparator());
                AddRunItems(items);
                break;
        }

        if (forTray)
        {
            if (DescribeBadge(view, now) is { Link: { } link })
                items.Add(Item(link == "Undo" ? "Undo skip" : $"Skip {SettingsSummary.EndActionNoun(_settings.AutoTimeoutAction).ToLower()} today", ToggleSkipToday));
            items.Add(new ToolStripSeparator());
            items.Add(Item("Show", ShowForm));
            items.Add(Item("Exit", ExitApp));
        }

        // Drop a trailing separator (e.g. empty sections)
        while (items.Count > 0 && items[^1] is ToolStripSeparator) items.RemoveAt(items.Count - 1);
    }

    // Mirrors the empty cases in FillMenu: nothing to offer besides the primary action.
    private bool HasMenuItems(View view) => view switch
    {
        View.ShutdownPending                  => false,
        View.QuickRun or View.QuickRunWaiting => _quickRun?.EndAt is not null,
        _                                     => true
    };

    /// <summary>"Run now" section: a disabled heading, then durations. Runs ignore schedule and idle.</summary>
    private void AddRunItems(ToolStripItemCollection items)
    {
        items.Add(new ToolStripMenuItem("Run now, ignoring schedule") { Enabled = false });
        foreach (var m in new[] { 30, 60, 120, 0 })
        {
            var label = m == 0 ? "Until I stop" : $"For {SettingsSummary.Duration(m)}";
            items.Add(Item(label, () => StartQuickRun(m)));
        }
        items.Add(Item("Custom…", ShowQuickRunDialog));
    }

    private static ToolStripMenuItem Item(string text, Action onClick)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += (_, _) => onClick();
        return item;
    }

    private ToolStripMenuItem Radio(string text, TimeoutAction action)
    {
        var item = Item(text, () => SetQuickRunEndAction(action));
        item.Checked = _settings.QuickRunEndAction == action;
        return item;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────
    #region Tray & window lifecycle

    private void SetupTrayIcon()
    {
        _trayMenu = new ContextMenuStrip { ShowImageMargin = false };
        _trayIcon = new NotifyIcon
        {
            Icon             = new Icon(Icon!, SystemInformation.SmallIconSize), // same icon as the exe, small size
            Text             = "SnapCqq2",
            ContextMenuStrip = _trayMenu,
            Visible          = true
        };
    }

    private void HideToTray()
    {
        Hide();
        if (!_trayHintShown)
        {
            _trayHintShown = true;
            _trayIcon.ShowBalloonTip(3000, "SnapCqq2 is still running", "Right-click the tray icon for options.", ToolTipIcon.None);
        }
    }

    private void ShowForm()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        RefreshNow();
    }

    private void ExitApp()
    {
        _exiting = true;
        Application.Exit();
    }

    protected override void SetVisibleCore(bool value)
    {
        // Launched via Windows startup (--minimized flag): stay in the tray on first show.
        if (!IsHandleCreated && _startMinimized)
        {
            CreateHandle();
            base.SetVisibleCore(false);
            return;
        }
        base.SetVisibleCore(value);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RefreshNow();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized) HideToTray();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // The close button keeps the app running in the tray while it has work to do.
        bool busy = _settings.AutoEnabled || _quickRun is not null || _shutdownAt is not null;
        if (!_exiting && e.CloseReason == CloseReason.UserClosing && busy)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        engineTimer?.Dispose();
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
