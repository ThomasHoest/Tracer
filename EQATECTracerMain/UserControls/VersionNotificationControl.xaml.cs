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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EQATEC.Tracer.UserControls
{
  /// <summary>
  /// Interaction logic for VersionNotificationControl.xaml
  /// </summary>
  public partial class VersionNotificationControl : UserControl
  {
    public event EventHandler VersionNotificationClicked;
    protected virtual void OnVersionNotificationClicked()
    {
      if (VersionNotificationClicked != null)
        VersionNotificationClicked(this, EventArgs.Empty);
    }

    public VersionNotificationControl()
    {
      InitializeComponent();
    }

    private void Grid_MouseUp( object sender, MouseButtonEventArgs e )
    {
      OnVersionNotificationClicked();
    }
  }
}