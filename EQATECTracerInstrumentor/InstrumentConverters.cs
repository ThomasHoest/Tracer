using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;

using EQATEC.Tracer.Utilities;
using EQATEC.Tracer.TracerInstrumentor;
using EQATEC.Tracer.Viewer;


namespace EQATEC.Tracer.TracerInstrumentor
{
  class TreeItemConverter : IValueConverter
  {
    #region IValueConverter Members

    public TreeItemConverter()
    {
      
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      Image imageSelected = UIHelpers.GetPngFromRessource("Resources/component_green.png");
      return new AssemblyTreeItem(value as ILType, imageSelected);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  class ExecutableConverter : IValueConverter
  {
    #region IValueConverter Members

    
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      AssemblyContainer assemCon = value as AssemblyContainer;
      DockPanel panel = new DockPanel();
      
      TextBlock name = new TextBlock();
      name.Text = assemCon.FullName;
      name.Foreground = (Brush)name.FindResource("WindowTextColor");
      DockPanel.SetDock(name, Dock.Right);     
      panel.Children.Add(name);

      Button launch = new Button();
      launch.Content = "Launch";
      launch.Tag = value;
      launch.Width = 100;
      launch.Height = 20;
      launch.Margin = new Thickness(5, 0, 5, 0);
      launch.HorizontalAlignment = HorizontalAlignment.Right;
      launch.Style = (Style)launch.FindResource("ButtonStyle");
      launch.Click += new RoutedEventHandler(launch_Click);
      DockPanel.SetDock(launch, Dock.Left);
      panel.Children.Add(launch);

      return panel;
    }

    void launch_Click(object sender, RoutedEventArgs e)
    {
      Button bt = sender as Button;
      SelectExecutable.SelectedAssembly = bt.Tag as AssemblyContainer;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
  
}
