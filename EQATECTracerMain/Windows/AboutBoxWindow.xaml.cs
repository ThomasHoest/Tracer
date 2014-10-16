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
using System.Diagnostics;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for AboutBoxWindow.xaml
  /// </summary>
  public partial class AboutBoxWindow : Window
  {
    public AboutBoxWindow()
    {
      InitializeComponent();

      Version thisVersion = typeof(AboutBoxWindow).Assembly.GetName().Version;
      this.m_txtVersion.Text = string.Format("Version {0}", thisVersion.ToString(3));
    }

    private void Image_MouseDown( object sender, MouseButtonEventArgs e )
    {
      this.Close();
    }

    private void m_txtLicenseLink_MouseUp( object sender, MouseButtonEventArgs e )
    {
      string path = ReleaseInfo.GetPathToLicense();
      if (string.IsNullOrEmpty(path))
        return;

      try
      {
        Process p = Process.Start(path);
      }
      catch (Exception exc)
      {
        Trace.TraceError("Could not open a local file by spawning a process with the filename. Error is {0}", exc.Message);
      }

    }
  }
}
