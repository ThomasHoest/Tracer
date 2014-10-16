using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.VersionCheckUtilities
{
  /// <summary>
  /// Contains the result of a version check
  /// </summary>
  public class VersionCheckResult
  {
    /// <summary>
    /// The lastest version, if available
    /// </summary>
    public readonly Version LatestVersion;
    /// <summary>
    /// Whether the version check was successfull at all
    /// </summary>
    public readonly bool CheckSuccessful;
    /// <summary>
    /// Whether the current version should be updated
    /// </summary>
    public readonly bool ShouldUpdate;
    /// <summary>
    /// The direct download url for the latest version
    /// </summary>
    public readonly string LatestVersionDownloadUrl;
    /// <summary>
    /// The download message delivered along with the versio check
    /// </summary>
    public readonly string DownloadMessage;
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionCheckResult"/> class.
    /// </summary>
    /// <param name="sucess">if set to <c>true</c> [sucess].</param>
    /// <param name="latestVersion">The latest version.</param>
    /// <param name="shouldUpdate">if set to <c>true</c> [should update].</param>
    public VersionCheckResult(bool sucess, Version latestVersion, bool shouldUpdate, string downloadUrl, string downloadMessage)
    {
      CheckSuccessful = sucess;
      LatestVersion = latestVersion;
      ShouldUpdate = shouldUpdate;
      LatestVersionDownloadUrl = downloadUrl;
      DownloadMessage = downloadMessage;
    }
  }

}
