using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using EQATEC.Tracer.TracerRuntime.Communication;
using System.Xml.Serialization;

namespace EQATEC.Tracer.TracerRuntime
{
  #region Trace to file

  public class LogFactory
  {
    static Dictionary<string, ITracerRuntimeLogger> m_CurrentLogs = new Dictionary<string, ITracerRuntimeLogger>();
    public static ITracerRuntimeLogger GetLog(string name)
    {
      ITracerRuntimeLogger logger;
      if (!m_CurrentLogs.ContainsKey(name))
      {
        logger = new RuntimeLog();
        logger.Name = name;
        logger.SetLevel(TracerRuntimeLoggerLevel.Debug);
        logger.Start();
        m_CurrentLogs[name] = logger;
      }
      else
      {
        logger = m_CurrentLogs[name];
      }
      return logger;
    }
  }

  public class RuntimeLogEmpty : ITracerRuntimeLogger
  {
    #region ITracerRuntimeLogger Members

    public void Error(string message)
    {
    }

    public void Info(string message)
    {
    }

    public void Debug(string message)
    {
    }

    public void Raw(string message)
    {
    }

    public void Stop()
    {
    }

    public void Start()
    {
    }

    public void SetLevel(TracerRuntimeLoggerLevel level)
    {
    }

    public string Name { get; set; }

    #endregion
  }

  internal class FileSender : ISend
  {
    ITracerRuntimeLogger mLog;
    protected XmlSerializer mMessageSerializer = null;
    Dictionary<int, string> mNameDictionary;

    public FileSender(ITracerRuntimeLogger log, Dictionary<int, string> nameDictionary)
    {
      mLog = log;
      mMessageSerializer = new XmlSerializer(typeof(DataMessage));
      mNameDictionary = nameDictionary;
    }

    #region ISend Members

    public bool SendMessage(DataMessage ms)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("[");
      sb.Append(ms.TimeStamp.ToString("HH:mm:ss:ffff"));
      sb.Append("]");
      sb.Append(" - ");
      sb.Append(GetTypeName(ms.Type));
      sb.Append(" - ");
      sb.Append(mNameDictionary[ms.ID]);
      sb.Append("(");
      
      for(int i = 0; i<ms.ValueList.Count; i++)
      {
        sb.Append(ms.ValueList[i]);
        if(i < ms.ValueList.Count - 1)
          sb.Append(", ");
      }
      sb.Append(")");
      mLog.Raw(sb.ToString());

      return true;
    }

    private string GetTypeName(RuntimeActionType runtimeActionType)
    {
      switch (runtimeActionType)
      {
        case RuntimeActionType.TraceData:
          return "Enter";
        case RuntimeActionType.TraceExitData:
          return "Leave";
        case RuntimeActionType.ExceptionData:
        case RuntimeActionType.CaughtException:
          return "Exception";
        case RuntimeActionType.TraceFlushed:
          return "Flushed";
        case RuntimeActionType.Log4NetTrace:
          return "Log4Net";
      }

      return "";
    }

    #endregion
  }

  public class RuntimeLog : ITracerRuntimeLogger, IDisposable
  {
    static object mFileSync = new object();
    public string NewLine = "\r\n";
    Queue<string> mLogQueue = new Queue<string>();
    Semaphore mQueueSemaphore = new Semaphore();
    private bool mKeepLoggin = true;
    private TracerRuntimeLoggerLevel mLevel;
    
    public RuntimeLog()
    {
    }

    public void Stop()
    {
      Dispose();
    }

    public void Start()
    {
      StartWriter();
    }

    private void StartWriter()
    {
      ThreadPool.QueueUserWorkItem((s) =>
                                     {
                                       try
                                       {

                                         while (true)
                                         {
                                           if (mQueueSemaphore.Wait(0))
                                           {
                                             if (!mKeepLoggin)
                                               return;
                                             
                                             using (StreamWriter sw = new StreamWriter(Name, true))
                                             {
                                               while (true)
                                               {
                                                 if (!mKeepLoggin)
                                                   return;

                                                 lock (mLogQueue)
                                                 {
                                                   if (mLogQueue.Count > 0)
                                                   {
                                                     string message = mLogQueue.Dequeue();
                                                     sw.Write(string.Concat(message, NewLine));
                                                     sw.Flush();
                                                   }
                                                 }

                                                 if (!mQueueSemaphore.Wait(200))
                                                   break;
                                               }
                                             }
                                           }
                                         }
                                       }
                                       catch { }
                                     });
    }

    private void AddToLog(string message)
    {
      lock (mLogQueue)
      {
        mLogQueue.Enqueue(string.Concat(DateTime.Now.ToString("HH:mm:ss:fff"), message));
        mQueueSemaphore.Signal();
      }
    }

    public void Raw(string message)
    {
      lock (mLogQueue)
      {
        mLogQueue.Enqueue(message);
        mQueueSemaphore.Signal();
      }        
    }

    public void Error(string message)
    {
      if (mLevel >= TracerRuntimeLoggerLevel.Error)
        AddToLog(string.Concat(" - ERROR - ", message));
    }

    public void Info(string message)
    {
      if (mLevel >= TracerRuntimeLoggerLevel.Info)
        AddToLog(string.Concat(" - INFO - ", message));
    }

    public void Debug(string message)
    {
      if (mLevel >= TracerRuntimeLoggerLevel.Debug)
        AddToLog(string.Concat(" - DEBUG - ", message));
    }

    public void Dispose()
    {
      mKeepLoggin = false;
      mQueueSemaphore.Signal();
    }


    public void SetLevel(TracerRuntimeLoggerLevel level)
    {
      mLevel = level;
    }

    public string Name { get; set; }
  }

  public enum TracerRuntimeLoggerLevel
  {
    None = 0,
    Debug = 1,
    Info = 2,
    Error = 3
  }

  public interface ITracerRuntimeLogger
  {
    void Error(string message);
    void Info(string message);
    void Debug(string message);
    void Raw(string message);
    void SetLevel(TracerRuntimeLoggerLevel level);
    string Name { get; set; }
    void Stop();
    void Start();

  }

  #endregion

  #region Tracer runtime exception

  class TracerRuntimeException : Exception
  {
    private Exception mTracerInnerException;

    public Exception TracerInnerException
    {
      get { return mTracerInnerException; }
      set { mTracerInnerException = value; }
    }

    private string mTracerMessage;

    public string TracerMessage
    {
      get { return mTracerMessage; }
      set { mTracerMessage = value; }
    }

    public TracerRuntimeException(string message, Exception ex)
    {
      mTracerMessage = message;
      mTracerInnerException = ex;
    }
  }

  #endregion


  #region Type Zoo

  class TypeZoo
  {
    int i = 0;
    Int16 i16 = 0;
    Int32 i32 = 0;
    Int64 i64 = 0;
    uint ui = 0;
    UInt16 ui16 = 0;
    UInt32 ui32 = 0;
    UInt64 ui64 = 0;
    ulong ul = 0;
    Single s = 0;
    Double d = 0;
    char c = '0';

    public TypeZoo()
    {
      d = s;
      ui16 = c;
      i = i16;
      i64 = i32;
      ui = ui16;
      ui64 = ui32;
      ul = ui32;
    }

  }

  #endregion
}
