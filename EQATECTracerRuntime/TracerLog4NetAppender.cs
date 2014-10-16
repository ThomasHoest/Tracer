using System;

using System.Collections.Generic;
using System.Text;
using log4net.Appender;
using log4net.Core;

namespace EQATEC.Tracer.TracerRuntime
{
  public class TracerLog4NetAppender : AppenderSkeleton
  {
    protected override void Append(LoggingEvent loggingEvent)
    {
      try
      {
        TracerRuntime.TraceLog4Net(loggingEvent.TimeStamp, loggingEvent.Level.ToString(), loggingEvent.RenderedMessage);
      }
      catch 
      {
      }
    }
  }
}   
