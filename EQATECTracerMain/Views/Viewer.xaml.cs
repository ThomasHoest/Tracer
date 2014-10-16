using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Net;
using EQATEC.Tracer.Converters;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;
using EQATEC.Tracer.Windows;
using log4net.Core;

namespace EQATEC.Tracer.Views
{
  /// <summary>
  /// Interaction logic for Viewer.xaml
  /// </summary>
  public partial class ViewerControl : UserControl
  {
    AssemblyViewer mViewHandler;

    public AssemblyViewer ViewHandler
    {
      get { return mViewHandler; }
    }

    Timer mViewerTick;
    const int mMaxLag = 300;
    int mCurrentSetLag;
    PersistentString mLogSavePath;
    PersistentString mLastIP;
    PersistentString mBufferExpanderState;
    TraceItemConverter mTraceConverter;
    ControlTreeItemConverter mTreeItemConverter;

    public ViewerControl()
    {
      InitializeComponent();
      //Setup trace converter and linemanager
      mTraceConverter = FindResource("traceItemConverter") as TraceItemConverter;
      
      if(mTraceConverter != null)
      {
        TraceLineManager lineManager = new TraceLineManager(200);
        lineManager.ParentList = m_lbTrace;
        mTraceConverter.LineManager = lineManager;
        mTraceConverter.TraceLineMenuHandler = TraceLineMenuHandler;
        mTreeItemConverter = FindResource("treeItemConverter") as ControlTreeItemConverter;

        if(mTreeItemConverter != null)
        {
          mTreeItemConverter.CheckBoxClickHandler = TreeCheckBoxHandler;
        }
      }      

      mViewHandler = new AssemblyViewer();
      mViewerTick = new Timer(500);
      mViewerTick.Elapsed += mViewerTick_Elapsed;
      mViewerTick.AutoReset = true;
      mViewerTick.Enabled = true;
      mViewerTick.Start();

      mLastIP = new PersistentString("LastIP", "LocalHost");
      //m_tbIP.Text = mLastIP.Value;
      mViewHandler.IPList = new ObservableCollection<IPinfo>();
      comboBoxLastIPs.DataContext = mViewHandler;
      DeserializeIPlist();

      mBufferExpanderState = new PersistentString("BufferExpanderState", "false");
      m_expBufferMan.IsExpanded = Boolean.Parse(mBufferExpanderState.Value);

      ResetViewer();

      mViewHandler.OnTreeAction += mView_OnTreeAction;
      mViewHandler.OnAssemblyConnectionChanged += mView_OnAssemblyConnectionChanged;
      mViewHandler.OnErrorParsingAssemblyData += mViewHandler_OnErrorParsingAssemblyData;
      m_grMainPanel.DataContext = mViewHandler;
      
      //m_lbTrace.DataContext = mViewHandler;
      //m_lbThreadIDs.DataContext = mViewHandler;
      m_pbBufferUse.Maximum = mMaxLag / 2;
      m_slSetBuffer.Value = mMaxLag / 2;

      m_lbLog4NetLevel.Items.Add(Level.All);
      m_lbLog4NetLevel.Items.Add(Level.Alert);
      m_lbLog4NetLevel.Items.Add(Level.Critical);
      m_lbLog4NetLevel.Items.Add(Level.Debug);
      m_lbLog4NetLevel.Items.Add(Level.Emergency);
      m_lbLog4NetLevel.Items.Add(Level.Error);
      m_lbLog4NetLevel.Items.Add(Level.Fatal);
      m_lbLog4NetLevel.Items.Add(Level.Fine);
      m_lbLog4NetLevel.Items.Add(Level.Finer);
      m_lbLog4NetLevel.Items.Add(Level.Finest);
      m_lbLog4NetLevel.Items.Add(Level.Info);
      m_lbLog4NetLevel.Items.Add(Level.Notice);
      m_lbLog4NetLevel.Items.Add(Level.Off);
      m_lbLog4NetLevel.Items.Add(Level.Severe);
      m_lbLog4NetLevel.Items.Add(Level.Trace);
      m_lbLog4NetLevel.Items.Add(Level.Verbose);
      m_lbLog4NetLevel.Items.Add(Level.Warn);      

      UpdateUI();
    }
    
