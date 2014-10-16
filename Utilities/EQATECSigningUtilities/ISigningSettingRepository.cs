using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Interface for storing signing settings
  /// </summary>
  public interface ISigningSettingRepository
  {
    /// <summary>
    /// Getting signing setting for a public key or null if none are found
    /// </summary>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    SigningSetting Get(byte[] publicKey);
    /// <summary>
    /// Save the signing setting
    /// </summary>
    /// <param name="setting"></param>
    void Save(SigningSetting setting);
    /// <summary>
    /// Remove stored signing setting
    /// </summary>
    /// <param name="setting"></param>
    void Remove(SigningSetting setting);
    /// <summary>
    /// Get all known signing settings
    /// </summary>
    /// <returns></returns>
    IList<SigningSetting> GetAllSettings();
  }
}
