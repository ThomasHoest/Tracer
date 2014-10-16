using System;
using System.Collections.Generic;
using System.Text;

namespace TestAssembly.Tests
{
  public class OperatorTests : ITestClass
  {
    public string m_member;
    public void ExerciseClass()
    {
      new InnerClass("est").Test();
      new InnerClass(null).Test();

      TestDoubleQuestionMarkOperator("hello");
      TestDoubleQuestionMarkOperator(null);
    }

    private class InnerClass
    {
      public string s_memeber;
      public InnerClass( string input )
      {
        s_memeber = input ?? string.Empty;
      }
      public void Test()
      {
        Console.Write("This is the test:" + s_memeber == null? "": s_memeber);
      }
    }

    public void TestDoubleQuestionMarkOperator( string input )
    {
      m_member = input ?? string.Empty;
    }
  }
}
