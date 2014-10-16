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

using EQATEC.Tracer.Utilities;

namespace EQATEC.Tracer.TracerInstrumentor
{
  /// <summary>
  /// Interaction logic for SelectExecutable.xaml
  /// </summary>
  public partial class SelectExecutable : Window
  {
    private static AssemblyContainer mSelectedAssembly = null;

    public static AssemblyContainer SelectedAssembly
    {
      get { return SelectExecutable.mSelectedAssembly; }
      set 
      { 
        SelectExecutable.mSelectedAssembly = value;
        if (mThisWindow != null)
          mThisWindow.Close();
      }
    }

    private static SelectExecutable mThisWindow = null;
   
    public SelectExecutable(List<AssemblyContainer> executables)
    {
      mThisWindow = this;
      InitializeComponent();
      m_lbExecutables.DataContext = executables;
    }

    private void m_btCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
