using System;
using System.Collections;
using System.Text;

namespace SudokuLibrary
{
	public struct Box
	{
		private int Name;
		private System.Collections.Hashtable ht;
		private Cell[ , ] m_Cells;

		public Box( int name )
		{
			ht = new Hashtable( 9 );
			m_Cells = new Cell[ 3 , 3 ];
			Name = name;
			InitializeData();
		}

		private void InitializeData()
		{
			Cells[ 0 , 0 ] = new Cell();
			Cells[ 0 , 1 ] = new Cell();
			Cells[ 0 , 2 ] = new Cell();

			Cells[ 1 , 0 ] = new Cell();
			Cells[ 1 , 1 ] = new Cell();
			Cells[ 1 , 2 ] = new Cell();

			Cells[ 2 , 0 ] = new Cell();
			Cells[ 2 , 1 ] = new Cell();
			Cells[ 2 , 2 ] = new Cell();
		}
		public Cell[,] Cells
		{
			get
			{
				return m_Cells;
			}
			set
			{
				m_Cells = value;
			}
		}
		public Cell this[ int x , int y ]
		{
			get{ return Cells[ x , y ]; }
			set
			{
				if ( value.Value == 0 )
				{
					ht.Remove( Cells[ x , y ].Value );
					Cells[ x , y ] = value;
					return;
				}

				if ( IsValideValue( x , y , value ) )
				{
					Cells[ x , y ] = value;
					ht.Add( value.Value , value );
				}
				else
					throw new ApplicationException( "The number exsist in box " );
			}
		}
				
		
		public bool IsContainsNumber( int number )
		{
			return ht.ContainsKey( number );
		}
		public bool IsValideValue( int x , int y , Cell value )
		{
			if ( value.Value < 1 || value.Value > 9 ||
				x < 0 || x > 2 ||
				y < 0 || y > 2
			  )
				return false;
			return !ht.ContainsKey( value.Value );
		}
		public bool IsRowContains( int row , Cell value )
		{
			return Cells[ row , 0 ] == value ||
				   Cells[ row , 1 ] == value ||
				   Cells[ row , 2 ] == value;
 
		}
		public bool IsColumnContains( int col , Cell value )
		{
			return Cells[ 0 , col ] == value ||
				   Cells[ 1 , col ] == value ||
				   Cells[ 2 , col ] == value;

		}
	}
}
