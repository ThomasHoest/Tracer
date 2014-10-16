using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;

namespace EQATEC.Tracer.Converters
{
  class TraceItemConverter : IValueConverter
  {
    #region IValueConverter Members

    TraceLineManager mLineManager;

    public TraceLineManager LineManager
    {
      get { return mLineManager; }
      set { mLineManager = value; }
    }

    bool mDrone = false;
    public bool Drone
    {
      get { return mDrone; }
      set { mDrone = value; }
    }

    //To be updated from Viewer.xaml.cs
    string mSearchString = "";
    public string SearchString
    {
      get { return mSearchString; }
      set { mSearchString = value; }
    }


    RoutedEventHandler mTraceLineMenuHandler;

    public RoutedEventHandler TraceLineMenuHandler
    {
      get { return mTraceLineMenuHandler; }
      set { mTraceLineMenuHandler = value; }
    }


    Dictionary<int, ContextMenu> mMemberMenues = new Dictionary<int, ContextMenu>();

    public TraceItemConverter()
    {      
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
      System.Diagnostics.Debug.Assert(mLineManager != null, "Traceconverter must have a linemanager");

      LineHolder holder = value as LineHolder;//holderProxy.Line;
      WordHolder[] wordList = UIUtils.CreateTraceType(holder, false);
      TraceLine traceLine = mLineManager.GetNewLine();

      if (mDrone)
        traceLine.UseIndent(holder.Level);

      holder.OnLineSelected += new LineHolder.LineSelectedHandler(traceLine.LineSelected);
      holder.OnLineSelectedBySearch += new LineHolder.LineSelectedHandlerBySearch(traceLine.LineSelectedBySearch);

      UIUtils.SearchAndMarkLineFor(SearchString, holder);

      if (holder.Selected)
      {
        //Console.WriteLine("Selected line. Id: " + holder.ID);
        traceLine.PanelBackground = WordColors.SelectedLineBrush;
        mLineManager.SelectedLine = traceLine;

      } 
      else if (holder.SelectedBySearch)
      {
        traceLine.PanelBackground = WordColors.SelectedLineBySearchBrush;
        mLineManager.SelectedLineBySearch = traceLine;
      }
      else
        traceLine.PanelBackground  = Brushes.Transparent;
      //Console.WriteLine("Converter called. Id: " + holder.ID);

      //TraceLine traceLine = new TraceLine(wordList);
      //traceLine.Background = SetLineBackground(holder.IThreadID);          

      if (holder.ID != -1 && holder.Member != null)
      {
        if (!mMemberMenues.ContainsKey(holder.ID))
        {
          ContextMenu menu = UIUtils.BuildMemberMenu(holder.Member, mTraceLineMenuHandler);
          mMemberMenues[holder.ID] = menu;
        }

        traceLine.ContextMenu = mMemberMenues[holder.ID];
      }
      //Search the line for the SearchString and mark matches.
      //TraceLineManager.SearchLineAndMark(SearchString, holder); //FIXME

      //traceLine.PanelBackground = EQATEC.Tracer.Utilities.WordColors.SelectedLineBySearchBrush;
      //try all....

      //if (holder.Selected == true)
      //{
      //  traceLine.PanelBackground = EQATEC.Tracer.Utilities.WordColors.SelectedLineBrush;
      //}
      //if (holder.SelectedBySearch == true)
      //{
      //  traceLine.PanelBackground = EQATEC.Tracer.Utilities.WordColors.SelectedLineBySearchBrush;
      //}
      traceLine.UpdateLine(wordList);
      traceLine.RawLine = holder;


            
      return traceLine;
      //return "This is a very long string test. La lalalalalalalalalalalalalalalalalalalalalalalalalalala";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}