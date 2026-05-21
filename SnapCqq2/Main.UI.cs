namespace SnapCqq2;

/// <summary>
/// All custom UI construction for Main. Kept separate from Main.Designer.cs
/// so the VS designer can manage that file without conflicts.
/// </summary>
partial class Main
{
    // ── Timers ────────────────────────────────────────────────────
    private System.Windows.Forms.Timer idleCheckTimer = null!;
    private System.Windows.Forms.Timer actionTimer    = null!;
    private System.Windows.Forms.Timer updateTimer    = null!;
    private System.Windows.Forms.Timer delayTimer     = null!;

    // ── Tab control ───────────────────────────────────────────────
    private System.Windows.Forms.TabControl tabControl = null!;
    private System.Windows.Forms.TabPage    tabAuto    = null!;
    private System.Windows.Forms.TabPage    tabManual  = null!;
    private System.Windows.Forms.TabPage    tabGeneral = null!;

    // ── Status bar ────────────────────────────────────────────────
    private System.Windows.Forms.Panel  pnlStatusBar     = null!;
    private System.Windows.Forms.Panel  pnlStatusMain    = null!;
    private System.Windows.Forms.Panel  pnlStatusDivider = null!;
    private System.Windows.Forms.Label  lblDot           = null!;
    private System.Windows.Forms.Label  lblStatusText    = null!;
    private System.Windows.Forms.Label  lblStatusSub     = null!;
    private System.Windows.Forms.Button btnAction        = null!;
    private System.Windows.Forms.Label  lblLastAction    = null!;

    // ── Auto — WHEN card ──────────────────────────────────────────
    private System.Windows.Forms.Panel   pnlWhen          = null!;
    private System.Windows.Forms.Panel   pnlWhenHeader    = null!;
    private System.Windows.Forms.Label   lblWhenTitle     = null!;
    private System.Windows.Forms.Panel   pnlWhenBody      = null!;
    private System.Windows.Forms.Button[] btnDay           = null!;
    private System.Windows.Forms.Label   lblHoursLabel    = null!;
    private System.Windows.Forms.DateTimePicker dtpWorkStart  = null!;
    private System.Windows.Forms.Label   lblTo            = null!;
    private System.Windows.Forms.DateTimePicker dtpWorkEnd    = null!;
    private System.Windows.Forms.Label   lblBreakLabel    = null!;
    private System.Windows.Forms.DateTimePicker dtpBreakStart = null!;
    private System.Windows.Forms.Label   lblBreakDash     = null!;
    private System.Windows.Forms.DateTimePicker dtpBreakEnd   = null!;
    private System.Windows.Forms.Label         lblActivateAfter = null!;
    private System.Windows.Forms.NumericUpDown numIdleThreshold = null!;
    private System.Windows.Forms.Label         lblIdleMin       = null!;
    private System.Windows.Forms.CheckBox      chkStopOnReturn  = null!;

    // ── Auto — HOW card ───────────────────────────────────────────
    private System.Windows.Forms.Panel        pnlHow         = null!;
    private System.Windows.Forms.Panel        pnlHowHeader   = null!;
    private System.Windows.Forms.Label        lblHowTitle    = null!;
    private System.Windows.Forms.Panel        pnlHowBody     = null!;
    private System.Windows.Forms.Label        lblActionsLabel = null!;
    private System.Windows.Forms.CheckBox     chkScrollLock       = null!;
    private System.Windows.Forms.CheckBox     chkMouseClick       = null!;
    private System.Windows.Forms.CheckBox     chkArrowKeys        = null!;
    private System.Windows.Forms.CheckBox     chkTeamsKeepAlive   = null!;
    private System.Windows.Forms.CheckBox     chkMouseMove        = null!;
    private System.Windows.Forms.Label        lblEveryAuto   = null!;
    private System.Windows.Forms.NumericUpDown numFrequency  = null!;
    private System.Windows.Forms.Label        lblFreqMinAuto = null!;

