using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace EQATEC.Tracer.Tools
{
  /// <summary>
  /// Simple interface for implementing item as part of treeview
  /// </summary>
  /// <typeparam name="T">Type of item in treeview</typeparam>
  public interface ICustomTreeviewItem < T >
  {
    Collection<T> GetSubItems();
  }

  public class CustomTreeviewItem<T> : INotifyPropertyChanged
    where T : ICustomTreeviewItem<T>
  {
    List<T> mItemIsExpandedCache;

    #region Properties

    ObservableCollection<CustomTreeviewItem<T>> mRoot;
    public ObservableCollection<CustomTreeviewItem<T>> Root
    {
      get { return mRoot; }
      set { mRoot = value; }
    }

    bool mIsExpanded;
    public bool IsExpanded
    {
      get { return mIsExpanded; }
      set 
      {        
        mIsExpanded = value;
        NotifyPropertyChanged("IsExpanded");
      }
    }
    
    T mNodeData;
    public T NodeData
    {
      get { return mNodeData; }
      set { mNodeData = value; }
    }

    int mLevel = 0;
    public int Level
    {
      get { return mLevel; }
      set { mLevel = value; }
    }
        
    int mChildCount = 0;
    public int ChildCount
    {
      get
      {
        return mChildCount;
      }
    }

    #endregion

    /// <summary>
    /// Private constructor for internal use
    /// </summary>
    /// <param name="nodeData">Data for new item</param>
    private CustomTreeviewItem(T nodeData)
    {
      mNodeData = nodeData;
      mChildCount = nodeData.GetSubItems().Count;
    }

    /// <summary>
    /// Create treeview root
    /// </summary>
    /// <param name="root">Root collection of one or more roots</param>
    public CustomTreeviewItem(Collection<T> root)
    {
      mItemIsExpandedCache = new List<T>();
      mRoot = new ObservableCollection<CustomTreeviewItem<T>>();
      foreach (T item in root)
        AddItem(item);      
    }

    private void AddItem(T item)
    {
      CustomTreeviewItem<T> newItem = new CustomTreeviewItem<T>(item);
      newItem.PropertyChanged += new PropertyChangedEventHandler(TreeItemPropertyChanged);
      mRoot.Add(newItem);
    }

    public void CollapseAll()
    {
      for (int i = 0; i < mRoot.Count; i++)
        CollapseFromNode(mRoot[i]);
    }

    private void CollapseFromNode(CustomTreeviewItem<T> item)
    {
      int indexParent = mRoot.IndexOf(item);
      if (indexParent + 1 < mRoot.Count && mRoot[indexParent + 1].Level > item.Level)
        CollapseFromNode(mRoot[indexParent + 1]);
      item.IsExpanded = false;
    }

    public void ExpandAll()
    {
      //CollapseAll(); // TODO JER: very quick fix, but it works

      //CustomTreeviewItem<T>[] origRootElements = mRoot.ToArray<CustomTreeviewItem<T>>();
      //foreach (CustomTreeviewItem<T> item in origRootElements)
      //  ExpandFromNode(item);
    }

    Stack<T> mExpandPath = new Stack<T>();

    public CustomTreeviewItem<T> ExpandToItem(T item)
    {
      mExpandPath.Clear();
      bool found = false;
      //Find node in tree
      foreach (CustomTreeviewItem<T> treeItem in mRoot)
      {
        if (FindItem(treeItem.NodeData, treeItem.NodeData.GetSubItems(), item))
        {
          found = true;
          break;
        }
      }

      if (found)
      {
        T[] nodesToExpand = mExpandPath.ToArray();
        int index = 0;
        for (int i = nodesToExpand.Length - 1; i >= 0; i--)
        {
          while(mRoot.Count > index)
          {
            if(mRoot[index].NodeData.Equals(nodesToExpand[i]))
            {
              if(!mRoot[index].IsExpanded)
                mRoot[index++].IsExpanded = true;
              break;
            }
            index++;
          }
        }

        while (mRoot.Count > index)
        {
          if (mRoot[index].NodeData.Equals(item))
            return mRoot[index];
          index++;
        }
      }

      return null;
    }

    private bool FindItem(T parent, Collection<T> nodes, T itemToFind)
    {
      mExpandPath.Push(parent);
      foreach (T item in nodes)
      {
        if (item.Equals(itemToFind))
          return true;

        Collection<T> subItems = item.GetSubItems();
  
        if(subItems.Count > 0)
          if (FindItem(item, subItems , itemToFind))
            return true;
      }
      mExpandPath.Pop();
      return false;
    }

    private void ExpandFromNode(CustomTreeviewItem<T> item)
    {
      int indexParent = mRoot.IndexOf(item);
      item.IsExpanded = true;
      int count = 1;

      while (indexParent + count < mRoot.Count && mRoot[indexParent + count].Level > item.Level)
      {
        ExpandFromNode(mRoot[indexParent + count]);
        count++;
      }
    }
           
    private void PopulateTreeNode(CustomTreeviewItem<T> parent)
    {
      int indexParent = mRoot.IndexOf(parent);
      List<CustomTreeviewItem<T>> mListToExpand = new List<CustomTreeviewItem<T>>();
      //Populate 
      //if (indexParent == mRoot.Count - 1 || mRoot[indexParent + 1].Level <= parent.Level)
      {
        Collection<T> nodes = parent.NodeData.GetSubItems();
        for (int i = 0; i < nodes.Count; i++)
        {
          T node = nodes[i];  
          CustomTreeviewItem<T> item = new CustomTreeviewItem<T>(node);
          item.Level = parent.Level + 1;
          item.PropertyChanged += new PropertyChangedEventHandler(TreeItemPropertyChanged);
          mRoot.Insert(indexParent + 1 + i, item);

          if (mItemIsExpandedCache.Contains(node))
          {
            mItemIsExpandedCache.Remove(node);
            mListToExpand.Add(item);
          }          
        }

        foreach (CustomTreeviewItem<T> item in mListToExpand)
          item.IsExpanded = true;

      }
    }

    private void ClearSubKeys(CustomTreeviewItem<T> parent)
    {
      int indexToRemove = mRoot.IndexOf(parent) + 1;
      while ((indexToRemove < mRoot.Count) && (mRoot[indexToRemove].Level > parent.Level))
      {
        if (mRoot[indexToRemove].IsExpanded)
          mItemIsExpandedCache.Add(mRoot[indexToRemove].NodeData);
        mRoot.RemoveAt(indexToRemove);
      }
    }

    void TreeItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsExpanded")
      {
        CustomTreeviewItem < T > node = (CustomTreeviewItem<T>)sender;
        if (node.IsExpanded)
        {
          this.PopulateTreeNode(node);
        }
        else
        {
          this.ClearSubKeys(node);
        }
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion
  }
}
