using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;

namespace EQATEC.VersionCheckUtilities
{
  public static class NavigationHelper
  {
    private static string ReturnDefaultBrowserExecutable()
    {
      string browser = "iexplore.exe";
      RegistryKey key = null;
      try
      {
        key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);
        // trim off quotes
        browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
        if (!browser.EndsWith(".exe"))
        {
          // get rid of everything after the ".exe"
          browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
        }
      }
      finally
      {
        if (key != null)
          key.Close();
      }
      return browser;
    }

    public static bool OpenWebPage(bool openWithBrowser, string url)
    {
      try
      {
        if (openWithBrowser)
        {
          string arg = '"' + url + '"';
          Process p = Process.Start(ReturnDefaultBrowserExecutable(), arg);
        }
        else
        {
          Process p = Process.Start(url);
        }
        return true;
      }
      catch (Exception exc)
      {
        Trace.TraceError("Could not open a web page by spawning a process with the url. Error is {0}", exc.Message);
        return false;
      }
    }
  }
}
