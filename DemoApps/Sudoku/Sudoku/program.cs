using System;
using System.Collections;
using System.Windows.Forms;

namespace SudokuGame
{
	 public class Program
	{

		[STAThread]
		static void Main()
		{
			//Application.EnableVisualStyles();
			Application.Run( new MainForm() );
		}
	}
}