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
        SuspendLayout();

        AutoScaleDimensions = new System.Drawing.SizeF(8f, 20f);
        AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize          = new System.Drawing.Size(580, 300);
        FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
        MaximizeBox         = false;
        StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text                = "SnapCqq2";
        Font                = new System.Drawing.Font("Segoe UI", 9f);

        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Main));
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

        ResumeLayout(false);
    }

    #endregion
}
