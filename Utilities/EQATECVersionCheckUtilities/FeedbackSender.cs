using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Xml;
using System.IO;

namespace EQATEC.VersionCheckUtilities
{
  public class FeedbackSender : IFeedbackSender
  {
    private static string FeedbackSenderURL = "http://tools.eqatec.com/feedback.ashx";
    public FeedbackSender()
    {
#if DEBUG
      FeedbackSenderURL = "http://localhost:2046/feedback.ashx";
#endif
    }

    private string CreateFeedbackContent( string application, string toolid, string from, string subject, string message )
    {
      Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
      XmlDocument doc = new XmlDocument();
      XmlNode root = doc.AppendChild(doc.CreateElement("feedback"));
      root.AppendChild(doc.CreateElement("application")).InnerText = application;
      root.AppendChild(doc.CreateElement("toolid")).InnerText = toolid;
      root.AppendChild(doc.CreateElement("version")).InnerText = thisVersion.ToString(3);
      root.AppendChild(doc.CreateElement("localtimeticks")).InnerText = DateTime.Now.Ticks.ToString();
      root.AppendChild(doc.CreateElement("from")).InnerText = from;
      root.AppendChild(doc.CreateElement("subject")).InnerText = subject;
      root.AppendChild(doc.CreateElement("message")).InnerText = message;
      return doc.OuterXml;
    }

    #region IFeedbackSender Members

    public bool SendFeedback( string application, string toolID, string from, string subject, string message )
    {
      if (!ValidInput(application, toolID, from, subject, message))
        return false;

      string feedbackContent = CreateFeedbackContent(application, toolID, from, subject, message);
      bool success = DoSendFeedback(feedbackContent);
      return success;
    }

    private bool DoSendFeedback( string feedbackContent)
    {
      try
      {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(FeedbackSenderURL);
        req.ContentType = "application/x-www-form-urlencoded";
        req.Method = "POST";
        req.KeepAlive = false;
        req.Timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

        byte[] bytes = Encoding.ASCII.GetBytes(feedbackContent); //TODO: should this really be ASCII
        req.ContentLength = bytes.Length;
        using (Stream os = req.GetRequestStream())
        {
          os.Write(bytes, 0, bytes.Length);
        }

        HttpWebResponse response = (HttpWebResponse)req.GetResponse();

        Trace.WriteLine("Feedback was accepted by server with StatusCode " + response.StatusCode);
        return (response.StatusCode == HttpStatusCode.OK);
      }
      catch (Exception exc)
      {
        Trace.WriteLine("Unable to send feedback. Error is " + exc.Message);
        return false;
      }
    }

    private bool ValidInput( string application, string toolID, string from, string subject, string message )
    {
      if (application.Trim().Length == 0 ||
        toolID.Trim().Length == 0 ||
        message.Trim().Length == 0)
      {
        return false;
      }
      return true;
    }

    #endregion
  }

}
