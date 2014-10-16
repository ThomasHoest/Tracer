using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Bubbles
{
  internal struct Bubble
  {
    public int x, y;
    public int xOld, yOld;
    public int velocityH, velocityV;
    public int width, height;

    static private Random Rand = new Random();

    public void Init( BubbleForm window )
    {
      Random rnd;
      int bubbleSize;
      int windowWidth, windowHeight;

      window.GetClientSize(out windowWidth, out windowHeight);

      rnd = Bubble.Rand;

      this.x = (rnd.Next() % windowWidth);
      this.y = (rnd.Next() % windowHeight);

      this.ResetDx();
      this.ResetDy();

      bubbleSize = rnd.Next(30) + 10;
      this.width = bubbleSize;
      this.height = bubbleSize;
    }

    void ResetDx()
    {
      this.velocityH = (Bubble.Rand.Next(10) - 5);
      if (this.velocityH == 0)
        this.velocityH = 3;
    }

    void ResetDy()
    {
      this.velocityV = (Bubble.Rand.Next(10) - 5);
      if (this.velocityV == 0)
        this.velocityV = 1;
    }

    public void DoTick( BubbleForm window )
    {
      int windowWidth, windowHeight;

      window.GetClientSize(out windowWidth, out windowHeight);

      this.xOld = this.x;
      this.yOld = this.y;

      this.x += this.velocityH;
      this.y += this.velocityV;

      if (this.x < 0)
      {
        this.x = 0;
        this.ResetDx();
      }

      if (this.x >= windowWidth - this.width)
      {
        this.x = windowWidth - this.width - 1;
        this.ResetDx();
      }


      if (this.y < 0)
      {
        this.y = 0;
        this.ResetDy();
      }

      if (this.y >= windowHeight - this.height)
      {
        this.y = windowHeight - this.height - 1;
        this.ResetDy();
      }
    }

    private void FillInnerReflection( Graphics graphics, ref int x, ref int y, ref int width, ref int height )
    {
      x += width / 8;
      y += height / 8;

      width /= 2;
      height /= 2;

      graphics.FillEllipse(new SolidBrush(Color.White), x, y, width, height);
    }

    public void Draw( Graphics graphics )
    {
      int x, y, width, height;

      graphics.DrawEllipse(new Pen(Color.White), this.x, this.y, this.width - 1, this.height - 1);

      x = this.x;
      y = this.y;
      width = this.width;
      height = this.height;

      FillInnerReflection(graphics, ref x, ref y, ref width, ref height);
    }

    public void Erase( Graphics graphics, Brush brush )
    {
      graphics.FillRectangle(brush, this.x, this.y, this.width, this.height);
    }

    public Rectangle WholeBounds
    {
      get
      {
        return (Rectangle.Union(new Rectangle(this.x, this.y, this.width + 1, this.height + 1), new Rectangle(this.xOld, this.yOld, this.width + 1, this.height + 1)));
      }
    }
  }
}
