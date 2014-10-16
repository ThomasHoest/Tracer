using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.VersionCheckUtilities
{
  public class AdditionalInformationEventArgs : EventArgs
  {
    private readonly IList<KeyValuePair<string, string>> m_additionalInformation;
    /// <summary>
    /// List of additional key/value pairs to send along with the version check
    /// </summary>
    public IList<KeyValuePair<string, string>> AdditionalInformation
    {
      get { return m_additionalInformation; }
    }
    public AdditionalInformationEventArgs()
    {
      m_additionalInformation = new List<KeyValuePair<string, string>>();
    }
  }
}
