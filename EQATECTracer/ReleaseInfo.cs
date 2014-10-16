using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace EQATEC.Tracer
{
  /// <summary>
  /// Providing information on the released software
  /// </summary>
  public static class ReleaseInfo
  {
    /// <summary>
    /// Get wether the software is running a basic release
    /// </summary>
    /// <returns></returns>
    public static bool IsBasicRelease()
    {
      bool isBasic = false;
#if BASIC_RELEASE
      isBasic = true;
#else
      isBasic = false;
#endif
      return isBasic;
    }

    /// <summary>
    /// Getting the toolid for this release (used for version checking)
    /// </summary>
    /// <returns></returns>
    public static string ToolID
    {
      get
      {
        return IsBasicRelease() ? "tracerbasic" : "tracerfull";
      }
    }
    /// <summary>
    /// Getting the application id (used for version checking)
    /// </summary>
    public static string ApplicationID
    {
      get
      {
        return "eqatectracer";
      }
    }

    /// <summary>
    /// Getting the root path were the application is installed.
    /// </summary>
    /// <returns></returns>
    public static string GetInstallationRootPath()
    {
      //first we try the registry
      try
      {
        string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\EQATEC\Tracer", "InstallDir", null) as string;
        if (!string.IsNullOrEmpty(path))
          return path;
      }
      catch (Exception exc)
      {
        Trace.WriteLine("Could not extract the installation path from the registry. Error is " + exc.Message);
      }

      //then we just return the location of the current executable
      string assemblyLocation = Assembly.GetEntryAssembly().Location;
      return ExistingFolder(Path.GetDirectoryName(assemblyLocation));
    }
    /// <summary>
    /// Getting the path to the index file for the user guide
    /// </summary>
    /// <returns></returns>
    public static string GetPathToUserGuideIndex()
    {
      string rootPath = GetInstallationRootPath();
      return ExistingFile(Path.Combine(rootPath, @"UserGuide\index.htm"));
    }
    /// <summary>
    /// Getting the url to the tracer forum
    /// </summary>
    /// <returns></returns>
    public static string GetOnlineForumUrl()
    {
      return "http://www.eqatec.com/forum/tracer";
    }
    /// <summary>
    /// Getting the path to the license file
    /// </summary>
    public static string GetPathToLicense()
    {
      string rootPath = GetInstallationRootPath();
      return ExistingFile(Path.Combine(rootPath, @"EQATECTracerLicense.rtf"));
    }
    /// <summary>
    /// Getting the path to the demoapp
    /// </summary>
    /// <returns></returns>
    public static string GetDemoAppFolder()
    {
      string rootPath = GetInstallationRootPath();
      return ExistingFolder(Path.Combine(rootPath, "DemoApp"));
    }

    private static string ExistingFile( string file )
    {
      if (File.Exists(file))
        return file;
      else
        return string.Empty;
    }
    private static string ExistingFolder( string folder )
    {
      if (Directory.Exists(folder))
        return folder;
      else
        return string.Empty;
    }
  }
}
