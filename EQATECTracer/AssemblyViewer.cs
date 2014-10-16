using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.TracerRuntime.Communication;

namespace EQATEC.Tracer
{
  public class AssemblyViewer : INotifyPropertyChanged, IDisposable
  {
    #region Members

    AssemblyConnection mConnectedAssembly;
    XmlSerializer mAssemSerializer;    
    List<ViewerAction> mActionList;
    int mActionIndex;
    string mCurrentIP;
    IsolatedStorageFile mUserStore;

    #endregion

    #region Events

    public delegate void AssemblyConnectionChangedHandler(bool connection);
    public event AssemblyConnectionChangedHandler OnAssemblyConnectionChanged;

    public delegate void ViewerErrorHandler(string errorMessage, Exception ex);
    public event ViewerErrorHandler OnErrorParsingAssemblyData;

    public delegate void TreeActionHandler();
    public event TreeActionHandler OnTreeAction;

    #endregion

    #region Properties

    bool mDrone = false;

    public bool Drone
    {
      get { return mDrone; }
      set { mDrone = value; }
    }

    Dictionary<int, MemberContainer> mFunctionDictionary;
    public Dictionary<int, MemberContainer> FunctionDictionary
    {
      get { return mFunctionDictionary; }
      set { mFunctionDictionary = value; }
    }

    Dictionary<string, MemberContainer> mEnabledList;
    public Dictionary<string, MemberContainer> EnabledList
    {
      get { return mEnabledList; }      
    }
      
    AssemblyCollection mAssemblyList;
    public AssemblyCollection AssemblyList
    {
      get
      {
        return mAssemblyList;
      }
    }

    public bool HasLog4Net
    {
      get 
      {
        if (mAssemblyList != null)
        {
          foreach (AssemblyContainer assemCon in mAssemblyList)
          {
            if (assemCon.HasLog4Net)
              return true;
          }
        }
        return false;      
      }
    }

    bool mTracePause = true;

    public bool TracePause
    {
      get { return mTracePause; }
      set { mTracePause = value; }
    }

    private int mCurrentLag;

    public int CurrentLag
    {
      get { return mCurrentLag; }
      set { mCurrentLag = value; }
    }
    
    CustomTreeviewItem<ILType> mTreeRoot;
    public CustomTreeviewItem<ILType> TreeRoot
    {
      get { return mTreeRoot; }
      set { mTreeRoot = value; }
    }
    
    ApplicationCollection mApplication;
    public ApplicationCollection Application
    {
      get { return mApplication; }
      set { mApplication = value; }
    }

    ThreadSafeObservableCollection<int> mThreadsInTrace;
    public ThreadSafeObservableCollection<int> ThreadsInTrace
    {
      get { return mThreadsInTrace; }
      set { mThreadsInTrace = value; }
    }

    //Collection for binding the data to the tree in the view
    ThreadSafeObservableCollection<LineHolder> mTraceData;
    public ThreadSafeObservableCollection<LineHolder> TraceData
    {
      get { return mTraceData; }
      set { mTraceData = value; }
    }
    /// <summary>
    /// Collection for binding last used "ip,port,name" to the combobox in the view
    /// </summary>
    ObservableCollection<IPinfo> mIPList;
    public ObservableCollection<IPinfo> IPList
    {
      get { return mIPList; }
      set { mIPList = value; }
    }
    #endregion

    public AssemblyViewer()
    {
      mConnectedAssembly = new AssemblyConnection();
      mTraceData = new ThreadSafeObservableCollection<LineHolder>();
      mThreadsInTrace = new ThreadSafeObservableCollection<int>();
      mAssemSerializer = new XmlSerializer(typeof(AssemblyIDWrapper));
      mFunctionDictionary = new Dictionary<int, MemberContainer>();
      mEnabledList = new Dictionary<string, MemberContainer>();
      mActionList = new List<ViewerAction>();
      mActionIndex = 0;
      mUserStore = IsolatedStorageFile.GetUserStoreForDomain();  
      mConnectedAssembly.OnTraceData += HandleIncomingData;
      mConnectedAssembly.OnAssemblyConnectionLost += mConnectedAssembly_OnAssemblyConnectionLost;
    }

    #region ViewerAction Mechanics

