using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EQATEC.Tracer.Tools
{
  public class CollectionTools
  {
    public static void SortCollection<T>(Collection<T> col)
    {
      //Todo: Inefficient sorting, room for improvement
      SortedList<string, T> assemblySort = new SortedList<string, T>();
      try
      {
        for (int i = 0; i < col.Count; i++)
        {
          T item = col[i];
          assemblySort.Add(item.ToString(), item);
        }
      }
      catch (Exception)
      {
        System.Diagnostics.Trace.WriteLine("Exception while sorting tree elements");
        System.Diagnostics.Debug.Assert(false);
        return;
      }

      col.Clear();

      foreach (KeyValuePair<string, T> pair in assemblySort)
        col.Add(pair.Value);
    }
  }
}