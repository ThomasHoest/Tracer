namespace EQATEC.VersionCheckUtilities.UI
{
  partial class DownloadStatusPanel
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_pictureBoxDownload = new System.Windows.Forms.PictureBox();
      this.m_linkLabelDownload = new System.Windows.Forms.LinkLabel();
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBoxDownload)).BeginInit();
      this.SuspendLayout();
      // 
      // m_pictureBoxDownload
      // 
      this.m_pictureBoxDownload.Cursor = System.Windows.Forms.Cursors.Hand;
      this.m_pictureBoxDownload.Image = global::EQATEC.VersionCheckUtilities.Properties.Resources.download_small;
      this.m_pictureBoxDownload.Location = new System.Drawing.Point(16, 1);
      this.m_pictureBoxDownload.Name = "m_pictureBoxDownload";
      this.m_pictureBoxDownload.Size = new System.Drawing.Size(24, 24);
      this.m_pictureBoxDownload.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.m_pictureBoxDownload.TabIndex = 0;
      this.m_pictureBoxDownload.TabStop = false;
      this.m_pictureBoxDownload.Click += new System.EventHandler(this.m_pictureBoxDownload_Click);
      // 
      // m_linkLabelDownload
      // 
      this.m_linkLabelDownload.ActiveLinkColor = System.Drawing.Color.Blue;
      this.m_linkLabelDownload.AutoSize = true;
      this.m_linkLabelDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_linkLabelDownload.Location = new System.Drawing.Point(46, 6);
      this.m_linkLabelDownload.Name = "m_linkLabelDownload";
      this.m_linkLabelDownload.Size = new System.Drawing.Size(223, 13);
      this.m_linkLabelDownload.TabIndex = 1;
      this.m_linkLabelDownload.TabStop = true;
      this.m_linkLabelDownload.Text = "A new version is available. Get it here";
      this.m_linkLabelDownload.VisitedLinkColor = System.Drawing.Color.Blue;
      this.m_linkLabelDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkLabelDownload_LinkClicked);
      // 
      // DownloadStatusPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.PaleGoldenrod;
      this.Controls.Add(this.m_linkLabelDownload);
      this.Controls.Add(this.m_pictureBoxDownload);
      this.Name = "DownloadStatusPanel";
      this.Size = new System.Drawing.Size(611, 26);
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBoxDownload)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox m_pictureBoxDownload;
    private System.Windows.Forms.LinkLabel m_linkLabelDownload;
  }
}
