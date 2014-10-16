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
using System.Windows.Shapes;
using EQATEC.Tracer.Tools;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for ParamWindow.xaml
  /// </summary>
  public partial class ParamWindow : Window
  {
    public ParamWindow()
    {
      InitializeComponent();
    }

    public ParamWindow(LineHolder line)
    {
      InitializeComponent();
      if (line.Type == LineHolder.LineType.Exception || line.Type == LineHolder.LineType.CaughtException)
      {
        m_pvParameterView.IsEnabled = false;
        m_pvParameterView.Visibility = Visibility.Hidden;
        m_lbData.IsEnabled = true;
        m_lbData.Visibility = Visibility.Visible;
        m_lbData.Items.Add(line.Data.Text);
        this.Width = 400;
        this.Height = 400;
      }
      else
        m_pvParameterView.Update(line);
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
      this.Topmost = true;
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      this.Topmost = false;
    }
  }
}
