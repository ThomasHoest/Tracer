using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.Windows;

namespace EQATEC.Tracer.UserControls
{
    //
    // This is a version of the ContainerList that avoids resetting the Content (and therefore the DataContext) property
    // of the containers when we scroll.  This has some further perf benefits.
    //
  public class ContainerListFastBinding : ContainerList
  {

    protected override void SetContainerData(UIElement container, object data)
    {
        //
        // Here we simply modify the DataProxy's data item.  The point is to avoid
        // resetting the ContentPresenter.Content property, which causes all bindings to 
        // be re-activated.  Since the proxy supports INotifyPropertyChange we'll still
        // update the binding values (in fact we must for the value to propagate to the UI).
        //
      if (data != null)
      {
        LineHolderProxy proxy;
        ContentPresenter contentPresenter = (ContentPresenter)container;
        
        if (contentPresenter.Content == null)
        {         
          contentPresenter.Content = new LineHolderProxy((LineHolder)data);          
          return;
        }

        proxy = (LineHolderProxy)contentPresenter.Content;
        proxy.DataItem = (LineHolder)data;        
      }
      if (ChildHeight == 0)
      {
        container.Measure(new Size(1000, 1000)); //What is the max??
        //_LineMaxLength = System.Math.Max(_LineMaxLength, container.DesiredSize.Width);
        
        if(container.DesiredSize.Height > 0)
          ChildHeight = container.DesiredSize.Height;
        //System.Diagnostics.Trace.WriteLine("Width: " + container.DesiredSize.Width);
      }
    }
      
  }

  /// <summary>
  /// This has the same properties as the original data object.  Each container binds to this instead; when the DataItem is changed
  /// due to a scroll we simply modify it inside the proxy.  This avoids changing the DataContext of the container.
  /// 
  /// Note that if the data item itself implements INotifyPropertyChanged it would be a good idea to listen to that event and pass
  /// it along by firing it on the proxy.  This one happens not to.
  /// </summary>
  public class LineHolderProxy : INotifyPropertyChanged
  {
    public LineHolderProxy(LineHolder dataItem)
    {
      _dataItem = dataItem;
    }

    public LineHolder Line
    {
      get
      {
        return _dataItem;
      }
    }

    public WordHolder Header
    {
      get { return _dataItem.Name; }          
    }
    
    public WordHolder ParamData
    {
      get { return _dataItem.Data; }
    }

    public ParameterHolder [] Params
    {
      get { return _dataItem.Params; }
    }

    public int IThreadID
    {
      get { return _dataItem.IThreadID; }
    }

    public string ThreadID
    {
      get { return _dataItem.ThreadID; }
    }

    public string Time
    {
      get { return _dataItem.Time; }
    }

    public LineHolder.LineType Type
    {
      get { return _dataItem.Type; }          
    }

    public LineHolder DataItem
    {
      get
      {
        return _dataItem;
      }

      set
      {
        if (_dataItem != value)
        {
          _dataItem = value;
          NotifyPropertyChanged("");  // empty string means 'all properties'
        }
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;


    private void NotifyPropertyChanged(string property)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(property));
      }
    }


    private LineHolder _dataItem;
  }
}