    public void Close()
    {
      if (mViewHandler != null)
      {
        ExtTraceWindow[] windows = new ExtTraceWindow[mSpawnedThreadWindows.Count]; ;
        mSpawnedThreadWindows.Values.CopyTo(windows, 0);
        for(int i = 0; i<windows.Length; i++)
          windows[i].Close();

        CloseAllParameterWindows();

        mViewHandler.DisconnectFromTarget();
        mViewHandler.Dispose();
      }

      this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
      {
        mBufferExpanderState.Value = m_expBufferMan.IsExpanded.ToString();
      });
    }

    #region Drone Control

    public void SetDroneState()
    {
      SetConnectedState();
      m_cldControlPane.MinWidth = 0;
      m_cldControlPane.Width = new GridLength(0); 
      m_cldViewSplitter.Width = new GridLength(0);
      m_bdrBufferBorder.Visibility = Visibility.Collapsed;
      m_lbThreadIDs.Visibility = Visibility.Collapsed;
      m_btSpawnTrace.Visibility = Visibility.Collapsed;
      mViewHandler.TracePause = false;
      mViewHandler.Drone = true;
      mTraceConverter.Drone = true;
      m_tglbEnableExc.Visibility = Visibility.Collapsed;
      m_lblThreadText.Visibility = Visibility.Collapsed;
      m_sepThreadSeparator.Visibility = Visibility.Collapsed;
      m_sepCountSeparator.Visibility = Visibility.Collapsed;
    }

    #endregion
    
    #region UI update


    public void UpdateUI()
    {
      bool tracePresent = mViewHandler != null && mViewHandler.TraceData.Count != 0;
      bool connected = mViewHandler != null && mViewHandler.Connected;

      m_btUndo.IsEnabled = mViewHandler.AnyUndoActions;
      m_btRedo.IsEnabled = mViewHandler.AnyRedoActions;
      
      m_btConnect.IsEnabled = !connected;      
      m_btDown.IsEnabled = connected;
      m_btUp.IsEnabled = connected;
      m_lbThreadIDs.IsEnabled = tracePresent;
      m_btCloseParam.IsEnabled = mParamWindowList.Count != 0;

      bool spawnEnabled = false;
      if(m_lbThreadIDs.SelectedIndex != -1)
      {
        int id = (int)m_lbThreadIDs.SelectedItem;
        spawnEnabled =  !mSpawnedThreadWindows.ContainsKey(id);
      }
      
      m_btSpawnTrace.IsEnabled = spawnEnabled;

      m_lbThreadIDs.IsEnabled = tracePresent;
      m_btClear.IsEnabled = connected;
      m_btSave.IsEnabled = connected;
      m_lbThreadIDs.IsEnabled = tracePresent;
      m_btPause.IsEnabled = connected && !mViewHandler.TracePause;
      m_btRun.IsEnabled = connected && mViewHandler.TracePause;      
      m_tglbEnableExc.IsEnabled = connected;      
      //m_tglbLog4Net.IsEnabled = connected && mViewHandler.HasLog4Net;
      m_lbLog4NetLevel.IsEnabled = connected && mViewHandler.HasLog4Net;
    }

