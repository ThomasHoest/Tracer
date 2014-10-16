//=====================================================================
//  File:      Bubble.cs
//
//---------------------------------------------------------------------
//  This file is part of the Microsoft .NET Framework SDK Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// 
//This source code is intended only as a supplement to Microsoft
//Development Tools and/or on-line documentation.  See these other
//materials for detailed information regarding Microsoft code samples.
// 
//THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
//KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//PARTICULAR PURPOSE.
//---------------------------------------------------------------------

using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Bubbles
{
  public class BubbleForm : Form
  {
    private const int MaxBubbles = 20;

    private Bubble[] bubbles = new Bubble[MaxBubbles];
    
    private Brush backgroundBrush = new SolidBrush(Color.SlateBlue);

    private Bitmap offscreenBitmap;
    private Graphics offscreenGraphics;
    private Graphics realGraphics;

    private int width, height;
    private bool isRunning;
    private int m_tickStart;
    private int m_tickLast;
    private int m_cframe = 0;
    private bool m_suspended;

    public void Init( int width, int height )
    {
      int bubble;

      this.BackColor = Color.SlateBlue;
      this.ClientSize = new Size(width, height);
      this.Text = "Bubbles";
      this.Visible = true;
      this.width = this.ClientSize.Width;
      this.height = this.ClientSize.Height;

      for (bubble = 0; bubble < MaxBubbles; bubble++)
        this.bubbles[bubble].Init(this);

      this.offscreenBitmap = new Bitmap(this.width, this.height);
      this.offscreenGraphics = Graphics.FromImage(this.offscreenBitmap);
      this.offscreenGraphics.FillRectangle(this.backgroundBrush, 0, 0, this.width, this.height);

      this.realGraphics = this.CreateGraphics();

      this.isRunning = true;
    }

    public void GetClientSize( out int width, out int height )
    {
      width = this.width;
      height = this.height;
    }

    public void DoTick()
    {
      if (m_suspended)
        return;
      int bubble;

      m_cframe++;

      EraseAll(this.offscreenGraphics);

      for (bubble = 0; bubble < MaxBubbles; bubble++)
        this.bubbles[bubble].DoTick(this);

      RedrawAll(this.offscreenGraphics);
      RefreshAll(this.realGraphics);

      if ((Environment.TickCount - m_tickLast) / 1000 > 3)
      {
        m_tickLast = Environment.TickCount;

        this.SyncTitleBar();
      }
    }

    private void SyncTitleBar()
    {
      int csec;
      int wRefresh;

      csec = ((Environment.TickCount - m_tickStart) / 1000);
      if (csec != 0)
        wRefresh = m_cframe / csec;
      else
        wRefresh = 0;

      this.Text = "Bubble - " + wRefresh + " frames/sec";
    }

    private void RefreshAll( Graphics graphicsPhys )
    {
      Rectangle rc;
      int bubble;

      for (bubble = 0; bubble < MaxBubbles; bubble++)
      {
        rc = this.bubbles[bubble].WholeBounds;

        graphicsPhys.DrawImage(this.offscreenBitmap, rc.X, rc.Y, rc, GraphicsUnit.Pixel);
      }
    }

    private void RedrawAll( Graphics graphics )
    {
      int bubble;

      for (bubble = 0; bubble < MaxBubbles; bubble++)
        this.bubbles[bubble].Draw(graphics);
    }

    private void EraseAll( Graphics graphics )
    {
      int bubble;

      for (bubble = 0; bubble < MaxBubbles; bubble++)
        this.bubbles[bubble].Erase(graphics, this.backgroundBrush);
    }

    private void Exit()
    {
      this.isRunning = false;
      this.Close();
    }

    protected override void OnMouseDown( MouseEventArgs mouseArgs )
    {
      //on right mouse down we change the brush to let the bubbles repaint the
      //background, on left mouse down we toggle the drawing
      if (mouseArgs.Button == MouseButtons.Right)
      {
        Random rand = new Random();
        if (this.backgroundBrush != null)
          this.backgroundBrush.Dispose();
        this.backgroundBrush = new SolidBrush(Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256)));
      }
      else if (mouseArgs.Button == MouseButtons.Left)
      {
        m_suspended = !m_suspended;
      }
    }


    protected override void OnClosing( CancelEventArgs cancelg )
    {
      this.isRunning = false;
      cancelg.Cancel = true;
    }

    protected override void OnClosed( EventArgs eventg )
    {
      this.isRunning = false;
    }

    protected override void OnResize( EventArgs evtg )
    {
      this.SyncTitleBar();
    }

    public void RunMe()
    {
      m_cframe = 0;
      m_tickStart = Environment.TickCount;
      m_tickLast = m_tickStart;

      while (this.isRunning)
      {
        this.DoTick();
        Application.DoEvents();
      }
    }

    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // BubbleForm
      // 
      this.ClientSize = new System.Drawing.Size(292, 266);
      this.MaximumSize = new System.Drawing.Size(300, 300);
      this.MinimumSize = new System.Drawing.Size(300, 300);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Name = "BubbleForm";
      this.Text = "Bubbles Demo App";
      this.ResumeLayout(false);

    }
  }
}
