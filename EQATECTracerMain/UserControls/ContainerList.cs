using System;
using System.Collections.Generic;
using System.Windows;
using System.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.UserControls;

namespace EQATEC.Tracer.UserControls
{
  //
  // ContainerList is a FrameworkElement that looks suspiciously like a ListBox.  Unlike ListBox it recycles its containers and isn't
  // particularly extensible.  This can serve as an example of how to write a custom, single-purpose, fast-scrolling control.
  //
  // This list basically creates a set of containers and 'fakes' scrolling by updating each container's data item.  This avoids the cost
  // of creating and throwing away new containers incurred by ListBox using VirtualizingStackPanel.  Since this element was just created
  // to show a concept, it doesn't handle computing the right number of containers for a given size.
  //
  // To see how scrolling is handled, see the ScrollBarValueChanged() and SetData methods().
  //
  public class ContainerList : FrameworkElement
  {
    private int mTopItemIndex = 0;
    private bool mUpdateLayout = false;

    private readonly Border mBorder = new Border();                 // _border is the single visual child of this control
    private readonly StackPanel mStackPanel = new StackPanel();
    private readonly ScrollBar mScrollBarVertical = new ScrollBar();
    private readonly ScrollViewer mScroller = new ScrollViewer();
    //private readonly ScrollBar _scrollBarHorizontal = new ScrollBar();
    private readonly DockPanel mDockPanel = new DockPanel();
    protected readonly List<UIElement> mContainers = new List<UIElement>();
    protected double mLineMaxLength = 0;
    private int mItemsPerPage = 0;

    public delegate void ItemSelectedHandler(LineHolder sender);
    public event ItemSelectedHandler OnItemSelected;
    public event ItemSelectedHandler OnItemDoubleClick;
    protected double mViewHeight = 0;

    #region Properties

    public Brush Background
    {
      get { return mStackPanel.Background; }
      set { mStackPanel.Background = value; }
    }

    public CornerRadius CornerRadius
    {
      get { return mBorder.CornerRadius; }
      set { mBorder.CornerRadius = value; }
    }

    public Thickness BorderThickness
    {
      get { return mBorder.BorderThickness; }
      set { mBorder.BorderThickness = value; }
    }

    //public static readonly DependencyProperty BorderProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ContainerList));

    //public Brush Border
    //{
    //  get { return (Brush)GetValue(BorderProperty); }
    //  set { SetValue(BorderProperty, value); }
    //}

    Brush mBorderBrush;
    public Brush Border
    {
      get { return mBorderBrush; }
      set { mBorderBrush = value; }
    }

    bool mAutoScroll = false;

    public bool AutoScroll
    {
      get { return mAutoScroll; }
      set { mAutoScroll = value; }
    }

    private double mChildHeight = 0.0;

    public double ChildHeight
    {
      get { return mChildHeight; }
      set
      {
        mChildHeight = value;
        AdaptSize();
      }
    }

