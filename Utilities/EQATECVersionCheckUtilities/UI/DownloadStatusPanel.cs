using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;


namespace EQATEC.VersionCheckUtilities.UI
{
  public partial class DownloadStatusPanel : UserControl
  {
    private VersionCheckResult m_lastVersionCheckResult;
    private IVersionChecker m_checker;
    public DownloadStatusPanel()
    {
      InitializeComponent();
    }

    public void Initialize(IVersionChecker checker)
    {
      m_checker = checker;
    }

    public void StartVersionCheck(string toolID, string application)
    {
      if (m_checker != null)
      {
        m_checker.VersionCheckCompleted += new EventHandler<VersionCheckEventArgs>(checker_VersionCheckCompleted);
        m_checker.BeginCheckForNewVersion(toolID, application);
      }
    }

    void checker_VersionCheckCompleted(object sender, VersionCheckEventArgs e)
    {
      m_lastVersionCheckResult = e.VersionCheckResult;
      DoSetVersionCheckResult();
    }

    private void DoSetVersionCheckResult()
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new EventHandler(delegate(object sender, EventArgs e)
        {
          DoSetVersionCheckResult();
        }), this, EventArgs.Empty);
        return;
      }

      if (m_lastVersionCheckResult.CheckSuccessful && m_lastVersionCheckResult.ShouldUpdate)
      {
        m_linkLabelDownload.Text = string.Format("A newer version ({0}) is available", m_lastVersionCheckResult.LatestVersion.ToString(3));
        this.Visible = true;
        Form parentForm = FindForm();
        if (parentForm != null)
        {
          //displays this the first time
          DownloadNotificationForm.ShowDownloadNotification(parentForm, m_lastVersionCheckResult);
        }
      }
    }

    public bool ShouldBeShown
    {
      get { return m_lastVersionCheckResult != null && m_lastVersionCheckResult.ShouldUpdate; }
    }

    private void HandleDownloadClicked()
    {
      if (m_lastVersionCheckResult != null)
      {
        if (m_lastVersionCheckResult.CheckSuccessful && m_lastVersionCheckResult.ShouldUpdate)
        {
          Form parentForm = FindForm();
          if (parentForm != null)
          {
            DownloadNotificationForm.ShowDownloadNotification(parentForm, m_lastVersionCheckResult, true);
          }
        }
      }
    } 

    private void m_linkLabelDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      HandleDownloadClicked();
    }

    private void m_pictureBoxDownload_Click(object sender, EventArgs e)
    {
      HandleDownloadClicked();
    }

    public delegate void AdditionalInformationCallback(IList<KeyValuePair<string,string>> additionInformationCollector);
  }
}
