using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Class encapsulating the actions to be taken for a given 
  /// public key
  /// </summary>
  public class SigningSetting
  {
    private byte[] m_PublicKey;
    private SigningAction m_SigningAction;
    private string m_UsedBy;
    private string m_CreatedAt;
    private string m_PathToKeyContainer;

    /// <summary>
    /// The public key for the settings
    /// </summary>
    public byte[] PublicKey { get { return m_PublicKey; } }
    /// <summary>
    /// Signing action for the public key
    /// </summary>
    public SigningAction SigningAction { get { return m_SigningAction; } }
    /// <summary>
    /// The creation time for this action
    /// </summary>
    public string CreatedAt { get { return m_CreatedAt; } }
    /// <summary>
    /// The name of the file (or, one of the files) it is used by
    /// </summary>
    public string UsedBy { get { return m_UsedBy; } }
    /// <summary>
    /// The full path to the key container
    /// </summary>
    public string PathToKeyContainer { get { return m_PathToKeyContainer; } }

    /// <summary>
    /// Construct the signing setting
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="signAction"></param>
    /// <param name="publicKeyContainerPath"></param>
    public SigningSetting(byte[] publicKey, SigningAction signAction, string createdAt, string usedBy, string publicKeyContainerPath)
    {
      if (publicKey == null)
        throw new ArgumentNullException("publicKey");

      m_PublicKey = publicKey;
      m_SigningAction = signAction;
      m_UsedBy = usedBy;
      m_CreatedAt = createdAt;
      m_PathToKeyContainer = publicKeyContainerPath;
    }
  }
}
