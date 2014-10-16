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
using EQATEC.VersionCheckUtilities;
using EQATEC.Analytics.Monitor;
using System.ComponentModel;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for VersionNotificationWindow.xaml
  /// </summary>
  public partial class VersionNotificationWindow : Window, INotifyPropertyChanged
  {
    public VersionAvailableEventArgs VersionCheckResult { get; set; }

    public VersionNotificationWindow()
    {
      InitializeComponent();
    }

    private void m_btOK_Click( object sender, RoutedEventArgs e )
    {
      this.Close();
    }

    internal void InitializeVersionCheckResult(VersionAvailableEventArgs versionCheckResult)
    {
      VersionCheckResult = versionCheckResult;
      Notify("VersionCheckResult");
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    private void Notify(string name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}