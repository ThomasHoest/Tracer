using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.Windows;

namespace EQATEC.Tracer.Converters
{
  class ExecutableConverter : IValueConverter
  {
    #region IValueConverter Members

    //Style mTextStyle = null;

    ExecutableConverter()
    {
      //Uri u = new Uri("ApplicationResources.xaml", UriKind.Relative);
      //ResourceDictionary res = Application.LoadComponent(u) as ResourceDictionary;
      //mTextStyle = res["TextBlockLink"] as Style;
    }


    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      AssemblyContainer assemCon = value as AssemblyContainer;
      DockPanel panel = new DockPanel();

      TextBlock name = new TextBlock();
      name.Text = assemCon.AssemblyName;
      name.Tag = value;
      name.MouseDown += new System.Windows.Input.MouseButtonEventHandler(name_MouseDown);
      name.Style = name.FindResource("TextBlockLink") as Style; // Resource mTextStyle;
      DockPanel.SetDock(name, Dock.Right);
      panel.Children.Add(name);
      Image img = UIUtils.GetPngImageFromRessource("../Resources/exefile.png");
      img.Width = 16;
      img.Margin = new Thickness(5, 0, 5, 0);
      img.HorizontalAlignment = HorizontalAlignment.Right;
      DockPanel.SetDock(img, Dock.Left);
      panel.Children.Add(img);      
      return panel;
    }

    void name_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      TextBlock tb = sender as TextBlock;
      SelectExecutable.SelectedAssembly = tb.Tag as AssemblyContainer;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}