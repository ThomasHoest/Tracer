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
using System.Threading;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for SelectExecutable.xaml
  /// </summary>
  public partial class Connection : Window
  {    
    AssemblyViewer mView;
    string mIP;
    int mPort;
    bool mAttemptConnection;

    bool mSuccess = false;

    public bool Success
    {
      get { return mSuccess; }
      set { mSuccess = value; }
    }

    public Connection(AssemblyViewer handler, string ip, int port)
    {
      InitializeComponent();
      mView = handler;
      mIP = ip;
      m_tbAddress.Text = "Connecting to " + ip;
      mPort = port;
      mAttemptConnection = true;
      ThreadPool.QueueUserWorkItem(new WaitCallback(AttemptConnection));
    }

    private void m_btCancel_Click(object sender, RoutedEventArgs e)
    {
      mAttemptConnection = false;
      Thread.Sleep(500);
      this.Close();
    }
    
    private void AttemptConnection(object state)
    {
      while (mAttemptConnection)
      {
        if (mView.ConnectToTarget(mIP, mPort))
        {
          mSuccess = true;
          this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Windows.Forms.MethodInvoker)delegate()
          {
            this.Close();
          });
          return;
        }
        else
          Thread.Sleep(100);
      }
    }
  }
}
