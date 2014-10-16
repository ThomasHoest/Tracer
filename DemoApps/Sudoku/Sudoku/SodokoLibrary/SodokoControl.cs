using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading;
using SudokuLibrary;
using SudokuGame;

namespace SudokuLibrary
{
	public class SudokuControl : UserControl
	{
		#region Fields
		int CELL = 40;
		const int widthLine = 10;
		Sudoku data;
		TextBox txb;
		Point[] MarkCell;
		Point[] MarkNotCell;
		Point[] MarkMissing;
		MarkCellEnum IsMarkCell = MarkCellEnum.Clean;
		Stack Undo;
		Rectangle MouseCell;
		Point MousePoint;
		PictureCollection pictures = new PictureCollection();

		public int m_MarkNumber;
		public int MarkNumber
		{
			get{ return m_MarkNumber; }
			set
			{ 
				m_MarkNumber  = value;
				switch( m_MarkNumber )
				{
					case 1:
						this.Cursor = pictures.One;
						break;
					case 2:
						this.Cursor = pictures.Two;
						break;
					case 3:
						this.Cursor = pictures.Three;
						break;

					case 4:
						this.Cursor = pictures.Four;
						break;
					case 5:
						this.Cursor = pictures.Five;
						break;
					case 6:
						this.Cursor = pictures.Six;
						break;

					case 7:
						this.Cursor = pictures.Seven;
						break;
					case 8:
						this.Cursor = pictures.Eight;
						break;
					case 9:
						this.Cursor = pictures.Nine;
						break;
				}
			}
		}
		public delegate void NumberFinishEventHandler(object sender , NumberFinishEventArgs e );
		public delegate void ErrorEventHandler(object sender , ErrorEventArgs e );
		public event ErrorEventHandler Error;
		public event NumberFinishEventHandler NumberFinish;
		public event NumberFinishEventHandler NumberNotFinish;
		
		public DateTime StartTime;
		public event EventHandler Finish;
		#endregion		

		public SudokuControl()
		{
			InitializeComponent();
		
			CELL = ( this.Width - widthLine ) / 9;

			data = new Sudoku();
			Undo = new Stack();

			txb = new TextBox();
			txb.Visible = false;
			txb.KeyUp += new KeyEventHandler( txb_KeyUP );
			this.Controls.Add( txb );
		}


		#region Events
		private void SetCell( int number )
		{
			txb.Visible = false;
			int x = txb.Location.X;
			int y = txb.Location.Y;
			x = x / CELL;
			y = y / CELL;

			// Delete Number if TextBox == " "
			if ( txb.Text == " " &&
				data[ x , y ].Status != StatusData.Application )
			{
				Cell c = new Cell( 0 , Color.Green , StatusData.User );
				if( NumberNotFinish != null )
					NumberNotFinish( this , 
						new NumberFinishEventArgs( 	data[ x , y ].Value ) );
				data[ x , y ] = c;
				Undo.Push( new UndoItem( x , y , c ) );
				CalcMarkCell();
				this.Invalidate();
				return;
			}
			

			if ( number > 0 && number < 10 )
			{
				if ( data[ x , y ].Status != StatusData.Application )
				{
					Cell c = new Cell( number , Color.Green , StatusData.User );
					Cell UndoCell = data[ x , y ];
					data[ x , y ] = c;

					if ( data.Error != string.Empty )
					{
						OnError( data.Error );
					}
					else
					{
						Undo.Push( new UndoItem( x , y , UndoCell ) );
					}
					CalcMarkCell();
					if( IsFinishNumber( number ) && NumberFinish != null )
						NumberFinish( this , new NumberFinishEventArgs( number ) );

				}
				else
					OnError( "You Can't Modify The Value" );
			}
			else
			{
				number = 0;
				OnError( "Only Numbers [ 1 - 9 ] or Space Key" );
			}
			txb.Text = string.Empty;
		}
		void txb_KeyUP( object sender , KeyEventArgs e )
		{
			int v;

			// try to convert to number
			try
			{
				v = int.Parse( txb.Text );
				SetCell( v );
			}
			catch ( Exception ex )
			{
				OnError( ex.Message );
				v = 0;
				txb.Text = string.Empty;
				return;				
			}

		}
		void SudokuControl_MouseDown( object sender , MouseEventArgs e )
		{
			int x = e.X - widthLine;
			int y = e.Y - widthLine;
			x = x / CELL;
			y = y / CELL;

			txb.Size = new Size( CELL - 9 , CELL );
			txb.Location = new Point( x * CELL + widthLine , y * CELL + widthLine );
			//txb.Text = MarkNumber.ToString();
			//txb.Visible = true;
			//txb.Focus();
			SetCell( MarkNumber );
		}
		
