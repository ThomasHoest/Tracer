using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace EQATEC.Tracer.Tools
{
  public class UIUtils
  {    //Get png picture from embedded resource
    public static Image GetPngImageFromRessource(string path)
    {
      Image img = null;
      StreamResourceInfo sr = Application.GetResourceStream(new Uri(path, UriKind.Relative));
      BitmapImage bmp = new BitmapImage();

      bmp.BeginInit();
      bmp.UriSource = new Uri(path, UriKind.Relative);
      bmp.DecodePixelHeight = 16;
      bmp.DecodePixelWidth = 16;
      bmp.EndInit();

      img = new Image();
      img.Source = bmp;
      return img;
    }

    public static BitmapImage GetPngBitmapImageFromRessource(string path)
    {
      StreamResourceInfo sr = Application.GetResourceStream(new Uri(path, UriKind.Relative));
      BitmapImage bmp = new BitmapImage();

      bmp.BeginInit();
      bmp.UriSource = new Uri(path, UriKind.Relative);
      bmp.DecodePixelHeight = 16;
      bmp.DecodePixelWidth = 16;
      bmp.EndInit();

      return bmp;
    }

    public static WordHolder[] CreateTraceType(LineHolder holder, bool useFullName)
    {
      switch (holder.Type)
      {
        case LineHolder.LineType.EnterTrace:
          return UIUtils.CreateEnterTraceType(holder, holder.Type, useFullName);
        case LineHolder.LineType.ReturnTrace:
          return UIUtils.CreateReturnTraceType(holder, useFullName);
        case LineHolder.LineType.Log4NetTrace:
          return UIUtils.CreateLog4NetTrace(holder);
        case LineHolder.LineType.EnterLeaveTrace:
          return UIUtils.CreateEnterTraceType(holder, holder.Type, useFullName);
        case LineHolder.LineType.Exception:
          return UIUtils.CreateExceptionTraceType(holder);
        case LineHolder.LineType.CaughtException:
          return UIUtils.CreateCaughtExceptionTraceType(holder);
          //traceLine.Background = WordColors.ExceptionBackgroundBrush;  
        case LineHolder.LineType.FlushInfo:
          return UIUtils.CreateFlushInfoType(holder);
        default:
          return null;
      }
    }

    private const int mLog4NetTraceLength = 3;

    private static WordHolder[] CreateLog4NetTrace(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mLog4NetTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, WordColors.TimeBrush);
      wordList[counter++].Separator = " [";

      wordList[counter] = new WordHolder(holder.TraceLevel, WordColors.Log4NetLevelBrush);
      wordList[counter++].Separator = "] ";

      wordList[counter] = holder.Data;
      wordList[counter].Brush = WordColors.Log4NetTraceBrush;
      
      return wordList;
    }

    const int mCaughtExceptionTraceLength = 4;

    private static WordHolder[] CreateCaughtExceptionTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mCaughtExceptionTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, WordColors.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(string.Format("0x{0:X}", holder.IThreadID), WordColors.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("Caught Exception : ");
      wordList[counter++].Brush = WordColors.ExceptionBrush;

      wordList[counter] = holder.Name;
      wordList[counter++].Brush = WordColors.ExceptionBrush;

      return wordList;
    }

    const int mFlushTraceLength = 3;

    private static WordHolder[] CreateFlushInfoType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mFlushTraceLength];

      int counter = 0;

      wordList[counter++] = new WordHolder(holder.Time, WordColors.TimeBrush);

      wordList[counter] = new WordHolder("Flush : ");
      wordList[counter++].Brush = WordColors.FlushBrush;

      wordList[counter] = holder.Data;
      wordList[counter++].Brush = WordColors.FlushBrush;

      return wordList;
    }

    const int mExceptionTraceLength = 4;

    private static WordHolder[] CreateExceptionTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mExceptionTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, WordColors.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(string.Format("0x{0:X}", holder.IThreadID), WordColors.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("Exception : ");
      wordList[counter++].Brush = WordColors.ExceptionBrush;

      wordList[counter] = holder.Name;
      wordList[counter++].Brush = WordColors.ExceptionBrush;

      return wordList;
    }

    const int mEnterTraceLength = 4;

    public static WordHolder[] CreateEnterTraceType(LineHolder holder, LineHolder.LineType type, bool useFullName)
    {
      WordHolder[] wordList = new WordHolder[mEnterTraceLength + holder.Params.Length];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, WordColors.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(string.Format("0x{0:X}", holder.IThreadID), WordColors.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      if (type == LineHolder.LineType.EnterTrace)
        wordList[counter] = new WordHolder("enter", WordColors.ReturnLeaveBrush);
      else if (type == LineHolder.LineType.EnterLeaveTrace)
        wordList[counter] = new WordHolder("enter+leave", WordColors.ReturnLeaveBrush);

      wordList[counter++].Separator = " ";

      if (useFullName)
        wordList[counter] = holder.FullName;
      else
        wordList[counter] = holder.Name;

      wordList[counter].Brush = WordColors.FunctionNameBrush;

      if (holder.Params.Length == 0)
        wordList[counter++].Separator = " ()";
      else
        wordList[counter++].Separator = " (";

      for (int i = 0; i < holder.Params.Length; i++)
      {
        wordList[counter + i] = holder.Params[i].Data;
        wordList[counter + i].Brush = WordColors.FunctionDataBrush;
        if (i < holder.Params.Length - 1)
          wordList[counter + i].Separator = ", ";
        else
          wordList[counter + i].Separator = ")";
      }

      return wordList;
    }

    const int mReturnTraceLength = 5;

    public static WordHolder[] CreateReturnTraceType(LineHolder holder, bool useFullName)
    {
      WordHolder[] wordList;

      if (holder.Data == null)
        wordList = new WordHolder[mReturnTraceLength - 1];
      else
        wordList = new WordHolder[mReturnTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, WordColors.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(string.Format("0x{0:X}", holder.IThreadID), WordColors.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("leave", WordColors.ReturnLeaveBrush);
      wordList[counter++].Separator = " ";

      if (useFullName)
        wordList[counter] = holder.FullName;
      else
        wordList[counter] = holder.Name;

      if (holder.Data != null)
        wordList[counter].Separator = " : ";

      wordList[counter++].Brush = WordColors.FunctionNameBrush;

      if (holder.Data != null)
      {
        wordList[counter] = holder.Data;
        wordList[counter++].Brush = WordColors.FunctionDataBrush;
      }

      return wordList;
    }

    public static ContextMenu BuildMemberMenu(MemberContainer member, RoutedEventHandler menuHandler)
    {
      ContextMenu menu = new ContextMenu();
      menu.Tag = member.ID;

      MenuItem item = new MenuItem();
      item.Header = "Enabled";
      item.IsCheckable = true;
      item.Tag = new TraceLineMenuAction(member.ID, 0, TraceLineMenuAction.MenuActionType.EnableDisable);
      //item.Click += menuHandler;

      Binding enabledBinding = new Binding("Enabled") {Source = member, Mode = BindingMode.TwoWay};
      item.SetBinding(MenuItem.IsCheckedProperty, enabledBinding);
      
      menu.Items.Add(item);
      
      item = new MenuItem();
      item.Header = "Disable and remove";
      item.Tag = new TraceLineMenuAction(member.ID, 0, TraceLineMenuAction.MenuActionType.DisableAndRemove);
      item.Click += menuHandler;

      Binding itemEnabledBinding = new Binding("Enabled") { Source = member, Mode = BindingMode.TwoWay };
      item.SetBinding(MenuItem.IsEnabledProperty, enabledBinding);

      menu.Items.Add(item);      

      item = new MenuItem();
      item.Header = "Find in tree";
      item.Tag = new TraceLineMenuAction(member.ID, 0, TraceLineMenuAction.MenuActionType.FindInTree);
      item.Click += menuHandler;
      menu.Items.Add(item);

      int prevCallers = 0;
      for (int i = 1; i <= 5; i++)
      {
        int temp = member.GetCallees(i);
        if (temp > 0 && temp > prevCallers)
        {
          if (i == 1)
          {
            Separator sep = new Separator();
            menu.Items.Add(sep);
          }

          item = new MenuItem();
          //item.Header = String.Format("Enable for callers. {0} Level ({1} functions)", i, temp);
          string callerCount = (temp == 1 ? "1 method" : String.Format("{0} methods", temp));
          if (i == 1)
            item.Header = String.Format("Enable callers ({0})", callerCount);
          else
            item.Header = String.Format("Enable {0}-level callers ({1})", i, callerCount);
          item.Tag = new TraceLineMenuAction(member.ID, i, TraceLineMenuAction.MenuActionType.EnableCallees);
          item.Click += menuHandler;
          menu.Items.Add(item);
          prevCallers = temp;
        }
        else
          break;
      }

      return menu;
    }
    
    public static bool SearchAndMarkLineFor(string SearchString, LineHolder traceLine)
    {
      bool found = false;
      if (traceLine.Name != null && SearchString != "")
      {
        if (traceLine.Name.Text.Contains(SearchString) || traceLine.Type.ToString().Contains(SearchString))
        {
          //traceLine.RawLine.SelectedBySearch = true;
          found = true;
        }
        else if(traceLine.Params != null)
        {
          foreach (ParameterHolder ph in traceLine.Params)
          {
            if (ph.Data.Text.Contains(SearchString))
            {
              //found += " parameter: " + ph.data.text;
              found = true;
              break;
            }
          }
        }
        if (traceLine.Selected == false)
          traceLine.SelectedBySearch = found;
      }
      return (found);
    }

  }
}