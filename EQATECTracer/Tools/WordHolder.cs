using System.Windows.Media;

namespace EQATEC.Tracer.Tools
{
  public class WordHolder
  {
    #region Properties

    string mText;
    public string Text
    {
      get { return mText; }
      set { mText = value; }
    }

    string mSeparator;
    public string Separator
    {
      get { return mSeparator; }
      set { mSeparator = value; }
    }

    Brush mBrush = Brushes.Black;
    public Brush Brush
    {
      get { return mBrush; }
      set { mBrush = value; }
    }

    #endregion

    public WordHolder()
    {
    }

    public WordHolder(string text, Brush brush)
    {
      mText = text;
      mBrush = brush;
    }

    public WordHolder(string text)
    {
      mText = text;
    }
  }
}