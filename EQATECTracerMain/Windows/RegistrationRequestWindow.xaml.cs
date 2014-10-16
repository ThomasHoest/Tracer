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
using System.Windows.Forms;
using System.Windows.Threading;
using EQATEC.Tracer.Tools;
using EQATEC.VersionCheckUtilities;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for RegistrationRequestWindow.xaml
  /// </summary>
  public partial class RegistrationRequestWindow : Window
  {
    private static PersistentInt s_TryCount = new PersistentInt(@"RegistrationRequestCount", 0);
    private static int s_MaxWait = 10;
    private static int s_IncFactor = 2;
    private DispatcherTimer m_dispatcherTimer;
    private int m_WaitCount;

    public RegistrationRequestWindow()
    {
      InitializeComponent();
      m_dispatcherTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnDispatcherTimer, this.Dispatcher);
      this.Loaded += new RoutedEventHandler(RegistrationRequestWindow_Loaded);
    }

    public static int TryCount
    {
      get{return s_TryCount.Value;}
      private set{s_TryCount.Value = value;}
    }

    void RegistrationRequestWindow_Loaded( object sender, RoutedEventArgs e )
    {
      int trycount = TryCount;
      TryCount = trycount + 1;

      m_WaitCount = trycount / s_IncFactor;
      m_WaitCount = Math.Min(Math.Max(m_WaitCount, 0), s_MaxWait);

      UpdateControls();
      if (m_WaitCount > 0)
      {
        m_dispatcherTimer.Start();
      }
    }

    private void OnDispatcherTimer( object sender, EventArgs e )
    {
      if (m_WaitCount == 0)
        return;
      if (--m_WaitCount == 0)
        m_dispatcherTimer.Stop();
      UpdateControls();
    }

    private void UpdateControls()
    {
      if (m_WaitCount > 0)
      {
        m_btOK.IsEnabled = false;
        m_btOK.Content = String.Format("Close ({0})", m_WaitCount);
      }
      else
      {
        m_btOK.IsEnabled = true;
        m_btOK.Content = "Close";
      }
    }

    private void m_btOK_Click( object sender, RoutedEventArgs e )
    {
      Close();
    }

    private void m_linkToFullVersion_Click( object sender, RoutedEventArgs e )
    {
      NavigationHelper.OpenWebPage(true, "http://www.eqatec.com/tools/tracer");
      e.Handled = true;
    }

    private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
    {
      e.Cancel = m_WaitCount > 0;
    }
  }
}