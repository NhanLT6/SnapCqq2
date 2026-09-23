using SnapCqq2.Models;

namespace SnapCqq2;

/// <summary>"Run for…" — custom quick-run duration, start delay and what happens when it ends.</summary>
public sealed class QuickRunDialog : Form
{
    private readonly NumericUpDown _duration;
    private readonly CheckBox      _untilStopped;
    private readonly NumericUpDown _delay;
    private readonly ComboBox      _then;

    public int DurationMinutes => _untilStopped.Checked ? 0 : (int)_duration.Value;
    public int DelayMinutes    => (int)_delay.Value;
    public TimeoutAction EndAction => _then.SelectedIndex switch
    {
        1 => TimeoutAction.LockAndClose,
        2 => TimeoutAction.ShutdownWindows,
        _ => TimeoutAction.DoNothing
    };

    public QuickRunDialog(int minutes, TimeoutAction then, Icon? icon)
    {
        Text            = "Run for…";
        Font            = new Font("Segoe UI", 9f);
        AutoScaleMode   = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96f, 96f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        ShowInTaskbar   = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.White;
        AutoSize        = true;
        AutoSizeMode    = AutoSizeMode.GrowAndShrink;
        Padding         = new Padding(12);
        if (icon is not null) Icon = icon;

        _duration     = new NumericUpDown { Minimum = 5, Maximum = 720, Increment = 15, Width = 64, Value = Math.Clamp(minutes > 0 ? minutes : 60, 5, 720) };
        _untilStopped = new CheckBox { Text = "Until I stop", AutoSize = true, Checked = minutes == 0, Margin = new Padding(3, 6, 3, 3) };
        _delay        = new NumericUpDown { Minimum = 0, Maximum = 240, Increment = 5, Width = 64 };
        _then         = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
        _then.Items.AddRange(["Back to schedule", "Lock Windows", "Shut down Windows"]);
        _then.SelectedIndex = then switch
        {
            TimeoutAction.LockAndClose    => 1,
            TimeoutAction.ShutdownWindows => 2,
            _                             => 0
        };
        _untilStopped.CheckedChanged += (_, _) => { _duration.Enabled = !_untilStopped.Checked; _then.Enabled = !_untilStopped.Checked; };
        _duration.Enabled = _then.Enabled = !_untilStopped.Checked;

        var start  = new Button { Text = "Start",  DialogResult = DialogResult.OK,     AutoSize = true, MinimumSize = new Size(80, 0) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size(80, 0) };
        AcceptButton = start;
        CancelButton = cancel;

        var grid = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill };
        grid.Controls.Add(Caption("Run for"),   0, 0); grid.Controls.Add(WithUnit(_duration, "min"), 1, 0);
        grid.Controls.Add(new Label(),          0, 1); grid.Controls.Add(_untilStopped, 1, 1);
        grid.Controls.Add(Caption("Start in"),  0, 2); grid.Controls.Add(WithUnit(_delay, "min"), 1, 2);
        grid.Controls.Add(Caption("When done"), 0, 3); grid.Controls.Add(_then, 1, 3);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 12, 0, 0) };
        buttons.Controls.AddRange([cancel, start]);
        grid.Controls.Add(buttons, 0, 4);
        grid.SetColumnSpan(buttons, 2);

        Controls.Add(grid);
    }

    private static Label Caption(string text) =>
        new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 12, 3) };

    private static FlowLayoutPanel WithUnit(Control input, string unit)
    {
        var flow = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        flow.Controls.Add(input);
        flow.Controls.Add(new Label { Text = unit, AutoSize = true, Margin = new Padding(3, 6, 3, 3), ForeColor = Color.FromArgb(95, 99, 104) });
        return flow;
    }
}
