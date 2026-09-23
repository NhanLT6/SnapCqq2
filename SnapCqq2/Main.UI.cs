namespace SnapCqq2;

/// <summary>
/// All custom UI construction for Main. Kept separate from Main.Designer.cs
/// so the VS designer can manage that file without conflicts.
///
/// Layout (top to bottom): status area, three setting rows (Schedule / Actions / At end)
/// that expand one at a time, footer. Positions are in 96-DPI units and scaled with <see cref="S"/>.
/// </summary>
partial class Main
{
    // ── Palette ───────────────────────────────────────────────────
    private static readonly Color ColBg        = Color.White;
    private static readonly Color ColText      = Color.FromArgb(32, 33, 36);
    private static readonly Color ColSecondary = Color.FromArgb(95, 99, 104);
    private static readonly Color ColMuted     = Color.FromArgb(128, 134, 139);
    private static readonly Color ColBorder    = Color.FromArgb(218, 220, 224);
    private static readonly Color ColHover     = Color.FromArgb(241, 243, 244);
    private static readonly Color ColButton    = Color.FromArgb(60, 64, 67);
    private static readonly Color ColButtonHi  = Color.FromArgb(80, 84, 88);
    private static readonly Color ColDangerBg  = Color.FromArgb(252, 232, 230);
    private static readonly Color ColDanger    = Color.FromArgb(197, 34, 31);
    private static readonly Color ColDayOff    = Color.FromArgb(232, 234, 237);
    // Status chip tones: text + background
    private static readonly Color ColActing    = Color.FromArgb(19, 115, 51);    // doing actions
    private static readonly Color ColActingBg  = Color.FromArgb(230, 244, 234);
    private static readonly Color ColArmed     = Color.FromArgb(24, 90, 188);    // on, waiting
    private static readonly Color ColArmedBg   = Color.FromArgb(232, 240, 254);

    private const int FormW  = 460;
    private const int Indent = 44;   // left edge of row text and row editors
    private const int RowH   = 40;
    private const int ValueX = Indent + 66; // where row summaries and editor controls start
    private const int BodyTop = 4, Pitch = 32, SlotH = 28; // editor grid rows

    private static readonly string IconFont =
        FontFamily.Families.Any(f => f.Name == "Segoe Fluent Icons") ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";

    // ── Timer ─────────────────────────────────────────────────────
    private System.Windows.Forms.Timer engineTimer = null!;

    // ── Status area ───────────────────────────────────────────────
    private Panel     pnlHero     = null!;
    private Label     lblChip     = null!;
    private Label     lblState    = null!;
    private Label     lblDetail   = null!;
    private Label     lblBadge    = null!;
    private LinkLabel lnkBadge    = null!;
    private Label     lblActivity = null!;
    private Button    btnPrimary  = null!;
    private Button    btnMenu     = null!;
    private ContextMenuStrip _actionMenu = null!;

    // ── Setting rows ──────────────────────────────────────────────
    private Panel pnlScheduleHeader = null!, pnlScheduleBody = null!;
    private Panel pnlActionsHeader  = null!, pnlActionsBody  = null!;
    private Panel pnlEndHeader      = null!, pnlEndBody      = null!;
    private Label lblScheduleSummary = null!, lblActionsSummary = null!, lblEndSummary = null!, lblEndKey = null!, lblEndIcon = null!;
    private readonly List<(Panel Divider, Panel Header, Panel Body, Label Chevron)> _rows = new();

    // Schedule editor
    private Button[]       btnDay           = null!;
    private DateTimePicker dtpWorkStart     = null!;
    private DateTimePicker dtpWorkEnd       = null!;
    private DateTimePicker dtpBreakStart    = null!;
    private DateTimePicker dtpBreakEnd      = null!;
    private NumericUpDown  numIdleThreshold = null!;
    private CheckBox       chkStopOnReturn  = null!;

    // Actions editor
    private CheckBox      chkMouseMove      = null!;
    private CheckBox      chkTeamsKeepAlive = null!;
    private CheckBox      chkScrollLock     = null!;
    private CheckBox      chkMouseClick     = null!;
    private CheckBox      chkArrowKeys      = null!;
    private NumericUpDown numFrequency      = null!;

    // At-end editor
    private ComboBox cmbEndAction = null!;

    // ── Footer ────────────────────────────────────────────────────
    private Panel    pnlFooter          = null!;
    private CheckBox chkLaunchOnStartup = null!;