    private void ClearHistory()
    {
      mEnabledList.Clear();
      mActionList.Clear();
      mFunctionDictionary.Clear();
      mThreadsInTrace.Clear();
      mTraceData.Clear();
      mActionIndex = 0;
      mAssemblyList = null;
      mApplication = null;
      mTreeRoot = null;      
      NotifyPropertyChanged("TreeRoot");
      NotifyPropertyChanged("ThreadsInTrace");
      ClearTrace();
    }

    public void ClearTrace()
    {
      mCallStack.Clear();
      mTraceData.Clear();
    }

    public void TraceCaughtExceptions(bool enable)
    {
      if (enable)
        mConnectedAssembly.EnableTraceCaughtExceptions();
      else
        mConnectedAssembly.DisableTraceCaughtExceptions();
    }

    public void TraceLog4Net(bool enable)
    {
      if (enable)
        mConnectedAssembly.EnableTraceLog4Net();
      else
        mConnectedAssembly.DisableTraceLog4Net();
    }

    public void ClearAll()
    {
      ViewerAction action = new ViewerAction(ViewerAction.ActionType.Disable);
      foreach (MemberContainer member in mEnabledList.Values)
        action.ActionTargets.Add(member);
      PerformNewAction(action);
    }

    public void FilterMember(int id)
    {
      List<LineHolder> linesToRemove = new List<LineHolder>();
      foreach(LineHolder line in mTraceData)
      {
        if (line.Member != null && line.Member.ID == id)
          linesToRemove.Add(line);
      }

      mTraceData.RemoveRange(linesToRemove);
    }

    public MemberContainer GetMemberFromID(int id)
    {
      return mFunctionDictionary[id];
    }

    public void Undo()
    {
      --mActionIndex;

      //foreach (ILType member in mActionList[mActionIndex].ActionTargets)
      //{
      //  member.SetParentCounters(mActionList[mActionIndex].Action != ViewerAction.ActionType.Enable, 1);
      //}

      PerformAction(mActionList[mActionIndex], true);
    }

    public bool AnyUndoActions
    {
      get
      {
        return mActionIndex > 0;
      }
    }

    public void Redo()
    {
      //foreach (ILType member in mActionList[mActionIndex].ActionTargets)
      //{
      //  member.SetParentCounters(mActionList[mActionIndex].Action == ViewerAction.ActionType.Enable, 1);
      //}

      PerformAction(mActionList[mActionIndex++], false);
    }

    public bool AnyRedoActions
    {
      get
      {
        return mActionIndex < mActionList.Count;
      }
    }
        
    private void PerformAction(ViewerAction action, bool undo)
    {
      switch (action.Action)
      {
        case ViewerAction.ActionType.Enable:
          if (undo)
            ChangeLogging(action.ActionTargets, false);
          else
            ChangeLogging(action.ActionTargets, true);
          break;
        case ViewerAction.ActionType.Disable:          
          if (undo)
            ChangeLogging(action.ActionTargets, true);
          else
            ChangeLogging(action.ActionTargets, false);
          break;        
      }

      if (OnTreeAction != null)
        OnTreeAction();      
    }

    public void SetLag(int maxLag)
    {      
      mConnectedAssembly.SetMaxLag(maxLag);
    }

    public void SetLog4NetLevel(int level)
    {
      mConnectedAssembly.SetLog4NetLevel(level);
    }

    private void ChangeLogging(List<ILType> list, bool enable)
    {
      mConnectedAssembly.ChangeLogging(list, enable);
      foreach (ILType type in list)
      {
        MemberContainer member = type as MemberContainer;
        
        if(member != null)
        {
          if (enable)
          {
            member.SetEnabledState(true);
            member.AssemblyState.AddEnabled(member);
            if (!mEnabledList.ContainsKey(member.FullNameWithParams))
              mEnabledList.Add(member.FullNameWithParams, member);
            else
              AnalyticsMonitor.Instance.Monitor.SendLog("Tried to add member which was already present. Name: ", member.FullNameWithParams);
          }
          else
          {
            member.SetEnabledState(false);
            member.AssemblyState.RemovedEnabled(member);
            mEnabledList.Remove(member.FullNameWithParams);
          }
        }
      }
    }

    private void PerformNewAction(ViewerAction action)
    {
      if(mActionIndex < mActionList.Count)
        mActionList.RemoveRange(mActionIndex, mActionList.Count - mActionIndex);
      
      mActionList.Add(action);
      mActionIndex++;
      PerformAction(action, false);
    }
    
