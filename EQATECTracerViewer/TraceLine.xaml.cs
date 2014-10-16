using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EQATEC.Tracer.Utilities;
using EQATEC.Tracer.Viewer;

namespace EQATEC.Tracer.TracerViewer
{
  /// <summary>
  /// Interaction logic for TraceLine.xaml
  /// </summary>
  public partial class TraceLine : UserControl
  { 
    public TraceLine()
    {
      InitializeComponent();
      InitWords();
      this.SetValue(DockPanel.ZIndexProperty, 1);
    }

    LineHolder mRawLine;

    public LineHolder RawLine
    {
      get { return mRawLine; }
      set { mRawLine = value; }
    }

    public TraceLine(WordHolder [] data)
    {
      InitializeComponent();
      InitWords();
      UpdateLine(data);
      this.SetValue(DockPanel.ZIndexProperty, 1);
    }

    private void InitWords()
    {
      TextLight word = new TextLight();
      m_spLine.Children.Add(word);      
    }

    public void UpdateLine(WordHolder [] data)
    {
      TextLight word = m_spLine.Children[0] as TextLight;
      word.Reset();
      for (int i = 0; i < data.Length; i++)
      {
        word.AddBlock(data[i].Text, data[i].Brush);
        
        if (data[i].Separator != null)
        {
          word.AddBlock(data[i].Separator, Brushes.Black);          
        }
      }
      word.Finish();     
    }

    private void m_miDisable_Click(object sender, RoutedEventArgs e)
    {

    }
  }

  public class TextLight : FrameworkElement
  {
    DrawingVisual mDVText;
    FormattedText mFormText;    
    StringBuilder mText;
    Brush [] mBrushes;
    int[] mTextStartIndexes;
    int[] mTextEndIndexes;    
    int mBlockCounter;
    double mWidth;
    double mHeight;

    const int mMaxWords = 40;
    private VisualCollection _children;

    public void Finish()
    {
      mFormText = new FormattedText(mText.ToString(),
         System.Globalization.CultureInfo.GetCultureInfo("en-us"),
         FlowDirection.LeftToRight,
         new Typeface("Verdana"),
         10,
         Brushes.Black);
      
      for(int i=0; i<mBlockCounter; i++)
      {
        mFormText.SetForegroundBrush(mBrushes[i], mTextStartIndexes[i], mTextEndIndexes[i]);
      }
      
      mWidth = mFormText.Width;
      mHeight = mFormText.Height;            
      //mDVText = CreateText();
    }

    public void Reset()
    {
      mBlockCounter = 0;      
      mText = new StringBuilder();
    }

    public void AddBlock(string s, Brush brush)
    {
      mTextStartIndexes[mBlockCounter] = mText.Length;
      mText.Append(s);
      mTextEndIndexes[mBlockCounter] = s.Length;
      mBrushes[mBlockCounter] = brush;
      ++mBlockCounter;
    }

    public TextLight()
    {
      mBrushes = new Brush[mMaxWords];
      mTextStartIndexes = new int[mMaxWords];
      mTextEndIndexes = new int[mMaxWords];
      mText = new StringBuilder();
      mBlockCounter = 0;
      _children = new VisualCollection(this);
      this.Initialized += new EventHandler(TextLight_Initialized);
      this.Loaded += new RoutedEventHandler(TextLight_Loaded);
    }

    void TextLight_Loaded(object sender, RoutedEventArgs e)
    {
      _children.Clear();
      TextLight_Initialized(sender, e);      
    }

    void TextLight_Initialized(object sender, EventArgs e)
    {
      mDVText = CreateText();
      // Call AddVisualChild and AddLogicalChild so the element hosting these visuals
      // knows about them and are accounted for in the layout process. This is what
      // Gets custom visuals actually rendered.
      _children.Add(mDVText);      
    }    
    
    /// <summary>
    /// Notice how the Create functions aren't being called in OnRender.
    /// </summary>
    private DrawingVisual CreateText()
    {
      DrawingVisual dv = new DrawingVisual();
      using (DrawingContext dc = dv.RenderOpen())
      {
        dc.DrawText(mFormText, new Point(0, 0));        
      }
      return dv;
    }
    /*
    protected override void OnRender(DrawingContext dc)
    { 
      dc.DrawText(mFormText, new Point(0, 0));      
    }*/

    protected override Size MeasureOverride(Size availableSize)
    {
      return new Size(mWidth, mHeight); ;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      return finalSize; // Returns the final Arranged size
    }

    
    #region Necessary Overrides -- Needed by WPF to maintain bookkeeping of our hosted visuals
    protected override int VisualChildrenCount
    {
      get { return _children.Count; }
    }

    protected override Visual GetVisualChild(int index)
    {
      if (index < 0 || index >= _children.Count)
      {
        throw new ArgumentOutOfRangeException();
      }

      return _children[index];
    }
    #endregion
  }
  
}
