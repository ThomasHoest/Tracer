using System;
using System.Collections;
using System.Collections.Generic;
namespace EQATEC.VersionCheckUtilities
{
  public interface IVersionChecker
  {
    void BeginCheckForNewVersion(string toolID, string application);
    VersionCheckResult CheckForNewVersion(string toolID, string application);
    
    event EventHandler<VersionCheckEventArgs> VersionCheckCompleted;
    event EventHandler<AdditionalInformationEventArgs> AddtionalInformationRequested;
  }
}
