using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Interface for assembly signers
  /// </summary>
  public interface IAssemblySigner
  {
    /// <summary>
    /// Sign the assembly with the key container and return success
    /// </summary>
    /// <param name="pathToAssembly"></param>
    /// <param name="pathToKeyContainer"></param>
    /// <returns></returns>
    bool SignAssembly(string pathToAssembly, string pathToKeyContainer);

    /// <summary>
    /// Sign the assembly
    /// </summary>
    /// <param name="pathToAssembly"></param>
    /// <param name="keyContainer"></param>
    /// <returns></returns>
    bool SignAssembly(string pathToAssembly, byte[] keyContainer);
  }

  /// <summary>
  /// Delegate for calling back to receive the assembly signing settings
  /// </summary>
  /// <param name="assemblyName"></param>
  /// <param name="assemblyPublicKey"></param>
  /// <returns></returns>
  public delegate SigningSetting AssemblySigningActionCallback( string assemblyName, byte[] assemblyPublicKey );

}