    public void ResetViewer()
    {
      mViewHandler.DisconnectFromTarget();
      SetConnectionLostState();

      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate()
      {
        try
        {
          m_lbTrace.Clear();
          CloseAllParameterWindows();
          UpdateUI();
        }
        catch
        {
        }
      });

    }

    void SetConnectionLostState()
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate()
      {
        try
        {
          UpdateUI();
          m_tbConnectionStatus.Text = "Not Connected";
          m_tbConnectionStatus.Foreground = Brushes.Black;
        }
        catch
        {
        }
      });
    }

    void SetConnectedState()
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate()
      {
        try
        {         
          m_tglbEnableExc.IsChecked = true;
          //m_tglbLog4Net.IsChecked = true;
          UpdateUI();
          m_tbConnectionStatus.Text = "Connected";
          m_tbConnectionStatus.Foreground = Brushes.Green;
        }
        catch
        {
        }
      });
    }

    private void UpdateControllerToolbar()
    {
      this.Dispatcher.Invoke(DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate
                                                                                            {
        try
        {
          UpdateUI();
        }
        catch
        {
        }
      });
    }

    private void UpdateProgressBar(int time)
    {
      this.Dispatcher.Invoke(DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate
                                                                                            {
        try
        {
          m_pbBufferUse.Value = time;
          m_tbBufferStat.Text = time + "s";
        }
        catch (Exception)
        {
        }
      });
    }

    #endregion

    #region EventHandlers

    void mViewHandler_OnErrorParsingAssemblyData(string errorMessage, Exception ex)
    {
      this.Dispatcher.Invoke(DispatcherPriority.Render, (System.Windows.Forms.MethodInvoker)delegate
      {
        try
        {
          MessageBox.Show(errorMessage, "Error parsing assembly data", MessageBoxButton.OK);
        }
        catch (Exception)
        {
        }
      });
    }
    
    void TraceLineMenuHandler(object sender, RoutedEventArgs e)
    {
      MenuItem item = sender as MenuItem;
      if(item != null)
      {
        TraceLineMenuAction action = item.Tag as TraceLineMenuAction;
  
        if (action == null)
          return;

        if (item.IsCheckable)
          action.Enable = item.IsChecked;
  
        switch (action.Action)
        {
          case TraceLineMenuAction.MenuActionType.EnableCallees:
            AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Menu.EnableCallees");
            mViewHandler.EnableMemberCallees(action.ID, action.Level);
            break;
          case TraceLineMenuAction.MenuActionType.EnableDisable:
            mViewHandler.EnableDisableMember(action.ID, action.Enable);         
            break;
          case TraceLineMenuAction.MenuActionType.DisableAndRemove:
            AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Menu.DisableAndRemove");
            mViewHandler.EnableDisableMember(action.ID, false);
            mViewHandler.FilterMember(action.ID);
            break;
          case TraceLineMenuAction.MenuActionType.FindInTree:
            AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Menu.FindInTree");
            CustomTreeviewItem<ILType> treeItem = mViewHandler.FindControlTreeNode(action.ID);
            if(treeItem != null)
            {
              m_lbControlTree.ScrollIntoView(treeItem);
              m_lbControlTree.SelectedItem = treeItem;
            }
            break;
        }
      }
    }

    void TreeCheckBoxHandler(object sender, RoutedEventArgs e)
    {
      CheckBox chkBox = sender as CheckBox;
      if(chkBox != null)
        mViewHandler.ChangeMemberEnabledState(chkBox.Tag as ILType, (bool)chkBox.IsChecked);
      
    }

    void mViewerTick_Elapsed(object sender, ElapsedEventArgs e)
    {
      int time = mViewHandler.CurrentLag;
      UpdateProgressBar(time);
    }

    void mView_OnAssemblyConnectionChanged(bool connection)
    {
      if (connection)
        SetConnectedState();
      else
        SetConnectionLostState();
    }
    
    private void m_cBox_Focus(object sender, RoutedEventArgs e)
    {
      comboBoxLastIPs.Foreground = Brushes.Black;
    }
    
    private void m_btConnect_Click(object sender, RoutedEventArgs e)
    {
      bool newConnection;
      IPinfo iPinfo;
      
      //If the input in the combobox isn't allowed
      if (ValidateComboBoxIPlist(out iPinfo, out newConnection) == false) 
        return;
      
      m_tbConnectionStatus.Text = "Connecting...";
      m_tbConnectionStatus.Foreground = Brushes.SlateBlue;
      if (!mViewHandler.ConnectToTarget(iPinfo.ip, iPinfo.port))
      {
        Connection conWindow = new Connection(mViewHandler, iPinfo.ip, iPinfo.port);
        conWindow.ShowDialog();
        if (!conWindow.Success)
        {
          m_tbConnectionStatus.Text = "Not Connected";
          m_tbConnectionStatus.Foreground = Brushes.Black;
          UpdateUI();
          return;
        }
      }
      //Only here if connection has been established.
      ModifyIPlist(newConnection, iPinfo);
      SerializeIPlist();

      m_lbControlTree.DataContext = mViewHandler;
      UpdateUI();
    }

    private void m_tbSearch_KeyUp(object sender, KeyEventArgs e)
    {
      SearchClear();
      if(SearchAndMarkLines(m_tbSearch.Text))
        m_tbSearch.Foreground = System.Windows.Media.Brushes.Black;
      else
        m_tbSearch.Foreground = System.Windows.Media.Brushes.Red;
      //m_lbTrace.ScrollSearch(-1);
      
    }

    private void m_btSearch_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Search");

      if (m_tbSearch.Text == "")
        return;
      
      Button hat = sender as Button;
      
      if(hat != null)
      {
        bool upOrDown = bool.Parse(hat.Tag.ToString());

        int ScrollTo = SearchFindNextLine(m_tbSearch.Text, upOrDown);

        if (ScrollTo == -1)
          m_tbSearch.Foreground = System.Windows.Media.Brushes.Red;
        else
        {
          m_tbSearch.Foreground = System.Windows.Media.Brushes.Black;
          m_lbTrace.ScrollSearch(ScrollTo);
        }
      }
    }

    private void m_btClear_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Clear");
      mViewHandler.ClearTrace();
      SearchClear();
      m_lbTrace.Clear();
      m_pvParameterView.Clear();
    }
    
    private void m_btUndo_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Tree.Undo");
      mViewHandler.Undo();
    }

    private void m_btRedo_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Tree.Redo");
      mViewHandler.Redo();
    }

    void mView_OnTreeAction()
    {
      UpdateControllerToolbar();      
    }

    LineHolder mSelectedLine;

    private void m_lbTrace_OnItemSelected(LineHolder line)
    {
      line.Selected = true;
      if (mSelectedLine != null)
        mSelectedLine.Selected = false;
      mSelectedLine = line;

      m_pvParameterView.Update(line);
    }

    List<ParamWindow> mParamWindowList = new List<ParamWindow>();
    bool mSupressParamWindowClose = false;

    private void m_lbTrace_OnItemDoubleClick(LineHolder line)
    { 
      ParamWindow paramWin = new ParamWindow(line);
      paramWin.Closing += paramWin_Closing;
      mParamWindowList.Add(paramWin);
      paramWin.Show();
      m_btCloseParam.IsEnabled = true;
    }

    private void m_btCloseParam_Click(object sender, RoutedEventArgs e)
    {
      CloseAllParameterWindows();
    }

    private void CloseAllParameterWindows()
    {
      mSupressParamWindowClose = true;
      foreach (ParamWindow win in mParamWindowList)
        win.Close();

      mParamWindowList.Clear();
      m_btCloseParam.IsEnabled = false;
      mSupressParamWindowClose = false;
    }

    void paramWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (!mSupressParamWindowClose)
      {
        ParamWindow window = sender as ParamWindow;
        mParamWindowList.Remove(window);
      }
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      m_lbTrace.AdaptSize();
    }

    private void m_btDown_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Down");
      m_lbTrace.ScrollToEnd();
    }

    private void m_btUp_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Up");
      m_lbTrace.ScrollToTop();
    }

    private void m_btSave_Click(object sender, RoutedEventArgs e)
    {
      if (mViewHandler.TraceData.Count == 0)
        return;

      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Save");


      Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
                                           {
                                             Title = "Save trace to disc",
                                             FileName = "Trace.txt"
                                           };

      mLogSavePath = new PersistentString("LogSavePath", string.Empty);

      if (mLogSavePath.Value != string.Empty)
        sfd.InitialDirectory = mLogSavePath.Value;

      try
      {

        if ((bool)sfd.ShowDialog())
        {
          using (StreamWriter sw = new StreamWriter(sfd.OpenFile()))
          {
            for (int i = 0; i < mViewHandler.TraceData.Count; i++)
            {
              sw.WriteLine(mViewHandler.TraceData[i].ToString());
            }
          }

          mLogSavePath.Value = Path.GetDirectoryName(sfd.FileName);
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Error saving file", "Save file error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
      }
    }

    private void m_btPause_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Pause");
      mViewHandler.TracePause = true;
      UpdateUI();
    }

    private void m_btRun_Click(object sender, RoutedEventArgs e)
    {
      AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.Run");
      mViewHandler.TracePause = false;
      UpdateUI();
    }

    private void m_slSetBuffer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (mViewHandler != null)
      {
        mCurrentSetLag = (int)e.NewValue;
        m_pbBufferUse.Maximum = mCurrentSetLag;
        m_tbSliderStat.Text = mCurrentSetLag.ToString() + "s";
        //System.Diagnostics.Trace.WriteLine("Slider value: " + mCurrentSetLag);
        mViewHandler.SetLag(mCurrentSetLag);
      }
    }

    Dictionary<int,ExtTraceWindow> mSpawnedThreadWindows = new Dictionary<int,ExtTraceWindow>();

    private void m_btSpawnTrace_Click(object sender, RoutedEventArgs e)
    {
      if (m_lbThreadIDs.SelectedIndex != -1)
      {
        AnalyticsMonitor.Instance.Monitor.TrackFeature("Viewer.Trace.SpawnTrace");
        int threadId = ((int)m_lbThreadIDs.SelectedItem);
        ExtTraceWindow extTrace = new ExtTraceWindow(threadId, mViewHandler.FunctionDictionary);
        extTrace.Title = string.Format("Trace for thread: 0x{0:x8}", threadId);
        mViewHandler.AddThreadViewHandler(threadId, extTrace.WindowViewHandler);
        extTrace.Closed += new EventHandler(extTrace_Closed);
        mSpawnedThreadWindows[threadId] = extTrace;
        extTrace.Show();
        UpdateUI();
      }
    }

    void extTrace_Closed(object sender, EventArgs e)
    {
      ExtTraceWindow window = sender as ExtTraceWindow;
      mViewHandler.RemoveThreadViewHandler(window.ThreadID);
      mSpawnedThreadWindows.Remove(window.ThreadID);
      UpdateUI();      
    }

    private void m_lbThreadIDs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {      
      if (m_lbThreadIDs.SelectedItem != null)
      {
        int id = ((int)m_lbThreadIDs.SelectedItem);
        m_btSpawnTrace.IsEnabled = !mSpawnedThreadWindows.ContainsKey(id);
      }
    }

    private void m_lbLog4NetLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if(m_lbLog4NetLevel.SelectedIndex != -1 && mViewHandler.Connected)
      {
        Level lvl = m_lbLog4NetLevel.SelectedItem as Level;
        if(lvl != null)
          mViewHandler.SetLog4NetLevel(lvl.Value);
      }
    }

    private void m_tglbEnableExc_Click(object sender, RoutedEventArgs e)
    {
      mViewHandler.TraceCaughtExceptions((bool)m_tglbEnableExc.IsChecked);
    }
    
    private void m_tglbLog4Net_Click(object sender, RoutedEventArgs e)
    {
      //mViewHandler.TraceLog4Net((bool)m_tglbLog4Net.IsChecked);
    }
    
    private void m_tbSearch_GotFocus(object sender, RoutedEventArgs e)
    {
      m_tbSearch.SelectAll();
    }

    private void m_tbSearch_GotMouseCapture(object sender, MouseEventArgs e)
    {
      m_tbSearch.SelectAll();
    }

    #endregion     
  
    #region IPHandling

    /// <summary>
    /// Converts string -> byte[] array.
    /// </summary>
    private static byte[] StrToByteArray(string str)
    {
      UTF8Encoding encoding = new UTF8Encoding();
      return encoding.GetBytes(str);
    }
    /// <summary>
    /// Check if an ip is valid.
    /// </summary>
    private bool IsValidIPAddress(string ip)
    {
      char[] splitter = { '.' };
      IPAddress address;
      //Check if there are 3 "." in input (xxx.xxx.xxx.xxx). TryParse allows strings like "34223"->"0.0.34.232"
      return IPAddress.TryParse(ip, out address) && ip.Split(splitter).Length == 4;
    }

    private void ModifyIPlist(bool newConnection, IPinfo mIPinfo)
    {
      if (newConnection)
      {
        bool exists = false;
        foreach (IPinfo savedIPs in mViewHandler.IPList)
        {
          if (savedIPs.port == mIPinfo.port && savedIPs.ip == mIPinfo.ip)
          {
            mViewHandler.IPList.Move(mViewHandler.IPList.IndexOf(savedIPs), 0);
            exists = true;
            break;
          }
        }
        if (!exists)
        {
          mViewHandler.IPList.Insert(0, mIPinfo);
          comboBoxLastIPs.SelectedIndex = 0; //Select first in list
        }
      }
      else
      {
        //If succesfully reconnecting to a know IP, then it's move to to top of the list.
        mViewHandler.IPList.Move(comboBoxLastIPs.SelectedIndex, 0);
        comboBoxLastIPs.SelectedIndex = 0; //Select first in list
      }
    }

    private void DeserializeIPlist()
    {
      XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<IPinfo>));
      //READ reg Works
      byte[] byteArray;
      byteArray = StrToByteArray(mLastIP.Value);
      using (Stream st = new MemoryStream(byteArray))
      {
        try
        {
          mViewHandler.IPList = ((ObservableCollection<IPinfo>)s.Deserialize(st));
        }
        catch (Exception) //((System.InvalidOperationException)(ex))
        {
          //Console.WriteLine("ErrorInDeserialization");
        }

      }
      //Nothing in reg or error reading it
      if (mViewHandler.IPList.Count == 0) mViewHandler.IPList.Add(new IPinfo("Default", "localhost", 6501));
      comboBoxLastIPs.SelectedIndex = 0;
    }
    private void SerializeIPlist()
    {
      //WRITE reg
      string str;
      using (MemoryStream w = new MemoryStream())
      {
        if (mViewHandler.IPList.Count > 10)
        {
          mViewHandler.IPList.RemoveAt(10);
        }
        XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<IPinfo>));

        XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
        xmlnsEmpty.Add("", "");
        //XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
        //xmlnsEmpty.Add("", "");
        /* Effect
         * "<?xml version=\"1.0\"?>\r\n<ArrayOfIPinfo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <IPinfo ip=\"199.22.2.1\" port=\"39\" name=\"local\" />\r\n  <IPinfo ip=\"lokalhoste\" port=\"22\" name=\"local\" />\r\n</ArrayOfIPinfo>"
         * become
         * "<?xml version=\"1.0\"?>\r\n<ArrayOfIPinfo>\r\n  <IPinfo ip=\"199.22.2.1\" port=\"39\" name=\"local\" />\r\n  <IPinfo ip=\"lokalhoste\" port=\"22\" name=\"local\" />\r\n</ArrayOfIPinfo>"
         * 
         * http://www.devnewsgroups.net/group/microsoft.public.dotnet.framework/topic22066.aspx
         */
        s.Serialize(w, mViewHandler.IPList, xmlnsEmpty);
        byte[] byteArrayWrite = new byte[w.Length];
        w.Position = 0;
        w.Read(byteArrayWrite, 0, (int)w.Length);
        UTF8Encoding enc = new UTF8Encoding();
        str = enc.GetString(byteArrayWrite);
        str = str.Replace("\r\n", ""); //Hack - Writing to registry is somehow i ASCII-format so "\r\n" is a problem. Haven't been able to write in UFT8
      }
      mLastIP.Value = str;
    }

    private bool ValidateComboBoxIPlist(out IPinfo iPinfo, out bool newConnection)
    {
      string input = comboBoxLastIPs.Text;
      newConnection = false;
      iPinfo = new IPinfo("", "", 0);
      //-1 if it's a new input or a selection that has been modified.
      if (comboBoxLastIPs.SelectedIndex != -1)
      {
        iPinfo = mViewHandler.IPList[comboBoxLastIPs.SelectedIndex];
      }
      else
      {
        if (input == "")
        {
          //Select first entry in list (last used or default, if none last used have been found)
          iPinfo = mViewHandler.IPList[0];
          comboBoxLastIPs.SelectedIndex = 0;
        }
        else //New input check if the input is valid
        {
          newConnection = true;
        }
      }

      if (newConnection)
      { //Validate input
        if (IsValidIPAddress(input) || input == "localhost")
        {
          iPinfo = new IPinfo("New connection", input, 6501);
        }
        else
        { //Not matching ip(xxx.xxx.xxx.xxx) or localhost
          comboBoxLastIPs.Foreground = Brushes.Red;
          return false;
        }
      }
      return true;
    }

    #endregion
    
    #region SearchHandling

    public void SearchClear()
    {
      mFoundLastIndex = 0;
    }
    
    int mFoundLastIndex;
    public int FoundLastIndex { set { mFoundLastIndex = value; } }

    public int SearchFindNextLine(string searchString, bool forward)
    {
      int direction;
      ThreadSafeObservableCollection<LineHolder> allLines = mViewHandler.TraceData;
      mFoundLastIndex = m_lbTrace.SelecteLineIndex == -1 ? 0 : m_lbTrace.SelecteLineIndex;

      direction = forward ? 1 : -1;
      
      for (int i = mFoundLastIndex + direction; i < allLines.Count && i > 0; i = i + direction)
      {
        if (UIUtils.SearchAndMarkLineFor(searchString, allLines[i]))
        {
          mFoundLastIndex = i;
          //mParentList.FireItemSelected(LHAll[FoundLastIndex]line, e);
          //LHAll[FoundLastIndex + 1].SelectedBySearch = true;
          return mFoundLastIndex;
        }
      }
      //If no new can be found
      return -1;
    }

    public void ClearSearchSelection()
    {
      mTraceConverter.SearchString = "";

      foreach(LineHolder holder in mViewHandler.TraceData)
      {
        holder.SelectedBySearch = false;
      }
    }

    public bool SearchAndMarkLines(string searchString)
    {
      if (searchString == "")
      {
        ClearSearchSelection();
        return false;
      }

      mTraceConverter.SearchString = searchString;
      bool atLeastOneFound = false;

      foreach (TraceLine TL in mTraceConverter.LineManager.LinesShowed)
      {
        if (UIUtils.SearchAndMarkLineFor(searchString, TL.RawLine)) 
          atLeastOneFound = true;
      }
      return (atLeastOneFound);
    }

    #endregion     
    
  }
}   