    //
    // Since we don't derive from ItemsControl we'll create our own ItemsSource DP
    //
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(ContainerList),
                                                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));

    public IList ItemsSource
    {
      set
      {
        SetValue(ItemsSourceProperty, value);
      }
      get { return (IList)GetValue(ItemsSourceProperty); }
    }

    #endregion

    #region Public interface

    public void ScrollToEnd()
    {
      if (ItemsSource.Count > mContainers.Count)
      {
        mTopItemIndex = ItemsSource.Count - mContainers.Count;
        mAutoScroll = true;
        mViewChanged = true;
        SetData();
      }
    }

    public void ScrollToTop()
    {
      mTopItemIndex = 0;
      mViewChanged = true;
      SetData();
    }
    public void ScrollSearch(int Searchindex)
    {

      if (Searchindex == -1)
      {
        Debug.Assert(Searchindex < 0, "ScrollSearch called with <0");
        return;
      }
      mSelectedLineIndex = Searchindex;
      if (Searchindex < mTopItemIndex || Searchindex + 1 > mTopItemIndex + mContainers.Count)
      {
        if (Searchindex < mTopItemIndex)
          mTopItemIndex = Searchindex;
        else
          mTopItemIndex = Searchindex - mContainers.Count + 1;
      }
      //If mTopItemIndex is negative not an option
      if (mTopItemIndex < 0)
        mTopItemIndex = 0;
      ////If mTopItemIndex to high so emthy lines would be showed
      else if (mTopItemIndex > ItemsSource.Count - mContainers.Count)//if 
        mTopItemIndex = ItemsSource.Count - mContainers.Count;

      OnItemSelected((LineHolder)ItemsSource[Searchindex]);
      mViewChanged = true;
      SetData();
    }

    public void AdaptSize()
    {
      int tempItems = 1;
      if (mChildHeight != 0)
      {
        mViewHeight = mScrollBarVertical.ActualHeight;
        tempItems = (int)(mViewHeight / mChildHeight) - 1;
      }

      if (tempItems != mItemsPerPage && tempItems > 0)
      {        
        mItemsPerPage = tempItems;
        mStackPanel.Children.Clear();
        mContainers.Clear();
        UpdateContainers();
        // Force an update of the data source
        mUpdateLayout = true;
        UpdateForItemsSourceChanged();
      }
      
    }

    public void Clear()
    {
      lock (mListSync)
      {
        System.Diagnostics.Trace.WriteLine("Clear all");
        for (int i = 0; i < mContainers.Count; i++)
        {
          ((ContentPresenter)mContainers[i]).Content = null;
        }

        mAutoScrollMargin = mAutoScrollMarginMin;
        mTopItemIndex = 0;
        mSelectedLineIndex = -1;
      }
    }

    #endregion

    #region UI Eventhandlers

    Brush mOldBrush = null;
    StackPanel mMouseOverPanel = null;

    bool mLineSelected = false;
    public int SelecteLineIndex { get { return mSelectedLineIndex; } }
    int mSelectedLineIndex = -1;

    void container_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (mMouseOverPanel != null && !mLineSelected)
      {
        mMouseOverPanel.Background = mOldBrush;
        mMouseOverPanel = null;
        mOldBrush = null;
      }
      mLineSelected = false;
    }

    void container_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {      
      //System.Diagnostics.Trace.WriteLine("mouseenter");
      FrameworkElement element = e.MouseDevice.DirectlyOver as FrameworkElement;
      if (element.DataContext is LineHolderProxy)
      {
        if (element != null)
        {
          StackPanel panel = null;
          
          if (element is StackPanel)
            panel = element as StackPanel;
          else if (element is TextLight)
            panel = element.Parent as StackPanel;
         
          if (panel != null && panel.Background == Brushes.Transparent)
          {
            mOldBrush = panel.Background;
            panel.Background = Brushes.LightBlue;
            mMouseOverPanel = panel;
          }
        }
      }
      //e.Handled = true;
      //System.Diagnostics.Trace.WriteLine("Enter container");
    }

    object mContainerSync = new object();

    public void FireItemSelected(TraceLine line, MouseButtonEventArgs e)
    {      
      Point p1 = e.GetPosition(mStackPanel); //Find current mouse position in list
      mSelectedLineIndex = (int)Math.Ceiling(p1.Y / mChildHeight) - 1 + mTopItemIndex; //Calc item position as list index
      //EQATEC.Tracer.UIHelpers.TraceLineManager.FoundLastIndex = mSelectedLineIndex;//FIXME very ugly

      mLineSelected = true;
      if (OnItemSelected != null)
        OnItemSelected(line.RawLine);

      if (mAutoScroll) //Check if we're at the correct line
      {
        mAutoScroll = false;
        int currentPosition = mSelectedLineIndex;
        //System.Diagnostics.Trace.WriteLine("CurrentPosition: " + currentPosition);

        lock (mContainerSync)
        {
          if(mSelectedLineIndex < ItemsSource.Count)
            while (mSelectedLineIndex != 0) //Find the line we clicked on
            {
              LineHolder listLine = ItemsSource[mSelectedLineIndex--] as LineHolder;
              if (listLine.Equals(line.RawLine)) //Is it a match
              {
                int lineOffset = mSelectedLineIndex - currentPosition + 1;
                
                if (lineOffset != 0)
                {
                  //If so move it into view and paint the elusive little **&%!!
                  this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    ((System.Windows.Forms.MethodInvoker)delegate()
                    {
                      try
                      {
                        MoveLines(lineOffset);
                        //line.PanelBackground = EQATEC.Tracer.Utilities.WordColors.SelectedLineBrush;
                      }
                      catch (Exception)
                      {
                      }
                    }));
                }

                break;
              }
            }
        }        
      }
      
    }

    void mViewKeyDown(object sender, KeyEventArgs e)
    {
      //System.Diagnostics.Trace.WriteLine("Key pressed: " + e.Key);
      
      if ( ( e.Key == Key.Up || e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Down ) 
        && mSelectedLineIndex != -1)
      {
        //If selectedline somehow hasn't been reset. Bail...
        if (ItemsSource.Count == 0)
          return;

        mAutoScroll = false;
        int oldSelectedLine = mSelectedLineIndex;

        switch (e.Key)
        {
          case Key.Down:
            mSelectedLineIndex++;
            break;
          case Key.Up:
            mSelectedLineIndex--;                          
            break;
          case Key.PageDown:
            mSelectedLineIndex += mContainers.Count;
            break;
          case Key.PageUp:
            mSelectedLineIndex -= mContainers.Count;
            break;
          default:
            break;
        }

        if (mSelectedLineIndex < 0)
          mSelectedLineIndex = 0;
        else if (mSelectedLineIndex > ( ItemsSource.Count - 1 ) )
          mSelectedLineIndex = ItemsSource.Count - 1;
        
        if (mSelectedLineIndex != oldSelectedLine)
        {
          LineHolder listLine = ItemsSource[mSelectedLineIndex] as LineHolder;
          if (OnItemSelected != null)
            OnItemSelected(listLine);

          int linePosition = mSelectedLineIndex - mTopItemIndex;

          //System.Diagnostics.Trace.WriteLine(string.Format("Line position {0}, SelectedLine {1}, Topindex {2}, ContainerCount {3}", linePosition, mSelectedLineIndex, mTopItemIndex, mContainers.Count));

          if (linePosition < 0)
          {
            MoveLines(linePosition);
          }
          else if ((linePosition - (mContainers.Count - 1)) > 0)
          {
            MoveLines(linePosition - (mContainers.Count - 1));
            SetAutoScroll();
          }
        }
      }
    }

    public void FireOnItemDoubleClick(LineHolder line)
    {
      mLineSelected = true;
      if (OnItemDoubleClick != null)
        OnItemDoubleClick(line);
    }

    private void MoveLines(int linesToMove)
    {
      //System.Diagnostics.Trace.WriteLine("Moving lines: " + linesToMove);
      mTopItemIndex += linesToMove;
      mViewChanged = true;
      SetData();
    }
   
    void stackPanel_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      if (e.Delta != 0)
      {
        if (e.Delta > 0)
          mTopItemIndex -= 3;
        else
          mTopItemIndex += 3;

        if (mTopItemIndex > (ItemsSource.Count - mItemsPerPage))
          mTopItemIndex = ItemsSource.Count - mItemsPerPage;

        if (mTopItemIndex < 0)
          mTopItemIndex = 0;

        SetAutoScroll();
        mViewChanged = true;
        SetData();
      }
    }

    int mPreviousCount = 0;

    void ContainerList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (mPreviousCount < mItemsPerPage || mPreviousCount > ItemsSource.Count)
        mViewChanged = true;

      if (mAutoScroll)
      {
        if (ItemsSource.Count > mContainers.Count)
        {
          //System.Diagnostics.Trace.WriteLine("Collection changed ");
          mTopItemIndex = ItemsSource.Count - mContainers.Count;
          mViewChanged = true;
        }
      }

      if (mAutoScrollMargin < mAutoScrollMarginMax)
      {
        int temp = (int)(ItemsSource.Count * mAutoScrollPart);
        mAutoScrollMargin = Math.Max(temp, mAutoScrollMarginMin);
      }
      mPreviousCount = ItemsSource.Count;
      SetData();      
      //mItemsSourceChanged = true;
      //UpdateForItemsSourceChanged();
    }

    bool mSuppressScrollEvent = false;

    private void VerticalScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!mSuppressScrollEvent)
      {
        int newTopItemIndex = (int)e.NewValue;
        if (newTopItemIndex != mTopItemIndex)
        {
          mTopItemIndex = newTopItemIndex;
          SetAutoScroll();
          mViewChanged = true;
          SetData();
        }
      }
    }

    bool mViewChanged = false;

    #endregion

    //
    // Overrides to act like a control
    //
    #region Overrides

    public override void EndInit()
    {
      base.EndInit();

      //
      // Init the border and set it as the only visual child of this FE
      //
      mBorder.BorderBrush = Brushes.Black;
      
      AddVisualChild(mBorder);

      mScrollBarVertical.Orientation = Orientation.Vertical;
      mScrollBarVertical.SetValue(DockPanel.DockProperty, Dock.Right);
      mScrollBarVertical.ValueChanged += new RoutedPropertyChangedEventHandler<double>(VerticalScrollBarValueChanged);

      mScroller.CanContentScroll = false;
      mScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      mScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
      mScroller.Content = mStackPanel;
      
      mStackPanel.CanHorizontallyScroll = true;
      
      mDockPanel.Children.Add(mScrollBarVertical);
      mDockPanel.Children.Add(mScroller);
     
      mBorder.Child = mDockPanel;
      mBorder.BorderBrush = mBorderBrush;
   
      mStackPanel.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(stackPanel_MouseWheel);
      mBorder.PreviewKeyDown += new KeyEventHandler(mViewKeyDown);
      
      AdaptSize();
    }

    private void UpdateContainers()
    {
      //
      // Create the number of containers used by this control
      //
      UIElement container;
      for (int i = 0; i < mItemsPerPage; i++)
      {
        container = CreateContainer();        
        container.MouseEnter += new System.Windows.Input.MouseEventHandler(container_MouseEnter);
        container.MouseLeave += new System.Windows.Input.MouseEventHandler(container_MouseLeave);        
        mStackPanel.Children.Add(container);        
        mContainers.Add(container);
      }
    }
 
    protected override Size MeasureOverride(Size availableSize)
    { 
      mBorder.Measure(availableSize);
      return mBorder.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      mBorder.Arrange(new Rect(finalSize));
      return mBorder.RenderSize;
    }

    protected override int VisualChildrenCount
    {
      get
      {
        return 1;
      }
    }

    protected override Visual GetVisualChild(int index)
    {
      if (index == 0)
      {
        return mBorder;
      }
      else
      {
        throw new ArgumentOutOfRangeException();
      }
    }

    #endregion 

    protected virtual void SetContainerData(UIElement container, object data)
    {
        // Note that the ContentPresenter.Content property sets the DataContext.
      ((ContentPresenter)container).Content = data;

      if (ChildHeight == 0)
      {
        container.Measure(new Size(1000, 1000)); //What is the max??
        //_LineMaxLength = System.Math.Max(_LineMaxLength, container.DesiredSize.Width);

        if (container.DesiredSize.Height > 0)
          ChildHeight = container.DesiredSize.Height;
        //System.Diagnostics.Trace.WriteLine("Width: " + container.DesiredSize.Width);
      }
    }
       
    //
    // Private Methods
    //

    #region Private Methods

    /// <summary>
    /// Conceptually similar to the GetContainerForItemOverride used by ItemsControl
    /// </summary>
    /// <returns></returns>
    private UIElement CreateContainer()
    {
        return new ContentPresenter();
    }

    private static void ItemsSourceChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        ContainerList list = (ContainerList)element;
        list.mUpdateLayout = true;
        list.UpdateForItemsSourceChanged();
        list.SetEvent();
    }

    private void SetEvent()
    {          
      ((ThreadSafeObservableCollection<LineHolder>)ItemsSource).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ContainerList_CollectionChanged);
    }

    private int mAutoScrollMargin = 5;
    private int mAutoScrollMarginMin = 5;
    private int mAutoScrollMarginMax = 50;
    private double mAutoScrollPart = 0.05;
        
    private void SetAutoScroll()
    {
      if (ItemsSource.Count > mContainers.Count)
      {
        if (mTopItemIndex > (ItemsSource.Count - mContainers.Count - mAutoScrollMargin))
          mAutoScroll = true;
        else
          mAutoScroll = false;
      }
    }

    object mListSync = new object();

    public object ListSync
    {
      get { return mListSync; }
      set { mListSync = value; }
    }

    /// <summary>
    /// This is the method where the container recycling work takes place.  Note that the containers stay in place; we simply iterate
    /// through each to give them their proper data item, giving the impression of scrolling.
    /// </summary>
    private void SetData()
    {
      IList source = ItemsSource;

      if (source != null)
      {
        //Debug.Assert(source.Count >= ItemsPerPage, "Things may break if the data is less than the page size.");
        Debug.Assert(mTopItemIndex >= 0);
        
        lock (mListSync)
        {
          int topItemIndex = Math.Min(mTopItemIndex, Math.Max(0, source.Count - mItemsPerPage));

          if (mViewChanged || mChildHeight == 0)
          {
            lock (mContainerSync)
            {
              //System.Diagnostics.Trace.WriteLine("Updating lines");
              for (int i = 0; i < mContainers.Count && i < source.Count; i++)
              {
                SetContainerData(mContainers[i], source[topItemIndex + i]);
              }
              mScroller.InvalidateMeasure();
            }
            mViewChanged = false;
          }
                    
          mSuppressScrollEvent = true;
          mScrollBarVertical.Value = topItemIndex;
          mScrollBarVertical.Maximum = source.Count - mItemsPerPage;
          mSuppressScrollEvent = false;
          //else
            //System.Diagnostics.Trace.WriteLine("No update: source count: " + source.Count + " mViewChanged: " + mViewChanged + " child: " + mChildHeight);
        }
        
      }
      else
      {
        for (int i = 0; i < mContainers.Count; i++)
        {
            SetContainerData(mContainers[i], null);
        }
      }
    }

    //public void Update()
    //{
    //  UpdateForItemsSourceChanged();
    //}
    
    private void UpdateForItemsSourceChanged()
    {
      if (mBorder != null && mUpdateLayout)
      {
        IList itemsSource = ItemsSource;
        if (itemsSource != null)
        {
          mScrollBarVertical.IsEnabled = true;
          mScrollBarVertical.Minimum = 0;
          mScrollBarVertical.Maximum = itemsSource.Count - mItemsPerPage;
          mScrollBarVertical.SmallChange = 1;
          mScrollBarVertical.ViewportSize = mItemsPerPage;
          mScrollBarVertical.LargeChange = mItemsPerPage;          
          mViewChanged = true;
        }
        else
        {
          //_scrollBarHorizontal.IsEnabled = false;
          mScrollBarVertical.IsEnabled = false;
        }

        SetData();

        mUpdateLayout = false;
      }
    }

    #endregion 
  }
}
