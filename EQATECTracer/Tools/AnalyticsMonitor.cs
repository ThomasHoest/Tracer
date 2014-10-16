using System;
using System.Collections.Generic;
using System.Text;
using EQATEC.Analytics.Monitor;

namespace EQATEC.Tracer.Tools
{
  public class AnalyticsMonitor
  {
    #region Singleton

    public static AnalyticsMonitor Instance
    {
      get
      {
        return Nested.instance;
      }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly AnalyticsMonitor instance = new AnalyticsMonitor();
    }

    #endregion

    private AnalyticsMonitor()
    {
      AnalyticsMonitorSettings analyticsSettings = new AnalyticsMonitorSettings("0BEB7984-111F-4034-B46E-27F4F95B127B");
      m_Monitor = AnalyticsMonitorFactory.Create(analyticsSettings);
    }

    IAnalyticsMonitor m_Monitor;

    public IAnalyticsMonitor Monitor
    {
      get{return m_Monitor;}
    }

  }
}
