using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace EQATEC.SigningUtilities.UI
{
  public partial class SignAssemblyForm : Form
  {
    private byte[] m_PublicKey;
    private ISigningSettingRepository m_signFileRepository;
    private SigningSetting m_existingSignFile;
    private DisplayMode m_mode;
    private string m_AssemblyName;

    private SignAssemblyForm()
    {
      InitializeComponent();
    }
    private SignAssemblyForm(
      DisplayMode mode,
      SigningSetting existingSignFile,
      ISigningSettingRepository signfileRepository,
      byte[] publicKey,
      string assemblyPath)
      :this()
    {
      m_mode = mode;
      m_PublicKey = publicKey;
      m_existingSignFile = existingSignFile;
      m_signFileRepository = signfileRepository;
      m_AssemblyName = assemblyPath;

      SetupForm();
      UpdateControls();
    }

    public static SigningSetting EditSignAssemblyAction(
      IWin32Window owner,
      ISigningSettingRepository repository,
      SigningSetting signFile)
    {
      using (SignAssemblyForm form = new SignAssemblyForm(DisplayMode.Edit, signFile, repository, signFile.PublicKey, signFile.UsedBy))
      {
        DialogResult dr = form.ShowDialog(owner);
        if (DialogResult.OK == dr)
        {
          SigningSetting sf = form.ConstructSigningSetting();
          if (sf != null)
            repository.Save(sf);
          return sf;
        }
        return null;
      }
    }

    public static SigningSetting SelectSignAssemblyAction(
      IWin32Window owner,
      ISigningSettingRepository repository,
      byte[] publicKey,
      string assemblyName)
    {
      using (SignAssemblyForm form = new SignAssemblyForm(DisplayMode.Select, null, repository, publicKey, assemblyName))
      {
        DialogResult res = form.ShowDialog(owner);
        if (res != DialogResult.OK)
          return null;
        SigningSetting sf = form.ConstructSigningSetting();
        if (sf != null)
          repository.Save(sf);
        return sf;
      }
    }

    private void SetupForm()
    {
      m_textBoxAssemblyName.Text = m_AssemblyName;
      m_textBoxPublicKey.Text = PublicKeyUtil.ToHexString(m_PublicKey);

      m_radioButton1Resign.Checked = false;
      m_radioButton2Skip.Checked = false;
      m_radioButton3Strip.Checked = false;

      if (m_mode == DisplayMode.Edit)
      {
        if (m_existingSignFile != null)
        {
          switch (m_existingSignFile.SigningAction)
          {
            case SigningAction.Strip: m_radioButton3Strip.Checked = true; break;
            case SigningAction.Sign: 
              m_radioButton1Resign.Checked = true;
              m_textBoxResignKeyFile.Text = m_existingSignFile.PathToKeyContainer;
              break;
            case SigningAction.Skip: m_radioButton2Skip.Checked = true; break;
          }
        }
      }
    }

    public enum DisplayMode
    {
      Edit,
      Select
    }
    private void BrowseForPublicKeyFile()
    {
      try
      {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
          ofd.Title = "Select public key container";
          ofd.CheckFileExists = true;
          ofd.CheckPathExists = true;
          ofd.DefaultExt = ".snk";
          ofd.Filter = "Strong name keys (*.snk)|*.snk|All files|*.*";
          ofd.FilterIndex = 0;
          ofd.Multiselect = false;

          string fileBasis = (m_existingSignFile != null && m_existingSignFile.PathToKeyContainer != null ? m_existingSignFile.PathToKeyContainer : m_AssemblyName);
          if (fileBasis != null)
          {
            if (File.Exists(fileBasis))
              ofd.InitialDirectory = new FileInfo(fileBasis).DirectoryName;
          }

          DialogResult dr = ofd.ShowDialog(this);
          if (dr == DialogResult.OK)
          {
            m_textBoxResignKeyFile.Text = ofd.FileName;
          }
        }
      }
      catch (Exception exc)
      {
        Trace.TraceError("Error browsing for public key files. Message is " + exc.Message);
      }
    }

    private bool CheckSelectionIsValid()
    {
      m_errorProvider.Clear();

      if (m_radioButton1Resign.Checked ||
        m_radioButton2Skip.Checked ||
        m_radioButton3Strip.Checked)
      {

        SigningSetting sf = ConstructSigningSetting();
        switch (sf.SigningAction)
        {
          case SigningAction.Skip:
          case SigningAction.Strip:
            return true;
          case SigningAction.Sign:
            {
              if (string.IsNullOrEmpty(sf.PathToKeyContainer) ||
                !File.Exists(sf.PathToKeyContainer))
              {
                m_errorProvider.SetError(m_textBoxResignKeyFile, "Please specfify a valid key container file");
                return false;
              }

              bool result = PublicKeyUtil.PublicKeyMatch(m_PublicKey, sf.PathToKeyContainer);
              if (!result)
                m_errorProvider.SetError(m_textBoxResignKeyFile, "Key container did not contain a matching public key");
              return result;
            }
          default:
            Debug.Assert(false, "Unknown action specified: " + sf.SigningAction);
            return false;
        }
      }
      else
      {
        m_errorProvider.SetError(m_radioButton1Resign, "Please select an action for the public key");
        m_errorProvider.SetError(m_radioButton2Skip, "Please select an action for the public key");
        m_errorProvider.SetError(m_radioButton3Strip, "Please select an action for the public key");
        return false;
      }
    }

    private SigningSetting ConstructSigningSetting()
    {
      SigningAction action;
      if (m_radioButton1Resign.Checked)
        action = SigningAction.Sign;
      else if (m_radioButton2Skip.Checked)
        action = SigningAction.Skip;
      else if (m_radioButton3Strip.Checked)
        action = SigningAction.Strip;
      else
        return null;

      string pathToSigningSetting = m_textBoxResignKeyFile.Text;
      string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      SigningSetting sf = new SigningSetting(m_PublicKey, action, createdAt, m_AssemblyName, pathToSigningSetting);
      return sf;
    }

    private void UpdateControls()
    {
      // Update key-file entry
      bool wantResign = m_radioButton1Resign.Checked;
      m_labelResignKeyFile.Enabled = wantResign;
      m_textBoxResignKeyFile.Enabled = wantResign;
      m_buttonResignBrowse.Enabled = wantResign;

      // Update OK button
      bool doResign = (m_radioButton1Resign.Checked && m_textBoxResignKeyFile.Text.Trim().Length > 0);
      bool doSkip = m_radioButton2Skip.Checked;
      bool doStrip = m_radioButton3Strip.Checked;
      m_buttonOK.Enabled = doResign || doSkip || doStrip;
    }

    #region Event Handlers
    private void m_radioButton1Resign_CheckedChanged(object sender, EventArgs e)
    {
      UpdateControls();
    }

    private void m_radioButton2Skip_CheckedChanged(object sender, EventArgs e)
    {
      UpdateControls();
    }

    private void m_radioButton3Strip_CheckedChanged(object sender, EventArgs e)
    {
      UpdateControls();
    }

    private void m_buttonResignBrowse_Click(object sender, EventArgs e)
    {
      BrowseForPublicKeyFile();
    }

    private void m_textBoxResignKeyFile_TextChanged(object sender, EventArgs e)
    {
      UpdateControls();
    }

    private void m_buttonOK_Click(object sender, EventArgs e)
    {
      if (CheckSelectionIsValid())
      {
        this.DialogResult = DialogResult.OK;
      }
    }
    private void m_buttonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }

    #endregion

  }
}