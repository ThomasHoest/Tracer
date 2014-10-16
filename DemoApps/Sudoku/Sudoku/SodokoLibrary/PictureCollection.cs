using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.IO;

namespace SudokuGame
{
	public class PictureCollection : System.Windows.Forms.UserControl
	{
		#region Fields
		private System.Windows.Forms.PictureBox picMark;
		private System.Windows.Forms.PictureBox picMarkRollOver;
		private System.Windows.Forms.PictureBox picMarkDown;
		private System.Windows.Forms.PictureBox picSolutionRollOver;
		private System.Windows.Forms.PictureBox picUndo;
		private System.Windows.Forms.PictureBox picUndoRollOver;
		private System.Windows.Forms.PictureBox picLevels;
		private System.Windows.Forms.PictureBox picLevelsRollOver;
		private System.Windows.Forms.PictureBox picSend;
		private System.Windows.Forms.PictureBox picSendRollOver;
		private System.Windows.Forms.PictureBox picHelp;
		private System.Windows.Forms.PictureBox picHelpRollOver;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.PictureBox picNewGame;
		private System.Windows.Forms.PictureBox picNewGameRollOver;
		private System.Windows.Forms.PictureBox picSolution;
		#endregion				

		ResourceManager resources;
		static Assembly		assem;

		public PictureCollection()
		{
			resources = new ResourceManager(typeof(PictureCollection));
			assem = this.GetType().Assembly;

			InitializeComponent();
		}

		
		#region Pictures Properties
		public Image GetPicture( string key )
		{
			return (Image)( resources.GetObject( key + ".Image" ) );
		}

		public Image NewGame
		{
			get{return GetPicture( "picNewGame" );}
		}
		public Image NewGameRollOver
		{
			get{return GetPicture( "picNewGameRollOver" );}
		}
		
		public Image Solution
		{
			get{return GetPicture( "picSolution" );}
		}
		public Image SolutionRollOver
		{
			get{return GetPicture( "picSolutionRollOver" );}
		}
		
		public Image Undo
		{
			get{return GetPicture( "picUndo" );}
		}
		public Image UndoRollOver
		{
			get{return GetPicture( "picUndoRollOver" );}
		}
		public Image Levels
		{
			get{return GetPicture( "picLevels" );}
		}
		public Image LevelsRollOver
		{
			get{return GetPicture( "picLevelsRollOver" );}
		}
		public Image Send
		{
			get{return GetPicture( "picSend" );}
		}
		public Image SendRollOver
		{
			get{return GetPicture( "picSendRollOver" );}
		}
		public Image Help
		{
			get{return GetPicture( "picHelp" );}
		}
		public Image HelpRollOver
		{
			get{return GetPicture( "picHelpRollOver" );}
		}
		
		public Image Mark
		{
			get{return GetPicture( "picMark" );}
		}
		public Image MarkDown
		{
			get{return GetPicture( "picMarkDown" );}
		}
		public Image MarkRollOver
		{
			get{return GetPicture( "picMarkRollOver" );}
		}
		
		#endregion

