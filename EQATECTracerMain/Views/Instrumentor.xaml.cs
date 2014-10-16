using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Threading;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.Views;
using EQATEC.SigningUtilities.UI;
using EQATEC.SigningUtilities;
using System.Windows.Interop;
using System.Windows.Threading;
using EQATEC.Tracer.Windows;

namespace EQATEC.Tracer.Views
{
  /// <summary>
  /// Interaction logic for Instrumentor.xaml
  /// </summary>
  public partial class InstrumentorControl : UserControl, System.Windows.Forms.IWin32Window
  {
    DirectoryInfo mTraceDir;
    InstrumentationHandler mInstrumentHandler;
    ILType mSelectedItem;
    DirectoryInfo mTargetInfo = null;
    ISigningSettingRepository mSignfileRepository;
    PersistentString mStartLocation;
    bool mUpdateFiles = true;
    bool mInstrumentationSuccessfull;

    #region Events

    public delegate void RunApplicationHandler();
    public event RunApplicationHandler OnApplicationRun;

    public delegate void InstrumentAppHandler();
    public event InstrumentAppHandler OnAppInstrumented;

    #endregion

    public InstrumentorControl()
    {
      InitializeComponent();
      
      m_btRun.IsEnabled = false;

      string demoAppFolder = ReleaseInfo.GetDemoAppFolder();
      mStartLocation = new PersistentString("InstrumentorStartFolder", demoAppFolder);

      mInstrumentHandler = new InstrumentationHandler(HandleAssemblySigningCallback);
      mInstrumentHandler.OnInstrumentationAction += new InstrumentationHandler.InstrumentationActionHandler(mView_OnInstrumentationAction);
      m_tbPath.Text = mStartLocation.Value;
    }

    public void SetSigningSettingsRepository( ISigningSettingRepository repository )
    {
      mSignfileRepository = repository;
    }

    public void Close()
    {
      this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
      {
        if (Directory.Exists(m_tbPath.Text))
          mStartLocation.Value = m_tbPath.Text;
      });
    }

    #region File handling

