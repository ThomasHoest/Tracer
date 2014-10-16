namespace EQATEC.SigningUtilities.UI
{
  partial class SignAssemblyForm
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
      this.components = new System.ComponentModel.Container();
      this.m_labelIntroText = new System.Windows.Forms.Label();
      this.m_labelAssemblyName = new System.Windows.Forms.Label();
      this.m_textBoxAssemblyName = new System.Windows.Forms.TextBox();
      this.m_labelPublicKey = new System.Windows.Forms.Label();
      this.m_textBoxPublicKey = new System.Windows.Forms.TextBox();
      this.m_radioButton1Resign = new System.Windows.Forms.RadioButton();
      this.m_labelResignHelp = new System.Windows.Forms.Label();
      this.m_labelResignKeyFile = new System.Windows.Forms.Label();
      this.m_textBoxResignKeyFile = new System.Windows.Forms.TextBox();
      this.m_radioButton2Skip = new System.Windows.Forms.RadioButton();
      this.m_labelSkipHelp = new System.Windows.Forms.Label();
      this.m_radioButton3Strip = new System.Windows.Forms.RadioButton();
      this.m_labelStripHelp1 = new System.Windows.Forms.Label();
      this.m_labelStripHelp2 = new System.Windows.Forms.Label();
      this.m_labelStripHelp3 = new System.Windows.Forms.Label();
      this.m_buttonOK = new System.Windows.Forms.Button();
      this.m_LabelIntroTitle = new System.Windows.Forms.Label();
      this.m_buttonCancel = new System.Windows.Forms.Button();
      this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.m_pictureBox = new System.Windows.Forms.PictureBox();
      this.m_buttonResignBrowse = new System.Windows.Forms.Button();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.pictureBox3 = new System.Windows.Forms.PictureBox();
      this.m_labelOriginal = new System.Windows.Forms.Label();
      this.m_labelProfiled = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.m_errorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
      this.SuspendLayout();
      // 
      // m_labelIntroText
      // 
      this.m_labelIntroText.Location = new System.Drawing.Point(59, 28);
      this.m_labelIntroText.Name = "m_labelIntroText";
      this.m_labelIntroText.Size = new System.Drawing.Size(442, 28);
      this.m_labelIntroText.TabIndex = 3;
      this.m_labelIntroText.Text = "Setup a preferred action for handling every assembly signed with this particular " +
          "public key. You can change this action later, if you wish.";
      // 
      // m_labelAssemblyName
      // 
      this.m_labelAssemblyName.Location = new System.Drawing.Point(31, 64);
      this.m_labelAssemblyName.Name = "m_labelAssemblyName";
      this.m_labelAssemblyName.Size = new System.Drawing.Size(60, 20);
      this.m_labelAssemblyName.TabIndex = 4;
      this.m_labelAssemblyName.Text = "Assembly:";
      this.m_labelAssemblyName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_textBoxAssemblyName
      // 
      this.m_textBoxAssemblyName.BackColor = System.Drawing.Color.WhiteSmoke;
      this.m_textBoxAssemblyName.Location = new System.Drawing.Point(93, 64);
      this.m_textBoxAssemblyName.Name = "m_textBoxAssemblyName";
      this.m_textBoxAssemblyName.ReadOnly = true;
      this.m_textBoxAssemblyName.Size = new System.Drawing.Size(446, 20);
      this.m_textBoxAssemblyName.TabIndex = 5;
      // 
      // m_labelPublicKey
      // 
      this.m_labelPublicKey.Location = new System.Drawing.Point(31, 85);
      this.m_labelPublicKey.Name = "m_labelPublicKey";
      this.m_labelPublicKey.Size = new System.Drawing.Size(60, 20);
      this.m_labelPublicKey.TabIndex = 6;
      this.m_labelPublicKey.Text = "Public Key:";
      this.m_labelPublicKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_textBoxPublicKey
      // 
      this.m_textBoxPublicKey.BackColor = System.Drawing.Color.WhiteSmoke;
      this.m_textBoxPublicKey.Location = new System.Drawing.Point(93, 85);
      this.m_textBoxPublicKey.Name = "m_textBoxPublicKey";
      this.m_textBoxPublicKey.ReadOnly = true;
      this.m_textBoxPublicKey.Size = new System.Drawing.Size(446, 20);
      this.m_textBoxPublicKey.TabIndex = 7;
      // 
      // m_radioButton1Resign
      // 
      this.m_radioButton1Resign.AutoSize = true;
      this.m_radioButton1Resign.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_radioButton1Resign.Location = new System.Drawing.Point(13, 133);
      this.m_radioButton1Resign.Name = "m_radioButton1Resign";
      this.m_radioButton1Resign.Size = new System.Drawing.Size(292, 19);
      this.m_radioButton1Resign.TabIndex = 8;
      this.m_radioButton1Resign.TabStop = true;
      this.m_radioButton1Resign.Text = "Re-sign traced assembly (the best option)";
      this.m_radioButton1Resign.UseVisualStyleBackColor = true;
      this.m_radioButton1Resign.CheckedChanged += new System.EventHandler(this.m_radioButton1Resign_CheckedChanged);
      // 
      // m_labelResignHelp
      // 
      this.m_labelResignHelp.AutoSize = true;
      this.m_labelResignHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelResignHelp.ForeColor = System.Drawing.SystemColors.ControlText;
      this.m_labelResignHelp.Location = new System.Drawing.Point(32, 152);
      this.m_labelResignHelp.Name = "m_labelResignHelp";
      this.m_labelResignHelp.Size = new System.Drawing.Size(360, 12);
      this.m_labelResignHelp.TabIndex = 9;
      this.m_labelResignHelp.Text = "Re-sign any assembly that has this particular Public Key. Use the original keyfil" +
          "e below.";
      // 
      // m_labelResignKeyFile
      // 
      this.m_labelResignKeyFile.Location = new System.Drawing.Point(31, 172);
      this.m_labelResignKeyFile.Name = "m_labelResignKeyFile";
      this.m_labelResignKeyFile.Size = new System.Drawing.Size(60, 13);
      this.m_labelResignKeyFile.TabIndex = 10;
      this.m_labelResignKeyFile.Text = "Key File:";
      this.m_labelResignKeyFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // m_textBoxResignKeyFile
      // 
      this.m_textBoxResignKeyFile.Location = new System.Drawing.Point(93, 169);
      this.m_textBoxResignKeyFile.Name = "m_textBoxResignKeyFile";
      this.m_textBoxResignKeyFile.Size = new System.Drawing.Size(244, 20);
      this.m_textBoxResignKeyFile.TabIndex = 11;
      this.m_textBoxResignKeyFile.TextChanged += new System.EventHandler(this.m_textBoxResignKeyFile_TextChanged);
      // 
      // m_radioButton2Skip
      // 
      this.m_radioButton2Skip.AutoSize = true;
      this.m_radioButton2Skip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_radioButton2Skip.Location = new System.Drawing.Point(13, 203);
      this.m_radioButton2Skip.Name = "m_radioButton2Skip";
      this.m_radioButton2Skip.Size = new System.Drawing.Size(206, 19);
      this.m_radioButton2Skip.TabIndex = 13;
      this.m_radioButton2Skip.TabStop = true;
      this.m_radioButton2Skip.Text = "Skip traced (the safe option)";
      this.m_radioButton2Skip.UseVisualStyleBackColor = true;
      this.m_radioButton2Skip.CheckedChanged += new System.EventHandler(this.m_radioButton2Skip_CheckedChanged);
      // 
      // m_labelSkipHelp
      // 
      this.m_labelSkipHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelSkipHelp.Location = new System.Drawing.Point(32, 222);
      this.m_labelSkipHelp.Name = "m_labelSkipHelp";
      this.m_labelSkipHelp.Size = new System.Drawing.Size(386, 29);
      this.m_labelSkipHelp.TabIndex = 14;
      this.m_labelSkipHelp.Text = "Do not trace any assembly that has this Public Key. Select this safe option if yo" +
          "u do not have the original keyfile. The app will run fine, but will not trace an" +
          "y methods in these assemblies.";
      // 
      // m_radioButton3Strip
      // 
      this.m_radioButton3Strip.AutoSize = true;
      this.m_radioButton3Strip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_radioButton3Strip.Location = new System.Drawing.Point(13, 266);
      this.m_radioButton3Strip.Name = "m_radioButton3Strip";
      this.m_radioButton3Strip.Size = new System.Drawing.Size(269, 19);
      this.m_radioButton3Strip.TabIndex = 15;
      this.m_radioButton3Strip.TabStop = true;
      this.m_radioButton3Strip.Text = "Strip signature (the dangerous option)";
      this.m_radioButton3Strip.UseVisualStyleBackColor = true;
      this.m_radioButton3Strip.CheckedChanged += new System.EventHandler(this.m_radioButton3Strip_CheckedChanged);
      // 
      // m_labelStripHelp1
      // 
      this.m_labelStripHelp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelStripHelp1.Location = new System.Drawing.Point(32, 285);
      this.m_labelStripHelp1.Name = "m_labelStripHelp1";
      this.m_labelStripHelp1.Size = new System.Drawing.Size(378, 27);
      this.m_labelStripHelp1.TabIndex = 16;
      this.m_labelStripHelp1.Text = "Trace, but do not sign these assembly afterwards. Use this if you do not have the" +
          " original keyfile, and the app does not require these assemblies to be signed.";
      // 
      // m_labelStripHelp2
      // 
      this.m_labelStripHelp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelStripHelp2.ForeColor = System.Drawing.Color.Red;
      this.m_labelStripHelp2.Location = new System.Drawing.Point(32, 312);
      this.m_labelStripHelp2.Name = "m_labelStripHelp2";
      this.m_labelStripHelp2.Size = new System.Drawing.Size(56, 16);
      this.m_labelStripHelp2.TabIndex = 17;
      this.m_labelStripHelp2.Text = "Warning:";
      // 
      // m_labelStripHelp3
      // 
      this.m_labelStripHelp3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelStripHelp3.Location = new System.Drawing.Point(79, 312);
      this.m_labelStripHelp3.Name = "m_labelStripHelp3";
      this.m_labelStripHelp3.Size = new System.Drawing.Size(347, 16);
      this.m_labelStripHelp3.TabIndex = 18;
      this.m_labelStripHelp3.Text = "The app will probably crash if it expects signed assemblies, so proceed at own ri" +
          "sk.";
      // 
      // m_buttonOK
      // 
      this.m_buttonOK.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonOK.Location = new System.Drawing.Point(399, 359);
      this.m_buttonOK.Name = "m_buttonOK";
      this.m_buttonOK.Size = new System.Drawing.Size(67, 23);
      this.m_buttonOK.TabIndex = 0;
      this.m_buttonOK.Text = "OK";
      this.m_buttonOK.UseVisualStyleBackColor = false;
      this.m_buttonOK.Click += new System.EventHandler(this.m_buttonOK_Click);
      // 
      // m_LabelIntroTitle
      // 
      this.m_LabelIntroTitle.AutoSize = true;
      this.m_LabelIntroTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_LabelIntroTitle.Location = new System.Drawing.Point(59, 8);
      this.m_LabelIntroTitle.Name = "m_LabelIntroTitle";
      this.m_LabelIntroTitle.Size = new System.Drawing.Size(159, 15);
      this.m_LabelIntroTitle.TabIndex = 2;
      this.m_LabelIntroTitle.Text = "Signed Assembly Action";
      // 
      // m_buttonCancel
      // 
      this.m_buttonCancel.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_buttonCancel.Location = new System.Drawing.Point(472, 359);
      this.m_buttonCancel.Name = "m_buttonCancel";
      this.m_buttonCancel.Size = new System.Drawing.Size(67, 23);
      this.m_buttonCancel.TabIndex = 1;
      this.m_buttonCancel.Text = "Cancel";
      this.m_buttonCancel.UseVisualStyleBackColor = false;
      this.m_buttonCancel.Click += new System.EventHandler(this.m_buttonCancel_Click);
      // 
      // m_errorProvider
      // 
      this.m_errorProvider.ContainerControl = this;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::EQATEC.SigningUtilities.Properties.Resources.option_resign;
      this.pictureBox1.Location = new System.Drawing.Point(445, 153);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(94, 40);
      this.pictureBox1.TabIndex = 20;
      this.pictureBox1.TabStop = false;
      // 
      // m_pictureBox
      // 
      this.m_pictureBox.Image = global::EQATEC.SigningUtilities.Properties.Resources.certificate;
      this.m_pictureBox.Location = new System.Drawing.Point(5, 8);
      this.m_pictureBox.Name = "m_pictureBox";
      this.m_pictureBox.Size = new System.Drawing.Size(48, 48);
      this.m_pictureBox.TabIndex = 17;
      this.m_pictureBox.TabStop = false;
      // 
      // m_buttonResignBrowse
      // 
      this.m_buttonResignBrowse.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonResignBrowse.Image = global::EQATEC.SigningUtilities.Properties.Resources.folder;
      this.m_buttonResignBrowse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.m_buttonResignBrowse.Location = new System.Drawing.Point(343, 167);
      this.m_buttonResignBrowse.Name = "m_buttonResignBrowse";
      this.m_buttonResignBrowse.Size = new System.Drawing.Size(67, 23);
      this.m_buttonResignBrowse.TabIndex = 12;
      this.m_buttonResignBrowse.Text = "Browse";
      this.m_buttonResignBrowse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.m_buttonResignBrowse.UseVisualStyleBackColor = false;
      this.m_buttonResignBrowse.Click += new System.EventHandler(this.m_buttonResignBrowse_Click);
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = global::EQATEC.SigningUtilities.Properties.Resources.option_skip;
      this.pictureBox2.Location = new System.Drawing.Point(445, 220);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(94, 40);
      this.pictureBox2.TabIndex = 21;
      this.pictureBox2.TabStop = false;
      // 
      // pictureBox3
      // 
      this.pictureBox3.Image = global::EQATEC.SigningUtilities.Properties.Resources.option_strip;
      this.pictureBox3.Location = new System.Drawing.Point(445, 285);
      this.pictureBox3.Name = "pictureBox3";
      this.pictureBox3.Size = new System.Drawing.Size(94, 40);
      this.pictureBox3.TabIndex = 22;
      this.pictureBox3.TabStop = false;
      // 
      // m_labelOriginal
      // 
      this.m_labelOriginal.AutoSize = true;
      this.m_labelOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelOriginal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(115)))), ((int)(((byte)(58)))));
      this.m_labelOriginal.Location = new System.Drawing.Point(442, 133);
      this.m_labelOriginal.Name = "m_labelOriginal";
      this.m_labelOriginal.Size = new System.Drawing.Size(50, 13);
      this.m_labelOriginal.TabIndex = 19;
      this.m_labelOriginal.Text = "Original";
      // 
      // m_labelProfiled
      // 
      this.m_labelProfiled.AutoSize = true;
      this.m_labelProfiled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_labelProfiled.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(125)))), ((int)(((byte)(172)))));
      this.m_labelProfiled.Location = new System.Drawing.Point(500, 133);
      this.m_labelProfiled.Name = "m_labelProfiled";
      this.m_labelProfiled.Size = new System.Drawing.Size(50, 13);
      this.m_labelProfiled.TabIndex = 20;
      this.m_labelProfiled.Text = "Profiled";
      // 
      // SignAssemblyForm
      // 
      this.AcceptButton = this.m_buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
      this.CancelButton = this.m_buttonCancel;
      this.ClientSize = new System.Drawing.Size(553, 394);
      this.Controls.Add(this.m_labelProfiled);
      this.Controls.Add(this.m_labelOriginal);
      this.Controls.Add(this.pictureBox3);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.m_buttonCancel);
      this.Controls.Add(this.m_LabelIntroTitle);
      this.Controls.Add(this.m_pictureBox);
      this.Controls.Add(this.m_buttonOK);
      this.Controls.Add(this.m_labelStripHelp3);
      this.Controls.Add(this.m_labelStripHelp2);
      this.Controls.Add(this.m_labelStripHelp1);
      this.Controls.Add(this.m_radioButton3Strip);
      this.Controls.Add(this.m_labelSkipHelp);
      this.Controls.Add(this.m_radioButton2Skip);
      this.Controls.Add(this.m_buttonResignBrowse);
      this.Controls.Add(this.m_textBoxResignKeyFile);
      this.Controls.Add(this.m_labelResignKeyFile);
      this.Controls.Add(this.m_labelResignHelp);
      this.Controls.Add(this.m_radioButton1Resign);
      this.Controls.Add(this.m_textBoxPublicKey);
      this.Controls.Add(this.m_labelPublicKey);
      this.Controls.Add(this.m_textBoxAssemblyName);
      this.Controls.Add(this.m_labelAssemblyName);
      this.Controls.Add(this.m_labelIntroText);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SignAssemblyForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Signed Assembly Action";
      ((System.ComponentModel.ISupportInitialize)(this.m_errorProvider)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label m_labelIntroText;
    private System.Windows.Forms.Label m_labelAssemblyName;
    private System.Windows.Forms.TextBox m_textBoxAssemblyName;
    private System.Windows.Forms.Label m_labelPublicKey;
    private System.Windows.Forms.TextBox m_textBoxPublicKey;
    private System.Windows.Forms.RadioButton m_radioButton1Resign;
    private System.Windows.Forms.Label m_labelResignHelp;
    private System.Windows.Forms.Label m_labelResignKeyFile;
    private System.Windows.Forms.TextBox m_textBoxResignKeyFile;
    private System.Windows.Forms.Button m_buttonResignBrowse;
    private System.Windows.Forms.RadioButton m_radioButton2Skip;
    private System.Windows.Forms.Label m_labelSkipHelp;
    private System.Windows.Forms.RadioButton m_radioButton3Strip;
    private System.Windows.Forms.Label m_labelStripHelp1;
    private System.Windows.Forms.Label m_labelStripHelp2;
    private System.Windows.Forms.Label m_labelStripHelp3;
    private System.Windows.Forms.Button m_buttonOK;
    private System.Windows.Forms.PictureBox m_pictureBox;
    private System.Windows.Forms.Label m_LabelIntroTitle;
    private System.Windows.Forms.Button m_buttonCancel;
    private System.Windows.Forms.ErrorProvider m_errorProvider;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.PictureBox pictureBox3;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label m_labelProfiled;
    private System.Windows.Forms.Label m_labelOriginal;
  }
}