    // ── Auto — AFTER card ─────────────────────────────────────────
    private System.Windows.Forms.Panel   pnlAfter       = null!;
    private System.Windows.Forms.Panel   pnlAfterHeader = null!;
    private System.Windows.Forms.Label   lblAfterTitle  = null!;
    private System.Windows.Forms.Panel   pnlAfterBody   = null!;
    private System.Windows.Forms.ComboBox cmbAutoTimeout = null!;

    // ── Manual — RUN card ─────────────────────────────────────────
    private System.Windows.Forms.Panel        pnlRun           = null!;
    private System.Windows.Forms.Panel        pnlRunHeader     = null!;
    private System.Windows.Forms.Label        lblRunTitle      = null!;
    private System.Windows.Forms.Panel        pnlRunBody       = null!;
    private System.Windows.Forms.Label        lblDurationLabel = null!;
    private System.Windows.Forms.NumericUpDown numDuration     = null!;
    private System.Windows.Forms.Label        lblDurationMin   = null!;
    private System.Windows.Forms.ComboBox     cmbManualTimeout = null!;
    private System.Windows.Forms.Label        lblDelayLabel    = null!;
    private System.Windows.Forms.NumericUpDown numDelay        = null!;
    private System.Windows.Forms.Label        lblDelayNote     = null!;

    // ── Manual — HOW card ─────────────────────────────────────────
    private System.Windows.Forms.Panel        pnlManualHow       = null!;
    private System.Windows.Forms.Panel        pnlManualHowHeader = null!;
    private System.Windows.Forms.Label        lblManualHowTitle  = null!;
    private System.Windows.Forms.Panel        pnlManualHowBody   = null!;
    private System.Windows.Forms.Label        lblManualActionsLabel = null!;
    private System.Windows.Forms.CheckBox     chkManualScrollLock        = null!;
    private System.Windows.Forms.CheckBox     chkManualMouseClick        = null!;
    private System.Windows.Forms.CheckBox     chkManualArrowKeys         = null!;
    private System.Windows.Forms.CheckBox     chkManualTeamsKeepAlive    = null!;
    private System.Windows.Forms.CheckBox     chkManualMouseMove         = null!;
    private System.Windows.Forms.Label        lblEveryManual        = null!;
    private System.Windows.Forms.NumericUpDown numManualFrequency   = null!;
    private System.Windows.Forms.Label        lblFreqMinManual      = null!;

    // ── General tab ───────────────────────────────────────────────
    private System.Windows.Forms.CheckBox  chkLaunchOnStartup  = null!;
    private System.Windows.Forms.Label     lblLaunchDesc       = null!;
    private System.Windows.Forms.Panel     pnlGeneralDiv1      = null!;
    private System.Windows.Forms.Label     lblDefaultModeLabel = null!;
    private System.Windows.Forms.ComboBox  cmbDefaultMode      = null!;
    private System.Windows.Forms.Label     lblDefaultModeDesc  = null!;

