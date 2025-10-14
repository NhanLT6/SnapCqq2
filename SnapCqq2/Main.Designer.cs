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
        numDuration = new System.Windows.Forms.NumericUpDown();
        label1 = new System.Windows.Forms.Label();
        chkAutoClose = new System.Windows.Forms.CheckBox();
        btnStartStop = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        actionTimer = new System.Windows.Forms.Timer(components);
        updateTimer = new System.Windows.Forms.Timer(components);
        lblRunFor = new System.Windows.Forms.Label();
        lblRemainingTime = new System.Windows.Forms.Label();
        lblActions = new System.Windows.Forms.Label();
        panelDivider = new System.Windows.Forms.Panel();
        chkMouseClick = new System.Windows.Forms.CheckBox();
        chkScrollLock = new System.Windows.Forms.CheckBox();
        chkArrowKeys = new System.Windows.Forms.CheckBox();
        lblActionFrequency = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        numFrequency = new System.Windows.Forms.NumericUpDown();
        panel1 = new System.Windows.Forms.Panel();
        ((System.ComponentModel.ISupportInitialize)numDuration).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numFrequency).BeginInit();
        SuspendLayout();
        // 
        // numDuration
        // 
        numDuration.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numDuration.Location = new System.Drawing.Point(84, 12);
        numDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numDuration.Name = "numDuration";
        numDuration.Size = new System.Drawing.Size(65, 27);
        numDuration.TabIndex = 0;
        numDuration.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // label1
        // 
        label1.Location = new System.Drawing.Point(155, 14);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(41, 23);
        label1.TabIndex = 1;
        label1.Text = "min";
        // 
        // chkAutoClose
        // 
        chkAutoClose.Location = new System.Drawing.Point(200, 13);
        chkAutoClose.Name = "chkAutoClose";
        chkAutoClose.Size = new System.Drawing.Size(168, 24);
        chkAutoClose.TabIndex = 4;
        chkAutoClose.Text = "Close when finished";
        chkAutoClose.UseVisualStyleBackColor = true;
        // 
        // btnStartStop
        // 
        btnStartStop.BackColor = System.Drawing.Color.FromArgb(((int)((byte)46)), ((int)((byte)125)), ((int)((byte)50)));
        btnStartStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        btnStartStop.ForeColor = System.Drawing.Color.White;
        btnStartStop.Location = new System.Drawing.Point(477, 46);
        btnStartStop.Name = "btnStartStop";
        btnStartStop.Size = new System.Drawing.Size(100, 100);
        btnStartStop.TabIndex = 5;
        btnStartStop.Text = "Start";
        btnStartStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
        btnStartStop.UseVisualStyleBackColor = false;
        btnStartStop.Click += btnStartStop_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new System.Drawing.Point(15, 183);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(34, 20);
        lblStatus.TabIndex = 6;
        lblStatus.Text = "Idle";
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
        // lblRunFor
        // 
        lblRunFor.Location = new System.Drawing.Point(15, 14);
        lblRunFor.Name = "lblRunFor";
        lblRunFor.Size = new System.Drawing.Size(63, 23);
        lblRunFor.TabIndex = 7;
        lblRunFor.Text = "Run for:";
        // 
        // lblRemainingTime
        // 
        lblRemainingTime.AutoSize = true;
        lblRemainingTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        lblRemainingTime.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)46)), ((int)((byte)125)), ((int)((byte)50)));
        lblRemainingTime.Location = new System.Drawing.Point(15, 160);
        lblRemainingTime.Name = "lblRemainingTime";
        lblRemainingTime.Size = new System.Drawing.Size(230, 20);
        lblRemainingTime.TabIndex = 9;
        lblRemainingTime.Text = "Start app to see remaining time";
        // 
        // lblActions
        // 
        lblActions.Location = new System.Drawing.Point(15, 73);
        lblActions.Name = "lblActions";
        lblActions.Size = new System.Drawing.Size(63, 23);
        lblActions.TabIndex = 10;
        lblActions.Text = "Actions:";
        // 
        // panelDivider
        // 
        panelDivider.BackColor = System.Drawing.Color.LightGray;
        panelDivider.Location = new System.Drawing.Point(15, 57);
        panelDivider.Name = "panelDivider";
        panelDivider.Size = new System.Drawing.Size(420, 1);
        panelDivider.TabIndex = 17;
        // 
        // chkMouseClick
        // 
        chkMouseClick.Location = new System.Drawing.Point(200, 72);
        chkMouseClick.Name = "chkMouseClick";
        chkMouseClick.Size = new System.Drawing.Size(115, 24);
        chkMouseClick.TabIndex = 11;
        chkMouseClick.Text = "Mouse click";
        chkMouseClick.UseVisualStyleBackColor = true;
        chkMouseClick.CheckedChanged += ActionCheckBox_CheckedChanged;
        // 
        // chkScrollLock
        // 
        chkScrollLock.Checked = true;
        chkScrollLock.CheckState = System.Windows.Forms.CheckState.Checked;
        chkScrollLock.Location = new System.Drawing.Point(84, 72);
        chkScrollLock.Name = "chkScrollLock";
        chkScrollLock.Size = new System.Drawing.Size(110, 24);
        chkScrollLock.TabIndex = 12;
        chkScrollLock.Text = "Scroll-lock";
        chkScrollLock.UseVisualStyleBackColor = true;
        chkScrollLock.CheckedChanged += ActionCheckBox_CheckedChanged;
        // 
        // chkArrowKeys
        // 
        chkArrowKeys.Location = new System.Drawing.Point(321, 72);
        chkArrowKeys.Name = "chkArrowKeys";
        chkArrowKeys.Size = new System.Drawing.Size(110, 24);
        chkArrowKeys.TabIndex = 13;
        chkArrowKeys.Text = "Arrow keys";
        chkArrowKeys.UseVisualStyleBackColor = true;
        chkArrowKeys.CheckedChanged += ActionCheckBox_CheckedChanged;
        // 
        // lblActionFrequency
        // 
        lblActionFrequency.Location = new System.Drawing.Point(15, 104);
        lblActionFrequency.Name = "lblActionFrequency";
        lblActionFrequency.Size = new System.Drawing.Size(63, 23);
        lblActionFrequency.TabIndex = 16;
        lblActionFrequency.Text = "Every:";
        // 
        // label3
        // 
        label3.Location = new System.Drawing.Point(155, 104);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(41, 23);
        label3.TabIndex = 15;
        label3.Text = "min";
        // 
        // numFrequency
        // 
        numFrequency.Location = new System.Drawing.Point(84, 102);
        numFrequency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numFrequency.Name = "numFrequency";
        numFrequency.Size = new System.Drawing.Size(65, 27);
        numFrequency.TabIndex = 14;
        numFrequency.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // panel1
        // 
        panel1.BackColor = System.Drawing.Color.LightGray;
        panel1.Location = new System.Drawing.Point(15, 145);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(420, 1);
        panel1.TabIndex = 18;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(592, 215);
        Controls.Add(panel1);
        Controls.Add(panelDivider);
        Controls.Add(lblActionFrequency);
        Controls.Add(label3);
        Controls.Add(numFrequency);
        Controls.Add(chkArrowKeys);
        Controls.Add(chkScrollLock);
        Controls.Add(chkMouseClick);
        Controls.Add(lblActions);
        Controls.Add(lblRunFor);
        Controls.Add(lblRemainingTime);
        Controls.Add(lblStatus);
        Controls.Add(btnStartStop);
        Controls.Add(chkAutoClose);
        Controls.Add(label1);
        Controls.Add(numDuration);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "SnapCqq2";
        ((System.ComponentModel.ISupportInitialize)numDuration).EndInit();
        ((System.ComponentModel.ISupportInitialize)numFrequency).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Panel panel1;

    private System.Windows.Forms.Label lblActionFrequency;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown numFrequency;
    private System.Windows.Forms.Panel panelDivider;

    private System.Windows.Forms.Timer actionTimer;
    private System.Windows.Forms.Timer updateTimer;
    private System.Windows.Forms.Label lblRunFor;
    private System.Windows.Forms.Label lblRemainingTime;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Button btnStartStop;
    private System.Windows.Forms.CheckBox chkAutoClose;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numDuration;
    private System.Windows.Forms.Label lblActions;
    private System.Windows.Forms.CheckBox chkMouseClick;
    private System.Windows.Forms.CheckBox chkScrollLock;
    private System.Windows.Forms.CheckBox chkArrowKeys;

    #endregion
}