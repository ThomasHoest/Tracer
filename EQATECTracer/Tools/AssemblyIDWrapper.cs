using System;
using System.Xml.Serialization;

namespace EQATEC.Tracer.Tools
{
  [Serializable]
  public class AssemblyIDWrapper
  {
    AssemblyCollection mAssemblies;

    public AssemblyCollection Assemblies
    {
      get { return mAssemblies; }
      set { mAssemblies = value; }
    }

    Guid mID;

    [XmlAttribute]
    public Guid ID
    {
      get { return mID; }
      set { mID = value; }
    }

    public AssemblyIDWrapper()
    {
    }

    public AssemblyIDWrapper(Guid id, AssemblyCollection assemCol)
    {
      mID = id;
      mAssemblies = assemCol;
    }
  }
}
