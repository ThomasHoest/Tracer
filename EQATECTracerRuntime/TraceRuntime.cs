using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using EQATEC.Tracer.TracerRuntime.Communication;
using EQATEC.Tracer.TracerRuntime;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System.Reflection;

namespace EQATEC.Tracer.TracerRuntime
{ 
  public class TracerRuntime
  {
    static ITracerRuntimeLogger sLog = LogFactory.GetLog("TracerRuntime.log");
    static ITracerRuntimeLogger sTrace;
    //static int mFunctionID = 0;    
    static Dictionary<int, bool> mFunctionMap = null;
    static Dictionary<int, string> mFunctionNameMap = null;

    static bool mConfigured = false;
    static bool mAllFunctionsRegistered = false;
    static bool mLog4NetInitDone = false;
    static bool mTraceToFile = false;
    static RuntimeTalkerServer mServer = null;
    static ISend mSender;
    static Hierarchy mRepository;

    static AutoResetEvent mClientConnectedEvent = new AutoResetEvent(false);
    static AutoResetEvent mBootHandshakeCompleted = new AutoResetEvent(false);

    static Queue<DataMessage> mMessageQueue = new Queue<DataMessage>();

    static bool mRunSenderThread;
    static Thread mMessageThread;

    static Semaphore mQueueSem = new Semaphore();

    static MessagePool<DataMessage> mMessagePool;
    static bool mConnectionEstablished = false;

    //Todo: remove magic constants
    static int mMaxQueueSize = 30000;
    static int mFullFlushSize = 15000;
    static int mDiscardSize = 0;
    static int mMaxTraceTimeLag = 240;
    static object mDiscardSync = new object();

    static bool mTraceCaughtExceptions = false;
    static bool mTraceLog4Net = false;
    
    #region EventHandlers

    static void mServer_OnClientConnected()
    {
      mClientConnectedEvent.Set();
    }

    static void mServer_OnCommandReceived(ControlCommand cmd)
    {
      DataMessage ms;
      //FileTracer.TraceToFile("Incoming command: " + cmd.mType);
      switch(cmd.mType)
      {
        case ControlCommandType.GetAssemblyData:
          string temp = GetAssemblyStructure();
          ms = mMessagePool.GetMessage();
          ms.Type = RuntimeActionType.AssemblyData;
          ms.Data = temp;
          ms.ThreadID = 0;
          mServer.SendMessage(ms);
          break;      
        case ControlCommandType.Enable:
          foreach(int id in cmd.mIdList)
            EnableLogging(id);
          break;
        case ControlCommandType.Disable:
          foreach (int id in cmd.mIdList)
            DisableLogging(id);
          break;
        case ControlCommandType.Go:
          mBootHandshakeCompleted.Set(); //Two threads can be waiting for the event. So set it twice.
          mBootHandshakeCompleted.Set();
          break;
        case ControlCommandType.LagTime:
          mMaxTraceTimeLag = cmd.mData;
          break;
        case ControlCommandType.CaughtExceptions:
          mTraceCaughtExceptions = cmd.mData == 1;
          break;
        case ControlCommandType.Log4Net:
          mTraceLog4Net = cmd.mData == 1;
          break;
        case ControlCommandType.Log4NetLevel:
          SetLog4NetLevel(cmd.mData);
          break;
        case ControlCommandType.EnableTraceToFile:
          mTraceToFile = true;
          sTrace = LogFactory.GetLog("Trace.log");
          mSender = new FileSender(sTrace, mFunctionNameMap);
          break;
        case ControlCommandType.DisableTraceToFile:
          mSender = mServer;
          mTraceToFile = false;
          sTrace.Stop();
          sTrace = null;
          break;
      }
    }

    #endregion

    #region Setup

    static void FirstCallInit(Assembly callingAssembly)
    {
      sLog.Debug("First call init");

      HookUnhandledExceptionHandler();
      RegisterAllMethods();

      mServer = new RuntimeTalkerServer();

      mMessagePool = new MessagePool<DataMessage>(mMaxQueueSize);

      mServer.OnCommandReceived += new RuntimeTalkerServer.CommandReceivedHandler(mServer_OnCommandReceived);
      mServer.OnClientConnected += new RuntimeTalkerServer.ClientConnectedHandler(mServer_OnClientConnected);
      mServer.OnConnectionClosed += new RuntimeTalker.ConnectionClosedHandler(mServer_OnConnectionClosed);
      mServer.StartServer(6501);

      //RegisterTracerLog4NetAdapter();

      mAllFunctionsRegistered = true;

      if (TraceToFile())
      {
        for (int i = 0; i < mFunctionMap.Count; i++)
          mFunctionMap[i] = true;

        mTraceToFile = true;
        sTrace = LogFactory.GetLog("Trace.log");
        mSender = new FileSender(sTrace, mFunctionNameMap);        
      }
      else
      {
        mSender = mServer;
      }
      
      StartMessageThread();
    }

