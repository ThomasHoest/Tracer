using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace TestApp
{
  public abstract class BaseClass
  {
    protected void SpecialPurposeBaseFunction()
    {
      Console.WriteLine("Iam in an abstract base class");
    }
  }

  public class EmpyClass
  {
  }

  public class ClassWithAnonymous
  {
    int i = 0;

    public int SomeIntegerFunction()
    {
      return i++;
    }

    public void SomeFunctionWithAnonymousDelegate()
    {
      List<int> intList = new List<int>();

      for (int i = 0; i < 10; i++)
        intList.Add(i);

      List<int> evenNumbers = intList.FindAll(
        delegate(int number)
        {
          return number % 2 == 0;
        }
      );
    }

  }

  class SomeClass : BaseClass
  {
    public class NestedClass
    {
      string mData;

      public NestedClass(string someData)
      {
        mData = someData;        
      }

      public void GenericParam(List<string> dataList)
      {
      }

      Utillities.AnotherClass mSomeOtherClass = new TestApp.Utillities.AnotherClass();


      public T GetProperty<T>(object propertyName) where T : Utillities.AnotherClass
      {
        return (T)this.mSomeOtherClass;
      }

      public T1 GetProperty<T1,T2>(object propertyName) 
      {
        return default(T1);
      }

      public T1 GetProperty<T1,T2,T3>(object propertyName)
      {
        return default(T1);
      }

      public string GetData()
      {
        return mData;
      }

    }

    string mName;

    string mReply;

    public virtual string Reply
    {
      get { return mReply; }
      set { mReply = value; }
    }

    public SomeClass(/*string name*/)
    {
      mName = "Sir Lancelot";// name;
      mReply = "No you didn't";
    }

    public void DeepException()
    {
      try
      {
        SomePrivateFunction();
      }
      catch(Exception ex)
      {/*
        int i = 0;
        System.Console.WriteLine(ex.Message);
        System.Console.WriteLine(ex.StackTrace);*/
      }
      finally
      {
        Console.WriteLine("Finally");
      }
    }

    private void SomePrivateFunction()
    {
      SomeExceptionFunction();
    }

    private void SomeExceptionFunction()
    {
      throw new InvalidOperationException();
    }
    

    public string TellMeYourNamePlease()
    {
      return string.Concat("My name is ", mName);
    }

    public string Gotcha()
    {
      return Reply;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LayoutTest
    {
      [FieldOffset(0)]
      public int i1;
      [FieldOffset(4)]
      public int i2;
      [FieldOffset(8)]
      public int i3;
      [FieldOffset(12)]
      public int i4;
    }

    public void SomeFunction()
    {
      SpecialPurposeBaseFunction();
      MyStruct s = new MyStruct();
      DateTime test = DateTime.Now;
      double dtest = 0.1;
      bool btest = false;
      string temp = "test";
      RefTest(ref test, ref dtest, ref btest, ref temp);
      Parameters1(1, 2.0, "test", MySpecialType.Type1, s);
      Parameters2(2);
    }

    public void Swap<T>(ref T a, ref T b)
    {
      Param(a);
      Param(b);
    }

    public IEnumerable Forever()
    {
      int i=0;
      while (true)
        yield return i++;
    }

    public void RefTest(ref DateTime test, ref double dtest, ref bool btest, ref string objectref)
    {
      Param(test);
      Param(dtest);
      Param(btest);
      Param(objectref);
    }

    public void Parameters1(int par1, double par2, string par3, MySpecialType type, MyStruct s)
    {
      Param(SomeReturnType(par1));
      Param(par2);
      Param(SomeReturnType(par3));
      Param(type);
      Param(s);/*
      foreach (int i in Forever())
      {
        if (i == 100)
          System.Diagnostics.Debugger.Break();
        Param(i);
      }*/
    }

    public void NativeInt(ref IntPtr someInt)
    {
      Param((int)someInt);
    }

    public void Parameters2(int par1)
    {
      Param(par1);
    }
    
    public unsafe void FunctionPointerTest(LayoutTest *test)
    {
      ToObject(*test);  
    }

    public unsafe void VoidPointer(void* voidTest)
    {
      ToObject((UInt32)voidTest);
    }
    
    //public unsafe 

    public void ToObject(object test)
    {
    }

    public void Parameters3(int id, params object[] list)
    {
    }

    public void Param(int p)
    {
      try
      {
        Console.WriteLine("Param " + p);
      }
      finally
      {
        Console.WriteLine("Finally something");
      }
    }

    public void Param(ref int p)
    {
      Console.WriteLine("Param " + p);
    }

    public void Param(float p)
    {
      Console.WriteLine("Param " + p);
    }

    public void Param(double p)
    {
      Console.WriteLine("Param " + p);
    }

    public void Param(short p)
    {
      Console.WriteLine("Param " + p);
    }

    public string[] ReturnSomeLargeArray()
    {
      string[] largeArray = new string[10000];
      for (int i = 0; i < 10000; i++)
        largeArray[i] = i.ToString(); ;
      return largeArray;
    }

    private int SomeReturnType(int p)
    {
      p++;
      return p;
    }

    private string SomeReturnType(string p)
    {
      p += "Add some text";
      return p;
    }

    public enum MySpecialType
    {
      Type1,
      Type2
    }

    public struct MyStruct
    {
      int i;
      float f;
    }

    public void Param(object p)
    {
      Console.WriteLine("Param " + p);
    }

    public Utillities.AnotherClass SomeReturnValue2()
    {
      Utillities.AnotherClass a = new Utillities.AnotherClass();
      return a;
    }

    public byte[] GetLotsOfBytes()
    {
      return new byte[] { 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12 };
    }

    public object SomeReturnValue3()
    {
      int i = 20;
      Param(ref i);
      return (object)i;
    }
  }

  namespace Utillities
  {
    class AnotherClass : SomeClass
    {
      string mSomeString = "Some string data";

      public override string Reply
      {
        get { return base.Reply; }
      }

      public string GiveMeAString()
      {
        return "A string";
      }
    }
  }
}