    public void EnableMember(MemberContainer member)
    {      
      ViewerAction action = new ViewerAction(member, ViewerAction.ActionType.Enable);
      PerformNewAction(action);
    }

    public void DisableMember(MemberContainer member)
    {
      ViewerAction action = new ViewerAction(member, ViewerAction.ActionType.Disable);
      PerformNewAction(action);
    }

    #endregion

    #region EventHandlers

    void mConnectedAssembly_OnAssemblyConnectionLost()
    {
      if (OnAssemblyConnectionChanged != null)
        OnAssemblyConnectionChanged(false);
    }

    Stack<LineHolder> mCallStack = new Stack<LineHolder>();

    public void HandleIncomingData(TraceEventArgs e)
    {
      mCurrentLag = e.TraceLag;

      if (CheckThread(e))
        return;

      switch (e.Action)
      {
        case RuntimeActionType.AssemblyData:
          if(e.Data != "")
            HandleAssemblyData(e.Data);
          break;
        case RuntimeActionType.Log4NetTrace:
          if (!mTracePause)
            HandleLog4NetTrace(e.TimeStamp, e.ValueList);
          break;
        case RuntimeActionType.TraceData:
          if(!mTracePause)
            HandleEnterTraceData(e.MethodID, e.ThreadID, e.TimeStamp, e.ValueList);
          break;
        case RuntimeActionType.TraceExitData:
          if (!mTracePause)
            HandleReturnTraceData(e.MethodID, e.ThreadID, e.TimeStamp, e.Data);
          break;
        case RuntimeActionType.ExceptionData:
          if (!mTracePause)
            HandleUnhandledExceptionData(e.ValueList, e.ThreadID, e.TimeStamp);
          break;
        case RuntimeActionType.CaughtException:
          if (!mTracePause)
            HandleCaughtExceptionData(e.MethodID, e.ThreadID, e.TimeStamp, e.ValueList);
          break;
        case RuntimeActionType.TraceFlushed:
          if (!mTracePause)
            HandleFlushData(e.TimeStamp, e.Data);
          mCallStack.Clear();
          break;
      }
    }

    private void HandleCaughtExceptionData(int id, int threadID, DateTime time, List<string> parameterData)
    {
      LineHolder line = new LineHolder(id, threadID, time, parameterData);
      line.Type = LineHolder.LineType.CaughtException;
      mCallStack.Clear();
      mTraceData.Add(line);      
    }

    
    void HandleFlushData(DateTime time, string data)
    {
      LineHolder line = new LineHolder(time, data);
      line.Type = LineHolder.LineType.FlushInfo;
      mTraceData.Add(line);
    }

    void HandleEnterTraceData(int id, int threadID, DateTime time, List<string> parameterData)
    {      
      MemberContainer mcon = mFunctionDictionary[id];
      LineHolder line = new LineHolder(id, mcon.ClassAndMethodName, threadID, time, parameterData, mcon.mTn, mcon.mPn);
      line.Member = mcon;
      line.FullName.Text = mcon.FullName;
      line.Type = LineHolder.LineType.EnterTrace;
      line.Level = mCallStack.Count;

      if (mDrone)
        mCallStack.Push(line);
      
      mTraceData.Add(line);
      System.Diagnostics.Trace.WriteLine("Trace received. Id: " + id);
    }

    void HandleLog4NetTrace(DateTime time, List<string> values)
    {
      LineHolder line = new LineHolder(time);
      line.Type = LineHolder.LineType.Log4NetTrace;
      line.TraceLevel = values[0];
      line.Data = new WordHolder(values[1]);
      
      if (mDrone)
        mCallStack.Push(line);

      mTraceData.Add(line);
      //System.Diagnostics.Trace.WriteLine("Trace received. Id: " + id);
    }