		void OnError( string error )
		{
			if( Error != null )
			{
				Error( this , new ErrorEventArgs( error ) );
			}
		}
		#endregion

		#region Override Methods
		protected override void OnPaint( PaintEventArgs e )
		{
			Graphics gp = e.Graphics;
			DrawCleanArea( gp );			
			DrawMarkNotCell( gp );
			DrawMarkEmptCell( gp );
			DrawMarkCell( gp );
			DrawSudoku( gp );
			DrawData( gp );
		}
		protected override void OnSizeChanged( EventArgs e )
		{

			this.Invalidate();
		}
		
		protected override void OnMouseMove( MouseEventArgs e )
		{
			int x = e.X - widthLine;
			int y = e.Y - widthLine;
			x = x / CELL;
			y = y / CELL;

			Point p = new Point( x , y );
			if( MousePoint != p )
			{
				DrawMouseCell( Color.Black );
				MousePoint = new Point( x , y );
				MouseCell = GetCellRectangle2( x , y );
				DrawMouseCell( Color.White );
			}
		}
		#endregion

		#region Draw Methods
		private void DrawCleanArea( Graphics g )
		{
			CELL = ( this.Width - widthLine ) / 9;
			this.Size = new Size( CELL * 9 + widthLine , CELL * 9 + widthLine );

			g.FillRectangle( new SolidBrush( Color.FromArgb(234,241,251) ) , new Rectangle( 0 , 0 , this.Width , this.Height ) ) ;
		}
		private void DrawData( Graphics g )
		{
			if ( data == null )
				return;

			for ( int y = 0 ; y < 9 ; y++ )
			{
				SudokuRow row = data.GetRow( y );
				for ( int x = 0 ; x < 9 ; x++ )
				{
					if( row[x].Value != 0 )
						SetData( x , y , row[ x ] );

				}
			}
		}
		private void DrawSudoku( Graphics g )
		{
			int wl = widthLine / 2;
			CELL = ( this.Width - widthLine ) / 9;
			this.Size = new Size( CELL * 9 + widthLine , CELL * 9 + widthLine );

			// Set the smoothing mode of the surface
			g.SmoothingMode = SmoothingMode.AntiAlias;

			Pen pen = new Pen( Color.FromArgb(47,117,232) , widthLine );
			g.DrawRectangle( pen , new Rectangle( 0 , 0 , this.Width , this.Height ) );

			pen.Width = widthLine / 2;
			// Draw shapes and lines
			Pen penBlack = Pens.Black;
			int index = 0;
			for ( int i = wl ; i < this.Width ; i += CELL )
			{	// V Line
				if ( index != 3 && index != 6 )
					g.DrawLine( penBlack , 0 + wl , i , this.Width - wl , i );
				else
					g.DrawLine( pen , 0 , i , this.Width , i );
				index++;
			}
			index = 0;
			for ( int i = wl ; i < this.Width ; i += CELL )
			{	// H Line
				if ( index != 3 && index != 6 )
					g.DrawLine( penBlack , i , 0 + wl , i , this.Height - wl );
				else
					g.DrawLine( pen , i , 0 , i , this.Height );
				index++;

			}

		}
		private void DrawMarkCell( Graphics g )
		{
			if ( IsMarkCell == MarkCellEnum.Good )
			{
				foreach ( Point p in MarkCell )
				{
					g.FillRectangle( new SolidBrush( Color.Yellow ) ,
						GetCellRectangle( p.X , p.Y ) );
				}
			}
		}
		private void DrawMarkNotCell( Graphics g )
		{
			if ( IsMarkCell == MarkCellEnum.Bad )
			{
				foreach ( Point p in MarkNotCell )
				{
					for ( int x = 0 ; x < 9 ; x++ )
					{
						g.FillRectangle( new SolidBrush( Color.WhiteSmoke ) ,
							GetCellRectangle( x , p.Y ) );
					}
					for ( int y = 0 ; y < 9 ; y++ )
					{
						g.FillRectangle( new SolidBrush( Color.WhiteSmoke ) ,
							GetCellRectangle( p.X , y ) );
					}
				}
			}
		}
		private void DrawMarkEmptCell( Graphics g )
		{
			if ( IsMarkCell == MarkCellEnum.Missing )
			{
				foreach ( Point p in MarkMissing )
				{
					g.FillRectangle( 
						new SolidBrush( Color.White ) ,
						GetCellRectangle(p.X ,p.Y) );					
				}
			}
		}
		private void DrawMouseCell( Color c )
		{
			using( Graphics g = this.CreateGraphics() )
				g.DrawRectangle( new Pen( c ) ,	MouseCell );
			
		}
		private Rectangle GetCellRectangle( int x , int y )
		{
			int xx = x * CELL + widthLine / 2 + 1;
			int yy = y * CELL + widthLine / 2 + 1;
			int width  = CELL - 1;
			int height = CELL - 1;
			#region Calc Cell
			switch ( x )
			{
				case 2:
					width -= 2;
					break;
				case 3:
					xx += 2;
					width -= 2;
					break;
				case 5:
					width -= 2;
					break;
				case 6:
					xx += 2;
					width -= 2;
					break;
			}
			switch ( y )
			{
				case 2:
					height -= 2;
					break;
				case 3:
					yy += 2;
					height -= 2;
					break;
				case 5:
					height -= 2;
					break;
				case 6:
					yy += 2;
					height -= 2;
					break;
			} 
			#endregion
			return new Rectangle( xx , yy , width , height );
		}
		private Rectangle GetCellRectangle2( int x , int y )
		{
			int xx = x * CELL + widthLine / 2 + 1;
			int yy = y * CELL + widthLine / 2 + 1;
			int width  = CELL - 1;
			int height = CELL - 1;
			#region Calc Cell
			switch ( x )
			{
				case 0:
					xx -= 1;
					width += 1;
					break;
				case 1:
					xx -= 1;
					width += 1;
					break;
				case 2:
					xx -= 1;
					width -= 1;
					break;
				case 3:
					xx += 1;
					width -= 1;
					break;
				case 4:
					xx -= 1;
					width += 1;
					break;
				case 5:
					xx -= 1;
					width -= 1;
					break;
				case 6:
					xx += 1;
					width -= 1;
					break;
				case 7:
					xx -= 1;
					width += 1;
					break;
				case 8:
					xx -= 1;
					width += 1;
					break;
			}
			switch ( y )
			{
				case 0:
					yy -= 1;
					height += 1;
					break;
				case 1:
					yy -= 1;
					height += 1;
					break;
				case 2:
					yy -= 1;
					height -= 1;
					break;
				case 3:
					yy += 1;
					height -= 1;
					break;
				case 4:
					yy -= 1;
					height += 1;
					break;
				case 5:
					yy -= 1;
					height -= 1;
					break;
				case 6:
					yy += 1;
					height -= 1;
					break;
				case 7:
					yy -= 1;
					height += 1;
					break;
				case 8:
					yy -= 1;
					height += 1;
					break;
			} 
			#endregion
			return new Rectangle( xx , yy , width , height );
		}
		#endregion

