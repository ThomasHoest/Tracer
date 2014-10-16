using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Security.Cryptography;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Signing settings repostiory in the registry
  /// </summary>
  public class RegistrySigningSettingRepository : ISigningSettingRepository
  {
    private static string FILEPATH = "FilePath";
    private static string ACTION = "Action";
    private static string PUBLICKEY = "PublicKey";
    private static string CREATED = "CreatedAt";
    private static string USEDBY = "UsedBy";
    private static string DEFAULT_REGISTRY_ROOT = @"Software\EQATEC\SigningSettings";

    private string m_rootRegistryPath;
    private SHA256 m_sha;
    
    /// <summary>
    /// Construct the default registry signing settings repository
    /// </summary>
    public RegistrySigningSettingRepository()
      :this(DEFAULT_REGISTRY_ROOT)
    {
    }
    /// <summary>
    /// Construct a registry signing setting repository at a specified
    /// root node in the registry
    /// </summary>
    /// <param name="rootRegsitryPathUnderCurrentUser"></param>
    public RegistrySigningSettingRepository(string rootRegsitryPathUnderCurrentUser)
    {
      m_rootRegistryPath = rootRegsitryPathUnderCurrentUser;
      string fullPath = @"HKEY_CURRENT_USER\" + m_rootRegistryPath;
      if (null == Registry.GetValue(fullPath, CREATED, null))
        Registry.SetValue(fullPath, CREATED, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegistryValueKind.String);
      
      m_sha = SHA256.Create();
    }

    #region Helpers
    private string GetHashOfPublicKey(byte[] publicKey)
    {
      byte[] hashBytes = m_sha.ComputeHash(publicKey);
      return PublicKeyUtil.ToHexString(hashBytes);
    }

    private RegistryKey GetOrCreateRegistryKeyForPublicKey(byte[] publicKey, bool create, bool writable)
    {
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(m_rootRegistryPath, writable || create))
      {
        if (key == null)
          return null;

        string hexName = GetHashOfPublicKey(publicKey);
        RegistryKey innerKey = key.OpenSubKey(hexName, writable);
        if (innerKey == null && create)
          innerKey = key.CreateSubKey(hexName);
        return innerKey;
      }
    }
    private SigningSetting GetSignSettingsFromKey(RegistryKey key)
    {
      if (key == null)
        return null;

      byte[] bytes = key.GetValue(PUBLICKEY, null) as byte[];
      SigningAction action = (SigningAction)((int)key.GetValue(ACTION, (int)SigningAction.Skip));
      string createdAt = key.GetValue(CREATED, null) as string ?? "";
      string usedBy = key.GetValue(USEDBY, null) as string ?? "";
      string fileName = key.GetValue(FILEPATH, null) as string ?? "";

      if (fileName == null || bytes == null)
        return null;

      return new SigningSetting(bytes, action, createdAt, usedBy, fileName);
    }
    #endregion

    public void Save(SigningSetting settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");

      using (RegistryKey k = GetOrCreateRegistryKeyForPublicKey(settings.PublicKey, true, true))
      {
        if (k == null)
          return;

        k.SetValue(PUBLICKEY, settings.PublicKey);
        k.SetValue(ACTION, (int)settings.SigningAction, RegistryValueKind.DWord);
        k.SetValue(CREATED, settings.CreatedAt);
        k.SetValue(USEDBY, settings.UsedBy);
        k.SetValue(FILEPATH, settings.PathToKeyContainer);
      }
    }

    public void Remove(SigningSetting settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");

      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(m_rootRegistryPath, true))
      {
        if (key == null)
          return;

        string hexName = GetHashOfPublicKey(settings.PublicKey);
        key.DeleteSubKeyTree(hexName);
      }
    }

    public SigningSetting Get(byte[] publicKeyToken)
    {
      using (RegistryKey key = GetOrCreateRegistryKeyForPublicKey(publicKeyToken, false, false))
      {
        return GetSignSettingsFromKey(key);
      }
    }

    public IList<SigningSetting> GetAllSettings()
    {
      List<SigningSetting> list = new List<SigningSetting>();
      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(m_rootRegistryPath, false))
      {
        if (key == null)
          return list;

        string[] subkeys = key.GetSubKeyNames();
        foreach (string sub in subkeys)
        {
          using (RegistryKey k = key.OpenSubKey(sub))
          {
            SigningSetting sf = GetSignSettingsFromKey(k);
            if (sf != null)
              list.Add(sf);
          }
        }
      }
      return list;
    }
  }
}