    static bool TraceToFile()
    {
      return false;
    }

    static bool DoAddLog4NetAppender()
    {
      return false;
    }

    static void SetLog4NetLevel(int levelNumber)
    {
      if (mRepository != null)
      {
        Level level = null;
                
        if (Level.Alert.Value == levelNumber)
          level = Level.Alert;
        else if (Level.All.Value == levelNumber)
          level = Level.All;
        else if (Level.Critical.Value == levelNumber)
          level = Level.Critical;
        else if (Level.Debug.Value == levelNumber)
          level = Level.Debug;
        else if (Level.Emergency.Value == levelNumber)
          level = Level.Emergency;
        else if (Level.Error.Value == levelNumber)
          level = Level.Error;
        else if (Level.Fatal.Value == levelNumber)
          level = Level.Fatal;
        else if (Level.Fine.Value == levelNumber)
          level = Level.Fine;
        else if (Level.Finer.Value == levelNumber)
          level = Level.Finer;
        else if (Level.Finest.Value == levelNumber)
          level = Level.Finest;
        else if (Level.Info.Value == levelNumber)
          level = Level.Info;
        else if (Level.Notice.Value == levelNumber)
          level = Level.Notice;
        else if (Level.Off.Value == levelNumber)
          level = Level.Off;
        else if (Level.Severe.Value == levelNumber)
          level = Level.Severe;
        else if (Level.Trace.Value == levelNumber)
          level = Level.Trace;
        else if (Level.Verbose.Value == levelNumber)
          level = Level.Verbose;
        else if (Level.Warn.Value == levelNumber)
          level = Level.Warn;

        if(level != null)
          mRepository.Root.Level = level;
      }
    }

    static void AddLog4NetAdapter(Hierarchy repository)
    {
      try
      {
        mRepository = repository;
        TracerLog4NetAppender log4NetAppender = new TracerLog4NetAppender();
        log4NetAppender.Name = "TracerAppender";

        //Notify the appender on the configuration changes  
        log4NetAppender.ActivateOptions();

        //and add the appender to the root level  
        //of the logging hierarchy  
        mRepository.Root.AddAppender(log4NetAppender);

        //configure the logging at the root.  
        mRepository.Root.Level = Level.All;

        //mark repository as configured and  
        //notify that is has changed.  
        mRepository.Configured = true;
        mRepository.RaiseConfigurationChanged(EventArgs.Empty);
      }
      catch (Exception ex)
      {
        sLog.Error("Error adding Log4Net adapter. Exception: " + ex.Message);
      }
    }

    static void mServer_OnConnectionClosed()
    {

    }

    static void WaitForInitialConnection()
    {
      if (mClientConnectedEvent.WaitOne(500, false))
        if (mBootHandshakeCompleted.WaitOne(10000, false))
          mConnectionEstablished = true;
    }

    static void StopMessageThread()
    {
      mBootHandshakeCompleted.Set();
      mRunSenderThread = false;
      if (!mMessageThread.Join(1000))
        mMessageThread.Abort();
    }

    static void StartMessageThread()
    {
      mBootHandshakeCompleted.Reset();
      mRunSenderThread = true;
      
      mMessageThread = new Thread(new ThreadStart(TracerRuntime.MessageSenderThread));

      mMessageThread.IsBackground = true;
      mMessageThread.Start();
    }

    private static void RegisterAllMethods()
    {
    }

    private static void RegisterMethod(int id)
    {
      if (mFunctionMap == null)
        mFunctionMap = new Dictionary<int, bool>();

      mFunctionMap.Add(id, false);
    }

    private static void RegisterMethodName(int id, string name)
    {
      if (mFunctionNameMap == null)
        mFunctionNameMap = new Dictionary<int, string>();

      mFunctionNameMap.Add(id, name);
    }

    private static string GetAssemblyStructure()
    {
      return "";
    }

    #endregion

    #region Unhandled Exception Hook

