using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using SudokuLibrary;

namespace SudokuGame
{
	public class MainForm : Form
	{
		#region Form Fields
		private System.Windows.Forms.MenuItem menuItemEmpty;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.PictureBox picMark;
		private System.Windows.Forms.PictureBox picSend2Friend;
		private System.Windows.Forms.PictureBox picNewGame;
		private System.Windows.Forms.PictureBox picSolution;
		private System.Windows.Forms.PictureBox picUndo;
		private System.Windows.Forms.PictureBox picLevels;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.PictureBox picHelp;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.MenuItem menuItemLevel2;
		private System.Windows.Forms.MenuItem menuItemLevel3;
		private System.Windows.Forms.MenuItem menuItemLevel1;
		private SudokuLibrary.SudokuControl SudokuControl1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Panel toolbarPanel;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button btn1;
		private System.Windows.Forms.Button btn2;
		private System.Windows.Forms.Button btn3;
		private System.Windows.Forms.Button btn4;
		private System.Windows.Forms.Button btn5;
		private System.Windows.Forms.Button btn6;
		private System.Windows.Forms.Button btn7;
		private System.Windows.Forms.Button btn8;
		private System.Windows.Forms.Button btn9;
		private System.Windows.Forms.StatusBarPanel statusBarPanelLevel;
		private System.Windows.Forms.StatusBarPanel statusBarPanelTime;
		private System.Windows.Forms.ContextMenu contextMenuLevel;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.Timer timer1;
		#endregion						
		
		PictureCollection pictures;
		int level = 5;		
		bool IsFinish;
		bool IsMark = false;
		int CurrentNumber;
		Color gray = Color.FromArgb( 215 , 215 , 215 );
		private System.Windows.Forms.Label lblMark;			
		Color blue = Color.FromArgb( 248 , 248 , 248 );

		public MainForm()
		{
			InitializeComponent();

			#region InitializePicture
			pictures = new PictureCollection();
			
			picNewGame.Image  = pictures.NewGame;
			picSolution.Image = pictures.Solution;
			picUndo.Image     = pictures.Undo;
			picLevels.Image   = pictures.Levels;
			picMark.Image	  = pictures.Mark;
			picSend2Friend.Image = pictures.Send;
			picHelp.Image    = pictures.Help;
			#endregion

			SetLevelText();

			

			SudokuControl1.Error +=new SudokuLibrary.SudokuControl.ErrorEventHandler(SudokuControl1_Error);
		}
				

