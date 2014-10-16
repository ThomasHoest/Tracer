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


using EQATEC.CecilToolBox;
using EQATEC.Tracer.Instrumentor;
using EQATEC.Tracer.Utilities;


namespace EQATEC.Tracer.TracerInstrumentor
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class InstrumentorMain : Window
  {
    DirectoryInfo mTraceDir;

    public InstrumentorMain()
    {
      InitializeComponent();
      
      m_btRun.IsEnabled = false;

      mView = new InstrumentationHandler();
      mView.OnInstrumentationAction += new InstrumentationHandler.InstrumentationActionHandler(mView_OnInstrumentationAction);      
    }

    void mView_OnInstrumentationAction(InstrumentationActionEventArgs e)
    {
      switch (e.Action)
      {
        case InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationStarted:
          //AddMessageTextInvoker("Started instrumenting: " + e.AssemblyName);
          break;
        case InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationEnded:
          AddMessageTextInvoker("Done: " + e.AssemblyName + " Methods instrumented: " + e.MethodCounter);
          break;
        case InstrumentationActionEventArgs.InstrumentationAction.InstrumentationFinished:
          AddMessageTextInvoker("Instrumentation completed. Total methods instrumented: " + e.MethodCounter);
          ButtonEnableDisableInvoker(true);
          break;
      }
    }

    InstrumentationHandler mView;
    ILType mSelectedItem;
    DirectoryInfo mTargetInfo = null;
    string mPath = "";

    private void m_tvTypeView_Drop(object sender, DragEventArgs e)
    {
      string [] files = (String[])e.Data.GetData(DataFormats.FileDrop);
      mPath = "";

      foreach (string s in files)
      { 
        DirectoryInfo dir = new DirectoryInfo(s);

        if (mPath == "")
        {
          if (files.Length == 1 && dir.Exists)
          {
            mPath = dir.Parent.FullName;
            mTargetInfo = dir;
          }
          else
          {
            mTargetInfo = dir.Parent;

            if (dir.Parent.Parent != null)
            {
              mPath = dir.Parent.Parent.FullName;
            }
            else
            {
              mPath = dir.Parent.FullName;
            }
          }
        }


        if (dir.Exists)
        {
          RecursiveAdd(dir);
        }
        else
        {
          FileInfo fi = new FileInfo(s);
          CheckAndAddAssembly(fi, mPath);
        }
      }

      m_tvTypeView.DataContext = mView;
    }

    private void RecursiveAdd(DirectoryInfo dir)
    {
      foreach (FileInfo fi in dir.GetFiles())
        CheckAndAddAssembly(fi, mPath);
      foreach (DirectoryInfo subDir in dir.GetDirectories())
        RecursiveAdd(subDir);
    }

    private void CheckAndAddAssembly(FileInfo fi, string rootPath)
    {
      if (fi.Extension.ToUpper() == ".EXE" || fi.Extension.ToUpper() == ".DLL")
        mView.AddAssembly(fi.FullName, rootPath);
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

    private void m_tvTypeView_DragOver(object sender, DragEventArgs e)
    {      

    }

    private void MenuShowIL_Click(object sender, RoutedEventArgs e)
    {      
      if (mSelectedItem is MemberContainer)
      {
        MemberContainer member = mSelectedItem as MemberContainer;        
        MessageBox.Show(member.Dump());
      }
    }

    private void m_tvTypeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      TreeView view = sender as TreeView;
      mSelectedItem = view.SelectedItem as ILType;
    }

    private void m_btReset_Click(object sender, RoutedEventArgs e)
    {
      m_btRun.IsEnabled = false;
      mTargetInfo = null;
      mPath = "";
      m_tvTypeView.DataContext = null;
      mView.AssemblyList.Clear();
    }

    private void m_btIntrument_Click(object sender, RoutedEventArgs e)
    {
      ButtonEnableDisable(false);
      ThreadPool.QueueUserWorkItem(new WaitCallback(InstrumentAssemblies));      
    }

    private void InstrumentAssemblies(object state)
    {
      string folderName;

      if (mTargetInfo != null)
      {
        if (mTargetInfo.Exists)
          folderName = mTargetInfo.Name + " - Tracified";
        else
          folderName = "Tracified";


        mTraceDir = new DirectoryInfo(System.IO.Path.Combine(mPath, folderName));

        if (!mTraceDir.Exists)
          mTraceDir.Create();
        
        CopyDir(mTargetInfo, mTraceDir);
                
        mView.Instrument(mTraceDir.FullName);
        mView.Save(mTraceDir.FullName);
      }
    }

    
    private void CopyDir(DirectoryInfo dir, DirectoryInfo target)
    {
      foreach (FileInfo fi in dir.GetFiles())
      {
        if(!fi.Exists)
          fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name));
      }

      foreach (DirectoryInfo di in dir.GetDirectories())
      {
        DirectoryInfo subDir = new DirectoryInfo(System.IO.Path.Combine(target.FullName, di.Name));
        if(!subDir.Exists)
          subDir.Create();
        CopyDir(di,subDir);
      }
    }

    private void ButtonEnableDisableInvoker(bool state)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new ButtonEnableDisableHandler(ButtonEnableDisable), state);
    }
    private delegate void ButtonEnableDisableHandler(bool message);
    private void ButtonEnableDisable(bool state)
    {
      m_btIntrument.IsEnabled = state;
      m_btReset.IsEnabled = state;

      if (state)
        if (mView.ExecutableList.Count > 0)
          m_btRun.IsEnabled = true;      
    }
    
    private void AddMessageTextInvoker(string message)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new AddMessageTextHandler(AddMessageText), message);
    }
    private delegate void AddMessageTextHandler(string message);
    private void AddMessageText(string message)
    {
      m_tbMessages.Items.Insert(0, message);
    }

    private void m_btRun_Click(object sender, RoutedEventArgs e)
    {
      if (mView.ExecutableList.Count > 1)
      {
        SelectExecutable selection = new SelectExecutable(mView.ExecutableList);
        selection.ShowDialog();
        if (SelectExecutable.SelectedAssembly != null)
          LaunchAssembly(SelectExecutable.SelectedAssembly);
      }
      else
        LaunchAssembly(mView.ExecutableList[0]);
    }

    private void LaunchAssembly(AssemblyContainer assemCon)
    {
      string path = System.IO.Path.Combine(mTraceDir.FullName, string.Concat(assemCon.AssemblyName, ".exe"));
      Process prc = new Process();
      prc.StartInfo = new ProcessStartInfo(path);
      prc.Start();
    }
  }
}
