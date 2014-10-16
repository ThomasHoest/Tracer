using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.SigningUtilities
{
  /// <summary>
  /// Enum for the action taken for an assembly in regards to 
  /// signing
  /// </summary>
  public enum SigningAction
  {
    /// <summary>
    /// Skip processing of the assembly
    /// </summary>
    Skip,
    /// <summary>
    /// Sign the assembly, requires access to the public key container
    /// </summary>
    Sign,
    /// <summary>
    /// Strip the signature from the assembly
    /// </summary>
    Strip
  }
}
