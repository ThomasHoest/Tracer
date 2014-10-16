using System;
using System.Collections.Generic;
using System.Text;
using EQATEC.Tracer.TracerRuntime.Communication;
using EQATEC.Tracer.TracerRuntime;

namespace EQATEC.Tracer.Tools
{
  public class InstrumentationActionEventArgs : EventArgs
  {
    public enum InstrumentationAction
    {
      AssemblyInstrumentationStarted,
      AssemblyInstrumentationEnded,
      InstrumentationFinished
    }

    InstrumentationAction mAction;

    public InstrumentationAction Action
    {
      get { return mAction; }
      set { mAction = value; }
    }

    string mAssemblyName;

    public string AssemblyName
    {
      get { return mAssemblyName; }
      set { mAssemblyName = value; }
    }

    int mMethodCounter;

    public int MethodCounter
    {
      get { return mMethodCounter; }
      set { mMethodCounter = value; }
    }

    public InstrumentationActionEventArgs(InstrumentationAction action, string name, int methodCounter)
    {
      mAction = action;
      mAssemblyName = name;
      mMethodCounter = methodCounter;
    }

    public InstrumentationActionEventArgs(InstrumentationAction action, string name)
    {
      mAction = action;
      mAssemblyName = name;
    }

    public InstrumentationActionEventArgs(InstrumentationAction action, int methodCounter)
    {
      mAction = action;
      mMethodCounter = methodCounter;
    }
  }

  public class TraceEventArgs : EventArgs
  {
    RuntimeActionType mAction;
    public RuntimeActionType Action
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mAction; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mAction = value; }
    }

    string mData;
    public string Data
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mData; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mData = value; }
    }

    int mMethodID;
    public int MethodID
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mMethodID; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mMethodID = value; }
    }

    int mThreadID;
    public int ThreadID
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mThreadID; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mThreadID = value; }
    }

    DateTime mTimeStamp;
    public DateTime TimeStamp
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mTimeStamp; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mTimeStamp = value; }
    }

    int mTraceLag;
    public int TraceLag
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mTraceLag; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mTraceLag = value; }
    }

    List<string> mValueList;
    public List<string> ValueList
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mValueList; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mValueList = value; }
    }

    public TraceEventArgs(RuntimeActionType action, DateTime timeStamp, int threadID, List<string> values)
    {
      mAction = action;
      mTimeStamp = timeStamp;
      mThreadID = threadID;
      mValueList = values;
    }

    public TraceEventArgs(RuntimeActionType action)
    {
      mAction = action;     
    }

    public TraceEventArgs()
    {     
    }
  }
}
