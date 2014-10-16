namespace EQATEC.SigningUtilities.UI
{
  partial class SignedAssemblyCacheForm
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
      this.m_LabelIntroTitle = new System.Windows.Forms.Label();
      this.m_buttonClose = new System.Windows.Forms.Button();
      this.m_labelIntroText = new System.Windows.Forms.Label();
      this.m_listView = new System.Windows.Forms.ListView();
      this.m_columnHeaderUnused = new System.Windows.Forms.ColumnHeader();
      this.m_columnHeaderAction = new System.Windows.Forms.ColumnHeader();
      this.m_columnHeaderCreatedAt = new System.Windows.Forms.ColumnHeader();
      this.m_columnHeaderUsedBy = new System.Windows.Forms.ColumnHeader();
      this.m_columnHeaderPublicKey = new System.Windows.Forms.ColumnHeader();
      this.m_columnHeaderKeyFile = new System.Windows.Forms.ColumnHeader();
      this.m_buttonDelete = new System.Windows.Forms.Button();
      this.m_buttonEdit = new System.Windows.Forms.Button();
      this.m_pictureBox = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // m_LabelIntroTitle
      // 
      this.m_LabelIntroTitle.AutoSize = true;
      this.m_LabelIntroTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_LabelIntroTitle.Location = new System.Drawing.Point(66, 12);
      this.m_LabelIntroTitle.Name = "m_LabelIntroTitle";
      this.m_LabelIntroTitle.Size = new System.Drawing.Size(166, 15);
      this.m_LabelIntroTitle.TabIndex = 1;
      this.m_LabelIntroTitle.Text = "Signed Assembly Actions";
      // 
      // m_buttonClose
      // 
      this.m_buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_buttonClose.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonClose.Location = new System.Drawing.Point(820, 326);
      this.m_buttonClose.Name = "m_buttonClose";
      this.m_buttonClose.Size = new System.Drawing.Size(67, 23);
      this.m_buttonClose.TabIndex = 0;
      this.m_buttonClose.Text = "Close";
      this.m_buttonClose.UseVisualStyleBackColor = false;
      this.m_buttonClose.Click += new System.EventHandler(this.m_buttonClose_Click);
      // 
      // m_labelIntroText
      // 
      this.m_labelIntroText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.m_labelIntroText.Location = new System.Drawing.Point(66, 32);
      this.m_labelIntroText.Name = "m_labelIntroText";
      this.m_labelIntroText.Size = new System.Drawing.Size(821, 20);
      this.m_labelIntroText.TabIndex = 2;
      this.m_labelIntroText.Text = "Edit or delete your preferences for dealing with assembly files that are signed w" +
          "ith a particular public key.";
      // 
      // m_listView
      // 
      this.m_listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_columnHeaderUnused,
            this.m_columnHeaderAction,
            this.m_columnHeaderCreatedAt,
            this.m_columnHeaderUsedBy,
            this.m_columnHeaderPublicKey,
            this.m_columnHeaderKeyFile});
      this.m_listView.FullRowSelect = true;
      this.m_listView.HideSelection = false;
      this.m_listView.Location = new System.Drawing.Point(12, 66);
      this.m_listView.Name = "m_listView";
      this.m_listView.Size = new System.Drawing.Size(875, 254);
      this.m_listView.TabIndex = 3;
      this.m_listView.UseCompatibleStateImageBehavior = false;
      this.m_listView.View = System.Windows.Forms.View.Details;
      this.m_listView.ItemActivate += new System.EventHandler(this.m_listView_ItemActivate);
      this.m_listView.SelectedIndexChanged += new System.EventHandler(this.m_listView_SelectedIndexChanged);
      // 
      // m_columnHeaderUnused
      // 
      this.m_columnHeaderUnused.Width = 1;
      // 
      // m_columnHeaderAction
      // 
      this.m_columnHeaderAction.Text = "Action";
      this.m_columnHeaderAction.Width = 65;
      // 
      // m_columnHeaderCreatedAt
      // 
      this.m_columnHeaderCreatedAt.Text = "Entered";
      this.m_columnHeaderCreatedAt.Width = 112;
      // 
      // m_columnHeaderUsedBy
      // 
      this.m_columnHeaderUsedBy.Text = "Used By";
      this.m_columnHeaderUsedBy.Width = 253;
      // 
      // m_columnHeaderPublicKey
      // 
      this.m_columnHeaderPublicKey.Text = "Public Key";
      this.m_columnHeaderPublicKey.Width = 166;
      // 
      // m_columnHeaderKeyFile
      // 
      this.m_columnHeaderKeyFile.Text = "Certificate Keyfile";
      this.m_columnHeaderKeyFile.Width = 274;
      // 
      // m_buttonDelete
      // 
      this.m_buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.m_buttonDelete.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonDelete.Location = new System.Drawing.Point(85, 326);
      this.m_buttonDelete.Name = "m_buttonDelete";
      this.m_buttonDelete.Size = new System.Drawing.Size(60, 23);
      this.m_buttonDelete.TabIndex = 5;
      this.m_buttonDelete.Text = "Delete";
      this.m_buttonDelete.UseVisualStyleBackColor = false;
      this.m_buttonDelete.Click += new System.EventHandler(this.m_buttonDelete_Click);
      // 
      // m_buttonEdit
      // 
      this.m_buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.m_buttonEdit.BackColor = System.Drawing.SystemColors.Control;
      this.m_buttonEdit.Location = new System.Drawing.Point(12, 326);
      this.m_buttonEdit.Name = "m_buttonEdit";
      this.m_buttonEdit.Size = new System.Drawing.Size(60, 23);
      this.m_buttonEdit.TabIndex = 4;
      this.m_buttonEdit.Text = "Edit";
      this.m_buttonEdit.UseVisualStyleBackColor = false;
      this.m_buttonEdit.Click += new System.EventHandler(this.m_buttonEdit_Click);
      // 
      // m_pictureBox
      // 
      this.m_pictureBox.Image = global::EQATEC.SigningUtilities.Properties.Resources.certificate_store;
      this.m_pictureBox.Location = new System.Drawing.Point(12, 12);
      this.m_pictureBox.Name = "m_pictureBox";
      this.m_pictureBox.Size = new System.Drawing.Size(48, 48);
      this.m_pictureBox.TabIndex = 0;
      this.m_pictureBox.TabStop = false;
      // 
      // SignedAssemblyCacheForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
      this.ClientSize = new System.Drawing.Size(899, 359);
      this.Controls.Add(this.m_buttonEdit);
      this.Controls.Add(this.m_buttonDelete);
      this.Controls.Add(this.m_listView);
      this.Controls.Add(this.m_LabelIntroTitle);
      this.Controls.Add(this.m_buttonClose);
      this.Controls.Add(this.m_labelIntroText);
      this.Controls.Add(this.m_pictureBox);
      this.MinimizeBox = false;
      this.Name = "SignedAssemblyCacheForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Signed Assembly Actions";
      ((System.ComponentModel.ISupportInitialize)(this.m_pictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox m_pictureBox;
    private System.Windows.Forms.Label m_LabelIntroTitle;
    private System.Windows.Forms.Button m_buttonClose;
    private System.Windows.Forms.Label m_labelIntroText;
    private System.Windows.Forms.ListView m_listView;
    private System.Windows.Forms.ColumnHeader m_columnHeaderAction;
    private System.Windows.Forms.ColumnHeader m_columnHeaderPublicKey;
    private System.Windows.Forms.ColumnHeader m_columnHeaderKeyFile;
    private System.Windows.Forms.ColumnHeader m_columnHeaderUnused;
    private System.Windows.Forms.Button m_buttonDelete;
    private System.Windows.Forms.Button m_buttonEdit;
    private System.Windows.Forms.ColumnHeader m_columnHeaderCreatedAt;
    private System.Windows.Forms.ColumnHeader m_columnHeaderUsedBy;
  }
}