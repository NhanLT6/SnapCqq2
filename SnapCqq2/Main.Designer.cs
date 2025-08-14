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
        numInterval = new System.Windows.Forms.NumericUpDown();
        label2 = new System.Windows.Forms.Label();
        chkAutoClose = new System.Windows.Forms.CheckBox();
        btnStartStop = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        actionTimer = new System.Windows.Forms.Timer(components);
        label3 = new System.Windows.Forms.Label();
        label4 = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)numDuration).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
        SuspendLayout();
        // 
        // numDuration
        // 
        numDuration.Increment = new decimal(new int[] { 15, 0, 0, 0 });
        numDuration.Location = new System.Drawing.Point(123, 16);
        numDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numDuration.Name = "numDuration";
        numDuration.Size = new System.Drawing.Size(168, 27);
        numDuration.TabIndex = 0;
        numDuration.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // label1
        // 
        label1.Location = new System.Drawing.Point(297, 18);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(100, 23);
        label1.TabIndex = 1;
        label1.Text = "min";
        // 
        // numInterval
        // 
        numInterval.Location = new System.Drawing.Point(123, 92);
        numInterval.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        numInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numInterval.Name = "numInterval";
        numInterval.Size = new System.Drawing.Size(168, 27);
        numInterval.TabIndex = 2;
        numInterval.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // label2
        // 
        label2.Location = new System.Drawing.Point(297, 94);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(100, 23);
        label2.TabIndex = 3;
        label2.Text = "s";
        // 
        // chkAutoClose
        // 
        chkAutoClose.Location = new System.Drawing.Point(123, 49);
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
        btnStartStop.Location = new System.Drawing.Point(360, 16);
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
        lblStatus.Location = new System.Drawing.Point(16, 148);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new System.Drawing.Size(34, 20);
        lblStatus.TabIndex = 6;
        lblStatus.Text = "Idle";
        // 
        // actionTimer
        // 
        actionTimer.Enabled = true;
        actionTimer.Tick += actionTimer_Tick;
        // 
        // label3
        // 
        label3.Location = new System.Drawing.Point(54, 18);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(63, 23);
        label3.TabIndex = 7;
        label3.Text = "Run for:";
        // 
        // label4
        // 
        label4.Location = new System.Drawing.Point(16, 94);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(101, 23);
        label4.TabIndex = 8;
        label4.Text = "Trigger every:";
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(482, 183);
        Controls.Add(label4);
        Controls.Add(label3);
        Controls.Add(lblStatus);
        Controls.Add(btnStartStop);
        Controls.Add(chkAutoClose);
        Controls.Add(label2);
        Controls.Add(numInterval);
        Controls.Add(label1);
        Controls.Add(numDuration);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "SnapCqq2";
        ((System.ComponentModel.ISupportInitialize)numDuration).EndInit();
        ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Timer actionTimer;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;

    private System.Windows.Forms.Label lblStatus;

    private System.Windows.Forms.Button btnStartStop;

    private System.Windows.Forms.CheckBox chkAutoClose;

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown numInterval;

    private System.Windows.Forms.NumericUpDown numDuration;

    #endregion
}