using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace EQATEC.Tracer.TracerRuntime.Communication
{
  #region RuntimeTalker Base

  public class RuntimeTalker
  {
    protected NetworkStream mSocketStream = null;
    protected Socket mSocket = null;
    protected int mPort;
    public bool Connected {get; protected set;}
    protected UTF8Encoding mEncoder;

    protected XmlSerializer mCommandSerializer = null;
    protected XmlSerializer mMessageSerializer = null;

    public delegate void ConnectionClosedHandler();
    public event ConnectionClosedHandler OnConnectionClosed;

    public RuntimeTalker()
    {
      mCommandSerializer = new XmlSerializer(typeof(ControlCommand));      
      mMessageSerializer = new XmlSerializer(typeof(DataMessage));
      mEncoder = new UTF8Encoding();
    }

    #region Manual Serialization

    protected byte[] StructureToByteArray(object obj)
    {
      int len = Marshal.SizeOf(obj);
      byte[] arr = new byte[len];
      IntPtr ptr = Marshal.AllocHGlobal(len);
      Marshal.StructureToPtr(obj, ptr, true);
      Marshal.Copy(ptr, arr, 0, len);
      Marshal.FreeHGlobal(ptr);
      return arr;
    }

    protected void ByteArrayToStructure(byte[] bytearray, ref object obj)
    {
      int len = Marshal.SizeOf(obj);
      IntPtr i = Marshal.AllocHGlobal(len);
      Marshal.Copy(bytearray, 0, i, len);
      obj = Marshal.PtrToStructure(i, obj.GetType());
      Marshal.FreeHGlobal(i);
    }

    #endregion

    protected virtual void HandleRecievedData(byte[] data, int len)
    {
    }

    protected virtual void OnConnectionClose()
    {
      mSocketStream.Flush();
      mSocketStream.Close();
      Connected = false;

      if (OnConnectionClosed != null)
        OnConnectionClosed();
    }

    public void SendData(string data)
    {
      byte[] buffer = mEncoder.GetBytes(data);
      SendData(buffer);
    }

    public bool SendData(byte[] data)
    {
      if (Connected)
      {
        //Transfer data to send buffer
        byte[] buffer = new byte[data.Length + 1];
        for (int i = 0; i < data.Length; i++)
          buffer[i] = data[i];

        buffer[data.Length] = 255; //Add termination character
        //Dump it all to the stream
        try
        {
          mSocketStream.Write(buffer, 0, buffer.Length);
          mSocketStream.Flush();
        }
        catch (Exception)
        {
          //Console.WriteLine("Exception while writing to socket. " + ex.Message + " Stack : " + ex.StackTrace);
          OnConnectionClose();
          Connected = false;
          return false;
        }

        return true;
      }

      return false;
    }
    
    public void DataListener(object state)
    {
      int length = 1024*1024;
      byte[] buffer = new byte[length];
      byte[] messageBuffer = new byte[length * 5];
      int lengthRead = 0;
      int lastIndex = 0;

      while (Connected)
      { 
        try
        {
          //Read as much as we can from socket
          lengthRead = mSocketStream.Read(buffer, 0, length);
        }
        catch (IOException) //Catch any break in connection and abort
        {
          OnConnectionClose();
          Connected = false;
          return;
        }
        
        if (lengthRead == 0) //Connection closed
        {
          OnConnectionClose();
          Connected = false;
          return;
        }

        //Transfer to read buffer
        for (int i = 0; i < lengthRead; i++)
        {          
          //Read until we get delimeter char
          if (buffer[i] != 255)
          {
            if (lastIndex >= messageBuffer.Length) //If we're about to overflow increase buffer size
            {
              int len = messageBuffer.Length;
              byte [] temp = new byte[len * 2];
              for (int p = 0; p < len; p++)
                temp[p] = messageBuffer[p];

              messageBuffer = temp;
            }

            messageBuffer[lastIndex++] = buffer[i];
          }
          else
          {
            //When a end of message has been received send message up and reset message index
            HandleRecievedData(messageBuffer, lastIndex);
            lastIndex = 0;
          }
        }
      }
    }
  }

  #endregion

  #region RuntimeTalker Client

  public class RuntimeTalkerClient : RuntimeTalker
  {
    public delegate void MessageReceivedHandler(DataMessage cmd);
    public event MessageReceivedHandler OnMessageReceived;

    TcpClient mClientSocket;
    UTF8Encoding mUTF8Enc = new UTF8Encoding();
    MemoryStream mMessageStream = new MemoryStream();
    
    public RuntimeTalkerClient()
    {
    }

    public bool SendCommand(ControlCommand cmd)
    {
      if (Connected)
      {
        using (MemoryStream memStream = new MemoryStream())
        {
          mCommandSerializer.Serialize(memStream, cmd);
          /*BinaryWriter bw = new BinaryWriter(memStream);
          bw.Write(StructureToByteArray(cmd));*/
          memStream.Position = 0;
          byte[] data = new byte[memStream.Length];
          memStream.Read(data, 0, (int)memStream.Length);
          SendData(data);
        }
        return true;
      }
      return false;
    }

    public void Disconnect()
    {
      if (Connected)
      {
        base.OnConnectionClose();
      }
    }
    
    protected override void HandleRecievedData(byte[] data, int len)
    {
      if (OnMessageReceived != null)
      {
        DataMessage ms = null;
        string temp = mUTF8Enc.GetString(data, 0, len);
        mMessageStream.Position = 0;
        mMessageStream.Write(data, 0, len);
        mMessageStream.SetLength(len);
        mMessageStream.Position = 0;
        
        try
        {
          ms = (DataMessage)mMessageSerializer.Deserialize(mMessageStream);
          OnMessageReceived(ms);
        }
        catch (Exception)
        {
        }
        
      }
    }

    /*
    protected override void OnConnectionClose()
    {
      base.OnConnectionClose();      
    }*/

    public bool ClientConnect(int port, string host)
    {      
      try
      {
        mClientSocket = new TcpClient(host, port);
        mSocketStream = mClientSocket.GetStream();
        Connected = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(DataListener));
        return true;
      }
      catch (Exception)
      {
        //Console.WriteLine("Socket connection failed. " + soex.Message);
        return false;
      }
      
     
    }
  }

  #endregion

  #region RuntimeTalker Server

  internal interface ISend
  {
    bool SendMessage(DataMessage ms);
  }

  public class RuntimeTalkerServer : RuntimeTalker, ISend
  {
    public delegate void CommandReceivedHandler(ControlCommand cmd);
    public event CommandReceivedHandler OnCommandReceived;
    
    public delegate void ClientConnectedHandler();
    public event ClientConnectedHandler OnClientConnected;

    private bool mListen;
    TcpListener mListener;

    public RuntimeTalkerServer()
    {      
    }

    public void StartServer(int port)
    {
      mPort = port;
      mListen = true;
      ThreadPool.QueueUserWorkItem(new WaitCallback(Listen));
    }

    public void StopServer()
    {
      mListen = false;
      Connected = false;
      mListener.Stop();
    }

    protected override void OnConnectionClose()
    {
      base.OnConnectionClose();      
      mSocket.Close();
    }

    protected override void HandleRecievedData(byte[] data, int len)
    {
      if (OnCommandReceived != null)
      {
        ControlCommand cmd;// = new ControlCommand();
        using (MemoryStream ms = new MemoryStream(data, 0, len))
        {
          cmd = (ControlCommand)mCommandSerializer.Deserialize(ms);
          /*Object obj = (object)cmd;
          ByteArrayToStructure(data, ref obj);*/
          OnCommandReceived(cmd);
        }
      }
    }

    public bool SendMessage(DataMessage ms)
    {
      if (Connected)
      {
        using (MemoryStream memStream = new MemoryStream())
        {
          mMessageSerializer.Serialize(memStream, ms);
          /*BinaryWriter bw = new BinaryWriter(memStream);
          bw.Write(StructureToByteArray(ms));*/
          memStream.Position = 0;
          byte[] data = new byte[memStream.Length];
          memStream.Read(data, 0, (int)memStream.Length);
          SendData(data);
        }
        return true;
      }
      return false;
    }

    public void Listen(object state)
    {
      System.Net.IPAddress ipAdd = System.Net.IPAddress.Any;
      System.Net.IPEndPoint ipEnd = new IPEndPoint(ipAdd, mPort);
      try
      {
        mListener = new TcpListener(ipEnd);
        mListener.Start();
        //Listen for incoming connections
        while (mListen)
        {
          Socket socket = mListener.AcceptSocket();
          //For now only accept one socket
          if (Connected == false)
          {
            //Create socketstream and spawn worker thread to handle it
            mSocket = socket;
            mSocketStream = new NetworkStream(mSocket);
            Connected = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(DataListener));
            if (OnClientConnected != null)
              OnClientConnected();            
          }
          else
          {
            socket.Close();            
          }
        }
      }
      catch (SocketException)
      {        
      }
    }
  }

  #endregion

  #region MessageTypes

  public enum ControlCommandType
  {
    Undefined,
    Enable,
    Disable,
    GetAssemblyData,
    Go,
    LagTime,
    CaughtExceptions,
    Log4Net,
    Log4NetLevel,
    EnableTraceToFile,
    DisableTraceToFile
  }

  public enum RuntimeActionType
  {
    Undefined,
    AssemblyData,
    TraceData,
    TraceExitData,
    ExceptionData,
    CaughtException,
    TraceFlushed,
    Log4NetTrace
  }

  public class ControlCommand
  {
    public ControlCommandType mType;
    public List<int> mIdList;
    public int mData;
    
    public ControlCommand()
    {
    }

    public ControlCommand(ControlCommandType type)
    {
      mType = type;
    }

    public ControlCommand(ControlCommandType type, int id)
    {
      mType = type;
      AddId(id);
    }

    public void AddId(int id)
    {
      if (mIdList == null)
        mIdList = new List<int>();

      mIdList.Add(id);
    }
  }

  public class DataMessage
  {
    public RuntimeActionType Type;
    public string Data;
    public int ID;
    public List<string> ValueList = new List<string>();
    public int LagTime;
    public DateTime TimeStamp;
    public int ThreadID;
    
    public DataMessage()
    {
    }

    public DataMessage(RuntimeActionType type)
    {
      Type = type;
      Data = "";
      ID = -1;
      ValueList = null;
      TimeStamp = DateTime.MinValue;
      ThreadID = -1;
    }

    public DataMessage(RuntimeActionType type, string data)
    {
      Type = type;
      Data = data;
      ID = -1;
      ValueList = null;
      TimeStamp = DateTime.MinValue;
      ThreadID = -1;
    }

    public DataMessage(RuntimeActionType type, int id)
    {
      Type = type;
      ID = id;
      Data = "";
      ValueList = null;
      TimeStamp = DateTime.MinValue;
      ThreadID = -1;
    }

    public DataMessage(RuntimeActionType type, int id, string data)
    {
      Type = type;
      ID = id;
      Data = data;
      ValueList = null;
      TimeStamp = DateTime.MinValue;
      ThreadID = -1;
    }
  }

  #endregion  
}
