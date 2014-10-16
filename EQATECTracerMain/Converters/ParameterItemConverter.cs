using System;
using System.Windows.Controls;
using System.Windows.Data;
using EQATEC.Tracer.Tools;

namespace EQATEC.Tracer.Converters
{
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