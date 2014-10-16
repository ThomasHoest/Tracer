using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EQATEC.Tracer.Utilities;
using EQATEC.Tracer.Viewer;

namespace EQATEC.Tracer.TracerViewer
{
  class TreeItemConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return new ControlTreeItem(value as ILType);      
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class TraceItemPool
  {
    object mQueueSync = new object();
    Queue<TraceLine> mFreeLines;

    int mPoolSize;

    public TraceItemPool(int size)
    {
      mPoolSize = size;

      mFreeLines = new Queue<TraceLine>(size);

      for (int i = 0; i < size; i++)
      {        
        TraceLine line = new TraceLine();
        //line.Unloaded += new TraceLine.UnloadedHandler(line_Unloaded);
        line.Unloaded += new System.Windows.RoutedEventHandler(line_Unloaded);
        mFreeLines.Enqueue(line);
      }
    }
    /*
    void line_Unloaded(TraceLine line)
    {
      lock (mQueueSync)
        mFreeLines.Enqueue(line);
    }*/


    
    void line_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
      TraceLine line = sender as TraceLine;
      lock (mQueueSync)
        mFreeLines.Enqueue(line);
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
          //line.Unloaded += new TraceLine.UnloadedHandler(line_Unloaded);
          line.Unloaded += new System.Windows.RoutedEventHandler(line_Unloaded);
        }
        else
          line = mFreeLines.Dequeue();
        
      }

      //Console.WriteLine("Line loaded. Lines: " + mFreeLines.Count);
      return line;
    }
  }

  class TraceItemConverter : IValueConverter
  {
    #region IValueConverter Members

    TraceItemPool mLinePool;

    public TraceItemConverter()
    {
      mLinePool = new TraceItemPool(200);
    }

    private Brush SetLineBackground(int id)
    {
      Random ran = new Random(id);
      byte b = (byte)(ran.Next(255));
      Color c = Color.FromArgb(25, 25, b, 25);
      Brush brush = new SolidColorBrush(c);
      return brush;
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      LineHolder holder = value as LineHolder;
      //LineHolderProxy holder = value as LineHolderProxy;
      WordHolder[] wordList = null;
      TraceLine traceLine = mLinePool.GetNewLine();
      traceLine.Background = SetLineBackground(holder.IThreadID);    

      switch (holder.Type)
      {
        case LineHolder.LineType.EnterTrace:
          wordList = CreateEnterTraceType(holder);
          break;
        case LineHolder.LineType.ReturnTrace:
          wordList = CreateReturnTraceType(holder);
          break;
        case LineHolder.LineType.ReturnLeaveTrace:
          wordList = CreateReturnLeaveTraceType(holder);
          break;
        case LineHolder.LineType.Exception:
          wordList = CreateExceptionTraceType(holder);
          traceLine.Background = ViewerBrushes.ExceptionBackgroundBrush;
          break;
      }
      //TraceLine traceLine = new TraceLine(wordList);
      //traceLine.Background = SetLineBackground(holder.IThreadID);          
      traceLine.UpdateLine(wordList);
      traceLine.RawLine = holder;
      return traceLine;
      //return "This is a very long string test. La lalalalalalalalalalalalalalalalalalalalalalalalalalala";
    }

    const int mExceptionTraceLength = 4;

    private WordHolder [] CreateExceptionTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mExceptionTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, ViewerBrushes.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(holder.ThreadID, ViewerBrushes.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("Exception : ");
      wordList[counter++].Brush = ViewerBrushes.ExceptionBrush;

      wordList[counter] = holder.Data;
      wordList[counter++].Brush = ViewerBrushes.ExceptionBrush;
     
      return wordList;
    }

    const int mEnterTraceLength = 4;

    public WordHolder[] CreateEnterTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mEnterTraceLength + holder.Params.Length];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, ViewerBrushes.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(holder.ThreadID, ViewerBrushes.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("enter", Brushes.Green);
      wordList[counter++].Separator = " ";

      wordList[counter] = holder.Name;
      wordList[counter].Brush = ViewerBrushes.FunctionNameBrush;

      if(holder.Params.Length == 0)
        wordList[counter++].Separator = " ()";
      else
        wordList[counter++].Separator = " (";
      
      for (int i = 0; i < holder.Params.Length; i++)
      {
        wordList[counter + i] = holder.Params[i].Data;
        wordList[counter + i].Brush = ViewerBrushes.FunctionDataBrush;
        if (i < holder.Params.Length - 1)
          wordList[counter + i].Separator = ", ";
        else
          wordList[counter + i].Separator = ")";
      }

      return wordList;
    }

    const int mReturnTraceLength = 4;

    public WordHolder[] CreateReturnTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mReturnTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, ViewerBrushes.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(holder.ThreadID, ViewerBrushes.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("leave", Brushes.Green);
      wordList[counter++].Separator = " ";

      wordList[counter] = holder.Name;
      wordList[counter++].Brush = ViewerBrushes.FunctionNameBrush;

      return wordList;
    }

    const int mReturnLeaveTraceLength = 4;

    public WordHolder[] CreateReturnLeaveTraceType(LineHolder holder)
    {
      WordHolder[] wordList = new WordHolder[mReturnLeaveTraceLength];

      int counter = 0;

      wordList[counter] = new WordHolder(holder.Time, ViewerBrushes.TimeBrush);
      wordList[counter++].Separator = " (";

      wordList[counter] = new WordHolder(holder.ThreadID, ViewerBrushes.ThreadIDBrush);
      wordList[counter++].Separator = ") ";

      wordList[counter] = new WordHolder("enter+leave", Brushes.Green);
      wordList[counter++].Separator = " ";

      wordList[counter] = holder.Name;
      wordList[counter++].Brush = ViewerBrushes.FunctionNameBrush;

      return wordList;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  class ParameterItemConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      ParameterHolder param = value as ParameterHolder;
      StackPanel panel = new StackPanel();
      //TextBlock 
      return panel;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
