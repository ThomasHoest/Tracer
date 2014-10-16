using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Bubbles
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      using (BubbleForm window = new BubbleForm())
      {
        window.Init(window.ClientSize.Width, window.ClientSize.Height);
        MessageBox.Show(window, "Left click form to toggle motion and right click to toggle bubble color");
        window.RunMe();
      }
    }
  }
}
