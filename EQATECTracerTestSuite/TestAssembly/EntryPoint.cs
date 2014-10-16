using System;
using System.Collections.Generic;
using System.Text;

namespace TestAssembly
{
  public static class EntryPoint
  {
    private static List<ITestClass> s_classesToTest = new List<ITestClass>();
    static EntryPoint()
    {
      foreach (Type t in typeof(EntryPoint).Assembly.GetTypes())
      {
        if (!t.IsAbstract &&
          !t.IsInterface)
        {
          if (t.IsSubclassOf(typeof(ITestClass)))
          {
            s_classesToTest.Add((ITestClass)Activator.CreateInstance(t));
          }
        }
      }
    }

    public static int ExerciseAssemblyContent()
    {
      int tested = 0; 
      foreach (ITestClass testclass in s_classesToTest)
      {
        testclass.ExerciseClass();
        tested++;
      }
      return tested;
    }
  }
}