    void HandleReturnTraceData(int id, int threadID, DateTime time, string returnData)
    {
      LineHolder line;
      
      if (mDrone)
      {
        if (mCallStack.Count > 0)
          mCallStack.Pop();
      }
      LineHolder prevLine = null;
      if(mTraceData.Count > 0)
        prevLine = mTraceData[mTraceData.Count - 1];

      if (!mDrone && returnData == null && prevLine != null && prevLine.ID == id && prevLine.Type == LineHolder.LineType.EnterTrace)
      {
        mTraceData[mTraceData.Count - 1].Type = LineHolder.LineType.EnterLeaveTrace;
      }
      else
      {
        MemberContainer mcon = mFunctionDictionary[id];        
        line = new LineHolder(id, mcon.ClassAndMethodName, threadID, time, returnData, mcon.ReturnType);
        line.Member = mcon;
        line.FullName.Text = mcon.FullName;
        line.Type = LineHolder.LineType.ReturnTrace;
        line.Level = mCallStack.Count;
        mTraceData.Add(line);
      }

      //System.Diagnostics.Trace.WriteLine("Trace received. Id: " + id);
    }

    void HandleUnhandledExceptionData(List<string> exceptionData, int threadID, DateTime time)
    {
      LineHolder line = new LineHolder(-1, threadID, time, exceptionData);
      line.Type = LineHolder.LineType.Exception;
      mTraceData.Add(line);
    }

    private bool CheckThread(TraceEventArgs e)
    {
      if (e.ThreadID != 0)
      {
        if (!mThreadsInTrace.Contains(e.ThreadID))
        {
          mThreadsInTrace.Add(e.ThreadID);
          NotifyPropertyChanged("ThreadsInTrace");
          
          //if (mThreadsInTrace.Count > 1)
          //  mCallStack.Clear();
        }

        if (mExternalThreadHandlers.ContainsKey(e.ThreadID))
        {
          mExternalThreadHandlers[e.ThreadID].HandleIncomingData(e);
          return true;
        }
      }
      else if (e.Action == RuntimeActionType.TraceFlushed)
      {
        foreach (KeyValuePair<int, AssemblyViewer> pair in mExternalThreadHandlers)
          pair.Value.HandleIncomingData(e);
      }

      return false;
    }

    private object mThreadHandlerListSync = new object();
    Dictionary<int, AssemblyViewer> mExternalThreadHandlers = new Dictionary<int, AssemblyViewer>(); 

    public void AddThreadViewHandler(int threadID, AssemblyViewer handler)
    {
      lock (mThreadHandlerListSync)
      {
        if (!mExternalThreadHandlers.ContainsKey(threadID))
          mExternalThreadHandlers.Add(threadID, handler);
      }
    }
    
    public void RemoveThreadViewHandler(int threadID)
    {
      lock(mThreadHandlerListSync)
        mExternalThreadHandlers.Remove(threadID);
    }

    void ErrorParsingAssemblyData(string error, Exception ex)
    {
      if (OnErrorParsingAssemblyData != null)
        OnErrorParsingAssemblyData(error, ex);
    }


    void HandleAssemblyData(string data)
    {
      AssemblyIDWrapper wrapper = GetSerializedAssembly(data); //Deserialize data

      if (wrapper == null)
        return;
      
      if (mAssemblyList == null || wrapper.ID != mAssemblyList.ID)
      {
        if (mAssemblyList != null)
          ClearHistory();

        mAssemblyList = wrapper.Assemblies;
        mAssemblyList.ID = wrapper.ID;

        BuildNameDictionary(); //Create function dicionary
        CreateCalleeTree(); //Build callee tree   
        SetupApplicationRoot();
        //FloodTracer();
      }
      else
      {
        List<ILType> listToEnable = new List<ILType>();

        foreach (MemberContainer memberCon in mEnabledList.Values)
          listToEnable.Add(memberCon);

        mConnectedAssembly.ChangeLogging(listToEnable, true);                  
      }
      mTracePause = false;
      mConnectedAssembly.EnableTraceCaughtExceptions();
      mConnectedAssembly.EnableTraceLog4Net();
      mConnectedAssembly.SignalHandshakeEnd();

      if (OnAssemblyConnectionChanged != null)
        OnAssemblyConnectionChanged(true);

    }

    void SetupApplicationRoot()
    {
      mApplication = new ApplicationCollection();
      ApplicationContainer appCon = new ApplicationContainer();
      appCon.Name = mCurrentIP;
      appCon.EnabledChanged += HandleEnabledChanged;
      bool? appEnabled = false;
      
      foreach (AssemblyContainer assemCon in mAssemblyList)
        foreach (ModuleContainer modCon in assemCon.Modules)
        {
          modCon.Parent = appCon;
          appCon.Modules.Add(modCon);
          appEnabled |= modCon.Enabled;
        }

      appCon.SetEnabledState(appEnabled);
      //appCon.CalculateMemberCount();
      //appCon.OnCheckBoxAction += new ILType.CheckHandler(treeItem_OnCheckBoxAction);
      mApplication.Add(appCon);

      mTreeRoot = new CustomTreeviewItem<ILType>(mApplication);
      NotifyPropertyChanged("TreeRoot"); //Notify UI that the collection has changed
    }

