using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace TestApp
{
  internal class Log
  {
    private static ILog m_Log;

    static Log()
    {
      m_Log = LogManager.GetLogger("TestAppLogger");

      using (Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestApp.log4net.xml"))
      {
        if(configStream != null)
          XmlConfigurator.Configure(configStream);
      }

      //XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
    }

    public static ILog Logger
    {
      get
      {
        return m_Log;
      }
    }

    //private static void AddLog4NetAdapter()
    //{
    //  TracerLog4NetAppender log4NetAppender = new TracerLog4NetAppender();
    //  log4NetAppender.Name = "TracerAppender";

    //  //Notify the appender on the configuration changes  
    //  log4NetAppender.ActivateOptions();

    //  //Get the logger repository hierarchy.  
    //  log4net.Repository.Hierarchy.Hierarchy repository =
    //     LogManager.GetRepository() as Hierarchy;

    //  //and add the appender to the root level  
    //  //of the logging hierarchy  
    //  repository.Root.AddAppender(log4NetAppender);

    //  //configure the logging at the root.  
    //  repository.Root.Level = Level.All;

    //  //mark repository as configured and  
    //  //notify that is has changed.  
    //  repository.Configured = true;
    //  repository.RaiseConfigurationChanged(EventArgs.Empty);
    //}


  }


}
