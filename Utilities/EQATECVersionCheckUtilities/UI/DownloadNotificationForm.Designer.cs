namespace EQATEC.VersionCheckUtilities.UI
{
  partial class DownloadNotificationForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
      this.m_pictureboxImage = new System.Windows.Forms.PictureBox();
      this.m_buttonClose = new System.Windows.Forms.Button();
      this.m_webBrowser = new System.Windows.Forms.WebBrowser();
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureboxImage)).BeginInit();
      this.SuspendLayout();
      // 
      // m_pictureboxImage
      // 
      this.m_pictureboxImage.Image = global::EQATEC.VersionCheckUtilities.Properties.Resources.download_large;
      this.m_pictureboxImage.Location = new System.Drawing.Point(12, 12);
      this.m_pictureboxImage.Name = "m_pictureboxImage";
      this.m_pictureboxImage.Size = new System.Drawing.Size(48, 48);
      this.m_pictureboxImage.TabIndex = 0;
      this.m_pictureboxImage.TabStop = false;
      // 
      // m_buttonClose
      // 
      this.m_buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_buttonClose.Location = new System.Drawing.Point(526, 328);
      this.m_buttonClose.Name = "m_buttonClose";
      this.m_buttonClose.Size = new System.Drawing.Size(76, 23);
      this.m_buttonClose.TabIndex = 4;
      this.m_buttonClose.Text = "Close";
      this.m_buttonClose.UseVisualStyleBackColor = true;
      this.m_buttonClose.Click += new System.EventHandler(this.m_buttonClose_Click);
      // 
      // m_webBrowser
      // 
      this.m_webBrowser.AllowWebBrowserDrop = false;
      this.m_webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.m_webBrowser.IsWebBrowserContextMenuEnabled = false;
      this.m_webBrowser.Location = new System.Drawing.Point(66, 12);
      this.m_webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      this.m_webBrowser.Name = "m_webBrowser";
      this.m_webBrowser.ScriptErrorsSuppressed = true;
      this.m_webBrowser.Size = new System.Drawing.Size(535, 310);
      this.m_webBrowser.TabIndex = 5;
      this.m_webBrowser.WebBrowserShortcutsEnabled = false;
      // 
      // DownloadNotificationForm
      // 
      this.AcceptButton = this.m_buttonClose;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.LightSteelBlue;
      this.CancelButton = this.m_buttonClose;
      this.ClientSize = new System.Drawing.Size(609, 359);
      this.ControlBox = false;
      this.Controls.Add(this.m_webBrowser);
      this.Controls.Add(this.m_buttonClose);
      this.Controls.Add(this.m_pictureboxImage);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DownloadNotificationForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "New version available";
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureboxImage)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox m_pictureboxImage;
    private System.Windows.Forms.Button m_buttonClose;
    private System.Windows.Forms.WebBrowser m_webBrowser;
  }
}