		#region Windows Form Designer generated code
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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MainForm ) );
        this.mainMenu1 = new System.Windows.Forms.MainMenu( this.components );
        this.statusBar = new System.Windows.Forms.StatusBar();
        this.statusBarPanelLevel = new System.Windows.Forms.StatusBarPanel();
        this.statusBarPanelTime = new System.Windows.Forms.StatusBarPanel();
        this.timer1 = new System.Windows.Forms.Timer( this.components );
        this.SudokuControl1 = new SudokuLibrary.SudokuControl();
        this.imageList1 = new System.Windows.Forms.ImageList( this.components );
        this.toolbarPanel = new System.Windows.Forms.Panel();
        this.label7 = new System.Windows.Forms.Label();
        this.picMark = new System.Windows.Forms.PictureBox();
        this.contextMenuLevel = new System.Windows.Forms.ContextMenu();
        this.menuItemLevel1 = new System.Windows.Forms.MenuItem();
        this.menuItemLevel2 = new System.Windows.Forms.MenuItem();
        this.menuItemLevel3 = new System.Windows.Forms.MenuItem();
        this.menuItemEmpty = new System.Windows.Forms.MenuItem();
        this.label6 = new System.Windows.Forms.Label();
        this.lblMark = new System.Windows.Forms.Label();
        this.picSend2Friend = new System.Windows.Forms.PictureBox();
        this.picHelp = new System.Windows.Forms.PictureBox();
        this.label4 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.panel1 = new System.Windows.Forms.Panel();
        this.label1 = new System.Windows.Forms.Label();
        this.picLevels = new System.Windows.Forms.PictureBox();
        this.picSolution = new System.Windows.Forms.PictureBox();
        this.picNewGame = new System.Windows.Forms.PictureBox();
        this.picUndo = new System.Windows.Forms.PictureBox();
        this.button4 = new System.Windows.Forms.Button();
        this.btn1 = new System.Windows.Forms.Button();
        this.btn2 = new System.Windows.Forms.Button();
        this.btn3 = new System.Windows.Forms.Button();
        this.btn4 = new System.Windows.Forms.Button();
        this.btn5 = new System.Windows.Forms.Button();
        this.btn6 = new System.Windows.Forms.Button();
        this.btn7 = new System.Windows.Forms.Button();
        this.btn8 = new System.Windows.Forms.Button();
        this.btn9 = new System.Windows.Forms.Button();
        this.toolTip1 = new System.Windows.Forms.ToolTip( this.components );
        ( (System.ComponentModel.ISupportInitialize)( this.statusBarPanelLevel ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.statusBarPanelTime ) ).BeginInit();
        this.toolbarPanel.SuspendLayout();
        ( (System.ComponentModel.ISupportInitialize)( this.picMark ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picSend2Friend ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picHelp ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picLevels ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picSolution ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picNewGame ) ).BeginInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picUndo ) ).BeginInit();
        this.SuspendLayout();
        // 
        // statusBar
        // 
        this.statusBar.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.statusBar.Location = new System.Drawing.Point( 0, 526 );
        this.statusBar.Name = "statusBar";
        this.statusBar.Panels.AddRange( new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanelLevel,
            this.statusBarPanelTime} );
        this.statusBar.ShowPanels = true;
        this.statusBar.Size = new System.Drawing.Size( 443, 22 );
        this.statusBar.TabIndex = 10;
        // 
        // statusBarPanelLevel
        // 
        this.statusBarPanelLevel.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
        this.statusBarPanelLevel.Name = "statusBarPanelLevel";
        this.statusBarPanelLevel.Text = "Level: ";
        this.statusBarPanelLevel.Width = 250;
        // 
        // statusBarPanelTime
        // 
        this.statusBarPanelTime.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
        this.statusBarPanelTime.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
        this.statusBarPanelTime.Name = "statusBarPanelTime";
        this.statusBarPanelTime.Text = "Time";
        this.statusBarPanelTime.Width = 160;
        // 
        // timer1
        // 
        this.timer1.Tick += new System.EventHandler( this.timer1_Tick );
        // 
        // SudokuControl1
        // 
        this.SudokuControl1.Location = new System.Drawing.Point( 8, 104 );
        this.SudokuControl1.MarkNumber = 0;
        this.SudokuControl1.Name = "SudokuControl1";
        this.SudokuControl1.Size = new System.Drawing.Size( 424, 424 );
        this.SudokuControl1.TabIndex = 13;
        // 
        // imageList1
        // 
        this.imageList1.ImageStream = ( (System.Windows.Forms.ImageListStreamer)( resources.GetObject( "imageList1.ImageStream" ) ) );
        this.imageList1.TransparentColor = System.Drawing.Color.Black;
        this.imageList1.Images.SetKeyName( 0, "" );
        this.imageList1.Images.SetKeyName( 1, "" );
        this.imageList1.Images.SetKeyName( 2, "" );
        this.imageList1.Images.SetKeyName( 3, "" );
        this.imageList1.Images.SetKeyName( 4, "" );
        this.imageList1.Images.SetKeyName( 5, "" );
        this.imageList1.Images.SetKeyName( 6, "" );
        this.imageList1.Images.SetKeyName( 7, "" );
        this.imageList1.Images.SetKeyName( 8, "" );
        this.imageList1.Images.SetKeyName( 9, "" );
        // 
        // toolbarPanel
        // 
        this.toolbarPanel.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                    | System.Windows.Forms.AnchorStyles.Right ) ) );
        this.toolbarPanel.BackgroundImage = ( (System.Drawing.Image)( resources.GetObject( "toolbarPanel.BackgroundImage" ) ) );
        this.toolbarPanel.Controls.Add( this.label7 );
        this.toolbarPanel.Controls.Add( this.picMark );
        this.toolbarPanel.Controls.Add( this.label6 );
        this.toolbarPanel.Controls.Add( this.lblMark );
        this.toolbarPanel.Controls.Add( this.picSend2Friend );
        this.toolbarPanel.Controls.Add( this.picHelp );
        this.toolbarPanel.Controls.Add( this.label4 );
        this.toolbarPanel.Controls.Add( this.label3 );
        this.toolbarPanel.Controls.Add( this.label2 );
        this.toolbarPanel.Controls.Add( this.panel1 );
        this.toolbarPanel.Controls.Add( this.label1 );
        this.toolbarPanel.Controls.Add( this.picLevels );
        this.toolbarPanel.Controls.Add( this.picSolution );
        this.toolbarPanel.Controls.Add( this.picNewGame );
        this.toolbarPanel.Controls.Add( this.picUndo );
        this.toolbarPanel.Location = new System.Drawing.Point( 0, 0 );
        this.toolbarPanel.Name = "toolbarPanel";
        this.toolbarPanel.Size = new System.Drawing.Size( 449, 96 );
        this.toolbarPanel.TabIndex = 14;
        // 
        // label7
        // 
        this.label7.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label7.Location = new System.Drawing.Point( 336, 40 );
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size( 32, 16 );
        this.label7.TabIndex = 19;
        this.label7.Text = "Send";
        // 
        // picMark
        // 
        this.picMark.BackColor = System.Drawing.Color.Transparent;
        this.picMark.ContextMenu = this.contextMenuLevel;
        this.picMark.Location = new System.Drawing.Point( 272, 2 );
        this.picMark.Name = "picMark";
        this.picMark.Size = new System.Drawing.Size( 37, 37 );
        this.picMark.TabIndex = 18;
        this.picMark.TabStop = false;
        this.toolTip1.SetToolTip( this.picMark, "Mark the good cells" );
        this.picMark.MouseLeave += new System.EventHandler( this.picMark_MouseLeave );
        this.picMark.Click += new System.EventHandler( this.picMark_Click );
        this.picMark.MouseEnter += new System.EventHandler( this.picMark_MouseEnter );
        // 
        // contextMenuLevel
        // 
        this.contextMenuLevel.MenuItems.AddRange( new System.Windows.Forms.MenuItem[] {
            this.menuItemLevel1,
            this.menuItemLevel2,
            this.menuItemLevel3,
            this.menuItemEmpty} );
        // 
        // menuItemLevel1
        // 
        this.menuItemLevel1.Index = 0;
        this.menuItemLevel1.Text = "Level 1";
        this.menuItemLevel1.Click += new System.EventHandler( this.Level1_Click );
        // 
        // menuItemLevel2
        // 
        this.menuItemLevel2.Index = 1;
        this.menuItemLevel2.Text = "Level 2";
        this.menuItemLevel2.Click += new System.EventHandler( this.Level3_Click );
        // 
        // menuItemLevel3
        // 
        this.menuItemLevel3.Index = 2;
        this.menuItemLevel3.Text = "Level 3";
        this.menuItemLevel3.Click += new System.EventHandler( this.Level5_Click );
        // 
        // menuItemEmpty
        // 
        this.menuItemEmpty.Index = 3;
        this.menuItemEmpty.Text = "Empty";
        this.menuItemEmpty.Click += new System.EventHandler( this.menuItemEmpty_Click );
        // 
        // label6
        // 
        this.label6.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label6.Location = new System.Drawing.Point( 403, 40 );
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size( 32, 16 );
        this.label6.TabIndex = 17;
        this.label6.Text = "Help";
        // 
        // lblMark
        // 
        this.lblMark.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.lblMark.Location = new System.Drawing.Point( 262, 40 );
        this.lblMark.Name = "lblMark";
        this.lblMark.Size = new System.Drawing.Size( 56, 16 );
        this.lblMark.TabIndex = 16;
        this.lblMark.Text = "Mark";
        this.lblMark.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // picSend2Friend
        // 
        this.picSend2Friend.BackColor = System.Drawing.Color.Transparent;
        this.picSend2Friend.ContextMenu = this.contextMenuLevel;
        this.picSend2Friend.Location = new System.Drawing.Point( 336, 1 );
        this.picSend2Friend.Name = "picSend2Friend";
        this.picSend2Friend.Size = new System.Drawing.Size( 37, 37 );
        this.picSend2Friend.TabIndex = 15;
        this.picSend2Friend.TabStop = false;
        this.toolTip1.SetToolTip( this.picSend2Friend, "Send to Friends" );
        this.picSend2Friend.MouseLeave += new System.EventHandler( this.picSend2Friend_MouseLeave );
        this.picSend2Friend.Click += new System.EventHandler( this.picSend2Friend_Click );
        this.picSend2Friend.MouseEnter += new System.EventHandler( this.picSend2Friend_MouseEnter );
        // 
        // picHelp
        // 
        this.picHelp.BackColor = System.Drawing.Color.Transparent;
        this.picHelp.Location = new System.Drawing.Point( 400, 1 );
        this.picHelp.Name = "picHelp";
        this.picHelp.Size = new System.Drawing.Size( 37, 37 );
        this.picHelp.TabIndex = 14;
        this.picHelp.TabStop = false;
        this.toolTip1.SetToolTip( this.picHelp, "Help" );
        this.picHelp.MouseLeave += new System.EventHandler( this.picHelp_MouseLeave );
        this.picHelp.Click += new System.EventHandler( this.btnHelp_Click );
        this.picHelp.MouseEnter += new System.EventHandler( this.picHelp_MouseEnter );
        // 
        // label4
        // 
        this.label4.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label4.Location = new System.Drawing.Point( 208, 40 );
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size( 40, 16 );
        this.label4.TabIndex = 13;
        this.label4.Text = "Levels";
        // 
        // label3
        // 
        this.label3.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label3.Location = new System.Drawing.Point( 144, 40 );
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size( 32, 16 );
        this.label3.TabIndex = 12;
        this.label3.Text = "Undo";
        // 
        // label2
        // 
        this.label2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label2.Location = new System.Drawing.Point( 72, 40 );
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size( 48, 16 );
        this.label2.TabIndex = 11;
        this.label2.Text = "Solution";
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ), ( (int)( ( (byte)( 224 ) ) ) ) );
        this.panel1.Location = new System.Drawing.Point( 0, 56 );
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size( 445, 1 );
        this.panel1.TabIndex = 10;
        // 
        // label1
        // 
        this.label1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.label1.Location = new System.Drawing.Point( 16, 40 );
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size( 32, 16 );
        this.label1.TabIndex = 9;
        this.label1.Text = "New";
        // 
        // picLevels
        // 
        this.picLevels.BackColor = System.Drawing.Color.Transparent;
        this.picLevels.ContextMenu = this.contextMenuLevel;
        this.picLevels.Location = new System.Drawing.Point( 208, 1 );
        this.picLevels.Name = "picLevels";
        this.picLevels.Size = new System.Drawing.Size( 37, 37 );
        this.picLevels.TabIndex = 8;
        this.picLevels.TabStop = false;
        this.toolTip1.SetToolTip( this.picLevels, "GameLevel" );
        this.picLevels.MouseLeave += new System.EventHandler( this.picLevels_MouseLeave );
        this.picLevels.Click += new System.EventHandler( this.btnLevel_Click );
        this.picLevels.MouseEnter += new System.EventHandler( this.picLevels_MouseEnter );
        // 
        // picSolution
        // 
        this.picSolution.BackColor = System.Drawing.Color.Transparent;
        this.picSolution.Location = new System.Drawing.Point( 80, 1 );
        this.picSolution.Name = "picSolution";
        this.picSolution.Size = new System.Drawing.Size( 37, 37 );
        this.picSolution.TabIndex = 7;
        this.picSolution.TabStop = false;
        this.toolTip1.SetToolTip( this.picSolution, "Solution" );
        this.picSolution.MouseLeave += new System.EventHandler( this.picSolution_MouseLeave );
        this.picSolution.Click += new System.EventHandler( this.Solution_Click );
        this.picSolution.MouseEnter += new System.EventHandler( this.picSolution_MouseEnter );
        // 
        // picNewGame
        // 
        this.picNewGame.BackColor = System.Drawing.Color.Transparent;
        this.picNewGame.Location = new System.Drawing.Point( 12, 1 );
        this.picNewGame.Name = "picNewGame";
        this.picNewGame.Size = new System.Drawing.Size( 37, 37 );
        this.picNewGame.TabIndex = 6;
        this.picNewGame.TabStop = false;
        this.toolTip1.SetToolTip( this.picNewGame, "New Game" );
        this.picNewGame.MouseLeave += new System.EventHandler( this.picNewGame_MouseLeave );
        this.picNewGame.Click += new System.EventHandler( this.NewGame_Click );
        this.picNewGame.MouseEnter += new System.EventHandler( this.picNewGame_MouseEnter );
        // 
        // picUndo
        // 
        this.picUndo.BackColor = System.Drawing.Color.Transparent;
        this.picUndo.Location = new System.Drawing.Point( 144, 1 );
        this.picUndo.Name = "picUndo";
        this.picUndo.Size = new System.Drawing.Size( 37, 37 );
        this.picUndo.TabIndex = 5;
        this.picUndo.TabStop = false;
        this.toolTip1.SetToolTip( this.picUndo, "Undo" );
        this.picUndo.MouseLeave += new System.EventHandler( this.picUndo_MouseLeave );
        this.picUndo.Click += new System.EventHandler( this.Undo_Click );
        this.picUndo.MouseEnter += new System.EventHandler( this.picUndo_MouseEnter );
        // 
        // button4
        // 
        this.button4.Location = new System.Drawing.Point( 50, 5 );
        this.button4.Name = "button4";
        this.button4.Size = new System.Drawing.Size( 75, 23 );
        this.button4.TabIndex = 0;
        // 
        // btn1
        // 
        this.btn1.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn1.Location = new System.Drawing.Point( 10, 64 );
        this.btn1.Name = "btn1";
        this.btn1.Size = new System.Drawing.Size( 30, 30 );
        this.btn1.TabIndex = 15;
        this.btn1.Text = "1";
        this.btn1.Click += new System.EventHandler( this.btn1_Click );
        // 
        // btn2
        // 
        this.btn2.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn2.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn2.Location = new System.Drawing.Point( 57, 64 );
        this.btn2.Name = "btn2";
        this.btn2.Size = new System.Drawing.Size( 30, 30 );
        this.btn2.TabIndex = 16;
        this.btn2.Text = "2";
        this.btn2.Click += new System.EventHandler( this.btn2_Click );
        // 
        // btn3
        // 
        this.btn3.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn3.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn3.Location = new System.Drawing.Point( 105, 64 );
        this.btn3.Name = "btn3";
        this.btn3.Size = new System.Drawing.Size( 30, 30 );
        this.btn3.TabIndex = 17;
        this.btn3.Text = "3";
        this.btn3.Click += new System.EventHandler( this.btn3_Click );
        // 
        // btn4
        // 
        this.btn4.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn4.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn4.Location = new System.Drawing.Point( 154, 64 );
        this.btn4.Name = "btn4";
        this.btn4.Size = new System.Drawing.Size( 30, 30 );
        this.btn4.TabIndex = 18;
        this.btn4.Text = "4";
        this.btn4.Click += new System.EventHandler( this.btn4_Click );
        // 
        // btn5
        // 
        this.btn5.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn5.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn5.Location = new System.Drawing.Point( 202, 64 );
        this.btn5.Name = "btn5";
        this.btn5.Size = new System.Drawing.Size( 30, 30 );
        this.btn5.TabIndex = 19;
        this.btn5.Text = "5";
        this.btn5.Click += new System.EventHandler( this.btn5_Click );
        // 
        // btn6
        // 
        this.btn6.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn6.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn6.Location = new System.Drawing.Point( 250, 64 );
        this.btn6.Name = "btn6";
        this.btn6.Size = new System.Drawing.Size( 30, 30 );
        this.btn6.TabIndex = 20;
        this.btn6.Text = "6";
        this.btn6.Click += new System.EventHandler( this.btn6_Click );
        // 
        // btn7
        // 
        this.btn7.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn7.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn7.Location = new System.Drawing.Point( 298, 64 );
        this.btn7.Name = "btn7";
        this.btn7.Size = new System.Drawing.Size( 30, 30 );
        this.btn7.TabIndex = 21;
        this.btn7.Text = "7";
        this.btn7.Click += new System.EventHandler( this.btn7_Click );
        // 
        // btn8
        // 
        this.btn8.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn8.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn8.Location = new System.Drawing.Point( 345, 64 );
        this.btn8.Name = "btn8";
        this.btn8.Size = new System.Drawing.Size( 30, 30 );
        this.btn8.TabIndex = 22;
        this.btn8.Text = "8";
        this.btn8.Click += new System.EventHandler( this.btn8_Click );
        // 
        // btn9
        // 
        this.btn9.Font = new System.Drawing.Font( "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ( (byte)( 177 ) ) );
        this.btn9.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.btn9.Location = new System.Drawing.Point( 400, 64 );
        this.btn9.Name = "btn9";
        this.btn9.Size = new System.Drawing.Size( 30, 30 );
        this.btn9.TabIndex = 23;
        this.btn9.Text = "9";
        this.btn9.Click += new System.EventHandler( this.btn9_Click );
        // 
        // MainForm
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
        this.BackColor = System.Drawing.Color.FromArgb( ( (int)( ( (byte)( 248 ) ) ) ), ( (int)( ( (byte)( 248 ) ) ) ), ( (int)( ( (byte)( 248 ) ) ) ) );
        this.ClientSize = new System.Drawing.Size( 443, 548 );
        this.Controls.Add( this.btn9 );
        this.Controls.Add( this.btn8 );
        this.Controls.Add( this.btn7 );
        this.Controls.Add( this.btn6 );
        this.Controls.Add( this.btn5 );
        this.Controls.Add( this.btn4 );
        this.Controls.Add( this.btn3 );
        this.Controls.Add( this.btn2 );
        this.Controls.Add( this.btn1 );
        this.Controls.Add( this.SudokuControl1 );
        this.Controls.Add( this.statusBar );
        this.Controls.Add( this.toolbarPanel );
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = ( (System.Drawing.Icon)( resources.GetObject( "$this.Icon" ) ) );
        this.MaximizeBox = false;
        this.Menu = this.mainMenu1;
        this.Name = "MainForm";
        this.Text = "Sudoku";
        ( (System.ComponentModel.ISupportInitialize)( this.statusBarPanelLevel ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.statusBarPanelTime ) ).EndInit();
        this.toolbarPanel.ResumeLayout( false );
        ( (System.ComponentModel.ISupportInitialize)( this.picMark ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picSend2Friend ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picHelp ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picLevels ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picSolution ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picNewGame ) ).EndInit();
        ( (System.ComponentModel.ISupportInitialize)( this.picUndo ) ).EndInit();
        this.ResumeLayout( false );

		}

		#endregion

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			TimeSpan result = DateTime.Now - SudokuControl1.StartTime;
			string time = string.Format( "Time: {0}:{1}:{2}" , result.Hours , result.Minutes , result.Seconds );
			statusBarPanelTime.Text = time;
		}
		
		void NewGame( int level )
		{
			IsFinish = false;
			AllBtnEnabled();
			SetButtonColor();
			SudokuControl1.Finish +=new EventHandler(SudokuControl1_Finish);
			SudokuControl1.NumberFinish +=new SudokuLibrary.SudokuControl.NumberFinishEventHandler(SudokuControl1_NumberFinish);
			SudokuControl1.NumberNotFinish +=new SudokuLibrary.SudokuControl.NumberFinishEventHandler(SudokuControl1_NumberNotFinish);
			
			SudokuControl1.NewGame( level );			
			SudokuControl1.Invalidate();

			timer1.Start();
			btn1_Click( null , null );
		}
		

		#region Mark Methods
		void SetButtonColor()
		{
			btn1.ForeColor = Color.FromName( "ActiveCaption" );
			btn2.ForeColor = Color.FromName( "ActiveCaption" );
			btn3.ForeColor = Color.FromName( "ActiveCaption" );

			btn4.ForeColor = Color.FromName( "ActiveCaption" );
			btn5.ForeColor = Color.FromName( "ActiveCaption" );
			btn6.ForeColor = Color.FromName( "ActiveCaption" );

			btn7.ForeColor = Color.FromName( "ActiveCaption" );
			btn8.ForeColor = Color.FromName( "ActiveCaption" );
			btn9.ForeColor = Color.FromName( "ActiveCaption" );
		}
		void MarkCell( int num )
		{
			SudokuControl1.MarkNumber = num;

			if( IsMark )
				SudokuControl1.MarkCellByNumber( num );
		}
		private void btn1_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 1;
			MarkCell( CurrentNumber );
						
			SetButtonColor();
			btn1.ForeColor = Color.Red;
		}

		private void btn2_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 2;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn2.ForeColor = Color.Red;	
		}

		private void btn3_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 3;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn3.ForeColor = Color.Red;	
		}

		private void btn4_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 4;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn4.ForeColor = Color.Red;	
		}

		private void btn5_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 5;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn5.ForeColor = Color.Red;	
		}

		private void btn6_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 6;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn6.ForeColor = Color.Red;	
		}

		private void btn7_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 7;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn7.ForeColor = Color.Red;	
		}

		private void btn8_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 8;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn8.ForeColor = Color.Red;	
		}

		private void btn9_Click(object sender, System.EventArgs e)
		{
			CurrentNumber = 9;
			MarkCell( CurrentNumber );
			SetButtonColor();
			btn9.ForeColor = Color.Red;	
		}
		#endregion

		#region Toolbar Events
		private void Solution_Click(object sender, System.EventArgs e)
		{
			SudokuControl1.Solution();
		}		private void NewGame_Click(object sender, System.EventArgs e)
		{
			NewGame( level );
		}
		private void Undo_Click(object sender, System.EventArgs e)
		{
			SudokuControl1.UndoAction();
		}
		private void picMark_Click(object sender, System.EventArgs e)
		{
			IsMark = !IsMark;
			if( !IsMark )
			{
				SudokuControl1.CleanMarkCell();
				picMark.Image = pictures.Mark;
				lblMark.Text  = "Mark"; 
			}
			else
			{
				MarkCell( CurrentNumber );
				picMark.Image = pictures.MarkDown;
				lblMark.Text  = "Unmark"; 
			}

			

		}
		private void picSend2Friend_Click(object sender, System.EventArgs e)
		{
			Process.Start("iexplore.exe" , "http://www.mysudoko.com/send.asp" );
		}
		private void btnHelp_Click(object sender, System.EventArgs e)
		{
			Process.Start("iexplore.exe" , "http://www.mysudoko.com/rules.html" );
		}
		#endregion

		#region  menuItem Levels
		private void btnLevel_Click(object sender, System.EventArgs e)
		{
			contextMenuLevel.Show( (Control)sender , new Point( 30 , 25 ) );
		}
		private void Level1_Click(object sender, System.EventArgs e)
		{
			level = 1;			
			SetLevelText();
		}

		private void Level2_Click(object sender, System.EventArgs e)
		{
			level = 2;
		}

		private void Level3_Click(object sender, System.EventArgs e)
		{
			level = 3;
			SetLevelText();
		}

		private void Level4_Click(object sender, System.EventArgs e)
		{
			level = 4;
		}

		private void Level5_Click(object sender, System.EventArgs e)
		{
			level = 5;			
			SetLevelText();
		}
		private void menuItemEmpty_Click(object sender, System.EventArgs e)
		{
			level = 0;
			SetLevelText();
		}
		private void SetLevelText()
		{
			switch( level )
			{
				case 0:
					menuItemLevel1.Checked = false;
					menuItemLevel2.Checked = false;
					menuItemLevel3.Checked = false;
					menuItemEmpty.Checked  = true;
					statusBarPanelLevel.Text = "Level: Empty Sudoku";
					break;
				case 1:
					menuItemLevel1.Checked = true;
					menuItemLevel2.Checked = false;
					menuItemLevel3.Checked = false;
					menuItemEmpty.Checked  = false;
					statusBarPanelLevel.Text = "Level: 1";
					break;
				case 3:					
					menuItemLevel1.Checked = false;
					menuItemLevel2.Checked = true;
					menuItemLevel3.Checked = false;
					menuItemEmpty.Checked  = false;
					statusBarPanelLevel.Text = "Level: 2";
					break;
				case 5:
					menuItemLevel1.Checked = false;
					menuItemLevel2.Checked = false;
					menuItemLevel3.Checked = true;
					menuItemEmpty.Checked  = false;
					statusBarPanelLevel.Text = "Level: 3";
					break;
			}
			NewGame( level );
		}
		#endregion

		private void SudokuControl1_Finish(object sender, EventArgs e)
		{
			if( !IsFinish )
			{
				timer1.Stop();
				string result  = "Finish in " + statusBarPanelTime.Text;
				statusBarPanelTime.Text = result;
				MessageBox.Show( result );
			}
			IsFinish = true;
		}		
		private void SudokuControl1_NumberFinish(object sender, NumberFinishEventArgs e)
		{
			switch( e.Number )
			{
				case 1:
					btn1.Enabled = false;
					btn1.BackColor = gray;
					break;
				case 2:
					btn2.Enabled = false;
					btn2.BackColor = gray;
					break;
				case 3:
					btn3.Enabled = false;
					btn3.BackColor = gray;
					break;
				case 4:
					btn4.Enabled = false;
					btn4.BackColor = gray;
					break;
				case 5:
					btn5.Enabled = false;
					btn5.BackColor = gray;
					break;
				case 6:
					btn6.Enabled = false;
					btn6.BackColor = gray;
					break;
				case 7:
					btn7.Enabled = false;
					btn7.BackColor = gray;
					break;
				case 8:
					btn8.Enabled = false;
					btn8.BackColor = gray;
					break;
				case 9:
					btn9.Enabled = false;
					btn9.BackColor = gray;
					break;
			}
		}

		private void SudokuControl1_NumberNotFinish(object sender, NumberFinishEventArgs e)
		{
			switch( e.Number )
			{
				case 1:
					btn1.Enabled = true;
					btn1.BackColor = blue;
					break;
				case 2:
					btn2.Enabled = true;
					btn2.BackColor = blue;
					break;
				case 3:
					btn3.Enabled = true;
					btn3.BackColor = blue;
					break;
				case 4:
					btn4.Enabled = true;
					btn4.BackColor = blue;
					break;
				case 5:
					btn5.Enabled = true;
					btn5.BackColor = blue;
					break;
				case 6:
					btn6.Enabled = true;
					btn6.BackColor = blue;
					break;
				case 7:
					btn7.Enabled = true;
					btn7.BackColor = blue;
					break;
				case 8:
					btn8.Enabled = true;
					btn8.BackColor = blue;
					break;
				case 9:
					btn9.Enabled = true;
					btn9.BackColor = blue;
					break;
			}
		}

		private void AllBtnEnabled()
		{
			btn1.Enabled = true;
			btn1.BackColor = blue;
			btn2.Enabled = true;
			btn2.BackColor = blue;
			btn3.Enabled = true;
			btn3.BackColor = blue;

			btn4.Enabled = true;
			btn4.BackColor = blue;
			btn5.Enabled = true;
			btn5.BackColor = blue;
			btn6.Enabled = true;
			btn6.BackColor = blue;

			btn7.Enabled = true;
			btn7.BackColor = blue;
			btn8.Enabled = true;
			btn8.BackColor = blue;
			btn9.Enabled = true;
			btn9.BackColor = blue;
		}

		
		#region Picture Roll Over
		private void picNewGame_MouseEnter(object sender, System.EventArgs e)
		{
			picNewGame.Image = pictures.NewGameRollOver;
		}

		private void picNewGame_MouseLeave(object sender, System.EventArgs e)
		{
			picNewGame.Image = pictures.NewGame;
		}

		private void picSolution_MouseEnter(object sender, System.EventArgs e)
		{
			picSolution.Image = pictures.SolutionRollOver;
		}

		private void picSolution_MouseLeave(object sender, System.EventArgs e)
		{
			picSolution.Image = pictures.Solution;
		}

		private void picUndo_MouseEnter(object sender, System.EventArgs e)
		{
			picUndo.Image = pictures.UndoRollOver;
		}

		private void picUndo_MouseLeave(object sender, System.EventArgs e)
		{
			picUndo.Image = pictures.Undo;
		}

		private void picLevels_MouseEnter(object sender, System.EventArgs e)
		{
			picLevels.Image = pictures.LevelsRollOver;
		}

		private void picLevels_MouseLeave(object sender, System.EventArgs e)
		{
			picLevels.Image = pictures.Levels;
		}
		private void picMark_MouseEnter(object sender, System.EventArgs e)
		{
			if( !IsMark )
				picMark.Image = pictures.MarkRollOver;
		}
		private void picMark_MouseLeave(object sender, System.EventArgs e)
		{
			if( !IsMark )
				picMark.Image = pictures.Mark;
		}
		private void picSend2Friend_MouseEnter(object sender, System.EventArgs e)
		{
			picSend2Friend.Image = pictures.SendRollOver;
		}

		private void picSend2Friend_MouseLeave(object sender, System.EventArgs e)
		{
			picSend2Friend.Image = pictures.Send;
		}

		private void picHelp_MouseEnter(object sender, System.EventArgs e)
		{
			picHelp.Image = pictures.HelpRollOver;
		}

		private void picHelp_MouseLeave(object sender, System.EventArgs e)
		{
			picHelp.Image = pictures.Help;
		}

		#endregion

		private void SudokuControl1_Error(object sender, ErrorEventArgs e)
		{
			this.toolTip1.AutoPopDelay = 1000;
			this.toolTip1.SetToolTip( SudokuControl1 , "\n"+e.Error+"\n" );
		}		
	}
}