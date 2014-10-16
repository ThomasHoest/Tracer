using System;
using System.Collections.Generic;
using System.Text;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.TracerRuntime.Communication;
using EQATEC.Tracer.TracerRuntime;

namespace EQATEC.Tracer
{
  public class AssemblyConnection
  {
    RuntimeTalkerClient mClient;
    TraceEventArgs mMessageEvent = new TraceEventArgs();

    public bool Connected
    {
      get
      {
        return mClient != null && mClient.Connected;
      }

    }

    #region Events

    public delegate void TraceDataHandler(TraceEventArgs e);
    public event TraceDataHandler OnTraceData;

    public delegate void AssemblyConnectionLost();
    public event AssemblyConnectionLost OnAssemblyConnectionLost;

    #endregion

    public AssemblyConnection()
    {
    }

    #region Assembly commands

    public bool SignalHandshakeEnd()
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Go);
      return mClient.SendCommand(cmd);
    }

    public bool EnableLogging(MemberContainer member)
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Enable, member.ID);
      return mClient.SendCommand(cmd);
    }

    public bool DisableLogging(MemberContainer member)
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Disable, member.ID);
      return mClient.SendCommand(cmd);
    }

    public bool EnableTraceCaughtExceptions()
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.CaughtExceptions);
      cmd.mData = 1;
      return mClient.SendCommand(cmd);
    }

    public bool DisableTraceCaughtExceptions()
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.CaughtExceptions);
      cmd.mData = 0;
      return mClient.SendCommand(cmd);
    }

    public bool SetLog4NetLevel(int level)
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Log4NetLevel);
      cmd.mData = level;
      return mClient.SendCommand(cmd);
    }

    public bool EnableTraceLog4Net()
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Log4Net);
      cmd.mData = 1;
      return mClient.SendCommand(cmd);
    }

    public bool DisableTraceLog4Net()
    {
      ControlCommand cmd = new ControlCommand(ControlCommandType.Log4Net);
      cmd.mData = 0;
      return mClient.SendCommand(cmd);
    }

    public bool ChangeLogging(List<ILType> list, bool mode)
    {
      ControlCommand cmd = new ControlCommand(mode ? ControlCommandType.Enable : ControlCommandType.Disable);

      foreach (ILType type in list)
        cmd.AddId(type.ID);

      return mClient.SendCommand(cmd);
    }

    public bool SetMaxLag(int maxLag)
    {
      if (mClient != null)
      {
        ControlCommand cmd = new ControlCommand(ControlCommandType.LagTime);
        cmd.mData = maxLag;
        return mClient.SendCommand(cmd);
      }
      return false;
    }

    public bool ConnectAndGetAssembly(string ip, int port)
    {
      mClient = new RuntimeTalkerClient();
      if (mClient.ClientConnect(port, ip))
      {
        mClient.OnConnectionClosed += new RuntimeTalker.ConnectionClosedHandler(mClient_OnConnectionClosed);
        mClient.OnMessageReceived += new RuntimeTalkerClient.MessageReceivedHandler(mClient_OnMessageReceived);
        ControlCommand cmd = new ControlCommand(ControlCommandType.GetAssemblyData, 0);
        mClient.SendCommand(cmd);
        return true;
      }
      return false;
    }

    void mClient_OnConnectionClosed()
    {
      if (OnAssemblyConnectionLost != null)
        OnAssemblyConnectionLost();
    }

    public void Disconnect()
    {
      if (mClient != null)
        mClient.Disconnect();
    }

    #endregion

    #region Message recieved handler

    void mClient_OnMessageReceived(DataMessage cmd)
    {
      mMessageEvent.Action = cmd.Type;
      mMessageEvent.ThreadID = cmd.ThreadID;
      mMessageEvent.TimeStamp = cmd.TimeStamp;
      mMessageEvent.MethodID = cmd.ID;
      mMessageEvent.Data = cmd.Data;
      mMessageEvent.ValueList = cmd.ValueList;
      mMessageEvent.TraceLag = cmd.LagTime;
      
      if (OnTraceData != null)
        OnTraceData(mMessageEvent);
    }

    #endregion
  }
}
