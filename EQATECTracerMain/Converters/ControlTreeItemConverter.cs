using System;
using System.Windows;
using System.Windows.Data;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;

namespace EQATEC.Tracer.Converters
{
  class ControlTreeItemConverter : IValueConverter
  {

    RoutedEventHandler mCheckBoxClickHandler;

    public RoutedEventHandler CheckBoxClickHandler
    {
      get { return mCheckBoxClickHandler; }
      set { mCheckBoxClickHandler = value; }
    }

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return new ControlTreeItem(value as ILType, mCheckBoxClickHandler);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}