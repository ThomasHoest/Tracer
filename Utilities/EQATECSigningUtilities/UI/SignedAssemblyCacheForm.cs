using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EQATEC.SigningUtilities.UI
{
  public partial class SignedAssemblyCacheForm : Form
  {
    private ISigningSettingRepository m_signfileRepository;
    private SignedAssemblyCacheForm()
    {
      InitializeComponent();
    }

    public SignedAssemblyCacheForm(ISigningSettingRepository repository)
      : this()
    {
      m_signfileRepository = repository;
      LoadSettingsIntoList();
      UpdateButtonsOnListViewSelectionChanged();
    }

    private void LoadSettingsIntoList()
    {
      m_listView.BeginUpdate();
      try
      {
        m_listView.Items.Clear();
        IList<SigningSetting> signFiles = m_signfileRepository.GetAllSettings();
        foreach (SigningSetting sf in signFiles)
        {
          ListViewItem item = m_listView.Items.Add(new ListViewItem());
          SetSigningSettingOnListViewItem(sf, item);
        }
        m_listView.Sort();
      }
      finally
      {
        m_listView.EndUpdate();
        m_listView.Refresh();
      }
    }

    private void SetSigningSettingOnListViewItem(SigningSetting setting, ListViewItem listviewItem)
    {
      listviewItem.Tag = setting;
      if (listviewItem.SubItems.Count != 6)
      {
        //adding empty placeholders
        listviewItem.SubItems.AddRange(new string[]{"","","","",""});
      }
      string action = String.Empty;
      switch (setting.SigningAction)
      {
        case SigningAction.Sign: action = "Re-sign"; break;
        case SigningAction.Skip: action = "Skip"; break;
        case SigningAction.Strip: action = "Strip"; break;
      }
      listviewItem.SubItems[1].Text = action;
      listviewItem.SubItems[2].Text = setting.CreatedAt;
      listviewItem.SubItems[3].Text = setting.UsedBy;
      listviewItem.SubItems[4].Text = PublicKeyUtil.ToHexString(setting.PublicKey);
      listviewItem.SubItems[5].Text = setting.PathToKeyContainer;

      switch (setting.SigningAction)
      {
        case SigningAction.Sign: listviewItem.BackColor = Color.FromArgb(0xf0fff0); break;
        case SigningAction.Skip: listviewItem.BackColor = Color.FromArgb(0xfffacd); break;
        case SigningAction.Strip: listviewItem.BackColor = Color.FromArgb(0xffe4e1); break;
      }
    }

    private void EditSignFile()
    {
      if (m_listView.SelectedIndices.Count == 1)
      {
        int index = m_listView.SelectedIndices[0];
        ListViewItem listviewItem = m_listView.Items[index];
        SigningSetting sf = listviewItem.Tag as SigningSetting;
        if (sf != null)
        {
          //show edit format
          SigningSetting newSetting = SignAssemblyForm.EditSignAssemblyAction(this, m_signfileRepository, sf);
          if (newSetting != null)
          {
            SetSigningSettingOnListViewItem(newSetting, listviewItem);
          }
        }
      }
    }
    private void DeleteSignFile()
    {
      if (m_listView.SelectedIndices.Count == 1)
      {
        int index = m_listView.SelectedIndices[0];
        ListViewItem listviewItem = m_listView.Items[index];
        SigningSetting sf = listviewItem.Tag as SigningSetting;

        if (DialogResult.OK == MessageBox.Show(this, "Do you want to delete the current action\nfor this particular public key?", "Delete Action", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
        {
          m_signfileRepository.Remove(sf);
          m_listView.Items.Remove(listviewItem);
          m_listView.Refresh();
        }
      }
    }

    private void UpdateButtonsOnListViewSelectionChanged()
    {
      bool somethingSelected = m_listView.SelectedIndices.Count == 1;
      m_buttonDelete.Enabled = somethingSelected;
      m_buttonEdit.Enabled = somethingSelected;
    }

    #region Event Handlers
    private void m_buttonClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void m_buttonEdit_Click(object sender, EventArgs e)
    {
      EditSignFile();
    }

    private void m_buttonDelete_Click(object sender, EventArgs e)
    {
      DeleteSignFile();
    }

    private void m_listView_ItemActivate(object sender, EventArgs e)
    {
      EditSignFile();
    }
    private void m_listView_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateButtonsOnListViewSelectionChanged();
    }
    #endregion
  }
}