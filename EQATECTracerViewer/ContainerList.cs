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

using EQATEC.Tracer.Utilities;
using EQATEC.Tracer.Viewer;

namespace EQATEC.Tracer.TracerViewer
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
    private bool mItemsSourceChanged = false;

    private readonly Border mBorder = new Border();                 // _border is the single visual child of this control
    private readonly StackPanel mStackPanel = new StackPanel();
    private readonly ScrollBar mScrollBarVertical = new ScrollBar();
    private readonly ScrollViewer mScroller = new ScrollViewer();
    //private readonly ScrollBar _scrollBarHorizontal = new ScrollBar();
    private readonly DockPanel mDockPanel = new DockPanel();
    private readonly List<UIElement> mContainers = new List<UIElement>();
    protected double mLineMaxLength = 0;
    private int mItemsPerPage = 1;


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

    public Brush BorderBrush
    {
      get { return mBorder.BorderBrush; }
      set { mBorder.BorderBrush = value; }
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

    //
    // Overrides to act like a control
    //

    #region Overrides

    public override void EndInit()
    {
      base.EndInit();

      #region build visual tree

      //
      // Init the border and set it as the only visual child of this FE
      //
      mBorder.BorderBrush = Brushes.Black;
      
      AddVisualChild(mBorder);

      mScrollBarVertical.Orientation = Orientation.Vertical;
      mScrollBarVertical.SetValue(DockPanel.DockProperty, Dock.Right);
      mScrollBarVertical.SizeChanged += new SizeChangedEventHandler(mScrollBarVertical_SizeChanged);
      mScrollBarVertical.ValueChanged += new RoutedPropertyChangedEventHandler<double>(VerticalScrollBarValueChanged);

      mScroller.CanContentScroll = false;
      mScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      mScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
      mScroller.Content = mStackPanel;
      
      mStackPanel.CanHorizontallyScroll = true;
      
      mDockPanel.Children.Add(mScrollBarVertical);
      mDockPanel.Children.Add(mScroller); 
      
      mBorder.Child = mDockPanel;

      mStackPanel.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(stackPanel_MouseWheel);
      mStackPanel.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(_stackPanel_MouseLeftButtonDown);
      mStackPanel.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(_stackPanel_MouseLeftButtonUp);

      AdaptSize();
      #endregion
    }

    void mScrollBarVertical_SizeChanged(object sender, SizeChangedEventArgs e)
    {
    }

    void _stackPanel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      FrameworkElement element = e.MouseDevice.DirectlyOver as FrameworkElement;
      if (element != null && mSelectedElement != null && mSelectedElement.Equals(element))
      {
        LineHolder line = null;
        if (element is Border)
        {
          LineHolderProxy proxy = ((Border)element).DataContext as LineHolderProxy;
          line = proxy.DataItem;
        }
        else
        {
          while (element.Parent != null)
            element = element.Parent as FrameworkElement;

          if(element is TraceLine)
            line = ((TraceLine)element).RawLine;
        }
        if (OnItemSelected != null && line != null)
          OnItemSelected(line);        
      }

      System.Diagnostics.Trace.WriteLine("Clickcount: " + e.ClickCount);      
    }

    FrameworkElement mSelectedElement;

    void _stackPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      FrameworkElement element = e.MouseDevice.DirectlyOver as FrameworkElement;

      if (e.ClickCount == 2)
      {
        if (element != null && mSelectedElement != null && mSelectedElement.Equals(element))
        {
          LineHolder line;
          if (element is Border)
          {
            LineHolderProxy proxy = ((Border)element).DataContext as LineHolderProxy;
            line = proxy.DataItem;
          }
          else
          {
            while (element.Parent != null)
              element = element.Parent as FrameworkElement;

            line = ((TraceLine)element).RawLine;
          }

          if (OnItemDoubleClick != null && line != null)
            OnItemDoubleClick(line);
        }        
      }

      mSelectedElement = element;      
    }

    public delegate void ItemSelectedHandler(LineHolder sender);
    public event ItemSelectedHandler OnItemSelected;
    public event ItemSelectedHandler OnItemDoubleClick;

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

    Brush mOldBrush = null;
    Border mMouseOverBorder = null;

    void container_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (mMouseOverBorder != null)
      {
        mMouseOverBorder.Background = mOldBrush;
        mMouseOverBorder = null;
        mOldBrush = null;
      }
      //e.Handled = true;
      //System.Diagnostics.Trace.WriteLine("Leave container");
    }

    void container_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      FrameworkElement element = e.MouseDevice.DirectlyOver as FrameworkElement;
      if (element != null)
      {
        if (element is Border)
        {
          Border border = element as Border;
          mOldBrush = border.Background;
          border.Background = Brushes.LightBlue;
          mMouseOverBorder = border;
        }        
      }
      //e.Handled = true;
      //System.Diagnostics.Trace.WriteLine("Enter container");
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

    protected double mViewHeight = 0;

    public void AdaptSize()
    {
      if (mChildHeight != 0)
      {
        mViewHeight = mScrollBarVertical.ActualHeight;
        mItemsPerPage = (int)(mViewHeight / mChildHeight);        
      }

      mStackPanel.Children.Clear();
      mContainers.Clear();
      UpdateContainers();
      // Force an update of the data source
      mItemsSourceChanged = true;
      UpdateForItemsSourceChanged();
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


    //
    // Protected Methods
    //

    #region Protected Methods

    protected virtual void SetContainerData(UIElement container, object data)
    {
        // Note that the ContentPresenter.Content property sets the DataContext.
      ((ContentPresenter)container).Content = data;            
    }

    #endregion 

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
        list.mItemsSourceChanged = true;
        list.UpdateForItemsSourceChanged();
        list.SetEvent();
    }

    private void SetEvent()
    {          
      ((ThreadSafeObservableCollection<LineHolder>)ItemsSource).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ContainerList_CollectionChanged);
    }

    void stackPanel_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      if (e.Delta != 0)
      {
        if(e.Delta > 0)
          mTopItemIndex -= 3;
        else
          mTopItemIndex += 3;

        if (mTopItemIndex < 0)
          mTopItemIndex = 0;
        else if (mTopItemIndex > (ItemsSource.Count - mItemsPerPage))
          mTopItemIndex = ItemsSource.Count - mItemsPerPage;
        SetData();
      }
    }

    private void VerticalScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      int newTopItemIndex = (int)e.NewValue;
      if (newTopItemIndex != mTopItemIndex)
      {
        mTopItemIndex = newTopItemIndex;
        SetData();
      }
    }

    //double mHorizontalOffset = 0.0;

    void HorizontalScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      ScrollBar scroll = sender as ScrollBar;
      mStackPanel.SetHorizontalOffset(scroll.Value);
      //_stackPanel.Margin = new Thickness(-scroll.Value, 0, 0, 0);
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

        int topItemIndex = Math.Min(mTopItemIndex, Math.Max(0, source.Count - mItemsPerPage));

        mScrollBarVertical.Value = topItemIndex;

        for (int i = 0; i < mContainers.Count && i < source.Count; i++)
        { 
          SetContainerData(mContainers[i], source[topItemIndex + i]);          
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

    void ContainerList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      SetData();
      mItemsSourceChanged = true;
      UpdateForItemsSourceChanged();
    }

    private void UpdateHorizontalScrollbar()
    {/*
      if (_LineMaxLength > _stackPanel.ActualWidth)
      {
        _scrollBarHorizontal.IsEnabled = true;
        _scrollBarHorizontal.Minimum = 0;
        _scrollBarHorizontal.Maximum = _LineMaxLength - _stackPanel.ActualWidth;
        _scrollBarHorizontal.ViewportSize = _stackPanel.ActualWidth;
      }
      else
        _scrollBarHorizontal.IsEnabled = false;*/

    }

    private void UpdateForItemsSourceChanged()
    {
      if (mBorder != null && mItemsSourceChanged)
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
          
          UpdateHorizontalScrollbar();
        }
        else
        {
          //_scrollBarHorizontal.IsEnabled = false;
          mScrollBarVertical.IsEnabled = false;
        }

        SetData();

        mItemsSourceChanged = false;
      }
    }

    #endregion 
  }
}