    public static void HookUnhandledExceptionHandler()
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {      
      Exception ex = e.ExceptionObject as Exception;

      //RuntimeLog.Logger.Error("Trace exception: " + ex.Message);
      //RuntimeLog.Logger.Error("Trace exception stack: " + ex.StackTrace);

      DataMessage ms = mMessagePool.GetMessage();
      
      if (ms != null)
      {
        ms.Type = RuntimeActionType.ExceptionData;
        ms.ValueList.Add(ex.Message);
        ms.ValueList.Add(ex.StackTrace);
        while (ex.InnerException != null)
        {
          ex = ex.InnerException;
          ms.ValueList.Add(ex.Message);
          ms.ValueList.Add(ex.StackTrace);
        }

        SafeSendMessage(ms);
      }
    }

    #endregion

    #region Trace

    public static void EnableLogging(int id)
    {
      mFunctionMap[id] = true;
    }

    public static void DisableLogging(int id)
    {
      mFunctionMap[id] = false;
    }

    public static bool DoFunctionTrace(int id)
    {
      try
      {
        if (!mConfigured)
        {
          if (!mLog4NetInitDone)
          {
            if (DoAddLog4NetAppender())
            {
              try
              {
                ILoggerRepository rep = LogManager.GetRepository(Assembly.GetCallingAssembly());

                if (rep != null && rep.Configured)
                {
                  sLog.Debug("Hooking up Log4Net repository " + ((Hierarchy)rep).Name);
                  AddLog4NetAdapter((Hierarchy)rep);
                  mLog4NetInitDone = true;
                }
              }
              catch (Exception ex)
              {
                sLog.Error("Exception thrown while getting Log4Net repository from calling assembly");
              }
            }
            else
              mLog4NetInitDone = true;
          }

          if (!mAllFunctionsRegistered)
          {
            FirstCallInit(Assembly.GetCallingAssembly());
            WaitForInitialConnection();
          }
          mConfigured = mAllFunctionsRegistered && mLog4NetInitDone;
        }
        //RuntimeLog.Logger.Debug("Function id " + id + " returning " + mFunctionMap[id]);
        return mFunctionMap[id];
      }
      catch (Exception ex)
      {
        sLog.Error("Exception thrown. Function id " + id);
        sLog.Error("Exception: " + ex.Message);
        return false;
      }
    }

    public static void TraceException(int id, string message, string stack)
    {
      if (mTraceCaughtExceptions)
      {
        DataMessage ms = mMessagePool.GetMessage();
        if (ms != null)
        {
          ms.Type = RuntimeActionType.CaughtException;
          ms.ID = id;
          ms.ValueList.Add(message);
          ms.ValueList.Add(stack);          
          SafeSendMessage(ms);
        }
      }
    }

    public static void Trace(int id, params object [] p)
    {      
      try
      {
        DataMessage ms = mMessagePool.GetMessage();
        if (ms != null)
        {
          ms.Type = RuntimeActionType.TraceData;
          ms.ID = id;
          foreach (object obj in p)
          {
            try
            {
              if (obj != null)
                ms.ValueList.Add(obj.ToString());
              else
                ms.ValueList.Add("null");
            }
            catch
            {
              ms.ValueList.Add("ERROR CONVERTING PARAMETER VALUE");
            }
          }

          SafeSendMessage(ms);
        }
      }
      catch (Exception ex)
      {
        sLog.Error("Exception: " + ex.Message + " Stack: " + ex.StackTrace);
        throw new TracerRuntimeException("Exception thrown in inner runtime trace", ex);
      }
    }

    public static void TraceLog4Net(DateTime time, string level, string data)
    {
      if (mTraceLog4Net)
      {
        DataMessage ms = mMessagePool.GetMessage();
        if (ms != null)
        {
          ms.Type = RuntimeActionType.Log4NetTrace;
          ms.TimeStamp = time;
          ms.ValueList.Add(level);
          ms.ValueList.Add(data);
          SafeSendMessage(ms);
        }
      }
    }

    public static void Trace(int id)
    {
      DataMessage ms = mMessagePool.GetMessage();
      if (ms != null)
      {
        ms.Type = RuntimeActionType.TraceData;
        ms.ID = id;
        SafeSendMessage(ms);
      }
    }

    public static void TraceExit(int id)
    {
      DataMessage ms = mMessagePool.GetMessage();
      if (ms != null)
      {
        ms.Type = RuntimeActionType.TraceExitData;
        ms.ID = id;
        SafeSendMessage(ms);
      }
    }

    public static void TraceExit(object ret, int id)
    {
      DataMessage ms = mMessagePool.GetMessage();
      if (ms != null)
      {
        ms.Type = RuntimeActionType.TraceExitData;
        ms.ID = id;

        try
        {
          if(ret != null)
            ms.Data = ret.ToString();
        }
        catch
        {
          ms.Data = "ERROR CONVERTING RETURN VALUE";
        }

        SafeSendMessage(ms);
      }
    }