    public CustomTreeviewItem<ILType> FindControlTreeNode(int id)
    {
      MemberContainer member; 
      return mFunctionDictionary.TryGetValue(id, out member) ? mTreeRoot.ExpandToItem(member) : null;
    }

    public void EnableDisableMember(int id, bool state)
    {
      MemberContainer member; 
      if(mFunctionDictionary.TryGetValue(id, out member))
      {
        ViewerAction action = new ViewerAction(state);

        if (!state)
        {
          if (mEnabledList.ContainsKey(member.FullNameWithParams))
            action.ActionTargets.Add(member);
        }
        else
        {
          if (!mEnabledList.ContainsKey(member.FullNameWithParams))
            action.ActionTargets.Add(member);
        }

        PerformNewAction(action);
      }
    }

    public void EnableMemberCallees(int id, int level)
    {
      MemberContainer member = mFunctionDictionary[id];
      ViewerAction action;

      if (level > 0)
      {
        action = new ViewerAction(ViewerAction.ActionType.Enable);

        if (!mEnabledList.ContainsKey(member.FullNameWithParams))
          action.ActionTargets.Add(member);

        GetCalleeTree(member, level, action.ActionTargets);
        System.Diagnostics.Trace.WriteLine("Enabling callers: " + action.ActionTargets.Count);

        PerformNewAction(action); 
      }
    }

    private void GetIlTypeTree(ref ViewerAction action, ILType member, bool enabled)
    {
      if(member is MemberContainer)
      {
        if(CheckMember(member as MemberContainer, enabled))
          action.ActionTargets.Add(member);
      }
      //else
      //{
      //  member.SetEnabledCounter(enabled);
      //}

      foreach (ILType ilType in member.GetSubItems())
      {
        GetIlTypeTree(ref action, ilType, enabled);
      }
    }

    public void ChangeMemberEnabledState(ILType member, bool enabled)
    {
      ViewerAction action = new ViewerAction(enabled);
      GetIlTypeTree(ref action, member, enabled);
      //member.SetParentCounters(enabled, action.ActionTargets.Count);
   
      //System.Diagnostics.Debug.Assert(action.ActionTargets.Count != 0, "No action targets for action: " + member.ToString());      
      System.Diagnostics.Trace.WriteLine("Enabling/Disabling: " + action.ActionTargets.Count);
      
      if(action.ActionTargets.Count > 0)
        PerformNewAction(action);
    }

    private bool CheckMember(MemberContainer mcon, bool wantedState)
    {
      if (wantedState)
      {
        if (!mEnabledList.ContainsKey(mcon.FullNameWithParams))
          return true;
      }
      else
      {
        if (mEnabledList.ContainsKey(mcon.FullNameWithParams))
          return true;
      }

      return false;
    }

    #endregion

    #region TestViewerData

    public void FloodTracer()
    {
      ThreadPool.QueueUserWorkItem(TraceFlooderThread);
    }

    public void TraceFlooderThread(object state)
    {
      int id = 30; 
      int threadID = 0;       
      List<string> parameterData = new List<string>() { "" };
      DateTime start, end;
      start = DateTime.Now;
      for (int i = 1; i < 50000; i++)
      {
        if (i % 1000 == 0)
        {
          end = DateTime.Now;
          Console.WriteLine("Time: " + (end - start).TotalMilliseconds);
          start = DateTime.Now;
        }
        DateTime time = DateTime.Now;
        HandleEnterTraceData(id, threadID, time, parameterData);
        Thread.Sleep(2);
      }
    }

    #endregion

    #region Structure Mechanics

