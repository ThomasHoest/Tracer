using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace EQATEC.Tracer.Tools
{
  public abstract class Persistent<T>
  {
    public static string DefaultRegistryRoot = @"Software\EQATEC\Tracer";

    protected abstract T ConvertFromString(string input);
    protected abstract string ConvertToString(T input);

    protected string Root {get; private set;}
    protected string Name {get; private set;}
    private T m_value; 

    protected Persistent(string root, string name)
    {
      Root = root;
      Name = name;
      TryInitializeValue(ConvertFromString(string.Empty));
    }
    protected Persistent( string root, string name, T defaultValue )
    {
      Root = root;
      Name = name;
      TryInitializeValue(defaultValue);
    }
    private void TryInitializeValue(T defaultValue)
    {
      try
      {
        RegistryKey key = Registry.CurrentUser.CreateSubKey(Root);
        m_value = ConvertFromString(key.GetValue(Name, defaultValue) as string);
      }
      catch
      {
        Value = defaultValue;
      }
    }
    /// <summary>
    /// Get or set the persistent value
    /// </summary>
    public T Value
    {
      get { return m_value; }
      set
      {
        m_value = value;
        try
        {
          RegistryKey key = Registry.CurrentUser.CreateSubKey(Root);
          key.SetValue(Name, ConvertToString(m_value));
        }
        catch { }
      }
    }
  }

  public class PersistentString : Persistent<string>
  {
    public PersistentString( string name ):
      base (DefaultRegistryRoot, name)
    {
    }
    public PersistentString( string name, string defaultValue ) :
      base(DefaultRegistryRoot, name, defaultValue)
    {
    }
    protected override string ConvertFromString( string input )
    {
      return input;
    }

    protected override string ConvertToString( string input )
    {
      return input;
    }
  }
  public class PersistentInt : Persistent<int>
  {
    public PersistentInt( string name )
      : base(DefaultRegistryRoot, name)
    {
    }
    public PersistentInt( string name, int defaultValue )
      : base(DefaultRegistryRoot, name, defaultValue)
    {
    }

    protected override int ConvertFromString( string input )
    {
      int output = 0;
      if (int.TryParse(input, out output))
        return output;
      return 0;
    }

    protected override string ConvertToString( int input )
    {
      return input.ToString();
    }
  }

  public class PersistentBool : Persistent<bool>
  {
    public PersistentBool( string name, bool defaultValue )
      :base(DefaultRegistryRoot, name, defaultValue)
    {
    }

    protected override bool ConvertFromString( string input )
    {
      bool output;
      if (bool.TryParse(input, out output))
        return output;
      return false;
    }

    protected override string ConvertToString( bool input )
    {
      return input.ToString();
    }
  }
}
