using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace EQATEC.VersionCheckUtilities.UI
{
  public partial class DownloadNotificationForm : Form
  {
    public event EventHandler<CancelDownloadNotificationDisplayEventArgs> CancelDownloadNotification;

    private bool NotificationCancelled(Version latestVersion)
    {
      if (CancelDownloadNotification != null)
      {
        CancelDownloadNotificationDisplayEventArgs args = new CancelDownloadNotificationDisplayEventArgs(latestVersion);
        CancelDownloadNotification(this, args);
        return args.Cancel;
      }
      return false;
    }

    public DownloadNotificationForm()
    {
      InitializeComponent();
    }

    private void SetDescription(VersionCheckResult versionCheckResult)
    {
      this.Text = string.Format("Version {0} is now available", versionCheckResult.LatestVersion.ToString(3));
      SetupMessage(versionCheckResult.DownloadMessage);
    }

    private void SetupMessage(string p)
    {
      m_webBrowser.DocumentText = p;
      m_webBrowser.Navigating += new WebBrowserNavigatingEventHandler(m_webBrowser_Navigating);
    }

    public static void ShowDownloadNotification(Form owner, VersionCheckResult versionCheckResult)
    {
      ShowDownloadNotification(owner, versionCheckResult, false);
    }

    public static void ShowDownloadNotification(Form owner, VersionCheckResult versionCheckResult, bool forceShow)
    {
      using (DownloadNotificationForm f = new DownloadNotificationForm())
      {
        if (!forceShow && f.NotificationCancelled(versionCheckResult.LatestVersion))
          return;

        f.SetDescription(versionCheckResult);
        f.Icon = owner.Icon;
        f.ShowDialog(owner);
      }
    }

    private void m_buttonClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void m_webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
    {
      NavigationHelper.OpenWebPage(true, e.Url.AbsoluteUri);
      e.Cancel = true; //should not be navigating inside the webbrowser
    }
  }
}