		private void SetData( int x , int y , Cell cell )
		{
			int pad = CELL / 3;
			Graphics g		= this.CreateGraphics();
			FontFamily arialFamily = new FontFamily( "Arial" );
			Font arialFont	= new Font( arialFamily , 22 , FontStyle.Bold );
			SolidBrush sb	= new SolidBrush( cell.Color );
			if ( cell.Status == StatusData.Application && cell.Value != 0 )
			{
				g.FillRectangle( new SolidBrush( Color.FromArgb(123,152,225)) ,
					GetCellRectangle( x , y ));
			}
			PointF point = new PointF( x * CELL + pad , y * CELL + pad );
			g.DrawString( cell.Value.ToString() , arialFont , sb , point );

			// Dispose of objects
			sb.Dispose();
			g.Dispose();

		}
		public void UndoAction()
		{
			if ( Undo.Count != 0 )
			{
				UndoItem item = (UndoItem)Undo.Pop();
				if( NumberNotFinish != null )
					NumberNotFinish( this , 
						new NumberFinishEventArgs( 	item.cell.Value ) );
				data[ item.x , item.y ] = item.cell;
				CalcMarkCell();
				this.Invalidate();
			}
		}


		#region Mark Methods
		public void MarkCellByNumber( int number )
		{
			IsMarkCell = MarkCellEnum.Good;
			MarkNumber = number;
			MarkCell = data.OptionCell( number );
			this.Invalidate();
		}
		public void MarkCellByBadNumber( int number )
		{
			IsMarkCell  = MarkCellEnum.Bad;
			MarkNumber  = number;
			MarkNotCell = data.NotOptionCell( number );
			this.Invalidate();
		}
		public void MarkMissingCell()
		{
			IsMarkCell  = MarkCellEnum.Missing;
			MarkMissing = data.EmptyCells();
			this.Invalidate();
		}
		public void CleanMarkCell()
		{
			IsMarkCell  = MarkCellEnum.Clean;
			MarkCell    = null;
			MarkNotCell = null;
			this.Invalidate();
		}
		void CalcMarkCell()
		{
			switch ( IsMarkCell )
			{
				case MarkCellEnum.Bad:
					MarkCellByBadNumber( (int)MarkNumber );
					break;
				case MarkCellEnum.Good:
					MarkCellByNumber( (int)MarkNumber );
					break;
				case MarkCellEnum.Clean:
					CleanMarkCell();
					break;
			}
		} 
		#endregion