    #endregion

    #region Queue Management


    static DateTime mLastTraceTime;

    public static void SafeSendMessage(DataMessage ms)
    {
      ms.TimeStamp = DateTime.Now;
      ms.ThreadID = Thread.CurrentThread.ManagedThreadId;

      if (mTraceToFile)
      {
        mSender.SendMessage(ms);
        return;
      }

      if (mMessageQueue.Count < mMaxQueueSize)
      {

        lock (mDiscardSync)
        {
          if (mDiscardSize > 0)
          {
            sLog.Info("Flushing messages");
            lock (mMessageQueue)
            {              
              mMessageQueue.Enqueue(CreateFlushMessage(mDiscardSize));
              mQueueSem.Signal();
            }

            mDiscardSize = 0;
          }
        }

        lock (mMessageQueue)
        {
          if (mMessageQueue.Count > 0)
          {
            DataMessage oldie = mMessageQueue.Peek();
            TimeSpan delay = oldie.TimeStamp - ms.TimeStamp;
            if (((int)delay.TotalSeconds) > mMaxTraceTimeLag)
              Flush(true);
          }

          mLastTraceTime = ms.TimeStamp;
          mMessageQueue.Enqueue(ms);
          mQueueSem.Signal();
        }
      }
      else
      {
        lock (mDiscardSync)
        {
          if (mDiscardSize < mFullFlushSize)
          {
            mDiscardSize++;
            ms.ValueList.Clear();
            mMessagePool.PutMessage(ms);
          }
          else
            Flush(true);                     
        }
      }      
    }

    private static void Flush(bool stopThread)
    {
      if(stopThread )
        StopMessageThread();
      lock (mMessageQueue)  
      {
        int temp = mMessageQueue.Count;
        for (int i = 0; i < temp; i++)
        {
          DataMessage ms = mMessageQueue.Dequeue();
          ms.ValueList.Clear();
          mMessagePool.PutMessage(ms);
        }

        mQueueSem.Clear();
        mSender.SendMessage(CreateFlushMessage(temp));
      } 
      if(stopThread)
        StartMessageThread();
    }

    private static DataMessage CreateFlushMessage(int lines)
    {
      DataMessage ms = new DataMessage();
      ms.TimeStamp = DateTime.Now;
      ms.ValueList = new List<string>();
      ms.Type = RuntimeActionType.TraceFlushed;
      ms.Data = "Overflow!! Flushed lines: " + lines;
      return ms;
    }

    private static void MessageSenderThread()
    {
      try
      {
        if (!mConnectionEstablished)
        {
          //RuntimeLog.Logger.Debug("MessageThread waiting");
          mBootHandshakeCompleted.WaitOne();
          mConnectionEstablished = true;
        }

        //RuntimeLog.Logger.Debug("MessageThread started");

        while (mRunSenderThread)
        {
          if (mQueueSem.Wait(0))
          {
            DataMessage ms;

            //RuntimeLog.Logger.Debug("Getting. Count before: " + mQueueSem.Count);            
            TimeSpan lag;
            lock (mMessageQueue)
            {
              ms = mMessageQueue.Dequeue();
              lag = mLastTraceTime - ms.TimeStamp;              
            }                        
            ms.LagTime = (int)lag.TotalSeconds;                        
            lag = DateTime.Now - ms.TimeStamp;
            //RuntimeLog.Logger.Debug("Lag: " + ms.mLagTime);            
            if (((int)lag.TotalSeconds) > mMaxTraceTimeLag)
              Flush(false);
            
            mSender.SendMessage(ms);
            ms.ValueList.Clear();
            mMessagePool.PutMessage(ms);
          }
        }
      }
      catch (ThreadAbortException)
      {
        //If for some reason we had to abort the thread we don't want to rethrow anything
      }
      catch (InvalidOperationException iex)
      {
        //RuntimeLog.Logger.Error("Exception: " + iex.Message + " Stack: " + iex.StackTrace);
        throw new TracerRuntimeException("Messagequeue on dequeue. Aborting", iex);
      }
      catch (Exception ex)
      {
        //RuntimeLog.Logger.Error("Exception: " + ex.Message + " Stack: " + ex.StackTrace);
        throw new TracerRuntimeException("Unknown exception", ex);
      }
    }

    #endregion

    ~TracerRuntime()
    {
      sLog.Debug("Runtime destructor called");
      StopMessageThread();
      mQueueSem.Release();
    }
  }

}
