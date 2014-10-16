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

using EQATEC.Tracer.Utilities;

namespace EQATEC.Tracer.TracerViewer
{
  /// <summary>
  /// Interaction logic for ParamWindow.xaml
  /// </summary>
  public partial class ParamWindow : Window
  {
    public ParamWindow(LineHolder line)
    {
      InitializeComponent();
      m_pvParameterView.Update(line);
    }
  }
}