    private void CopyDir(DirectoryInfo dir, DirectoryInfo target)
    {
      foreach (FileInfo fi in dir.GetFiles())
      {
        if (!File.Exists(System.IO.Path.Combine(target.FullName, fi.Name)))
          fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name));
      }

      foreach (DirectoryInfo di in dir.GetDirectories())
      {
        DirectoryInfo subDir = new DirectoryInfo(System.IO.Path.Combine(target.FullName, di.Name));
        if (!subDir.Exists)
          subDir.Create();
        CopyDir(di, subDir);
      }
    }

    public void RefreshFileList()
    {
      if (mTargetInfo != null)
      {
        AddFolder(mTargetInfo);
      }
    }

    private void AddFolder(DirectoryInfo dir)
    {
      ResetUI();

      if (dir.Exists)
      {
        mTargetInfo = dir;
        mInstrumentHandler.SourceBase = mTargetInfo.FullName;
        RecursiveAdd(dir);
        m_tvTypeView.DataContext = mInstrumentHandler;
      }
      else
        AddMessageText("Path not found: " + dir.FullName, true);
    }

    private void RecursiveAdd(DirectoryInfo dir)
    {
      try
      {
        foreach (FileInfo fi in dir.GetFiles())
          CheckAndAddAssembly(fi);
        foreach (DirectoryInfo subDir in dir.GetDirectories())
          RecursiveAdd(subDir);
      }
      catch (DirectoryNotFoundException)
      {
        AddMessageText("Directory not found: " + dir.FullName, true);
      }
      catch (Exception)
      {
        AddMessageText("Error adding directory: " + dir.FullName, true);
      }
    }

    #endregion

    #region AssemblyHandling

    private void CheckAndAddAssembly(FileInfo fi)
    {
      if (fi.Extension.ToUpper() == ".EXE" || fi.Extension.ToUpper() == ".DLL")
      {
        if (!mInstrumentHandler.AddAssembly(fi.FullName))
        {
          foreach (string s in mInstrumentHandler.ErrorList)
            AddMessageText(s, true);

          mInstrumentHandler.ErrorList.Clear();
        }
      }
    }

    private void InstrumentAssemblies(object state)
    {
      mInstrumentationSuccessfull = false;
      AddMessageText("Starting instrumentation", false);
      string folderName;

      if (mTargetInfo.Exists)
      {
        folderName = mTargetInfo.Name + " (Traced)";
        string path = mTargetInfo.Parent.FullName;

        mTraceDir = new DirectoryInfo(System.IO.Path.Combine(path, folderName));

        try
        {
          if (mTraceDir.Exists)
            mTraceDir.Delete(true);

          mTraceDir.Create();
        }
        catch (Exception)
        {
          AddMessageText("Unable to create or clean output directory. Insufficient privileges or traced application is running", true);
          ButtonEnableDisable(true);
          return;
        }

        AddMessageText("Copying files...", false);

        try
        {
          CopyDir(mTargetInfo, mTraceDir);
        }
        catch (Exception)
        {
          AddMessageText("Unable to copy files to output directory", true);
          ButtonEnableDisable(true);
          return;
        }
                
        mInstrumentHandler.Instrument(mTraceDir.FullName);
        
        AddMessageText("Assemblies saved to " + mTraceDir.FullName, false);        
      }
    }

    private bool LaunchAssembly(AssemblyContainer assemCon)
    {
      string path = System.IO.Path.Combine(mTraceDir.FullName, assemCon.AssemblyName);
      Process prc = new Process();
      prc.StartInfo = new ProcessStartInfo(path);
      prc.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
      
      try
      {
        prc.Start();
      }
      catch (Exception)
      {
        return false;
      }

      if (OnApplicationRun != null)
        OnApplicationRun();

      return true;
    }

    private SigningUtilities.SigningSetting HandleAssemblySigningCallback( string assemblyName, byte[] assemblyPublicKey )
    {
      if (mSignfileRepository == null)
        return null;

      SigningSetting setting = mSignfileRepository.Get(assemblyPublicKey);
      if (setting == null)
      {
        setting = SignAssemblyForm.SelectSignAssemblyAction(this , mSignfileRepository, assemblyPublicKey, assemblyName);
      }
      return setting;
    }

    #endregion

    #region UI Updaters

    private void ButtonEnableDisable(bool state)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (System.Windows.Forms.MethodInvoker)delegate()
      {
        try
        {
          m_btIntrument.IsEnabled = state;

          if (state)
          {
            if (mInstrumentHandler.ExecutableList.Count > 0 && mInstrumentationSuccessfull)
              m_btRun.IsEnabled = true;

            this.Cursor = Cursors.Arrow;
          }
        }
        catch (Exception)
        {
        }
      });
    }
    
    private void AddMessageText(string message, bool error)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (System.Windows.Forms.MethodInvoker)delegate()
      {
        try
        {
          if (error)
          {
            TextBlock tb = new TextBlock();
            tb.Text = message;
            tb.Foreground = Brushes.Red;
            m_tbMessages.Items.Add(tb);
          }
          else
            m_tbMessages.Items.Add(message);

          m_tbMessages.ScrollIntoView(message);
        }
        catch (Exception)
        {
        }
      });
    }

    private void ResetUI()
    {
      m_btRun.IsEnabled = false;
      mTargetInfo = null;
      m_tvTypeView.DataContext = null;
      mInstrumentHandler.Reset();      
    }

    #endregion

    #region Eventhandlers

    private void OnFileDrop(object sender, DragEventArgs e)
    {     
      string[] files = (String[])e.Data.GetData(DataFormats.FileDrop);

      if (files != null && files.Length > 0)
      {
        if (files.Length > 1 || !Directory.Exists(files[0]))
        {
          AddMessageText("Only drop one directory. Not single or multiple files and not multiple directories", true);
          return;
        }
        this.Cursor = Cursors.Wait;
        DirectoryInfo dir = new DirectoryInfo(files[0]);
        m_tbMessages.Items.Clear();
        AddFolder(dir);
        mUpdateFiles = false;
        m_tbPath.Text = dir.FullName;
        mUpdateFiles = true;
        this.Cursor = Cursors.Arrow;
      }

      e.Handled = true;
    }

    private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
    {
      TreeViewItem item = sender as TreeViewItem;
      if (item != null)
      {
        item.Focus();
        e.Handled = true;
      }
    }

    private void m_tvTypeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      TreeView view = sender as TreeView;
      mSelectedItem = view.SelectedItem as ILType;
    }
        
    private void m_btIntrument_Click(object sender, RoutedEventArgs e)
    {
      if (mInstrumentHandler.AssemblyList.Count > 0)
      {
        this.Cursor = Cursors.Wait;
        ButtonEnableDisable(false);
        m_tbMessages.Items.Clear();
        ThreadPool.QueueUserWorkItem(new WaitCallback(InstrumentAssemblies));
      }
    }

    private void m_btRun_Click(object sender, RoutedEventArgs e)
    {
      bool success = false;
      if (mInstrumentHandler.ExecutableList.Count > 1)
      {
        SelectExecutable selection = new SelectExecutable(mInstrumentHandler.ExecutableList);
        selection.ShowDialog();
        if (SelectExecutable.SelectedAssembly != null)
          success = LaunchAssembly(SelectExecutable.SelectedAssembly);
            
      }
      else
        success = LaunchAssembly(mInstrumentHandler.ExecutableList[0]);

      if (!success)
        AddMessageText("Failed to launch assembly. Please check if the executable is present in the output directory", true);
    }

    private void m_btInput_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
      fbd.Description = "Choose application path";
      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        m_tbPath.Text = fbd.SelectedPath;
      }
    }


    private void m_tbPath_PreviewDragEnter(object sender, DragEventArgs e)
    {
      e.Effects = DragDropEffects.Move;
      e.Handled = true;
    }

    private void m_tbPath_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (Directory.Exists(m_tbPath.Text) && m_tbPath.IsFocused == false && mUpdateFiles)
      {
        AddFolder(new DirectoryInfo(m_tbPath.Text));
        mStartLocation.Value = m_tbPath.Text;
      }

    }

    private void m_tbPath_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        if (Directory.Exists(m_tbPath.Text))
        {
          AddFolder(new DirectoryInfo(m_tbPath.Text));
        }
      }
    }

    private void m_tbUncheckAll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      foreach (AssemblyContainer assemCon in mInstrumentHandler.AssemblyList)
        assemCon.SetEnabledState(false);
    }

    private void m_tbCheckAll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      foreach (AssemblyContainer assemCon in mInstrumentHandler.AssemblyList)
        assemCon.SetEnabledState(true);
    }

    void mView_OnInstrumentationAction(InstrumentationActionEventArgs e)
    {
      switch (e.Action)
      {
        case InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationStarted:
          if (OnAppInstrumented != null)
            OnAppInstrumented();
          //AddMessageTextInvoker("Started instrumenting: " + e.AssemblyName);
          break;
        case InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationEnded:
          if (mInstrumentHandler.ErrorList.Count > 0)
          {
            foreach (string s in mInstrumentHandler.ErrorList)
            {
              AddMessageText(s, true);
            }

            ButtonEnableDisable(true);

            mInstrumentHandler.ErrorList.Clear();
          }

          if (mInstrumentHandler.MessageList.Count > 0)
          {
            foreach (string s in mInstrumentHandler.MessageList)
            {
              AddMessageText(e.AssemblyName + " - " + s, false);
            }
            mInstrumentHandler.MessageList.Clear();
          }
          AddMessageText(e.AssemblyName + " - Methods instrumented: " + e.MethodCounter, false);          
          break;
        case InstrumentationActionEventArgs.InstrumentationAction.InstrumentationFinished:
          AddMessageText("Instrumentation completed. Total methods instrumented: " + e.MethodCounter, false);
          mInstrumentationSuccessfull = true;
          ButtonEnableDisable(true);
          break;
      }
    }

    private void m_cbTraceProperties_Clicked(object sender, RoutedEventArgs e)
    {
      mInstrumentHandler.InstrumentProperties = (bool)m_cbTraceProperties.IsChecked;
    }

    private void m_cbTraceToFile_Clicked(object sender, RoutedEventArgs e)
    {
      mInstrumentHandler.TraceToFile = (bool)m_cbTraceToFile.IsChecked;
    }

    #endregion

    #region IWin32Window Members

    public IntPtr Handle
    {
      get {
        DependencyObject parent = this.Parent;

        while (parent != null && !(parent is Window))
          parent = LogicalTreeHelper.GetParent(parent);
        Window w = parent as Window;
        if (w == null)
          return IntPtr.Zero;
        else
          return new WindowInteropHelper(w).Handle;
      }
    }

    #endregion            
    
  }
}
