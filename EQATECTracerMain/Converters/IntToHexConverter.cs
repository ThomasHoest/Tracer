using System;
using System.Windows.Data;

namespace EQATEC.Tracer.Converters
{
  class IntToHexConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null)
      {
        if (value is string)
          return "";
        return string.Format("0x{0:X}", ((int)value));
      }
      return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}