    private AssemblyIDWrapper GetSerializedAssembly(string data)
    {
      //Unfold, decompress, extract pheeww
      System.Diagnostics.Trace.WriteLine("string length: " + data.Length);
      //Get byte buffer from the string
      byte[] buffer = NoBullshitEncoder.GetBytes(data);
      AssemblyIDWrapper wrapper = null;
      System.Diagnostics.Trace.WriteLine("UU length: " + buffer.Length);
      try
      {
        //Use memory stream to hold ingoing data
        using (MemoryStream inflMemStream = new MemoryStream(buffer))
        {
          inflMemStream.Position = 0;

          using (MemoryStream uuStream = new MemoryStream())
          {
            //First reverse UUencoding
            UUEncoder.UUDecode(inflMemStream, uuStream);
            uuStream.Flush();
            uuStream.Position = 0;
            //The deflate the stuff
            System.Diagnostics.Trace.WriteLine("compressed length: " + uuStream.Length);
            DeflateStream ds = new DeflateStream(uuStream, CompressionMode.Decompress, true);
            //int d = ds.ReadByte();
            //The serializer takes a deflatestream as well. Very convenient

            List<byte> bytes = new List<byte>();
            while (true)
            {
              int d = ds.ReadByte();

              if (d != -1)
                bytes.Add((byte)d);
              else
                break;
            }

            ds.Close();

            byte[] desBuffer;
            desBuffer = bytes.ToArray();

            MemoryStream desStream = new MemoryStream(desBuffer);
            desStream.Position = 0;

            wrapper = mAssemSerializer.Deserialize(desStream) as AssemblyIDWrapper;

          }
        }
      }
      catch (Exception ex)
      {
        ErrorParsingAssemblyData("Error occured while getting control tree structure. (This for example happens with obfuscated assemblies)", ex);      
      }

      return wrapper;      
    }

    private void BuildNameDictionary()
    {
      foreach (AssemblyContainer assemCon in mAssemblyList)
      {
        bool gotSavedSettings = CheckForSavedSettings(assemCon);

        if(assemCon.AssemblyState == null)
          assemCon.AssemblyState = new ViewerAssemblyState();

        assemCon.EnabledChanged += HandleEnabledChanged;

        foreach (ModuleContainer moduleCon in assemCon.Modules)
        {
          moduleCon.EnabledChanged += HandleEnabledChanged;
          moduleCon.Parent = assemCon;
          //moduleCon.OnCheckBoxAction += new MemberContainer.CheckHandler(treeItem_OnCheckBoxAction);
          foreach (TypeContainer typeCon in moduleCon.Types)
          {
            typeCon.EnabledChanged += HandleEnabledChanged;
            NamespaceContainer nameCon = moduleCon.AddToNamespace(typeCon);
            nameCon.EnabledChanged += HandleEnabledChanged;
            if (nameCon.Parent == null)
            {
              //nameCon.OnCheckBoxAction += new ILType.CheckHandler(treeItem_OnCheckBoxAction);
              nameCon.Parent = moduleCon;
            }
            
            typeCon.Parent = nameCon;
            //typeCon.OnCheckBoxAction += new MemberContainer.CheckHandler(treeItem_OnCheckBoxAction);
            foreach (MemberContainer memberCon in typeCon.Members)
            {
              memberCon.Parent = typeCon;
              memberCon.AssemblyState = assemCon.AssemblyState;
              memberCon.EnabledChanged += HandleEnabledChanged;

              if (memberCon.ID != -1)
              {
                //memberCon.OnCheckBoxAction += new MemberContainer.CheckHandler(treeItem_OnCheckBoxAction);
                //memberCon.OnMenuAction += new ILType.MenuHandler(memberCon_OnMenuAction);
                mFunctionDictionary.Add(memberCon.ID, memberCon);
              }
            }
          }

          moduleCon.Namespaces.Sort();
        }

        if (gotSavedSettings)
          ApplySavedState(assemCon);
        
      }

      if (OnTreeAction != null)
        OnTreeAction();
    }

    private void HandleEnabledChanged(object sender, EventArgs e)
    {
      ILType ilType = sender as ILType;
      if(ilType != null)
        ChangeMemberEnabledState(ilType, (bool)ilType.Enabled);
    }

    void GetCalleeTree(MemberContainer mcon, int level, ICollection<ILType> calleeList)
    {
      //Recurively enable all in the callee tree to the defined level
      //System.Diagnostics.Trace.WriteLine("GetCallee tree. Level: " + level);
      if (level-- == 0)
        return;

      foreach (MemberContainer callee in mcon.Callees)
      {
        if(!mEnabledList.ContainsKey(callee.FullNameWithParams) && !calleeList.Contains(callee))
          calleeList.Add(callee);

        GetCalleeTree(callee, level, calleeList);
      }
    }

