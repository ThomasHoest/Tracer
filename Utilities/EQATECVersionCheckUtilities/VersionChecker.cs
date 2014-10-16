using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;

namespace EQATEC.VersionCheckUtilities
{
  public class VersionChecker : IVersionChecker
  {
    public event EventHandler<VersionCheckEventArgs> VersionCheckCompleted;
    public event EventHandler<AdditionalInformationEventArgs> AddtionalInformationRequested;

    private static string DEFAULT_VERSIONCHECK_URL = "http://tools.eqatec.com/versioncheck.ashx";
    private string m_baseVersionCheckURL = string.Empty;

    public VersionChecker(string versionCheckURL)
    {
      m_baseVersionCheckURL = versionCheckURL;
    }
    public VersionChecker()
      : this(DEFAULT_VERSIONCHECK_URL)
    {
    }

    private string GetVersionCheckUrl(string toolID, string application, Version thisVersion)
    {
      return string.Format("{0}?toolid={1}&version={2}&application={3}{4}", m_baseVersionCheckURL, toolID, thisVersion.ToString(3), application, GetExtraInfoAsParamters());
    }

    private string GetExtraInfoAsParamters()
    {
      StringBuilder sb = new StringBuilder();
      if (AddtionalInformationRequested != null)
      {
        AdditionalInformationEventArgs args = new AdditionalInformationEventArgs();
        AddtionalInformationRequested(this, args);
        foreach (KeyValuePair<string, string> pair in args.AdditionalInformation)
        {
          sb.AppendFormat("&{0}={1}", pair.Key, pair.Value);
        }
      }
      return sb.ToString();
    }

    private struct CheckContext
    {
      public string ToolID;
      public string Application;
      public CheckContext(string toolID, string application)
      {
        ToolID = toolID;
        Application = application;
      }
    }


    public void BeginCheckForNewVersion(string toolID, string application)
    {
      //queuing the versioncheck
      CheckContext context = new CheckContext(toolID, application);
      ThreadPool.QueueUserWorkItem(new WaitCallback(WaitCallbackCheckVersion), context);
    }

    private void WaitCallbackCheckVersion(object context)
    {
      CheckContext callContext = (CheckContext)context;
      VersionCheckResult result = CheckForNewVersion(callContext.ToolID, callContext.Application);
      if (VersionCheckCompleted != null)
      {
        VersionCheckCompleted(this, new VersionCheckEventArgs(result));
      }
    }

    public VersionCheckResult CheckForNewVersion(string toolID, string application)
    {
      try
      {
        Version thisVersion = typeof(VersionChecker).Assembly.GetName().Version;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetVersionCheckUrl(toolID, application, thisVersion));
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string content = string.Empty;
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
          content = reader.ReadToEnd();
        }
        //Trace.WriteLine("Received version check content:" + content);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(content);


        return ParseXmlContent(thisVersion, doc);
      }
      catch (Exception exc)
      {
        Trace.WriteLine("Could not correctly check for a new version of the application. Error is " + exc.Message);
        return new VersionCheckResult(false, null, false, string.Empty, string.Empty);
      }
    }

    private static VersionCheckResult ParseXmlContent(Version thisVersion, XmlDocument doc)
    {
      XmlElement root = doc.SelectSingleNode("/versioninfo") as XmlElement;
      if (root == null)
      {
        Trace.WriteLine("Could not correctly parse the version check structure");
        return new VersionCheckResult(false, null, false, string.Empty, string.Empty);
      }

      string result = root.GetAttribute("status");
      if (result.Equals("failure", StringComparison.OrdinalIgnoreCase))
      {
        XmlNode messageNode = root.SelectSingleNode("message");
        Trace.WriteLine("VersionCheck failed with message: {0}", messageNode == null ? string.Empty : messageNode.InnerText);
        return new VersionCheckResult(false, null, false, string.Empty, string.Empty);
      }
      else if (result.Equals("success", StringComparison.OrdinalIgnoreCase))
      {
        XmlElement toolElement = root.SelectSingleNode("tool") as XmlElement;
        XmlNode versionNode = toolElement.SelectSingleNode("version");
        XmlNode downloadNode = toolElement.SelectSingleNode("downloadurl");
        XmlNode descriptionNode = toolElement.SelectSingleNode("description");

        Version newVersion = new Version(versionNode.InnerText);
        return new VersionCheckResult(true, newVersion, newVersion > thisVersion, downloadNode.InnerText, descriptionNode.InnerText);
      }
      else
      {
        Trace.WriteLine("VersionCheck failed, could not recognize the format");
        return new VersionCheckResult(false, null, false, string.Empty, string.Empty);
      }
    }
  }
}
