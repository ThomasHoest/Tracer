using System;
using System.Collections.Generic;
using System.Text;

namespace TestAssembly.Tests
{
  public class GenericDelegateTesting : ITestClass
  {
    public void ExerciseClass()
    {
      new ResourcePool<GenericDelegateTesting>().GetResource(
        new ResourcePool<GenericDelegateTesting>.ResourceMatch(delegate( GenericDelegateTesting resourceLoaded )
          {
            return false;
          }));
    }
  }

  public class ResourcePool<T> where T:class
  {
    public delegate T ResourceConstructionDelegate();
    public delegate bool ResourceMatch( T resourceLocated );

    public T GetResource( ResourceMatch match )
    {
      return null;
    }
  }
}
