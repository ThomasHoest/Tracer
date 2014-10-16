using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using EQATEC.Tracer.UserControls;
using EQATEC.VersionCheckUtilities;
using System.Windows;

namespace EQATEC.Tracer.UserControls
{
  public class ViewerBrushes
  {
    public static Brush TypeBrush = new SolidColorBrush(Properties.Settings.Default.TypeColor);
    public static Brush FunctionNameBrush = new SolidColorBrush(Properties.Settings.Default.FunctionNameColor);
    public static Brush FunctionDataBrush = new SolidColorBrush(Properties.Settings.Default.FunctionDataColor);
    public static Brush ThreadIDBrush = new SolidColorBrush(Properties.Settings.Default.ThreadIdColor);
    public static Brush Log4NetTraceBrush = new SolidColorBrush(Properties.Settings.Default.Log4NetTraceColor);
    public static Brush Log4NetLevelBrush = new SolidColorBrush(Properties.Settings.Default.Log4NetLevelColor);
    public static Brush TimeBrush = new SolidColorBrush(Properties.Settings.Default.TimeColor);
    public static Brush ExceptionBrush = new SolidColorBrush(Properties.Settings.Default.ExceptionColor);
    //public static Brush ExceptionBackgroundBrush = new SolidColorBrush(Properties.Settings.Default.ExceptionBackgroundColor);
    public static Brush FlushBrush = new SolidColorBrush(Properties.Settings.Default.FlushColor);
    public static Brush SelectedLineBrush = new SolidColorBrush(Properties.Settings.Default.SelectedLine);
    public static Brush ReturnLeaveBrush = new SolidColorBrush(Properties.Settings.Default.ReturnLeaveColor);
  }

  public class TreeViewWithLoaders : TreeView
  {    
    protected override System.Windows.DependencyObject GetContainerForItemOverride()
    {
      return base.GetContainerForItemOverride();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
      return base.IsItemItsOwnContainerOverride(item);      
    }
  }

  //public class TracerFeedBack
  //{
  //  public static void SendFeedBack(Exception ex, object sender, string from, string errorType)
  //  {
  //    string newl = Environment.NewLine;

  //    FeedbackSender feedbackSender = new FeedbackSender();
  //    StringBuilder sb = new StringBuilder();
  //    sb.AppendFormat("Sender is {0}{1}", sender != null ? sender : "null" , newl);
  //    sb.AppendFormat("Exception info for type {0}{1}", ex.GetType().FullName, newl);
  //    sb.AppendFormat("Message is: {0}{1}", ex.Message, newl);
  //    sb.AppendFormat("StackTrace is: {0}{1}", ex.StackTrace, newl);
  //    sb.AppendFormat("Inner exception is: {0}{1}", ex.InnerException != null ? ex.InnerException.ToString() : "null", newl);

  //    feedbackSender.SendFeedback(ReleaseInfo.ApplicationID, ReleaseInfo.ToolID, from, errorType, sb.ToString());
  //  }
  //}

  public class VisualTreeHelpers
  {
    private static childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
      {
        DependencyObject child = VisualTreeHelper.GetChild(obj, i);
        if (child != null && child is childItem)
          return (childItem)child;
        
        childItem childOfChild = FindVisualChild<childItem>(child);
        if (childOfChild != null)
          return childOfChild;
      }
      return null;
    }

  }

  #region TraceLineManager

  public class TraceLineManager
  {
    object mQueueSync = new object();
    Queue<TraceLine> mFreeLines;

    ContainerList mParentList;

    public int PoolSize
    {
      get { return mPoolSize; }
    }

    public ContainerList ParentList
    {
      get { return mParentList; }
      set { mParentList = value; }
    }

    TraceLine mSelectedLine = null;

    public TraceLine SelectedLine
    {
      get { return mSelectedLine; }
      set { mSelectedLine = value; }
    }
    
    /// <summary>
    /// Lines currently showed on screen.
    /// </summary>
    List<TraceLine> mLinesShowed = new List<TraceLine>();
    public List<TraceLine> LinesShowed
    {
      get { return mLinesShowed; }
      set { mLinesShowed = value; }
    }
    
    ///// <summary>
    ///// All Lines
    ///// </summary>
    //static List<TraceLine> mLinesAll = new List<TraceLine>();
    //public static List<TraceLine> LinesAll
    //{
    //  get { return mLinesAll; }
    //  set { mLinesAll = value; }
    //}

    TraceLine mSelectedLineBySearch = null;

    public TraceLine SelectedLineBySearch
    {
      get { return mSelectedLineBySearch; }
      set { mSelectedLineBySearch = value; }
    }
    
    int mPoolSize;

    public TraceLineManager(int size)
    {
      mPoolSize = size;

      mFreeLines = new Queue<TraceLine>(size);

      for (int i = 0; i < size; i++)
      {
        TraceLine line = new TraceLine();
        line.MouseDown += line_MouseDown;
        line.Unloaded += line_Unloaded;
        mFreeLines.Enqueue(line);
      }
    }

    void line_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      lock (mParentList.ListSync)
      {
        TraceLine line = sender as TraceLine;

        if (e.ClickCount == 2)
        {
          if(line != null)
          {
            mParentList.FireOnItemDoubleClick(line.RawLine);
          }
          e.Handled = true;
        }
        else if (line != null)
        {

          if (mSelectedLine != line)
          {
            if(mSelectedLine != null)
              mSelectedLine.PanelBackground = Brushes.Transparent;
            mParentList.FireItemSelected(line, e);
            mSelectedLine = line;
          }
          
          //System.Diagnostics.Trace.WriteLine("Painting line");
        }
      }

    }

    void line_Unloaded(object sender, RoutedEventArgs e)
    {
      TraceLine line = sender as TraceLine;
      lock (mQueueSync)
      {
        mLinesShowed.Remove(line);
        mFreeLines.Enqueue(line);
      }
      //Console.WriteLine("Line unloaded. Lines: " + mFreeLines.Count);
    }

    public TraceLine GetNewLine()
    {
      TraceLine line;
      lock (mQueueSync)
      {
        if (mFreeLines.Count == 0)
        {
          line = new TraceLine();
          line.Unloaded += line_Unloaded;
          line.MouseDown += line_MouseDown;
        }
        else
          line = mFreeLines.Dequeue();

        mLinesShowed.Add(line);
      }

      //Console.WriteLine("Line loaded. Lines: " + mFreeLines.Count);
      //Lines currently showed on screen.
      return line;
    }
  }

  #endregion 

}