    // ─────────────────────────────────────────────────────────────
    /// <summary>Builds all custom UI controls. Called from the constructor after InitializeComponent.</summary>
    private void BuildUI()
    {
        SuspendLayout();

        // ── Timers ────────────────────────────────────────────────
        idleCheckTimer = new System.Windows.Forms.Timer(components) { Enabled = false };
        actionTimer    = new System.Windows.Forms.Timer(components) { Enabled = false };
        updateTimer    = new System.Windows.Forms.Timer(components) { Enabled = false, Interval = 1000 };
        delayTimer     = new System.Windows.Forms.Timer(components) { Enabled = false, Interval = 1000 };

        // ── Tab control ───────────────────────────────────────────
        tabControl = new System.Windows.Forms.TabControl();
        tabAuto    = new System.Windows.Forms.TabPage { Text = "Auto" };
        tabManual  = new System.Windows.Forms.TabPage { Text = "Manual" };
        tabGeneral = new System.Windows.Forms.TabPage { Text = "General" };

        // ── Status bar ────────────────────────────────────────────
        pnlStatusBar     = new System.Windows.Forms.Panel();
        pnlStatusMain    = new System.Windows.Forms.Panel();
        pnlStatusDivider = new System.Windows.Forms.Panel();
        lblDot           = new System.Windows.Forms.Label();
        lblStatusText    = new System.Windows.Forms.Label();
        lblStatusSub     = new System.Windows.Forms.Label();
        btnAction        = new System.Windows.Forms.Button();
        lblLastAction    = new System.Windows.Forms.Label();

        // ── Auto tab — WHEN card ──────────────────────────────────
        pnlWhen       = new System.Windows.Forms.Panel();
        pnlWhenHeader = new System.Windows.Forms.Panel();
        lblWhenTitle  = new System.Windows.Forms.Label();
        pnlWhenBody   = new System.Windows.Forms.Panel();

        btnDay = new System.Windows.Forms.Button[7];
        string[] dayNames = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        for (int i = 0; i < 7; i++)
        {
            btnDay[i] = new System.Windows.Forms.Button
            {
                Text     = dayNames[i],
                Size     = new System.Drawing.Size(38, 26),
                Location = new System.Drawing.Point(8 + i * 42, 8),
                Font     = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                TabStop  = false
            };
            btnDay[i].FlatAppearance.BorderSize = 0;
        }

        lblHoursLabel  = new System.Windows.Forms.Label { Text = "Hours", Location = new System.Drawing.Point(8, 42),   Size = new System.Drawing.Size(52, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        dtpWorkStart   = new System.Windows.Forms.DateTimePicker { Format = System.Windows.Forms.DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = 65, Location = new System.Drawing.Point(62, 42) };
        lblTo          = new System.Windows.Forms.Label { Text = "to",    Location = new System.Drawing.Point(130, 42), Size = new System.Drawing.Size(26, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        dtpWorkEnd     = new System.Windows.Forms.DateTimePicker { Format = System.Windows.Forms.DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = 65, Location = new System.Drawing.Point(158, 42) };
        lblBreakLabel  = new System.Windows.Forms.Label { Text = "Break", Location = new System.Drawing.Point(228, 42), Size = new System.Drawing.Size(48, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        dtpBreakStart  = new System.Windows.Forms.DateTimePicker { Format = System.Windows.Forms.DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = 65, Location = new System.Drawing.Point(278, 42) };
        lblBreakDash   = new System.Windows.Forms.Label { Text = "–",     Location = new System.Drawing.Point(346, 42), Size = new System.Drawing.Size(16, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        dtpBreakEnd    = new System.Windows.Forms.DateTimePicker { Format = System.Windows.Forms.DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = 65, Location = new System.Drawing.Point(364, 42) };

        lblActivateAfter  = new System.Windows.Forms.Label { Text = "Activate after", Location = new System.Drawing.Point(8, 74),   Size = new System.Drawing.Size(96, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        numIdleThreshold  = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(106, 74), Width = 50, Minimum = 1, Maximum = 60, Value = 4 };
        lblIdleMin        = new System.Windows.Forms.Label { Text = "min idle", Location = new System.Drawing.Point(160, 74), Size = new System.Drawing.Size(62, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        chkStopOnReturn   = new System.Windows.Forms.CheckBox { Text = "Stop when user returns", Location = new System.Drawing.Point(228, 76), AutoSize = true, Checked = true };

        // ── Auto tab — HOW card ───────────────────────────────────
        pnlHow       = new System.Windows.Forms.Panel();
        pnlHowHeader = new System.Windows.Forms.Panel();
        lblHowTitle  = new System.Windows.Forms.Label();
        pnlHowBody   = new System.Windows.Forms.Panel();

        lblActionsLabel   = new System.Windows.Forms.Label { Text = "Actions", Location = new System.Drawing.Point(8, 8),  Size = new System.Drawing.Size(55, 21), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        chkScrollLock     = new System.Windows.Forms.CheckBox { Text = "Scroll-lock",      Location = new System.Drawing.Point(66,  9), AutoSize = true, Checked = true };
        chkMouseClick     = new System.Windows.Forms.CheckBox { Text = "Mouse click",      Location = new System.Drawing.Point(172, 9), AutoSize = true };
        chkArrowKeys      = new System.Windows.Forms.CheckBox { Text = "Arrow keys",       Location = new System.Drawing.Point(282, 9), AutoSize = true };
        chkTeamsKeepAlive = new System.Windows.Forms.CheckBox { Text = "Teams keepalive",  Location = new System.Drawing.Point(392, 9), AutoSize = true };
        chkMouseMove      = new System.Windows.Forms.CheckBox { Text = "Mouse move",       Location = new System.Drawing.Point(66, 33), AutoSize = true };
        lblEveryAuto    = new System.Windows.Forms.Label { Text = "Every", Location = new System.Drawing.Point(8, 57), Size = new System.Drawing.Size(55, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        numFrequency    = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(66, 57), Width = 50, Minimum = 1, Maximum = 60, Value = 1 };
        lblFreqMinAuto  = new System.Windows.Forms.Label { Text = "min", Location = new System.Drawing.Point(120, 57), Size = new System.Drawing.Size(35, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

        // ── Auto tab — AFTER card ─────────────────────────────────
        pnlAfter       = new System.Windows.Forms.Panel();
        pnlAfterHeader = new System.Windows.Forms.Panel();
        lblAfterTitle  = new System.Windows.Forms.Label();
        pnlAfterBody   = new System.Windows.Forms.Panel();

        cmbAutoTimeout = new System.Windows.Forms.ComboBox
        {
            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
            Location      = new System.Drawing.Point(8, 8),
            Width         = 220
        };
        cmbAutoTimeout.Items.AddRange(new object[] { "Do nothing", "Lock Windows", "Shutdown Windows" });
        cmbAutoTimeout.SelectedIndex = 0;

        // ── Manual tab — RUN card ─────────────────────────────────
        pnlRun       = new System.Windows.Forms.Panel();
        pnlRunHeader = new System.Windows.Forms.Panel();
        lblRunTitle  = new System.Windows.Forms.Label();
        pnlRunBody   = new System.Windows.Forms.Panel();

        lblDurationLabel  = new System.Windows.Forms.Label { Text = "Duration", Location = new System.Drawing.Point(8, 8),   Size = new System.Drawing.Size(60, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        numDuration       = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(70, 8), Width = 60, Minimum = 1, Maximum = 480, Increment = 15, Value = 30 };
        lblDurationMin    = new System.Windows.Forms.Label { Text = "min  then", Location = new System.Drawing.Point(134, 8), Size = new System.Drawing.Size(65, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        cmbManualTimeout  = new System.Windows.Forms.ComboBox
        {
            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
            Location      = new System.Drawing.Point(200, 8),
            Width         = 185
        };
        cmbManualTimeout.Items.AddRange(new object[] { "Close when finished", "Lock Windows", "Shutdown Windows", "Do nothing" });
        cmbManualTimeout.SelectedIndex = 0;

        lblDelayLabel  = new System.Windows.Forms.Label { Text = "Delay", Location = new System.Drawing.Point(8, 40), Size = new System.Drawing.Size(60, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        numDelay       = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(70, 40), Width = 60, Minimum = 0, Maximum = 120, Increment = 5, Value = 0 };
        lblDelayNote   = new System.Windows.Forms.Label { Text = "min  (0 = start immediately)", Location = new System.Drawing.Point(134, 40), Size = new System.Drawing.Size(220, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

        // ── Manual tab — HOW card ─────────────────────────────────
        pnlManualHow       = new System.Windows.Forms.Panel();
        pnlManualHowHeader = new System.Windows.Forms.Panel();
        lblManualHowTitle  = new System.Windows.Forms.Label();
        pnlManualHowBody   = new System.Windows.Forms.Panel();

        lblManualActionsLabel      = new System.Windows.Forms.Label { Text = "Actions", Location = new System.Drawing.Point(8, 8), Size = new System.Drawing.Size(55, 21), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        chkManualScrollLock        = new System.Windows.Forms.CheckBox { Text = "Scroll-lock",     Location = new System.Drawing.Point(66,  9), AutoSize = true, Checked = true };
        chkManualMouseClick        = new System.Windows.Forms.CheckBox { Text = "Mouse click",     Location = new System.Drawing.Point(172, 9), AutoSize = true };
        chkManualArrowKeys         = new System.Windows.Forms.CheckBox { Text = "Arrow keys",      Location = new System.Drawing.Point(282, 9), AutoSize = true };
        chkManualTeamsKeepAlive    = new System.Windows.Forms.CheckBox { Text = "Teams keepalive", Location = new System.Drawing.Point(392, 9), AutoSize = true };
        chkManualMouseMove         = new System.Windows.Forms.CheckBox { Text = "Mouse move",      Location = new System.Drawing.Point(66, 33), AutoSize = true };
        lblEveryManual        = new System.Windows.Forms.Label { Text = "Every", Location = new System.Drawing.Point(8, 57), Size = new System.Drawing.Size(55, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        numManualFrequency    = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(66, 57), Width = 50, Minimum = 1, Maximum = 60, Value = 1 };
        lblFreqMinManual      = new System.Windows.Forms.Label { Text = "min", Location = new System.Drawing.Point(120, 57), Size = new System.Drawing.Size(35, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

        // ── General tab ───────────────────────────────────────────
        chkLaunchOnStartup  = new System.Windows.Forms.CheckBox { Text = "Launch on Windows startup",                   Location = new System.Drawing.Point(12, 14), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9f) };
        lblLaunchDesc       = new System.Windows.Forms.Label    { Text = "Start SnapCqq2 automatically when you log in (Auto mode arms itself)", Location = new System.Drawing.Point(30, 34), Size = new System.Drawing.Size(420, 18), ForeColor = System.Drawing.Color.Gray };
        pnlGeneralDiv1      = new System.Windows.Forms.Panel    { Location = new System.Drawing.Point(0, 58), Height = 1, BackColor = System.Drawing.Color.LightGray };
        lblDefaultModeLabel = new System.Windows.Forms.Label    { Text = "Default mode on launch",                      Location = new System.Drawing.Point(12, 68), Size = new System.Drawing.Size(170, 23), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        cmbDefaultMode      = new System.Windows.Forms.ComboBox { DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, Location = new System.Drawing.Point(190, 68), Width = 100 };
        cmbDefaultMode.Items.AddRange(new object[] { "Auto", "Manual" });
        cmbDefaultMode.SelectedIndex = 0;
        lblDefaultModeDesc  = new System.Windows.Forms.Label    { Text = "Which tab is shown first when the app opens", Location = new System.Drawing.Point(30, 94), Size = new System.Drawing.Size(380, 18), ForeColor = System.Drawing.Color.Gray };

        // ═══════════════════════════════════════════════════════════
        // Assembly
        // ═══════════════════════════════════════════════════════════

        var green        = System.Drawing.Color.FromArgb(46, 125, 50);
        var cardHeaderBg = System.Drawing.Color.FromArgb(235, 235, 235);
        var cardBg       = System.Drawing.Color.FromArgb(248, 248, 248);
        var boldFont     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        // ── WHEN card ─────────────────────────────────────────────
        lblWhenTitle.Text      = "▼ 📅 When";
        lblWhenTitle.Font      = boldFont;
        lblWhenTitle.ForeColor = green;
        lblWhenTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblWhenTitle.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
        lblWhenTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        pnlWhenHeader.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlWhenHeader.Height    = 28;
        pnlWhenHeader.BackColor = cardHeaderBg;
        pnlWhenHeader.Controls.Add(lblWhenTitle);

        foreach (var b in btnDay) pnlWhenBody.Controls.Add(b);
        pnlWhenBody.Controls.AddRange(new System.Windows.Forms.Control[] {
            lblHoursLabel, dtpWorkStart, lblTo, dtpWorkEnd,
            lblBreakLabel, dtpBreakStart, lblBreakDash, dtpBreakEnd,
            lblActivateAfter, numIdleThreshold, lblIdleMin, chkStopOnReturn });
        pnlWhenBody.Dock      = System.Windows.Forms.DockStyle.Fill;
        pnlWhenBody.BackColor = cardBg;

        pnlWhen.Controls.Add(pnlWhenBody);
        pnlWhen.Controls.Add(pnlWhenHeader);
        pnlWhen.BackColor = cardBg;

        // ── HOW card (auto) ───────────────────────────────────────
        lblHowTitle.Text      = "▶ ⚡ How";
        lblHowTitle.Font      = boldFont;
        lblHowTitle.ForeColor = green;
        lblHowTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblHowTitle.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
        lblHowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblHowTitle.Cursor    = System.Windows.Forms.Cursors.Hand;

        pnlHowHeader.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlHowHeader.Height    = 28;
        pnlHowHeader.BackColor = cardHeaderBg;
        pnlHowHeader.Cursor    = System.Windows.Forms.Cursors.Hand;
        pnlHowHeader.Controls.Add(lblHowTitle);

        pnlHowBody.Controls.AddRange(new System.Windows.Forms.Control[] {
            lblActionsLabel, chkScrollLock, chkMouseClick, chkArrowKeys, chkTeamsKeepAlive,
            chkMouseMove, lblEveryAuto, numFrequency, lblFreqMinAuto });
        pnlHowBody.Dock      = System.Windows.Forms.DockStyle.Fill;
        pnlHowBody.BackColor = cardBg;
        pnlHowBody.Visible   = false;

        pnlHow.Controls.Add(pnlHowBody);
        pnlHow.Controls.Add(pnlHowHeader);
        pnlHow.BackColor = cardBg;

        // ── AFTER card ────────────────────────────────────────────
        lblAfterTitle.Text      = "▶ 🔚 After schedule ends";
        lblAfterTitle.Font      = boldFont;
        lblAfterTitle.ForeColor = green;
        lblAfterTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblAfterTitle.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
        lblAfterTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblAfterTitle.Cursor    = System.Windows.Forms.Cursors.Hand;

        pnlAfterHeader.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlAfterHeader.Height    = 28;
        pnlAfterHeader.BackColor = cardHeaderBg;
        pnlAfterHeader.Cursor    = System.Windows.Forms.Cursors.Hand;
        pnlAfterHeader.Controls.Add(lblAfterTitle);

        pnlAfterBody.Controls.Add(cmbAutoTimeout);
        pnlAfterBody.Dock      = System.Windows.Forms.DockStyle.Fill;
        pnlAfterBody.BackColor = cardBg;
        pnlAfterBody.Visible   = false;

        pnlAfter.Controls.Add(pnlAfterBody);
        pnlAfter.Controls.Add(pnlAfterHeader);
        pnlAfter.BackColor = cardBg;

        tabAuto.Controls.AddRange(new System.Windows.Forms.Control[] { pnlWhen, pnlHow, pnlAfter });
        tabAuto.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        // ── RUN card (manual) ─────────────────────────────────────
        lblRunTitle.Text      = "▼ ▶ Run";
        lblRunTitle.Font      = boldFont;
        lblRunTitle.ForeColor = green;
        lblRunTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblRunTitle.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
        lblRunTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        pnlRunHeader.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlRunHeader.Height    = 28;
        pnlRunHeader.BackColor = cardHeaderBg;
        pnlRunHeader.Controls.Add(lblRunTitle);

        pnlRunBody.Controls.AddRange(new System.Windows.Forms.Control[] {
            lblDurationLabel, numDuration, lblDurationMin, cmbManualTimeout,
            lblDelayLabel, numDelay, lblDelayNote });
        pnlRunBody.Dock      = System.Windows.Forms.DockStyle.Fill;
        pnlRunBody.BackColor = cardBg;

        pnlRun.Controls.Add(pnlRunBody);
        pnlRun.Controls.Add(pnlRunHeader);
        pnlRun.BackColor = cardBg;

        // ── HOW card (manual) ─────────────────────────────────────
        lblManualHowTitle.Text      = "▶ ⚡ How";
        lblManualHowTitle.Font      = boldFont;
        lblManualHowTitle.ForeColor = green;
        lblManualHowTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblManualHowTitle.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
        lblManualHowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        lblManualHowTitle.Cursor    = System.Windows.Forms.Cursors.Hand;

        pnlManualHowHeader.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlManualHowHeader.Height    = 28;
        pnlManualHowHeader.BackColor = cardHeaderBg;
        pnlManualHowHeader.Cursor    = System.Windows.Forms.Cursors.Hand;
        pnlManualHowHeader.Controls.Add(lblManualHowTitle);

        pnlManualHowBody.Controls.AddRange(new System.Windows.Forms.Control[] {
            lblManualActionsLabel, chkManualScrollLock, chkManualMouseClick,
            chkManualArrowKeys, chkManualTeamsKeepAlive,
            chkManualMouseMove, lblEveryManual, numManualFrequency, lblFreqMinManual });
        pnlManualHowBody.Dock      = System.Windows.Forms.DockStyle.Fill;
        pnlManualHowBody.BackColor = cardBg;
        pnlManualHowBody.Visible   = false;

        pnlManualHow.Controls.Add(pnlManualHowBody);
        pnlManualHow.Controls.Add(pnlManualHowHeader);
        pnlManualHow.BackColor = cardBg;

        tabManual.Controls.AddRange(new System.Windows.Forms.Control[] { pnlRun, pnlManualHow });
        tabManual.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        // ── General tab ───────────────────────────────────────────
        pnlGeneralDiv1.Width = tabGeneral.Width;
        tabGeneral.Controls.AddRange(new System.Windows.Forms.Control[] {
            chkLaunchOnStartup, lblLaunchDesc,
            pnlGeneralDiv1, lblDefaultModeLabel, cmbDefaultMode, lblDefaultModeDesc });
        tabGeneral.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        // ── TabControl ────────────────────────────────────────────
        tabControl.Controls.AddRange(new System.Windows.Forms.TabPage[] { tabAuto, tabManual, tabGeneral });
        tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        tabControl.Font = new System.Drawing.Font("Segoe UI", 9f);

        // ── Status bar ────────────────────────────────────────────
        lblDot.Text      = "●";
        lblDot.Font      = new System.Drawing.Font("Segoe UI", 11f);
        lblDot.ForeColor = System.Drawing.Color.Gray;
        lblDot.AutoSize  = true;
        lblDot.Location  = new System.Drawing.Point(10, 9);

        lblStatusText.Text     = "Disabled";
        lblStatusText.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        lblStatusText.AutoSize = true;
        lblStatusText.Location = new System.Drawing.Point(28, 4);

        lblStatusSub.Text      = "Click Enable to arm the scheduler";
        lblStatusSub.Font      = new System.Drawing.Font("Segoe UI", 8f);
        lblStatusSub.ForeColor = System.Drawing.Color.Gray;
        lblStatusSub.AutoSize  = true;
        lblStatusSub.Location  = new System.Drawing.Point(28, 19);

        btnAction.Text      = "Enable";
        btnAction.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        btnAction.ForeColor = System.Drawing.Color.White;
        btnAction.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAction.FlatAppearance.BorderSize = 0;
        btnAction.Size      = new System.Drawing.Size(80, 24);
        btnAction.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnAction.Location  = new System.Drawing.Point(492, 6);
        btnAction.UseVisualStyleBackColor = false;

        pnlStatusMain.Height    = 36;
        pnlStatusMain.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlStatusMain.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        pnlStatusMain.Controls.AddRange(new System.Windows.Forms.Control[] { lblDot, lblStatusText, lblStatusSub, btnAction });

        pnlStatusDivider.Height    = 1;
        pnlStatusDivider.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlStatusDivider.BackColor = System.Drawing.Color.LightGray;

        lblLastAction.Text      = "—";
        lblLastAction.Font      = new System.Drawing.Font("Consolas", 8.5f);
        lblLastAction.ForeColor = System.Drawing.Color.DimGray;
        lblLastAction.Dock      = System.Windows.Forms.DockStyle.Fill;
        lblLastAction.Padding   = new System.Windows.Forms.Padding(10, 2, 0, 0);

        // Controls added in reverse visual order because Dock=Top stacks from bottom
        pnlStatusBar.Controls.Add(lblLastAction);
        pnlStatusBar.Controls.Add(pnlStatusDivider);
        pnlStatusBar.Controls.Add(pnlStatusMain);
        pnlStatusBar.Height    = 56;
        pnlStatusBar.Dock      = System.Windows.Forms.DockStyle.Bottom;
        pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

        // ── Wire into Form ────────────────────────────────────────
        Controls.Add(tabControl);
        Controls.Add(pnlStatusBar);

        ResumeLayout(false);
    }
}