    /// <summary>Scales a 96-DPI length to the current DPI.</summary>
    private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);

    // ─────────────────────────────────────────────────────────────
    /// <summary>Builds all custom UI controls. Called from the constructor after InitializeComponent.</summary>
    private void BuildUI()
    {
        SuspendLayout();
        AutoScaleMode = AutoScaleMode.None; // we scale explicitly with S()
        BackColor     = ColBg;
        ForeColor     = ColText;
        ClientSize    = new Size(S(FormW), S(300));

        engineTimer = new System.Windows.Forms.Timer(components) { Interval = 1000 };

        BuildHero();

        // ── Schedule row ──────────────────────────────────────────
        // Editors use a two-column grid: labels under the row name, controls under the summary text.
        pnlScheduleBody = NewBody(BodyTop + 5 * Pitch);
        btnDay = new Button[7];
        string[] dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        for (int i = 0; i < 7; i++)
        {
            btnDay[i] = Place(new Button
            {
                Text      = dayNames[i],
                Size      = new Size(S(40), S(26)),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                TabStop   = false,
                Cursor    = Cursors.Hand
            }, ValueX + i * 44, 0);
            btnDay[i].FlatAppearance.BorderSize = 0;
        }
        pnlScheduleBody.Controls.AddRange(btnDay);

        dtpWorkStart     = Place(NewTimePicker(), ValueX,      1);
        dtpWorkEnd       = Place(NewTimePicker(), ValueX + 84, 1);
        dtpBreakStart    = Place(NewTimePicker(), ValueX,      2);
        dtpBreakEnd      = Place(NewTimePicker(), ValueX + 84, 2);
        numIdleThreshold = Place(new NumericUpDown { Width = S(50), Minimum = 1, Maximum = 60, Value = 4 }, ValueX, 3);
        chkStopOnReturn  = Place(new CheckBox { Text = "Stop when I'm back", AutoSize = true }, ValueX, 4);
        pnlScheduleBody.Controls.AddRange([
            RowLabel("Days", 0),
            RowLabel("Hours", 1),     dtpWorkStart,  Dash(ValueX + 60, 1), dtpWorkEnd,
            RowLabel("Break", 2),     dtpBreakStart, Dash(ValueX + 60, 2), dtpBreakEnd,
            RowLabel("Act after", 3), numIdleThreshold, Unit("min idle", ValueX + 56, 3),
            chkStopOnReturn ]);
        (pnlScheduleHeader, lblScheduleSummary, _, _) = AddRow("\uE787", "Schedule", pnlScheduleBody);

        // ── Actions row ───────────────────────────────────────────
        pnlActionsBody    = NewBody(BodyTop + 4 * Pitch);
        chkMouseMove      = Place(new CheckBox { Text = "Mouse move",      AutoSize = true }, ValueX,       0);
        chkTeamsKeepAlive = Place(new CheckBox { Text = "Teams keepalive", AutoSize = true }, ValueX + 140, 0);
        chkScrollLock     = Place(new CheckBox { Text = "Scroll lock",     AutoSize = true }, ValueX,       1);
        chkMouseClick     = Place(new CheckBox { Text = "Mouse click",     AutoSize = true }, ValueX + 140, 1);
        chkArrowKeys      = Place(new CheckBox { Text = "Arrow keys",      AutoSize = true }, ValueX,       2);
        numFrequency      = Place(new NumericUpDown { Width = S(50), Minimum = 1, Maximum = 60, Value = 1 }, ValueX, 3);
        pnlActionsBody.Controls.AddRange([
            RowLabel("Use", 0), chkMouseMove, chkTeamsKeepAlive, chkScrollLock,
            chkMouseClick, chkArrowKeys,
            RowLabel("Every", 3), numFrequency, Unit("min", ValueX + 56, 3) ]);
        (pnlActionsHeader, lblActionsSummary, _, _) = AddRow("\uE962", "Actions", pnlActionsBody);

        // ── At-end row ────────────────────────────────────────────
        pnlEndBody   = NewBody(BodyTop + Pitch + 40);
        cmbEndAction = Place(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = S(180) }, ValueX, 0);
        cmbEndAction.Items.AddRange(["Do nothing", "Lock Windows", "Shut down Windows"]);
        cmbEndAction.SelectedIndex = 0;
        var endNote = new Label
        {
            Text      = "At the end of work hours, only if you're away. Shutdown waits 60 s so you can cancel.",
            ForeColor = ColMuted,
            Font      = new Font("Segoe UI", 8.25f),
            Bounds    = new Rectangle(S(ValueX), RowY(1), S(FormW - ValueX - 16), S(34))
        };
        pnlEndBody.Controls.AddRange([RowLabel("Action", 0), cmbEndAction, endNote]);
        (pnlEndHeader, lblEndSummary, lblEndKey, lblEndIcon) = AddRow("\uE7E8", "At end", pnlEndBody);

        // ── Footer ────────────────────────────────────────────────
        chkLaunchOnStartup = new CheckBox { Text = "Start with Windows", Location = new Point(S(16), S(11)), AutoSize = true, ForeColor = ColSecondary };
        var footerLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = ColBorder };
        pnlFooter = new Panel { BackColor = ColBg, Height = S(40) };
        pnlFooter.Controls.AddRange([footerLine, chkLaunchOnStartup]);
        Controls.Add(pnlFooter);

        ResumeLayout(false);
    }

    private void BuildHero()
    {
        pnlHero = new Panel { BackColor = ColBg };

        lblChip = new Label
        {
            AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f),
            Padding = new Padding(S(6), S(2), S(8), S(3)), UseMnemonic = false
        };
        lblState = new Label
        {
            AutoSize = true, Font = new Font("Segoe UI Semibold", 12f),
            ForeColor = ColText, Location = new Point(S(14), S(12)), UseMnemonic = false
        };
        lblDetail = new Label
        {
            AutoSize = false, AutoEllipsis = true, ForeColor = ColSecondary,
            Bounds = new Rectangle(S(17), S(42), S(260), S(18)), UseMnemonic = false
        };
        lblBadge = new Label
        {
            AutoSize = true, Padding = new Padding(S(6), S(2), S(6), S(3)),
            Font = new Font("Segoe UI", 8.5f), UseMnemonic = false
        };
        lnkBadge = new LinkLabel
        {
            AutoSize = true, Font = new Font("Segoe UI", 8.5f),
            LinkColor = ColSecondary, ActiveLinkColor = ColText, LinkBehavior = LinkBehavior.HoverUnderline
        };
        lblActivity = new Label
        {
            AutoSize = false, AutoEllipsis = true, ForeColor = ColMuted, Font = new Font("Segoe UI", 8.25f),
            Bounds = new Rectangle(S(17), S(66), S(FormW - 33), S(16)), UseMnemonic = false
        };

        btnPrimary = NewDarkButton(Font);
        btnPrimary.Font = new Font("Segoe UI Semibold", 9f);
        btnMenu    = NewDarkButton(new Font(IconFont, 8f));
        btnMenu.Text = "\uE70D"; // ChevronDown

        _actionMenu = new ContextMenuStrip { ShowImageMargin = false, ShowCheckMargin = true };

        pnlHero.Controls.AddRange([lblChip, lblState, lblDetail, lblBadge, lnkBadge, lblActivity, btnPrimary, btnMenu]);
        Controls.Add(pnlHero);
    }

    /// <summary>Places hero children; called whenever its text changes (badge and button widths vary).</summary>
    private void LayoutHero(bool badgeVisible, bool linkVisible, bool menuVisible)
    {
        // The button group keeps one width in every state; without ▾ the main button takes its space.
        int w      = ClientSize.Width;
        int menuW  = S(28);
        int textW  = TextRenderer.MeasureText(btnPrimary.Text, btnPrimary.Font).Width + S(24);
        int groupW = Math.Max(S(112), textW) + 1 + menuW;
        btnMenu.Visible = menuVisible;
        btnMenu.SetBounds(w - S(16) - menuW, S(16), menuW, S(32));
        btnPrimary.SetBounds(w - S(16) - groupW, S(16), menuVisible ? groupW - menuW - 1 : groupW, S(32));
        lblDetail.Width = btnPrimary.Left - lblDetail.Left - S(8);
        lblChip.Location = new Point(lblState.Right + S(6), lblState.Top + (lblState.Height - lblChip.Height) / 2 + S(1));

        lblBadge.Visible = badgeVisible;
        lnkBadge.Visible = badgeVisible && linkVisible;
        int y = S(66);
        if (badgeVisible)
        {
            lblBadge.Location = new Point(S(16), S(66));
            lnkBadge.Location = new Point(lblBadge.Right + S(8), S(68));
            y = S(92);
        }
        lblActivity.Top  = y;
        pnlHero.Height   = y + S(28);
    }

    /// <summary>Stacks hero, rows and footer top to bottom and fits the form height to them.</summary>
    private void RelayoutRows()
    {
        int w = ClientSize.Width;
        int y = pnlHero.Height;
        pnlHero.SetBounds(0, 0, w, pnlHero.Height);
        foreach (var (divider, header, body, _) in _rows)
        {
            divider.SetBounds(0, y, w, 1);          y += 1;
            header.SetBounds(0, y, w, S(RowH));     y += header.Height;
            if (body.Visible) { body.SetBounds(0, y, w, body.Height); y += body.Height; }
        }
        pnlFooter.SetBounds(0, y, w, pnlFooter.Height);
        y += pnlFooter.Height;
        ClientSize = new Size(w, y);
    }

    // ── Row helpers ───────────────────────────────────────────────

    private (Panel Header, Label Summary, Label Key, Label Icon) AddRow(string glyph, string key, Panel body)
    {
        var divider = new Panel { BackColor = ColBorder, Height = 1 };
        var header  = new Panel { BackColor = ColBg, Cursor = Cursors.Hand };
        var icon    = new Label { Text = glyph, Font = new Font(IconFont, 11f), ForeColor = ColSecondary, TextAlign = ContentAlignment.MiddleCenter, Bounds = new Rectangle(S(12), 0, S(24), S(RowH)) };
        var keyLbl  = new Label { Text = key, ForeColor = ColSecondary, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(S(Indent), 0, S(64), S(RowH)) };
        var summary = new Label { AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, Bounds = new Rectangle(S(ValueX), 0, S(FormW - ValueX - 40), S(RowH)) };
        var chevron = new Label { Text = "\uE70D", Font = new Font(IconFont, 8f), ForeColor = ColMuted, TextAlign = ContentAlignment.MiddleCenter, Bounds = new Rectangle(S(FormW - 36), 0, S(24), S(RowH)) };

        header.Controls.AddRange([icon, keyLbl, summary, chevron]);
        foreach (Control c in header.Controls) { c.BackColor = Color.Transparent; c.Cursor = Cursors.Hand; }

        void Toggle(object? _, EventArgs __) => ToggleRow(body);
        void Enter(object? _, EventArgs __)  => header.BackColor = ColHover;
        void Leave(object? _, EventArgs __)
        {
            if (!header.ClientRectangle.Contains(header.PointToClient(Cursor.Position)))
                header.BackColor = ColBg;
        }
        header.Click += Toggle; header.MouseEnter += Enter; header.MouseLeave += Leave;
        foreach (Control c in header.Controls) { c.Click += Toggle; c.MouseEnter += Enter; c.MouseLeave += Leave; }

        body.Visible = false;
        Controls.AddRange([divider, header, body]);
        _rows.Add((divider, header, body, chevron));
        return (header, summary, keyLbl, icon);
    }

    /// <summary>Accordion: opening one row closes the others.</summary>
    private void ToggleRow(Panel body)
    {
        bool open = !body.Visible;
        foreach (var row in _rows)
        {
            row.Body.Visible  = row.Body == body && open;
            row.Chevron.Text  = row.Body.Visible ? "\uE70E" : "\uE70D"; // ChevronUp / ChevronDown
        }
        RelayoutRows();
    }

    private Panel NewBody(int height) =>
        new() { BackColor = ColBg, Height = S(height) };

    private int RowY(int row) => S(BodyTop + row * Pitch);

    /// <summary>Puts a control at column x of editor grid row <paramref name="row"/>, centred vertically in the slot.</summary>
    private T Place<T>(T c, int x, int row) where T : Control
    {
        int h = c.AutoSize ? c.PreferredSize.Height : c.Height;
        c.Location = new Point(S(x), RowY(row) + (S(SlotH) - h) / 2);
        return c;
    }

    private Label RowLabel(string text, int row) =>
        new() { Text = text, ForeColor = ColSecondary, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(S(Indent), RowY(row), S(64), S(SlotH)) };

    private Label Unit(string text, int x, int row) =>
        new() { Text = text, ForeColor = ColSecondary, TextAlign = ContentAlignment.MiddleLeft, Bounds = new Rectangle(S(x), RowY(row), S(80), S(SlotH)) };

    private Label Dash(int x, int row) =>
        new() { Text = "–", ForeColor = ColSecondary, TextAlign = ContentAlignment.MiddleCenter, Bounds = new Rectangle(S(x), RowY(row), S(24), S(SlotH)) };

    private DateTimePicker NewTimePicker() =>
        new() { Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = S(60) };

    private static Button NewDarkButton(Font font)
    {
        var b = new Button
        {
            Font = font, ForeColor = Color.White, BackColor = ColButton,
            FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false, Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize         = 0;
        b.FlatAppearance.MouseOverBackColor = ColButtonHi;
        b.FlatAppearance.MouseDownBackColor = ColText;
        return b;
    }
}
