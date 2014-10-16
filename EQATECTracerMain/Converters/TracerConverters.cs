using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using EQATEC.Tracer.Properties;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;

namespace EQATEC.Tracer.Converters
{
  class AssemblyItemConverter : IValueConverter
  {
    #region IValueConverter Members

    public AssemblyItemConverter()
    {

    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      Image imageSelected = UIUtils.GetPngImageFromRessource("../Resources/component_green.png");
      return new AssemblyTreeItem(value as ILType, imageSelected);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