    private void CreateCalleeTree()
    {
      //Run through all functions and map all callee from ids to membercontainers
      foreach(KeyValuePair<int, MemberContainer> pair in mFunctionDictionary)
      {
        MemberContainer mcon = pair.Value;
        CreateCallees(mcon);
      }
    }

    private void CreateCallees(MemberContainer mcon)
    {
      //Recursively run through all called functions from a given function thereby creating callee tree
      //Run through ids of all callers
      foreach (int i in mcon.mCl)
      {
        //Get called function from dictionary
        MemberContainer call = mFunctionDictionary[i];
        //If called function already has the callee in the list we've already constructed this node
        if (!call.Callees.Contains(mcon))
        {
          //Otherwize add to callees and progress further down the tree
          call.Callees.Add(mcon);
          CreateCallees(call);
        }
      }
    }

    #endregion

    #region Settings in isolatedStorage

    private void ApplySavedState(AssemblyContainer assemCon)
    {
      List<ILType> listToSend = new List<ILType>();
      foreach (KeyValuePair<int, MemberContainer> pair in mFunctionDictionary)
      {
        if (assemCon.AssemblyState.EnabledMembers.ContainsKey(pair.Value.FullNameWithParams))
          listToSend.Add(pair.Value);
      }
      
      if (listToSend.Count > 0)
      {
        System.Diagnostics.Trace.WriteLine("Found saved settings for. " + assemCon.Name + " Count: " + listToSend.Count);
        assemCon.AssemblyState.EnabledMembers.Clear();
        ChangeLogging(listToSend, true);
        //foreach (ILType member in listToSend)
        //{
        //  member.SetParentCounters(true, 1);
        //}
      }
    }

    private bool CheckForSavedSettings(AssemblyContainer assemCon)
    {
      string file = string.Concat(assemCon.Name, ".set");
      ViewerAssemblyState state = DeserializeFromStorage(file);
      if (state != null)
      {
        assemCon.AssemblyState = state;
        DeleteStorageFile(file);
        return true;
      }

      return false;
    }

    private ViewerAssemblyState DeserializeFromStorage(string file)
    {
      ViewerAssemblyState state = null;
      if (mUserStore.GetFileNames(file).Length == 1)
      {
        bool loadError = false;
        using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(file, FileMode.Open, mUserStore))
        {
          try
          {
            BinaryFormatter bw = new BinaryFormatter();
            state = bw.Deserialize(isoStream) as ViewerAssemblyState;
          }
          catch
          {
            loadError = true;
          }
        }

        if(loadError)
          mUserStore.DeleteFile(file);
      }
      return state;
    }

    private void SaveSettingsToStorage()
    {
      if (mAssemblyList != null)
      {
        foreach (AssemblyContainer assemCon in mAssemblyList)
        {
          if (assemCon.AssemblyState.EnabledMembers.Count > 0)
          {
            CreateStorageFile(string.Concat(assemCon.Name, ".set"), assemCon.AssemblyState);
          }
          else
            DeleteStorageFile(string.Concat(assemCon.Name, ".set"));
        }
      }
    }

    private void DeleteStorageFile(string file)
    {
      if (mUserStore.GetFileNames(file).Length == 1)
        mUserStore.DeleteFile(file);
    }

    private void CreateStorageFile(string file, ViewerAssemblyState state)
    {
      using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(file, FileMode.Create, mUserStore))
      {
        BinaryFormatter bw = new BinaryFormatter();
        bw.Serialize(isoStream, state);
      }
    }

    

    #endregion

    #region Connection

    public bool ConnectToTarget(string ip, int port)
    {
      mCurrentIP = ip;
      return mConnectedAssembly.ConnectAndGetAssembly(ip, port);
    }

    public bool Connected
    {
      get
      {
        return mConnectedAssembly.Connected;
      }
    }

    public void DisconnectFromTarget()
    {
      //SerializeAssemblies();
      SaveSettingsToStorage();
      mConnectedAssembly.Disconnect();
      ClearHistory();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }


    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      //SerializeAssemblies();
      SaveSettingsToStorage();
    }

    #endregion
    
  }
}
