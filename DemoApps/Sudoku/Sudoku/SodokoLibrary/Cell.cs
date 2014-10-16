using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace SudokuLibrary
{
	public struct Cell : IComparable
	{
		private int m_Value;		
		public  int Value
		{
			get
			{
				return m_Value;
			}
			set
			{
				if ( value >= 1 && value <= 9 )
					m_Value = value;
				else
					m_Value = 0;
			}
		}

		public Color		Color;
		public StatusData	Status;

		public Cell( int val , Color color , StatusData status )
		{
			this.m_Value	= val;
			this.Color		= color;
			this.Status		= status;
		}

		
		#region IComparable Members
		public int CompareTo( object other )
		{

			int val  = this.Value;
			int oval = ((Cell)other).Value;
			return val.CompareTo( oval );
		}
		#endregion

		public static bool operator ==( Cell a , Cell b )
		{
			return a.Value == b.Value;
		}
		public static bool operator !=( Cell a , Cell b )
		{
			return a.Value != b.Value;
		}
    }

	public enum StatusData
	{
		User ,
		Application
	}
}
