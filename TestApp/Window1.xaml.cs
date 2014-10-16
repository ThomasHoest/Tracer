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
using System.Windows.Threading;
using System.Threading;
//using EQATEC.Tracer.TracerRuntime;

namespace TestApp
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Window1 : Window
  {
    public Window1()
    {
      //System.AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      //System.Windows.Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
      try
      {
        InitializeComponent();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Message: " + ex.Message + " Stack " + ex.StackTrace);

      }
      //EQATEC.Tracer.TracerRuntime.TracerRuntime.FirstCallInit();
    }

    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      MessageBox.Show("Unhandled exception caught");
    }

    void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      MessageBox.Show("Unhandled exception caught in ui thread");
    }

    private void m_btThread1_Click(object sender, RoutedEventArgs e)
    {
      if (!mLazyThreadRunning)
      {
        mLazyThreadRunning = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(LazyThread));
      }
      else
        mLazyThreadRunning = false;
    }

    bool mLazyThreadRunning = false;

    private void LazyThread(object state)
    {
      int counter = 0;
      while (mLazyThreadRunning)
      {
        DoSomeLazyWork(counter++);
        Thread.Sleep(500);
      }
    }

    private void DoSomeLazyWork(int times)
    {
      Console.WriteLine("LazyThread: Working hard or hardly working..");
      Console.WriteLine("LazyThread: Going back to sleep for the " + times + " time");
      SomeClass sc = new SomeClass();
      sc.TellMeYourNamePlease();
      sc.SomeFunction();
    }

    private void m_btFunc1_Click(object sender, RoutedEventArgs e)
    {
      Log.Logger.Info("Func 1 called");
      SomeClass aClass = new SomeClass();//"Sir Lancelot");
      m_lbMessages.Items.Add("My local class says: " + aClass.TellMeYourNamePlease());
      aClass.SomeFunction();
    }

    private void m_btThread2_Click(object sender, RoutedEventArgs e)
    {
      if (!mBusyThreadRunning)
      {
        mBusyThreadRunning = true;
        ThreadPool.QueueUserWorkItem(new WaitCallback(BusyThread));
      }
      else
        mBusyThreadRunning = false;

    }

    bool mBusyThreadRunning = false;

    private void BusyThread(object state)
    {
      int counter = 0;
      while (mBusyThreadRunning)
      {
        DoSomeBusyWork(counter++);
        Thread.Sleep(10);
      }
    }

    private void DoSomeBusyWork(int times)
    {     
    }

    private bool ReturnJustABool()
    {
      return true;
    }

    private string ReturnJustAString()
    {
      return "Test";
    }


    private void m_btFunc2_Click(object sender, RoutedEventArgs e)
    {
      //RuntimeLog log = new RuntimeLog();
      //log.Name = "test.txt";
      //log.SetLevel(TracerRuntimeLoggerLevel.Info);
      //log.Start();

      //for (int i = 0; i < 100; i++)
      //{
      //  log.Info("Test + " + i.ToString());
      //}

      //log.Info("The end");

      //Log.Logger.Info("Func 2 called");
      ClassWithAnonymous aclass = new ClassWithAnonymous();
      aclass.SomeFunctionWithAnonymousDelegate();
      
      Utillities.AnotherClass aClass = new Utillities.AnotherClass();//"Sir Lancelot");
      m_lbMessages.Items.Add("My local class says: " + aClass.Reply);
      SomeClass sm = new SomeClass();
      string [] ar = sm.ReturnSomeLargeArray();
      //sm.DeepException();
    }

    //RuntimeTalkerServer mServer;

    private void m_btStartServer_Click(object sender, RoutedEventArgs e)
    {/*
      mServer = new RuntimeTalkerServer();
      mServer.StartServer(6001);
      mServer.OnCommandReceived += new RuntimeTalkerServer.CommandReceivedHandler(mServer_OnCommandReceived);
      TracerRuntime.RegisterMethod(0);
      TracerRuntime.EnableLogging(0);
      TracerRuntime.DoFunctionTrace(0);
      TracerRuntime.Trace(0);*/
    }
    /*
    void mServer_OnCommandReceived(ControlCommand cmd)
    {
      DataMessage ms = new DataMessage(RuntimeActionType.AssemblyData, "This is a test");
      mServer.SendMessage(ms);
    }*/

    private void m_btUException_Click(object sender, RoutedEventArgs e)
    {
      throw new InvalidOperationException("Test");
    }

    private void m_btHException_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        throw new InvalidOperationException();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Caught exception: " + ex.Message);
      }
    }

    private void ExceptionThread(object state)
    {
      throw new ThreadInterruptedException();
    }

    private void m_btThreadException_Click(object sender, RoutedEventArgs e)
    {
      ThreadPool.QueueUserWorkItem(new WaitCallback(ExceptionThread));
    }

  }
}
