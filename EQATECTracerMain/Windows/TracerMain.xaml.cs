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
using System.Diagnostics;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;
using EQATEC.VersionCheckUtilities;
using EQATEC.SigningUtilities;
using System.Windows.Threading;
using EQATEC.Analytics.Monitor;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class TracerMain : Window
  {
    private PersistentVersion m_lastShownVersion = new PersistentVersion("LastShownVersion");
    PersistentString mTabState;

    private VersionAvailableEventArgs m_lastVersionCheckResult;
    private SigningUtilities.ISigningSettingRepository mSigningSettingsRepository;
    private delegate void VoidDelegate();

    public TracerMain()
    {
      //Unhandled exceptions
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      System.Windows.Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);

      InitializeComponent();

      AnalyticsMonitor.Instance.Monitor.Start();
      AnalyticsMonitor.Instance.Monitor.VersionAvailable += Monitor_VersionAvailable;            

      //EQATEC.Tracer.Utilities.WordColors.ExceptionBackgroundBrush = EQATEC.Tracer.Viewer.ViewerBrushes.ExceptionBackgroundBrush;
      WordColors.ExceptionBrush = ViewerBrushes.ExceptionBrush;
      WordColors.FunctionDataBrush = ViewerBrushes.FunctionDataBrush;
      WordColors.FunctionNameBrush = ViewerBrushes.FunctionNameBrush;
      WordColors.ThreadIDBrush = ViewerBrushes.ThreadIDBrush;
      WordColors.Log4NetTraceBrush = ViewerBrushes.Log4NetTraceBrush;
      WordColors.Log4NetLevelBrush = ViewerBrushes.Log4NetLevelBrush;
      WordColors.TimeBrush = ViewerBrushes.TimeBrush;
      WordColors.TypeBrush = ViewerBrushes.TypeBrush;
      WordColors.FlushBrush = ViewerBrushes.FlushBrush;
      WordColors.SelectedLineBrush = ViewerBrushes.SelectedLineBrush;
      WordColors.ReturnLeaveBrush = ViewerBrushes.ReturnLeaveBrush;

      
      mSigningSettingsRepository = new RegistrySigningSettingRepository();
      m_instrumentorControl.SetSigningSettingsRepository(mSigningSettingsRepository);
      m_versionControl.VersionNotificationClicked += new EventHandler(m_versionControl_VersionNotificationClicked);
          
      mTabState = new PersistentString("TabState", "0");
      m_tcTabs.SelectedIndex = int.Parse(mTabState.Value);

      throw new InvalidOperationException("Test");     
    }
    
    private void CloseTracer()
    {
      this.Dispatcher.Invoke(DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
                                                                                              {
                                                                                                try
                                                                                                {
                                                                                                  mTabState.Value = m_tcTabs.SelectedIndex.ToString();
                                                                                                }
                                                                                                catch (Exception)
                                                                                                {
                                                                                                }
                                                                                              });

      m_viewerControl.Close();
      m_instrumentorControl.Close();
    }

    #region Eventhandlers

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      CloseTracer();
    }

    private void m_instrumentorControl_OnAppInstrumented()
    {
      m_viewerControl.ResetViewer();
    }
        
    void Current_DispatcherUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs e )
    {
      GlobalExceptionHandler(sender, e.Exception);
    }

    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      GlobalExceptionHandler(sender, e.ExceptionObject);
    }

    object m_ErrorSync = new object();
    bool m_ErrorOccurred = false;
    private void GlobalExceptionHandler( object sender, object exceptionObj )
    {
      try
      {
        lock (m_ErrorSync)
        {
          if (m_ErrorOccurred)
            return;

          string newl = Environment.NewLine;
          Exception exc = exceptionObj as Exception;
          AnalyticsMonitor.Instance.Monitor.TrackException(exc);
          CloseTracer();
          Error showError = new Error();
          showError.ShowDialog();
          m_ErrorOccurred = true;
          this.Close();
        }
      }
      catch { }
    }


    private void m_instrumentorControl_OnApplicationRun()
    {
      m_tcTabs.SelectedIndex = 1;
    }

    #endregion   

    #region Version Checks

    private void ShowVersionNotificationWindow()
    {
      if (m_lastVersionCheckResult != null)
      {
        VersionNotificationWindow w = new VersionNotificationWindow();
        w.Owner = this;
        w.InitializeVersionCheckResult(m_lastVersionCheckResult);
        w.ShowDialog();
      }
    }

    void m_versionControl_VersionNotificationClicked( object sender, EventArgs e )
    {
      ShowVersionNotificationWindow();
    }

    void Monitor_VersionAvailable(object sender, VersionAvailableEventArgs e)
    {
      if (Dispatcher.CheckAccess())
      {
        try
        {          
          m_lastVersionCheckResult = e;
          // first time we see a new versiono weshould display a form and 
          // on subsequent checks we should only display a small notice
          if (m_lastShownVersion.Value < e.OfficialVersion)
          {
            m_lastShownVersion.Value = e.OfficialVersion;
            ShowVersionNotificationWindow();
          }

          //show the version check
          m_versionPanel.Visibility = Visibility.Visible;
                    
        }
        catch (Exception exc)
        {
          Trace.WriteLine("Failed to handle the result from the version check. Error message is " + exc.Message);
        }
      }
      else
      {
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidDelegate(delegate
                                                                                                 {
                                                                                                   try
                                                                                                   {
                                                                                                     Monitor_VersionAvailable(sender, e);
                                                                                                   }
                                                                                                   catch (Exception)
                                                                                                   {
                                                                                                   }
                                                                                                 }));
      }

    }

    //void versionChecker_VersionCheckCompleted( object sender, EQATEC.VersionCheckUtilities.VersionCheckEventArgs e )
    //{
    //  if (Dispatcher.CheckAccess())
    //  {
    //    try
    //    {
    //      if (e.VersionCheckResult.CheckSuccessful)
    //      {
    //        m_lastVersionCheckResult = e.VersionCheckResult;
    //        if (e.VersionCheckResult.ShouldUpdate)
    //        {
    //          // first time we see a new versiono weshould display a form and 
    //          // on subsequent checks we should only display a small notice
    //          if (m_lastShownVersion.Value < e.VersionCheckResult.LatestVersion)
    //          {
    //            m_lastShownVersion.Value = e.VersionCheckResult.LatestVersion;
    //            ShowVersionNotificationWindow();
    //          }

    //          //show the version check
    //          m_versionPanel.Visibility = Visibility.Visible;
    //        }
    //      }
    //    }
    //    catch (Exception exc)
    //    {
    //      Trace.WriteLine("Failed to handle the result from the version check. Error message is " + exc.Message);
    //    }
    //  }
    //  else
    //  {
    //    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidDelegate(delegate
    //                                                                                             {
    //                                                                                               try
    //                                                                                               {
    //                                                                                                 versionChecker_VersionCheckCompleted(sender, e);
    //                                                                                               }
    //                                                                                               catch (Exception)
    //                                                                                               {
    //                                                                                               }
    //                                                                                             }));
    //  }
    //}
    #endregion

    #region HelperClass
    private class PersistentVersion : Persistent<Version>
    {
      public PersistentVersion( string name )
        : base(DefaultRegistryRoot, name, new Version(0, 0, 0))
      {
      }

      protected override Version ConvertFromString( string input )
      {
        try
        {
          if(input != null)
            return new Version(input);
        }
        catch { }
        return new Version(0, 0, 0);
      }

      protected override string ConvertToString( Version input )
      {
        return input.ToString(3);
      }
    }

    #endregion

    #region Menu EventHandlers
    private void MenuItem_Exit( object sender, RoutedEventArgs e )
    {
      this.Close();
    }

    private void MenuItem_UserGuide( object sender, RoutedEventArgs e )
    {
      NavigationHelper.OpenWebPage(false, ReleaseInfo.GetPathToUserGuideIndex());
    }

    private void MenuItem_Forum( object sender, RoutedEventArgs e )
    {
      NavigationHelper.OpenWebPage(true, ReleaseInfo.GetOnlineForumUrl());
    }

    private void MenuItem_About( object sender, RoutedEventArgs e )
    {
      Windows.AboutBoxWindow aboutWindow = new AboutBoxWindow();
      aboutWindow.Owner = this;
      aboutWindow.ShowDialog();
    }

    private void MenuItem_ManageSigningSettings( object sender, RoutedEventArgs e )
    {
      using (SigningUtilities.UI.SignedAssemblyCacheForm sForm = new EQATEC.SigningUtilities.UI.SignedAssemblyCacheForm(mSigningSettingsRepository))
      {
        sForm.ShowDialog();
        m_instrumentorControl.RefreshFileList();
      }
    }

    #endregion        
        
  }
}