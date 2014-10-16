using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Utility class for public keys
  /// </summary>
  public static class PublicKeyUtil
  {
    /// <summary>
    /// Formats public key to hex string
    /// </summary>
    /// <param name="byteArray"></param>
    /// <returns></returns>
    public static string ToHexString(byte[] byteArray)
    {
      StringBuilder sb = new StringBuilder();
      foreach (byte b in byteArray)
        sb.Append(b.ToString("X2"));
      return sb.ToString();
    }
    /// <summary>
    /// Check if the publickey stored in key container is equal to
    /// the public key
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="pathToKeyContainer"></param>
    /// <returns></returns>
    public static bool PublicKeyMatch(byte[] publicKey, string pathToKeyContainer)
    {
      if (!File.Exists(pathToKeyContainer))
        return false;

      StrongNameKeyPair keyPair = new StrongNameKeyPair(File.ReadAllBytes(pathToKeyContainer));
      return PublicKeyMatch(publicKey, keyPair.PublicKey);
    }
    /// <summary>
    /// Check that public keys match
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="otherPublicKey"></param>
    /// <returns></returns>
    public static bool PublicKeyMatch(byte[] publicKey, byte[] otherPublicKey)
    {
      if (null == publicKey || null == otherPublicKey)
        return false;

      if (publicKey.Length != otherPublicKey.Length)
        return false;

      for (int i = 0; i < publicKey.Length; i++)
      {
        if (publicKey[i] != otherPublicKey[i])
          return false;
      }
      return true;
    }
  }
}
