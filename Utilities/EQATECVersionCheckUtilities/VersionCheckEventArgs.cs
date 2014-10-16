using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.VersionCheckUtilities
{
  public class VersionCheckEventArgs : EventArgs
  {
    public readonly VersionCheckResult VersionCheckResult;
    public VersionCheckEventArgs(VersionCheckResult result)
    {
      VersionCheckResult = result;
    }
  }
}