		private void InitializeComponent()
		{
			// 
			// SudokuControl
			// 
			this.Name = "SudokuControl";
			this.Size = new System.Drawing.Size(424, 424);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txb_KeyUP);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SudokuControl_MouseDown);

		}

		public void NewGame( int level )
		{
			data.NewGame( level );
			StartTime    = DateTime.Now;
			data.Finish += new EventHandler(data_Finish);
			CheckForFinishNumber();			
		}
		public void Solution()
		{
			data.Solution();
			this.Invalidate();
		}

		private void data_Finish(object sender, EventArgs e)
		{
			if( Finish != null )
				Finish( this , null );
		}
		public bool IsFinishNumber( int number )
		{
			return data.IsFinishNumber( number );
		}
		public void CheckForFinishNumber()
		{
			for( int i = 1 ; i < 10 ; i++ )
			{
				if( IsFinishNumber( i ) )
					OnNumberFinish( i );
			}
		}
		public void OnNumberFinish( int i )
		{
			if( NumberFinish != null )
				NumberFinish( this , new NumberFinishEventArgs( i ) );
		}

	}

	#region Helper Class
	public enum MarkCellEnum
	{
		Clean,
		Bad,
		Good,
		Missing		
	}
	public class UndoItem
	{
		public int x;
		public int y;
		public Cell cell;

		public UndoItem( int x , int y , Cell cell )
		{
			this.x = x;
			this.y = y;
			this.cell = cell;
		}
	}
	public class NumberFinishEventArgs : EventArgs
	{
		int m_Number;

		public NumberFinishEventArgs( int number )
		{
			this.Number = number;
		}

		public int Number
		{
			get{ return m_Number;}
			set{ m_Number = value;}
		}

	}
	public class ErrorEventArgs : EventArgs
	{
		public string Error;

		public ErrorEventArgs( string error )
		{
			this.Error = error;
		}
	}
	#endregion
}
