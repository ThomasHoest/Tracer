using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace SudokuLibrary
{
	public class Sudoku
	{		
		private Box[ , ] Boxs;
		private Box[ , ] SolutionBoxs;
		public event EventHandler Finish;
		bool IsNewGame;
		string error = string.Empty;
		Random rn;
		bool HasSolution = true;

		public Sudoku()
		{
			Boxs = new Box[ 3 , 3 ];
			rn = new Random();
			InitializeData();
		}


		public Cell this[ int x , int y ]
		{
			get
			{
				if ( x < 0 || x > 8 || y < 0 || y > 8 )
					throw new ApplicationException( "Invalid Values" );

				int X = x / 3;
				int Y = y / 3;
				int row = y % 3;
				int col = x % 3;

				return Boxs[ X , Y ][ col , row ];
			}
			set
			{
				error = string.Empty;
				if ( x < 0 || x > 8 || y < 0 || y > 8 )
					throw new ApplicationException( "Invalid x or y" );

				int X = x / 3;
				int Y = y / 3;
				int row = y % 3;
				int col = x % 3;

				if ( value.Value == 0 )
				{
					Boxs[ X , Y ][ col , row ] = value;
					return;
				}

				if ( IsValideColumn( X , Y , row , col , value ))
					if ( IsValideRow( X , Y , row , col , value ) )
					{
						try
						{
							Boxs[ X , Y ][ col , row ] = value;
						}
						catch ( ApplicationException ex)
						{

							error = ex.Message;
						}
					}
					else
						error = "Invalid Value, same number in row";
				else
					error =  "Invalid Value, same Number in column";

				if ( IsFinish && Finish != null )
					Finish( this , null );
			}
		}
		public SudokuRow[] DataSource
		{
			get
			{
				SudokuRow[] rows = new SudokuRow[ 9 ];

				rows[ 0 ] = GetRow( 0 );
				rows[ 1 ] = GetRow( 1 );
				rows[ 2 ] = GetRow( 2 );
				
				rows[ 3 ] = GetRow( 3 );
				rows[ 4 ] = GetRow( 4 );
				rows[ 5 ] = GetRow( 5 );

				rows[ 6 ] = GetRow( 6 );
				rows[ 7 ] = GetRow( 7 );
				rows[ 8 ] = GetRow( 8 );

				return rows;
			}
		}

		private void InitializeData()
		{
			Boxs[ 0 , 0 ] = new Box( 0 );
			Boxs[ 0 , 1 ] = new Box( 1 );
			Boxs[ 0 , 2 ] = new Box( 2 );

			Boxs[ 1 , 0 ] = new Box( 3 );
			Boxs[ 1 , 1 ] = new Box( 4 );
			Boxs[ 1 , 2 ] = new Box( 5 );

			Boxs[ 2 , 0 ] = new Box( 6 );
			Boxs[ 2 , 1 ] = new Box( 7 );
			Boxs[ 2 , 2 ] = new Box( 8 );
		}
		

		#region New Game Methods
		public void Solution()
		{
			if( HasSolution )
				Boxs = SolutionBoxs;
		}
		public void NewGame( int level )
		{
			IsNewGame = true;
			// Initialize Boxs & SolutionBoxs;
			Boxs = new Box[ 3 , 3 ];
			InitializeData();
			SolutionBoxs = Boxs;
			if( level != 0 )
			{
				FillSudoku();
				CopySolutionBoxs();
				DeleteCell( level * 15 );
				HasSolution = true;
			}
			else
			{
				HasSolution = false;
			}

			IsNewGame = false;
		}

		private void DeleteCell( int number)
		{
			for ( int i = 0 ; i < number ; i++ )
			{
				int index = rn.Next( 0 , 81 );
				int y = index / 9;
				int x = index % 9;
				int Col = x % 3;
				int row = y % 3;
				int bx = x / 3;
				int by = y / 3;
				Boxs[ Col , row ][ bx , by ] = new Cell();
			}
		}
		private void CopySolutionBoxs()
		{
			SolutionBoxs = new Box[ 3 , 3 ];
			for ( int x = 0 ; x < 3 ; x++ )
			{
				for ( int y = 0 ; y < 3 ; y++ )
				{
					SolutionBoxs[ x , y ] = new Box(-1);
					for ( int xx = 0 ; xx < 3 ; xx++ )
					{
						for ( int yy = 0 ; yy < 3 ; yy++ )
						{
							SolutionBoxs[ x , y ][xx,yy] = Boxs[ x , y ][xx,yy];
						}
					}
				}
			}
		}
		private Box FillBox( int name )
		{
			Box b = new Box( name );
			Cell[] c = GetRandom9Numbers();
			int cellIndex = 0;
			for ( int x = 0 ; x < 3 ; x++ )
			{
				for ( int y = 0 ; y < 3 ; y++ )
				{
					b[ x , y ] = c[ cellIndex++ ];
				}
			}
			return b;
		}
		private Cell[] GetRandom9Numbers()
		{
			Cell[] c = new Cell[ 9 ];
			ArrayList num = new ArrayList();

			for ( int i = 1 ; i < 10 ; i++ )
			{
				num.Add( i );
			}
			for ( int t = 0 ; t < 9 ; t++ )
			{
				int index = rn.Next( 1 , num.Count ) - 1;
				int v = (int)num[ index ];
				num.RemoveAt( index );
				c[ t ] = new Cell( v , Color.Red , StatusData.Application );
			}
			return c;
		}
		private void FillSudoku()
		{
			Cell[] line1 = GetRandom9Numbers();
			for ( int i = 0 ; i < 9 ; i++ )
			{
				this[ i , 0 ] = line1[ i ];
			}

			// Box 1
			this[ 0 , 1 ] = line1[ 3 ];
			this[ 1 , 1 ] = line1[ 4 ];
			this[ 2 , 1 ] = line1[ 5 ];

			this[ 0 , 2 ] = line1[ 6 ];
			this[ 1 , 2 ] = line1[ 7 ];
			this[ 2 , 2 ] = line1[ 8 ];
			// Box 2
			this[ 3 , 1 ] = line1[ 6 ];
			this[ 4 , 1 ] = line1[ 7 ];
			this[ 5 , 1 ] = line1[ 8 ];

			this[ 3 , 2 ] = line1[ 0 ];
			this[ 4 , 2 ] = line1[ 1 ];
			this[ 5 , 2 ] = line1[ 2 ];
			// Box 3
			this[ 6 , 1 ] = line1[ 0 ];
			this[ 7 , 1 ] = line1[ 1 ];
			this[ 8 , 1 ] = line1[ 2 ];

			this[ 6 , 2 ] = line1[ 3 ];
			this[ 7 , 2 ] = line1[ 4 ];
			this[ 8 , 2 ] = line1[ 5 ];

			// Box 4
			FillBoxByBox( 0 , 1 );
			// Box 5
			FillBoxByBox( 1 , 1 );
			// Box 6
			FillBoxByBox( 2 , 1 );
			// Box 7
			FillBoxByBox( 0 , 2 );
			// Box 8
			FillBoxByBox( 1 , 2 );
			// Box 9
			FillBoxByBox( 2 , 2 );
		}
		private void FillBoxByBox( int col , int row  )
		{
			col = col * 3;
			row = row * 3;
			int index = 0;

			for (int x = 0; x < 3 ; x++)
			{
				for ( int y = 0 ; y < 3 ; y++ )
				{
					index = ( col + x + 1 == 9 ? 0 : col + x + 1 );
					this[ col + x , row + y ] = this[ index , row + y - 3 ];
					this[ col + x , row + y ] = this[ index , row + y - 3 ];
					this[ col + x , row + y ] = this[ index , row + y - 3 ];
				}
			}
		}

		#endregion

		public bool IsFinishNumber( int number )
		{
			for( int x = 0 ; x < 3 ; x++ )
			{
				for( int y = 0 ; y < 3 ; y++ )
				{
					if( !Boxs[ x , y ].IsContainsNumber( number ) )
						return false;
				}
			}
			return true;
		}
		private bool IsValideRow( int x , int y , int row , int col , Cell value )
		{
			return !(
				Boxs[ 0 , y ].IsColumnContains( row , value ) ||
				Boxs[ 1 , y ].IsColumnContains( row , value ) ||
				Boxs[ 2 , y ].IsColumnContains( row , value ));
		}
		private bool IsValideColumn( int x , int y , int row , int col , Cell value )
		{
			return !(
				Boxs[ x , 0 ].IsRowContains( col , value ) ||
				Boxs[ x , 1 ].IsRowContains( col , value ) ||
				Boxs[ x , 2 ].IsRowContains( col , value ) );
		}
		
		public SudokuRow GetRow( int y )
		{
			SudokuRow row = new SudokuRow();
			for ( int i = 0 ; i < 9 ; i++ )
			{
				row.Cells[ i ] = this[ i , y ];
			}
			return row;
		}
		public void SetRow( int y  , SudokuRow row )
		{
			for ( int i = 0 ; i < 9 ; i++ )
			{
				this[ i , y ] =  row[i];
			}
		}

		
		#region Mark Methods
		public Point[] OptionCell( int number )
		{
			if ( number > 9 || number < 1 ) return null;

			ArrayList list = new ArrayList();
			for ( int y = 0 ; y < 9 ; y++ )
			{
				for ( int x = 0 ; x < 9 ; x++ )
				{
					if ( this[ x , y ].Value != 0 )
						continue;

					Cell tempCell = this[ x , y ];
					int row = y % 3;
					int col = x % 3;
					tempCell.Value = number;

					if ( IsValideRow( x / 3 , y / 3 , row , col , tempCell ) &&
						IsValideColumn( x / 3 , y / 3 , row , col , tempCell ) &&
						Boxs[ x / 3 , y / 3 ].IsValideValue( x % 3 , y % 3 , tempCell ) )
					{
						list.Add( new Point( x , y ) );
					}
				}
			}
			return (Point[])list.ToArray( typeof(Point) );
		}
		public Point[] NotOptionCell( int number )
		{
			if ( number > 9 || number < 1 ) return null;

			ArrayList list = new ArrayList();
			for ( int y = 0 ; y < 9 ; y++ )
			{
				for ( int x = 0 ; x < 9 ; x++ )
				{
					if ( this[ x , y ].Value == number )
					{
						list.Add( new Point( x , y ) );
					}
				}
			}
			return (Point[])list.ToArray( typeof(Point) );
		}
		public Point[] EmptyCells()
		{
			ArrayList list = new ArrayList();
			for ( int y = 0 ; y < 9 ; y++ )
			{
				for ( int x = 0 ; x < 9 ; x++ )
				{
					if ( this[ x , y ].Value == 0 )
					{
						list.Add( new Point( x , y ) );
					}
				}
			}
			return (Point[])list.ToArray( typeof(Point) );
		} 
		#endregion

		public bool IsFinish
		{
			get
			{
				if( IsNewGame )
					return false;

				for ( int x = 0 ; x < 9 ; x++ )
				{
					for ( int y = 0 ; y < 9 ; y++ )
					{
						if ( this[ x , y ].Value == 0 )
							return false;
					}
				}
				return true;
			}
		}		
		public string Error
		{
			get
			{
				return error;
			}
		}
	}

	#region Sudoku Row
public class SudokuRow
{
	public Cell[] Cells;

	public SudokuRow()
	{
		Cells = new Cell[ 9 ];
	}

	public Cell this[ int x ]
	{
		get
		{
			return Cells[ x ];
		}
		set
		{
			Cells[ x ] = value;
		}
	}

	public int Cell0
	{
		get
		{
			return Cells[ 0 ].Value;
		}
		set
		{
			Cells[ 0 ].Value = value;
		}
	}
	public int Cell1
	{
		get
		{
			return Cells[ 1 ].Value;
		}
		set
		{
			Cells[ 1 ].Value = value;
		}
	}
	public int Cell2
	{
		get
		{
			return Cells[ 2 ].Value;
		}
		set
		{
			Cells[ 2 ].Value = value;
		}
	}

	public int Cell3
	{
		get
		{
			return Cells[ 3 ].Value;
		}
		set
		{
			Cells[ 3 ].Value = value;
		}
	}
	public int Cell4
	{
		get
		{
			return Cells[ 4 ].Value;

		}
		set
		{
			Cells[ 4 ].Value = value;
		}
	}
	public int Cell5
	{
		get
		{
			return Cells[ 5 ].Value;
		}
		set
		{
			Cells[ 5 ].Value = value;
		}
	}

	public int Cell6
	{
		get
		{
			return Cells[ 6 ].Value;
		}
		set
		{
			Cells[ 6 ].Value = value;
		}
	}
	public int Cell7
	{
		get
		{
			return Cells[ 7 ].Value;
		}
		set
		{
			Cells[ 7 ].Value = value;
		}
	}
	public int Cell8
	{
		get
		{
			return Cells[ 8 ].Value;
		}
		set
		{
			Cells[ 8 ].Value = value;
		}
	}

} 
#endregion
}
