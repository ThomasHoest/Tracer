using System;
using System.Collections.Generic;
using System.Text;
using EQATEC.Tracer;
using EQATEC.Tracer.Tools;
using NUnit.Framework;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using Mono.Cecil;
using EQATEC.CecilToolBox;
using TestAssembly;
using System.Diagnostics;
using System.Security.Policy;

namespace EQATECTracerTestSuite
{
  [TestFixture]
  public class InstrumentationTests
  {
    [Test]
    public void TestInstrumentationOfTestAssembly()
    {
      BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
      Type entryType = typeof(TestAssembly.EntryPoint);
      MethodInfo entryMethod = entryType.GetMethod("ExerciseAssemblyContent", flags);
      Assert.IsNotNull(entryMethod);

      //finding and addding the TestAssembly
      FileInfo assemblyFile = LocatePathToTestAssembly();
      string folderPathToTracifiedOutput = Path.Combine(assemblyFile.Directory.FullName, "Tracified");
      Directory.CreateDirectory(folderPathToTracifiedOutput);

      int instrumentationCount = 0;
      InstrumentationHandler handler = new InstrumentationHandler(null);
      handler.OnInstrumentationAction += new InstrumentationHandler.InstrumentationActionHandler(delegate( InstrumentationActionEventArgs e )
        {
          switch (e.Action)
          {
            case InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationEnded:
              instrumentationCount++;
              break;
          }
        });

      handler.AddAssembly(assemblyFile.FullName);
      handler.Instrument(folderPathToTracifiedOutput);
      Assert.AreEqual(1, instrumentationCount);

      //find the instrumented assembly
      FileInfo instrumentedAssemblyFile = LocateInstrumentedAssembly(folderPathToTracifiedOutput, assemblyFile);

      //load the instrumented assembly and fire off the EntryPoint
      Assembly instrumentedAssembly = Assembly.LoadFrom(instrumentedAssemblyFile.FullName);
      Trace.WriteLine("Loaded version " + instrumentedAssembly.GetName().Version);
      Type t = instrumentedAssembly.GetType(entryType.FullName);
      MethodInfo m = t.GetMethod(entryMethod.Name, flags);

      //now we run the method which is responsible for exercising all content of the files
      object result = m.Invoke(null, null);
      Console.WriteLine("Invoke returned:" + result.ToString());
    }

    [Test]
    public void TestThatInvalidReferencedAssemblyFails_ThrowsAnInnerExceptionOfInvalidProgramException()
    {
      Tester remoteTester = CreateInstanceInNewAppDomain<Tester>();
      remoteTester.RunTest();
    }

    public T CreateInstanceInNewAppDomain<T>()
    {
      AppDomain appDomain = AppDomain.CreateDomain(
        "AppDomainFor_"+typeof(T).Name,
        AppDomain.CurrentDomain.Evidence,
        new FileInfo(typeof(T).Assembly.Location).Directory.FullName,
        AppDomain.CurrentDomain.RelativeSearchPath,
        true);
      object t = appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
      return (T)t;
    }

    [Serializable]
    public class Tester : MarshalByRefObject
    {
      public void RunTest()
      {
        FileInfo thisAssemblyLocation = new FileInfo(typeof(InstrumentationTests).Assembly.Location);
        DirectoryInfo testingDirectory = new DirectoryInfo(Path.Combine(thisAssemblyLocation.Directory.FullName, "InvalidILAssemblyTesting"));
        if (!testingDirectory.Exists)
          testingDirectory.Create();

        using (Stream stream = GetType().Assembly.GetManifestResourceStream("EQATECTracerTestSuite.ReferenceAssemblies.InvalidILAssemblies.EQATECTracerRuntime.dll"))
        {
          byte[] contents = new byte[stream.Length];
          stream.Read(contents, 0, contents.Length);
          using (FileStream fs = new FileStream(Path.Combine(testingDirectory.FullName, "EQATECTracerRuntime.dll"), FileMode.Create, FileAccess.Write))
          {
            fs.Write(contents, 0, contents.Length);
          }
        }
        string outputAssembly = Path.Combine(testingDirectory.FullName, "TestAssembly.dll");
        using (Stream stream = GetType().Assembly.GetManifestResourceStream("EQATECTracerTestSuite.ReferenceAssemblies.InvalidILAssemblies.TestAssembly.dll"))
        {
          byte[] contents = new byte[stream.Length];
          stream.Read(contents, 0, contents.Length);
          using (FileStream fs = new FileStream(outputAssembly, FileMode.Create, FileAccess.Write))
          {
            fs.Write(contents, 0, contents.Length);
          }
        }

        try
        {
          Assembly instrumentedAssembly = Assembly.LoadFrom(outputAssembly);
          Assert.IsNotNull(instrumentedAssembly);
          Assert.AreEqual(new Version(1, 0, 0, 0), instrumentedAssembly.GetName().Version, "Did not load expected version of the assembly, probably loaded an already existingly loaded assembly");

          Type t = instrumentedAssembly.GetType("TestAssembly.EntryPoint");
          MethodInfo m = t.GetMethod("ExerciseAssemblyContent", BindingFlags.Public | BindingFlags.Static);

          //now we run the method which is responsible for exercising all content of the files
          object result = m.Invoke(null, null);
          Assert.Fail("Should not be able to successfully call the invalid il assembly");
        }
        catch (AssertionException)
        {
          throw;
        }
        catch (Exception exc)
        {
          Trace.WriteLine(exc.ToString());
          Assert.That(HasInnerExceptionThatIsInvalidProgramException(exc), "Did not throw expected exception when loading the invalid assembly");
        }
      }

      private bool HasInnerExceptionThatIsInvalidProgramException( Exception exc )
      {
        Trace.WriteLine(string.Format("Looking at exception of type {0}", exc.GetType().Name));
        if (exc is InvalidProgramException)
          return true;
        if (exc.InnerException != null)
          return HasInnerExceptionThatIsInvalidProgramException(exc.InnerException);
        return false;
      }
    }

    #region Helpers
    private FileInfo LocateInstrumentedAssembly( string folderPathToTracifiedOutput, FileInfo assemblyFile )
    {
      return new FileInfo(Path.Combine(folderPathToTracifiedOutput, assemblyFile.Name));
    }

    private FileInfo LocatePathToTestAssembly()
    {
      string codebase = typeof(EntryPoint).Assembly.Location;
      return new FileInfo(codebase);
    }

    private CompilerParameters CreateCompilerOptions(Assembly assembly)
    {
      CompilerParameters options = new CompilerParameters();
      options.GenerateInMemory = true;
      options.GenerateExecutable = false;
      options.ReferencedAssemblies.Add(assembly.Location);
      foreach (AssemblyName name in assembly.GetReferencedAssemblies())
      {
        Assembly loaded = Assembly.Load(name.FullName);
        options.ReferencedAssemblies.Add(loaded.Location);
      }
      options.IncludeDebugInformation = true;
      return options;
    }
    #endregion
  }
}
