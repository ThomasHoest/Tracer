using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace EQATEC.Tracer.Tools
{
  public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
  {
    private static UInt64 _Counter = 0;

    private int _SuspendCollectionChangedCount = 0;

    private ChangeCollection _ChangesCollection = new ChangeCollection();

    protected Dispatcher _Dispatcher;
    ReaderWriterLock _lock;

    public UInt64 ID
    {
      get { return _ID; }
    }
    private UInt64 _ID = _Counter++;
    // TODO: Must this be an atomic command?

    public ThreadSafeObservableCollection()
    {
      _Dispatcher = Dispatcher.CurrentDispatcher;
      _lock = new ReaderWriterLock();
    }

    protected override void ClearItems()
    {
      LockCookie c = _lock.UpgradeToWriterLock(-1);
      base.ClearItems();
      _lock.DowngradeFromWriterLock(ref c);
    }

    protected override void InsertItem(int index, T item)
    {
      if (index > this.Count)
        return;
      LockCookie c = _lock.UpgradeToWriterLock(-1);
      base.InsertItem(index, item);
      _lock.DowngradeFromWriterLock(ref c);
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
      if (oldIndex >= this.Count | newIndex >= this.Count | oldIndex == newIndex)
        return;
      LockCookie c = _lock.UpgradeToWriterLock(-1);
      base.MoveItem(oldIndex, newIndex);
      _lock.DowngradeFromWriterLock(ref c);
    }

    protected override void RemoveItem(int index)
    {
      if (index >= this.Count)
        return;
      LockCookie c = _lock.UpgradeToWriterLock(-1);
      base.RemoveItem(index);
      _lock.DowngradeFromWriterLock(ref c);
    }

    protected override void SetItem(int index, T item)
    {
      LockCookie c = _lock.UpgradeToWriterLock(-1);
      base.SetItem(index, item);
      _lock.DowngradeFromWriterLock(ref c);
    }

    public T[] ToSyncArray()
    {
      _lock.AcquireReaderLock(-1);
      T[] _sync = new T[this.Count];
      this.CopyTo(_sync, 0);
      _lock.ReleaseReaderLock();
      return _sync;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      //If the CollectionChanged event has been suspended, don't fire it, instead append changed to the ChangesCollection
      if (_SuspendCollectionChangedCount == 0)
      {
        if (CollectionChanged != null)
          FireCollectionChanged(e);
      }
      else
        _ChangesCollection.ApplyChanges(e);
    }

    private void FireCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      //Determine the correct thread (dispatcher) for each listener to fire the CollectionChanged event on
      foreach (NotifyCollectionChangedEventHandler handler in CollectionChanged.GetInvocationList())
      {
        DispatcherObject dispatcherInvoker = handler.Target as DispatcherObject;
        ISynchronizeInvoke syncInvoker = handler.Target as ISynchronizeInvoke;
        if (dispatcherInvoker != null)
        {
          // We are running inside DispatcherSynchronizationContext
          FireCollectionChangedDelegate(dispatcherInvoker.Dispatcher, handler, e);
        }
        else if (syncInvoker != null)
        {
          // We are running inside WindowsFormsSynchronizationContext
          if (syncInvoker.InvokeRequired)
            syncInvoker.BeginInvoke(new ParameterizedThreadStart(FireCollectionChangedDelegate), new Object[] { handler, e });
          else
            FireCollectionChangedDelegate(new Object[] { handler, e });
        }
        else
        {
          FireCollectionChangedDelegate(_Dispatcher, handler, e);
        }
      }
    }

    private void FireCollectionChangedDelegate(Dispatcher dispatcher, NotifyCollectionChangedEventHandler handler, NotifyCollectionChangedEventArgs e)
    {
      if (dispatcher.CheckAccess())
        FireCollectionChangedDelegate(new Object[] { handler, e });
      else
        dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new ParameterizedThreadStart(FireCollectionChangedDelegate), new Object[] { handler, e });
    }

    private void FireCollectionChangedDelegate(object state)
    {
      object[] args = state as object[];
      NotifyCollectionChangedEventHandler handler = args[0] as NotifyCollectionChangedEventHandler;
      NotifyCollectionChangedEventArgs e = args[1] as NotifyCollectionChangedEventArgs;
      if (ArgumentsContainsMultipleChanges(e) && handler.Target is System.Windows.Data.CollectionView)
        (handler.Target as System.Windows.Data.CollectionView).Refresh();
      else
      {
        _lock.AcquireReaderLock(-1);
        handler(this, e);
        _lock.ReleaseReaderLock();
      }
    }

    private bool ArgumentsContainsMultipleChanges(NotifyCollectionChangedEventArgs args)
    {
      return (args.NewItems != null && args.NewItems.Count > 1) || (args.OldItems != null && args.OldItems.Count > 1);
    }

    public override event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Suspends the CollectionChanged event until Resume is called
    /// </summary>
    protected void Suspend()
    {
      _SuspendCollectionChangedCount++;
    }

    /// <summary>
    /// Gets whether or not the CollectionChanged event is fired after list changes.
    /// </summary>
    public bool Suspended
    {
      get { return _SuspendCollectionChangedCount != 0; }
    }

    /// <summary>
    /// Reactivates the CollectionChanged event.
    /// <para> </para>
    /// <para>
    /// IMPORTANT NOTE! The Suspend-Resume functionality is implemented to handle nested Suspending using a counter.
    /// It is very important to ensure that Resume() is called exactly as many times as Suspend(), or the
    /// CollectionChanged event might fire at inappropriate times or not at all.
    /// </para>
    /// To ensure this, always implement the constallation in a try-finally block, with the Resume() call in the finally part.
    /// </summary>
    protected void Resume()
    {
      _SuspendCollectionChangedCount--;

      if (_SuspendCollectionChangedCount == 0)
      {
        NotifyCollectionChangedEventArgs args = _ChangesCollection.GetChangesEventArgs();
        if (args != null)
          OnCollectionChanged(args);
        _ChangesCollection.Clear();
      }
    }

    public void AddRange(IList<T> list)
    {
      try
      {
        Suspend();

        foreach (T item in list)
          Add(item);
      }
      finally
      {
        Resume();
      }
    }

    public void RemoveRange(IList<T> list)
    {
      try
      {
        Suspend();

        foreach (T item in list)
          Remove(item);
      }
      finally
      {
        Resume();
      }
    }

    private object obj_Lock = new object();/*
    public void Update<TKey>(IList<T> sourceList, Func<T, TKey> keySelector)
    {
      //Todo: Is this still a hack???
      lock (obj_Lock) //TODO HACK - bottleneck to use a global lock
      {
        try
        {
          Suspend();

          Dictionary<TKey, T> destinationDic = this.ToDictionary(keySelector);
          Dictionary<TKey, T> sourceDic = sourceList.ToDictionary(keySelector);

          // remove old items
          foreach (TKey id in destinationDic.Keys)
          {
            if (!sourceDic.ContainsKey(id))
              Remove(destinationDic[id]);
          }

          // update items
          foreach (TKey id in sourceDic.Keys)
          {
            if (destinationDic.ContainsKey(id))
            {
              //if (!sourceDic[id].Equals(destinationDic[id]))
              //Tools.CopyTo(sourceDic[id], destinationDic[id]);
            }
            else
              Add(sourceDic[id]);
          }
        }
        finally
        {
          Resume();
        }
      }
    }*/

    private class ChangeCollection
    {
      private ArrayList _AddedList = new ArrayList();
      private ArrayList _RemovedList = new ArrayList();

      /// <summary>
      /// Clears the changes.
      /// </summary>
      internal void Clear()
      {
        _AddedList = new ArrayList();
        _RemovedList = new ArrayList();
      }

      /// <summary>
      /// Applies new changes.
      /// </summary>
      /// <param name="changes">The NotifyCollectionChangedEventArgs containing the changes to apply.</param>
      internal void ApplyChanges(NotifyCollectionChangedEventArgs changes)
      {
        if (changes.NewItems != null)
          _AddedList.AddRange(changes.NewItems);
        if (changes.OldItems != null)
          _RemovedList.AddRange(changes.OldItems);
      }

      /// <summary>
      /// Gets a NotifyCollectionChangedEventArgs object based on the applied changes.
      /// </summary>
      /// <returns>A NotifyCollectionChangedEventArgs object based on the applied changes, or null if no changes has been applied.</returns>
      internal NotifyCollectionChangedEventArgs GetChangesEventArgs()
      {
        bool itemsAdded = _AddedList.Count > 0;
        bool itemsRemoved = _RemovedList.Count > 0;
        if (itemsAdded)
        {
          if (itemsRemoved)
            return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, _AddedList, _RemovedList);
          return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _AddedList);
        }
        if (itemsRemoved)
          return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _RemovedList);
        return null;
      }
    }
  }
}
