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

using EQATEC.Tracer.Utilities;

namespace EQATEC.Tracer.TracerViewer
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

    ThreadSafeObservableCollection<ParameterHolder> mParameterData;

    public ThreadSafeObservableCollection<ParameterHolder> ParameterData
    {
      get { return mParameterData; }
      set { mParameterData = value; }
    }

    public ParameterView()
    {
      InitializeComponent();
      Init();
    }

    public ParameterView(LineHolder line)
    {
      InitializeComponent();
      Init();
      Update(line);
    }

    private void Init()
    {
      mParameterData = new ThreadSafeObservableCollection<ParameterHolder>();
      m_lbParameters.DataContext = mParameterData;
    }

    public void Update(LineHolder line)    
    {
      mLine = line;
      mParameterData.Clear();
      ParseLine();            
    }

    private void ParseLine()
    {
      for (int i = 0; i < mLine.Params.Length; i++)
      {
        mParameterData.Add(mLine.Params[i]);  
      }
    }
  }

}
