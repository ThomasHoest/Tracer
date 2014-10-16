using System;
using System.Windows.Data;

namespace EQATEC.Tracer.Converters
{
  public class ConvertLevelToIndent : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return (int)value * 16;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException("Not supported - ConvertBack should never be called in a OneWay Binding.");
    }
  }
}