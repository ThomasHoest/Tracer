namespace EQATEC.Tracer.Tools
{
  public class ParameterHolder
  {
    WordHolder mData;
    public WordHolder Data
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mData; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mData = value; }
    }
    WordHolder mType;
    public WordHolder Type
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mType; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mType = value; }
    }
    WordHolder mName;
    public WordHolder Name
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mName; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mName = value; }
    }

    public ParameterHolder(WordHolder data, WordHolder type, WordHolder name)
    {
      mData = data;
      mType = type;
      mName = name;
    }
  }
}