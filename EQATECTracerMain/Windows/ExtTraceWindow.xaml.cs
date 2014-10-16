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
using EQATEC.Tracer.Tools;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for ExtTraceWindow.xaml
  /// </summary>
  public partial class ExtTraceWindow : Window
  {
    public AssemblyViewer WindowViewHandler
    {
      get
      {
        return m_viewerControl.ViewHandler;
      }
    }

    int mThreadID;

    public int ThreadID
    {
      get { return mThreadID; }
    }

    public ExtTraceWindow(int threadID, Dictionary<int, MemberContainer> functionDictionary)
    {
      InitializeComponent();
      mThreadID = threadID;
      m_viewerControl.SetDroneState();
      m_viewerControl.ViewHandler.FunctionDictionary = functionDictionary;


    }
  }
}