		#region Cursor Properties
		public Cursor One
		{
			get
			{
				return
          new Cursor( assem.GetManifestResourceStream( "SudokuGame.Cursor.1.cur" ) );
			}
		}
		public Cursor Two
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.2.cur"));
			}
		}
		public Cursor Three
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.3.cur"));
			}
		}
		public Cursor Four
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.4.cur"));
			}
		}
		public Cursor Five
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.5.cur"));
			}
		}
		public Cursor Six
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.6.cur"));
			}
		}
		public Cursor Seven
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.7.cur"));
			}
		}
		public Cursor Eight
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.8.cur"));
			}
		}
		public Cursor Nine
		{
			get
			{
				return 
					new Cursor( assem.GetManifestResourceStream("SudokuGame.Cursor.9.cur"));
			}
		}
		#endregion

		#region Component Designer generated code
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
									base.Dispose( disposing );
								}
						
								private void InitializeComponent()
								{
									System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PictureCollection));
									this.picNewGame = new System.Windows.Forms.PictureBox();
									this.picNewGameRollOver = new System.Windows.Forms.PictureBox();
									this.picSolution = new System.Windows.Forms.PictureBox();
									this.picSolutionRollOver = new System.Windows.Forms.PictureBox();
									this.picUndo = new System.Windows.Forms.PictureBox();
									this.picUndoRollOver = new System.Windows.Forms.PictureBox();
									this.picLevels = new System.Windows.Forms.PictureBox();
									this.picLevelsRollOver = new System.Windows.Forms.PictureBox();
									this.picSend = new System.Windows.Forms.PictureBox();
									this.picSendRollOver = new System.Windows.Forms.PictureBox();
									this.picHelp = new System.Windows.Forms.PictureBox();
									this.picHelpRollOver = new System.Windows.Forms.PictureBox();
									this.picMark = new System.Windows.Forms.PictureBox();
									this.picMarkRollOver = new System.Windows.Forms.PictureBox();
									this.picMarkDown = new System.Windows.Forms.PictureBox();
									this.SuspendLayout();
									// 
									// picNewGame
									// 
									this.picNewGame.Image = ((System.Drawing.Image)(resources.GetObject("picNewGame.Image")));
									this.picNewGame.Location = new System.Drawing.Point(16, 16);
									this.picNewGame.Name = "picNewGame";
									this.picNewGame.Size = new System.Drawing.Size(37, 37);
									this.picNewGame.TabIndex = 0;
									this.picNewGame.TabStop = false;
									// 
									// picNewGameRollOver
									// 
									this.picNewGameRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picNewGameRollOver.Image")));
									this.picNewGameRollOver.Location = new System.Drawing.Point(80, 16);
									this.picNewGameRollOver.Name = "picNewGameRollOver";
									this.picNewGameRollOver.Size = new System.Drawing.Size(37, 37);
									this.picNewGameRollOver.TabIndex = 1;
									this.picNewGameRollOver.TabStop = false;
									// 
									// picSolution
									// 
									this.picSolution.Image = ((System.Drawing.Image)(resources.GetObject("picSolution.Image")));
									this.picSolution.Location = new System.Drawing.Point(16, 88);
									this.picSolution.Name = "picSolution";
									this.picSolution.Size = new System.Drawing.Size(37, 37);
									this.picSolution.TabIndex = 2;
									this.picSolution.TabStop = false;
									// 
									// picSolutionRollOver
									// 
									this.picSolutionRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picSolutionRollOver.Image")));
									this.picSolutionRollOver.Location = new System.Drawing.Point(80, 88);
									this.picSolutionRollOver.Name = "picSolutionRollOver";
									this.picSolutionRollOver.Size = new System.Drawing.Size(37, 37);
									this.picSolutionRollOver.TabIndex = 3;
									this.picSolutionRollOver.TabStop = false;
									// 
									// picUndo
									// 
									this.picUndo.Image = ((System.Drawing.Image)(resources.GetObject("picUndo.Image")));
									this.picUndo.Location = new System.Drawing.Point(16, 152);
									this.picUndo.Name = "picUndo";
									this.picUndo.Size = new System.Drawing.Size(37, 37);
									this.picUndo.TabIndex = 4;
									this.picUndo.TabStop = false;
									// 
									// picUndoRollOver
									// 
									this.picUndoRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picUndoRollOver.Image")));
									this.picUndoRollOver.Location = new System.Drawing.Point(80, 152);
									this.picUndoRollOver.Name = "picUndoRollOver";
									this.picUndoRollOver.Size = new System.Drawing.Size(37, 37);
									this.picUndoRollOver.TabIndex = 5;
									this.picUndoRollOver.TabStop = false;
									// 
									// picLevels
									// 
									this.picLevels.Image = ((System.Drawing.Image)(resources.GetObject("picLevels.Image")));
									this.picLevels.Location = new System.Drawing.Point(16, 216);
									this.picLevels.Name = "picLevels";
									this.picLevels.Size = new System.Drawing.Size(37, 37);
									this.picLevels.TabIndex = 6;
									this.picLevels.TabStop = false;
									// 
									// picLevelsRollOver
									// 
									this.picLevelsRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picLevelsRollOver.Image")));
									this.picLevelsRollOver.Location = new System.Drawing.Point(80, 216);
									this.picLevelsRollOver.Name = "picLevelsRollOver";
									this.picLevelsRollOver.Size = new System.Drawing.Size(37, 37);
									this.picLevelsRollOver.TabIndex = 7;
									this.picLevelsRollOver.TabStop = false;
									// 
									// picSend
									// 
									this.picSend.Image = ((System.Drawing.Image)(resources.GetObject("picSend.Image")));
									this.picSend.Location = new System.Drawing.Point(184, 16);
									this.picSend.Name = "picSend";
									this.picSend.Size = new System.Drawing.Size(37, 37);
									this.picSend.TabIndex = 8;
									this.picSend.TabStop = false;
									// 
									// picSendRollOver
									// 
									this.picSendRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picSendRollOver.Image")));
									this.picSendRollOver.Location = new System.Drawing.Point(256, 16);
									this.picSendRollOver.Name = "picSendRollOver";
									this.picSendRollOver.Size = new System.Drawing.Size(37, 37);
									this.picSendRollOver.TabIndex = 9;
									this.picSendRollOver.TabStop = false;
									// 
									// picHelp
									// 
									this.picHelp.Image = ((System.Drawing.Image)(resources.GetObject("picHelp.Image")));
									this.picHelp.Location = new System.Drawing.Point(184, 72);
									this.picHelp.Name = "picHelp";
									this.picHelp.Size = new System.Drawing.Size(37, 37);
									this.picHelp.TabIndex = 10;
									this.picHelp.TabStop = false;
									// 
									// picHelpRollOver
									// 
									this.picHelpRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picHelpRollOver.Image")));
									this.picHelpRollOver.Location = new System.Drawing.Point(256, 72);
									this.picHelpRollOver.Name = "picHelpRollOver";
									this.picHelpRollOver.Size = new System.Drawing.Size(37, 37);
									this.picHelpRollOver.TabIndex = 11;
									this.picHelpRollOver.TabStop = false;
									// 
									// picMark
									// 
									this.picMark.Image = ((System.Drawing.Image)(resources.GetObject("picMark.Image")));
									this.picMark.Location = new System.Drawing.Point(184, 152);
									this.picMark.Name = "picMark";
									this.picMark.Size = new System.Drawing.Size(37, 37);
									this.picMark.TabIndex = 12;
									this.picMark.TabStop = false;
									// 
									// picMarkRollOver
									// 
									this.picMarkRollOver.Image = ((System.Drawing.Image)(resources.GetObject("picMarkRollOver.Image")));
									this.picMarkRollOver.Location = new System.Drawing.Point(256, 150);
									this.picMarkRollOver.Name = "picMarkRollOver";
									this.picMarkRollOver.Size = new System.Drawing.Size(37, 37);
									this.picMarkRollOver.TabIndex = 13;
									this.picMarkRollOver.TabStop = false;
									// 
									// picMarkDown
									// 
									this.picMarkDown.Image = ((System.Drawing.Image)(resources.GetObject("picMarkDown.Image")));
									this.picMarkDown.Location = new System.Drawing.Point(328, 152);
									this.picMarkDown.Name = "picMarkDown";
									this.picMarkDown.Size = new System.Drawing.Size(37, 37);
									this.picMarkDown.TabIndex = 14;
									this.picMarkDown.TabStop = false;
									// 
									// PictureCollection
									// 
									this.Controls.Add(this.picMarkDown);
									this.Controls.Add(this.picMarkRollOver);
									this.Controls.Add(this.picMark);
									this.Controls.Add(this.picHelpRollOver);
									this.Controls.Add(this.picHelp);
									this.Controls.Add(this.picSendRollOver);
									this.Controls.Add(this.picSend);
									this.Controls.Add(this.picLevelsRollOver);
									this.Controls.Add(this.picLevels);
									this.Controls.Add(this.picUndoRollOver);
									this.Controls.Add(this.picUndo);
									this.Controls.Add(this.picSolutionRollOver);
									this.Controls.Add(this.picSolution);
									this.Controls.Add(this.picNewGameRollOver);
									this.Controls.Add(this.picNewGame);
									this.Name = "PictureCollection";
									this.Size = new System.Drawing.Size(592, 336);
									this.ResumeLayout(false);

								}
		#endregion
	}
}
