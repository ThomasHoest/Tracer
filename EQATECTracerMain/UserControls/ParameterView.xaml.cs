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
using EQATEC.Tracer.Tools;

namespace EQATEC.Tracer.UserControls
{
  public class ListViewItemStyleSelector : StyleSelector
  {
    private int i = 0;
    public override Style SelectStyle(object item, DependencyObject container)
    {
      // makes sure the first item always gets the first style, even when restyling
      ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(container);
      if (item == ic.Items[0])
      {
        i = 0;
      }
      string styleKey;
      if (i % 2 == 0)
      {
        styleKey = "ItemStyle1";
      }
      else
      {
        styleKey = "ItemStyle2";
      }
      i++;
      return (Style)(ic.FindResource(styleKey));
    }
  }

  /// <summary>
  /// Interaction logic for ParameterView.xaml
  /// </summary>
  public partial class ParameterView : UserControl
  {
    LineHolder mLine;

    ThreadSafeObservableCollection<ParameterHolder> mParameterData = new ThreadSafeObservableCollection<ParameterHolder>();

    public ThreadSafeObservableCollection<ParameterHolder> ParameterData
    {
      get { return mParameterData; }
      set { mParameterData = value; }
    }

    public ParameterView()
    {
      InitializeComponent();
    }

    public ParameterView(LineHolder line)
    {
      InitializeComponent();
      Update(line);
    }

    private void Init()
    {
    }

    public void Update(LineHolder line)    
    {
      Clear();
      mLine = line;
      ParseEnterLine();
      TraceLine traceLine = new TraceLine(UIUtils.CreateTraceType(line, false));
      ToolTip tt = new ToolTip();
      tt.StaysOpen = true;
      tt.Content = line.FullName.Text;
      m_bdPanelBorder.ToolTip = tt;
      traceLine.VerticalAlignment = VerticalAlignment.Center;
      m_lbInfo.Children.Add(traceLine);     

    }

    public void Clear()
    {
      mParameterData.Clear();
      m_lbInfo.Children.Clear();
    }

    private void ParseEnterLine()
    {
      if (mLine.Params != null)
      {
        for (int i = 0; i < mLine.Params.Length; i++)
        {
          mParameterData.Add(mLine.Params[i]);
        }
      }
    }
  }
}