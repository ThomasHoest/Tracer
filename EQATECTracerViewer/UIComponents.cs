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

namespace EQATEC.Tracer.TracerViewer
{
  public class ViewerBrushes
  {
    public static Brush TypeBrush = new SolidColorBrush(Properties.Settings.Default.TypeColor);
    public static Brush FunctionNameBrush = new SolidColorBrush(Properties.Settings.Default.FunctionNameColor);
    public static Brush FunctionDataBrush = new SolidColorBrush(Properties.Settings.Default.FunctionDataColor);
    public static Brush ThreadIDBrush = new SolidColorBrush(Properties.Settings.Default.ThreadIdColor);
    public static Brush TimeBrush = new SolidColorBrush(Properties.Settings.Default.TimeColor);
    public static Brush ExceptionBrush = new SolidColorBrush(Properties.Settings.Default.ExceptionColor);
    public static Brush ExceptionBackgroundBrush = new SolidColorBrush(Properties.Settings.Default.ExceptionBackgroundColor);
  }
}
