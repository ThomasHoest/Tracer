using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.VersionCheckUtilities
{
  public class CancelDownloadNotificationDisplayEventArgs : EventArgs
  {
    private Version m_VersionNumber;
    /// <summary>
    /// The version number of the download to show notification for
    /// </summary>
    public Version VersionNumber
    {
      get { return m_VersionNumber; }
    }

    private bool m_Cancel;
    /// <summary>
    /// Whether to cancel the notification to the user
    /// </summary>
    public bool Cancel
    {
      get { return m_Cancel; }
      set { m_Cancel = value; }
    }

    public CancelDownloadNotificationDisplayEventArgs(Version version)
    {
      m_VersionNumber = version;
    }
  }
}
