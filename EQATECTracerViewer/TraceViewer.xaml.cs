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

using EQATEC.Tracer.Utilities;
using EQATEC.Tracer.Viewer;
using EQATEC.CecilToolBox;
using EQATEC.Tracer.TracerRuntime;

namespace EQATEC.Tracer.TracerViewer
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class ViewerMain : Window
  {
    ViewHandler mView;
    private delegate void ControllerToolbarHandler();
    private ControllerToolbarHandler mControllerToolbarUpdater;
        
    public ViewerMain()
    {
      InitializeComponent();
      mView = new ViewHandler();

      mControllerToolbarUpdater = new ControllerToolbarHandler(UpdateControllerToolbar);
      
      m_btRedo.IsEnabled = false;
      m_btUndo.IsEnabled = false;
      m_btClearAll.IsEnabled = false;
      m_btDisconnect.IsEnabled = false;

      mView.OnNewTraceItem += new ViewHandler.NewTraceItem(mView_OnNewTraceItem);
      mView.OnTreeAction += new ViewHandler.TreeActionHandler(mView_OnTreeAction);
      m_lbTrace.DataContext = mView;            
    }

    private childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
      {
        DependencyObject child = VisualTreeHelper.GetChild(obj, i);
        if (child != null && child is childItem)
          return (childItem)child;
        else
        {
          childItem childOfChild = FindVisualChild<childItem>(child);
          if (childOfChild != null)
            return childOfChild;
        }
      }
      return null;
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

    void mView_OnNewTraceItem()
    {/*
      if (mTraceScroller == null)
        mTraceScroller = FindVisualChild<ScrollViewer>(m_lbTrace);

      if ((mTraceScroller.ViewportHeight + mTraceScroller.VerticalOffset) == m_lbTrace.Items.Count - 1)
        mTraceScroller.LineDown();      
      
      TraceAddedHandler trace = new TraceAddedHandler(TraceAdded);
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, trace); */
    }

    private delegate void TraceAddedHandler();

    private void TraceAdded()
    {
      /*
      if(mTraceScroller == null)
        mTraceScroller = FindVisualChild<ScrollViewer>(m_lbTrace);
      
      if( ( mTraceScroller.ViewportHeight + mTraceScroller.VerticalOffset ) == m_lbTrace.Items.Count - 1)
        mTraceScroller.LineDown();      */
      
    }

    /*
    private void m_btReset_Click(object sender, RoutedEventArgs e)
    {
      m_tvTypeView.DataContext = null;
      m_tvTypeView.Items.Clear();      
    }*/

    private void m_btConnect_Click(object sender, RoutedEventArgs e)
    {
      if (mView.ConnectToTarget())
      {
        m_tvTypeView.DataContext = mView;
        m_btDisconnect.IsEnabled = true;
        m_btConnect.IsEnabled = false;
      }
    }

    private void m_btDisconnect_Click(object sender, RoutedEventArgs e)
    {
      mView.DisconnectFromTarget();      
      m_btConnect.IsEnabled = true;
      m_btDisconnect.IsEnabled = false;
    }

    private void m_btClear_Click(object sender, RoutedEventArgs e)
    {
      //m_lbTrace.Items.Clear();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      m_lbTrace.AdaptSize();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (mView != null)
      {
        mView.DisconnectFromTarget();
        mView.Dispose();
      }
    }

    private void m_btUndo_Click(object sender, RoutedEventArgs e)
    {
      mView.Undo();
    }

    private void m_btRedo_Click(object sender, RoutedEventArgs e)
    {
      mView.Redo();
    }

    void mView_OnTreeAction()
    {      
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, mControllerToolbarUpdater);
    }

    private void UpdateControllerToolbar()
    {
      if (!mView.AnyUndoActions)
        m_btUndo.IsEnabled = false;
      else
        m_btUndo.IsEnabled = true;

      if (!mView.AnyRedoActions)
        m_btRedo.IsEnabled = false;
      else
        m_btRedo.IsEnabled = true;

      if (mView.EnabledList.Count == 0)
        m_btClearAll.IsEnabled = false;
      else
        m_btClearAll.IsEnabled = true;
    }

    private void m_btClearAll_Click(object sender, RoutedEventArgs e)
    {
      mView.ClearAll();
    }

    private void m_lbTrace_OnItemSelected(LineHolder line)
    {
      m_pvParameterView.Update(line);
    }

    private void m_lbTrace_OnItemDoubleClick(LineHolder line)
    {
      ParamWindow paramWin = new ParamWindow(line);
      paramWin.Show();      
    }
  }
}
