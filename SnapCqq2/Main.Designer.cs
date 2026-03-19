namespace SnapCqq2;

partial class Main
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
        numDuration = new System.Windows.Forms.NumericUpDown();
        label1 = new System.Windows.Forms.Label();
        cmbTimeoutAction = new System.Windows.Forms.ComboBox();
        btnStartStop = new System.Windows.Forms.Button();
        btnStartNow = new System.Windows.Forms.Button();
        btnCancel = new System.Windows.Forms.Button();
        actionTimer = new System.Windows.Forms.Timer(components);
        updateTimer = new System.Windows.Forms.Timer(components);
        delayTimer = new System.Windows.Forms.Timer(components);
        lblRunFor = new System.Windows.Forms.Label();
        lblToggleOptions = new System.Windows.Forms.Label();
        pnlOptions = new System.Windows.Forms.Panel();
        pnlStatus = new System.Windows.Forms.Panel();
        lblRemainingTime = new System.Windows.Forms.Label();
        lblStatus = new System.Windows.Forms.Label();
        panelMiniDivider = new System.Windows.Forms.Panel();
        // options panel controls
        lblActions = new System.Windows.Forms.Label();
        chkMouseClick = new System.Windows.Forms.CheckBox();
        chkScrollLock = new System.Windows.Forms.CheckBox();
        chkArrowKeys = new System.Windows.Forms.CheckBox();
        lblActionFrequency = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        numFrequency = new System.Windows.Forms.NumericUpDown();
        lblDelay = new System.Windows.Forms.Label();
        numDelay = new System.Windows.Forms.NumericUpDown();
        lblDelayMin = new System.Windows.Forms.Label();
        panelDivider = new System.Windows.Forms.Panel();

        ((System.ComponentModel.ISupportInitialize)numDuration).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numFrequency).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDelay).BeginInit();
        pnlOptions.SuspendLayout();
        pnlStatus.SuspendLayout();
        SuspendLayout();

        //
        // numDuration
        //
        numDuration.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numDuration.Location = new System.Drawing.Point(82, 12);
        numDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numDuration.Name = "numDuration";
        numDuration.Size = new System.Drawing.Size(65, 27);
        numDuration.TabIndex = 0;
        numDuration.Value = new decimal(new int[] { 30, 0, 0, 0 });
        //
        // label1 ("min")
        //
        label1.Location = new System.Drawing.Point(152, 15);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(35, 23);
        label1.TabIndex = 1;
        label1.Text = "min";
        //
        // cmbTimeoutAction
        //
        cmbTimeoutAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbTimeoutAction.FormattingEnabled = true;
        cmbTimeoutAction.Items.AddRange(new object[] { "Close when finished", "Lock Windows", "Shutdown Windows", "Do nothing" });
        cmbTimeoutAction.Location = new System.Drawing.Point(192, 12);
        cmbTimeoutAction.Name = "cmbTimeoutAction";
        cmbTimeoutAction.Size = new System.Drawing.Size(185, 28);
        cmbTimeoutAction.TabIndex = 2;
        cmbTimeoutAction.SelectedIndex = 0;
        //
        // btnStartStop
        //
        btnStartStop.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnStartStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnStartStop.ForeColor = System.Drawing.Color.White;
        btnStartStop.Location = new System.Drawing.Point(477, 10);
        btnStartStop.Name = "btnStartStop";
        btnStartStop.Size = new System.Drawing.Size(100, 95);
        btnStartStop.TabIndex = 3;
        btnStartStop.Text = "Start";
        btnStartStop.UseVisualStyleBackColor = false;
        btnStartStop.Click += btnStartStop_Click;
        //
        // btnStartNow
        //
        btnStartNow.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
        btnStartNow.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnStartNow.ForeColor = System.Drawing.Color.White;
        btnStartNow.Location = new System.Drawing.Point(477, 10);
        btnStartNow.Name = "btnStartNow";
        btnStartNow.Size = new System.Drawing.Size(100, 44);
        btnStartNow.TabIndex = 4;
        btnStartNow.Text = "Start Now";
        btnStartNow.UseVisualStyleBackColor = false;
        btnStartNow.Visible = false;
        btnStartNow.Click += btnStartNow_Click;
        //
        // btnCancel
        //
        btnCancel.BackColor = System.Drawing.Color.FromArgb(183, 28, 28);
        btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnCancel.ForeColor = System.Drawing.Color.White;
        btnCancel.Location = new System.Drawing.Point(477, 61);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new System.Drawing.Size(100, 44);
        btnCancel.TabIndex = 5;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = false;
        btnCancel.Visible = false;
        btnCancel.Click += btnCancel_Click;
        //
        // actionTimer
        //
        actionTimer.Tick += actionTimer_Tick;
        //
        // updateTimer
        //
        updateTimer.Interval = 1000;
        updateTimer.Tick += updateTimer_Tick;
        //
        // delayTimer
        //
        delayTimer.Interval = 1000;
        delayTimer.Tick += delayTimer_Tick;
        //
        // lblRunFor
        //
        lblRunFor = new System.Windows.Forms.Label();
        lblRunFor.Location = new System.Drawing.Point(15, 15);
        lblRunFor.Name = "lblRunFor";
        lblRunFor.Size = new System.Drawing.Size(63, 23);
        lblRunFor.TabIndex = 6;
        lblRunFor.Text = "Run for:";
        //
        // lblToggleOptions
        //
        lblToggleOptions.AutoSize = true;
        lblToggleOptions.Cursor = System.Windows.Forms.Cursors.Hand;
        lblToggleOptions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblToggleOptions.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
        lblToggleOptions.Location = new System.Drawing.Point(15, 50);
        lblToggleOptions.Name = "lblToggleOptions";
        lblToggleOptions.Size = new System.Drawing.Size(80, 20);
        lblToggleOptions.TabIndex = 7;
        lblToggleOptions.Text = "▶ Options";
        lblToggleOptions.Click += lblToggleOptions_Click;
        //
        // panelMiniDivider
        //
        panelMiniDivider.BackColor = System.Drawing.Color.LightGray;
        panelMiniDivider.Location = new System.Drawing.Point(0, 75);
        panelMiniDivider.Name = "panelMiniDivider";
        panelMiniDivider.Size = new System.Drawing.Size(592, 1);
        panelMiniDivider.TabIndex = 8;
        //
        // pnlOptions (collapsed by default, Visible = false)
        //
        pnlOptions.Location = new System.Drawing.Point(0, 76);
        pnlOptions.Name = "pnlOptions";
        pnlOptions.Size = new System.Drawing.Size(592, 110);
        pnlOptions.TabIndex = 9;
        pnlOptions.Visible = false;
        //
        // --- Controls inside pnlOptions ---
        //
        // lblActions
        //
        lblActions.Location = new System.Drawing.Point(15, 12);
        lblActions.Name = "lblActions";
        lblActions.Size = new System.Drawing.Size(63, 23);
        lblActions.TabIndex = 0;
        lblActions.Text = "Actions:";
        //
        // chkScrollLock
        //
        chkScrollLock.Checked = true;
        chkScrollLock.CheckState = System.Windows.Forms.CheckState.Checked;
        chkScrollLock.Location = new System.Drawing.Point(82, 11);
        chkScrollLock.Name = "chkScrollLock";
        chkScrollLock.Size = new System.Drawing.Size(110, 24);
        chkScrollLock.TabIndex = 1;
        chkScrollLock.Text = "Scroll-lock";
        chkScrollLock.UseVisualStyleBackColor = true;
        chkScrollLock.CheckedChanged += ActionCheckBox_CheckedChanged;
        //
        // chkMouseClick
        //
        chkMouseClick.Location = new System.Drawing.Point(197, 11);
        chkMouseClick.Name = "chkMouseClick";
        chkMouseClick.Size = new System.Drawing.Size(115, 24);
        chkMouseClick.TabIndex = 2;
        chkMouseClick.Text = "Mouse click";
        chkMouseClick.UseVisualStyleBackColor = true;
        chkMouseClick.CheckedChanged += ActionCheckBox_CheckedChanged;
        //
        // chkArrowKeys
        //
        chkArrowKeys.Location = new System.Drawing.Point(317, 11);
        chkArrowKeys.Name = "chkArrowKeys";
        chkArrowKeys.Size = new System.Drawing.Size(110, 24);
        chkArrowKeys.TabIndex = 3;
        chkArrowKeys.Text = "Arrow keys";
        chkArrowKeys.UseVisualStyleBackColor = true;
        chkArrowKeys.CheckedChanged += ActionCheckBox_CheckedChanged;
        //
        // lblActionFrequency
        //
        lblActionFrequency.Location = new System.Drawing.Point(15, 44);
        lblActionFrequency.Name = "lblActionFrequency";
        lblActionFrequency.Size = new System.Drawing.Size(63, 23);
        lblActionFrequency.TabIndex = 4;
        lblActionFrequency.Text = "Every:";
        //
        // numFrequency
        //
        numFrequency.Location = new System.Drawing.Point(82, 41);
        numFrequency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numFrequency.Name = "numFrequency";
        numFrequency.Size = new System.Drawing.Size(65, 27);
        numFrequency.TabIndex = 5;
        numFrequency.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // label3 ("min")
        //
        label3.Location = new System.Drawing.Point(152, 44);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(35, 23);
        label3.TabIndex = 6;
        label3.Text = "min";
        //
        // lblDelay
        //
        lblDelay.Location = new System.Drawing.Point(15, 76);
        lblDelay.Name = "lblDelay";
        lblDelay.Size = new System.Drawing.Size(63, 23);
        lblDelay.TabIndex = 7;
        lblDelay.Text = "Delay:";
        //
        // numDelay
        //
        numDelay.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numDelay.Location = new System.Drawing.Point(82, 73);
        numDelay.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        numDelay.Name = "numDelay";
        numDelay.Size = new System.Drawing.Size(65, 27);
        numDelay.TabIndex = 8;
        numDelay.Value = new decimal(new int[] { 0, 0, 0, 0 });
        //
        // lblDelayMin
        //
        lblDelayMin.Location = new System.Drawing.Point(152, 76);
        lblDelayMin.Name = "lblDelayMin";
        lblDelayMin.Size = new System.Drawing.Size(280, 23);
        lblDelayMin.TabIndex = 9;
        lblDelayMin.Text = "min  (0 = start immediately)";
        //
        // panelDivider (bottom of options panel)
        //
        panelDivider.BackColor = System.Drawing.Color.LightGray;
        panelDivider.Location = new System.Drawing.Point(0, 108);
        panelDivider.Name = "panelDivider";
        panelDivider.Size = new System.Drawing.Size(592, 1);
        panelDivider.TabIndex = 10;
        //
        // Add controls to pnlOptions
        //
        pnlOptions.Controls.Add(lblActions);
        pnlOptions.Controls.Add(chkScrollLock);
        pnlOptions.Controls.Add(chkMouseClick);
        pnlOptions.Controls.Add(chkArrowKeys);
        pnlOptions.Controls.Add(lblActionFrequency);
        pnlOptions.Controls.Add(numFrequency);
        pnlOptions.Controls.Add(label3);
        pnlOptions.Controls.Add(lblDelay);
        pnlOptions.Controls.Add(numDelay);
        pnlOptions.Controls.Add(lblDelayMin);
        pnlOptions.Controls.Add(panelDivider);
        //
        // pnlStatus
        //
        pnlStatus.Location = new System.Drawing.Point(0, 77);
        pnlStatus.Name = "pnlStatus";
        pnlStatus.Size = new System.Drawing.Size(470, 48);
        pnlStatus.TabIndex = 10;
        //
        // lblRemainingTime
        //
        lblRemainingTime.AutoSize = true;
        lblRemainingTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblRemainingTime.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
        lblRemainingTime.Location = new System.Drawing.Point(15, 5);
        lblRemainingTime.Name = "lblRemainingTime";
        lblRemainingTime.Size = new System.Drawing.Size(230, 20);
        lblRemainingTime.TabIndex = 0;
        lblRemainingTime.Text = "Start app to see remaining time";
        //
        // lblStatus
        //
        lblStatus.AutoSize = true;
        lblStatus.Location = new System.Drawing.Point(15, 27);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(34, 20);
        lblStatus.TabIndex = 1;
        lblStatus.Text = "Idle";
        //
        // Add controls to pnlStatus
        //
        pnlStatus.Controls.Add(lblRemainingTime);
        pnlStatus.Controls.Add(lblStatus);
        //
        // Main form
        //
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(592, 125);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        Controls.Add(lblRunFor);
        Controls.Add(numDuration);
        Controls.Add(label1);
        Controls.Add(cmbTimeoutAction);
        Controls.Add(btnStartStop);
        Controls.Add(btnStartNow);
        Controls.Add(btnCancel);
        Controls.Add(lblToggleOptions);
        Controls.Add(panelMiniDivider);
        Controls.Add(pnlOptions);
        Controls.Add(pnlStatus);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "SnapCqq2";

        ((System.ComponentModel.ISupportInitialize)numDuration).EndInit();
        ((System.ComponentModel.ISupportInitialize)numFrequency).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDelay).EndInit();
        pnlOptions.ResumeLayout(false);
        pnlStatus.ResumeLayout(false);
        pnlStatus.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.NumericUpDown numDuration;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cmbTimeoutAction;
    private System.Windows.Forms.Button btnStartStop;
    private System.Windows.Forms.Button btnStartNow;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Timer actionTimer;
    private System.Windows.Forms.Timer updateTimer;
    private System.Windows.Forms.Timer delayTimer;
    private System.Windows.Forms.Label lblRunFor;
    private System.Windows.Forms.Label lblToggleOptions;
    private System.Windows.Forms.Panel pnlOptions;
    private System.Windows.Forms.Panel pnlStatus;
    private System.Windows.Forms.Panel panelMiniDivider;
    private System.Windows.Forms.Label lblRemainingTime;
    private System.Windows.Forms.Label lblStatus;
    // Options panel controls
    private System.Windows.Forms.Label lblActions;
    private System.Windows.Forms.CheckBox chkMouseClick;
    private System.Windows.Forms.CheckBox chkScrollLock;
    private System.Windows.Forms.CheckBox chkArrowKeys;
    private System.Windows.Forms.Label lblActionFrequency;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown numFrequency;
    private System.Windows.Forms.Label lblDelay;
    private System.Windows.Forms.NumericUpDown numDelay;
    private System.Windows.Forms.Label lblDelayMin;
    private System.Windows.Forms.Panel panelDivider;
}
