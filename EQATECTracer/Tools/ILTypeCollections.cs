#region

using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Mono.Cecil;

#endregion

namespace EQATEC.Tracer.Tools
{

  #region Collections

  [Serializable]
  [XmlInclude(typeof(ApplicationContainer))]
  public class ApplicationCollection : Collection<ILType>
  {
  }

  [Serializable]
  [XmlInclude(typeof(AssemblyContainer))]
  public class AssemblyCollection : Collection<ILType>
  {
    public Guid ID { get; set; }

    public void Sort()
    {
      CollectionTools.SortCollection(this);
    }
  }

  [Serializable]
  [XmlInclude(typeof(ModuleContainer))]
  public class ModuleCollection : Collection<ILType>
  {
    public ModuleCollection()
    {
    }

    public ModuleCollection(ModuleDefinitionCollection moduleCol, ILType parent)
    {
      foreach(ModuleDefinition module in moduleCol)
      {
        ModuleContainer modCon = new ModuleContainer(module);
        modCon.Parent = parent;
        Add(modCon);
      }

      Sort();
    }

    public void Sort()
    {
      CollectionTools.SortCollection(this);
    }
  }

  [Serializable]
  [XmlInclude(typeof(NamespaceContainer))]
  public class NamespaceCollection : Collection<ILType>
  {
    public void Sort()
    {
      CollectionTools.SortCollection(this);
    }

    /* .Net 3.5 only
    public IOrderedEnumerable<NamespaceContainer> SortedList()
    {
      return this.OrderBy(mod => mod.Name);
    }
    */

    /*public NamespaceCollection(TypeDefinitionCollection typeCol, ILType parent)
    {
      foreach (TypeDefinition type in typeCol)
      {
        if (type.Name != "<Module>" && type.Methods.Count > 0)
        {
          TypeContainer typeCon = new TypeContainer(type);
          typeCon.Parent = parent;
          Add(typeCon);
        }
      }
    }*/
  }

  [Serializable]
  [XmlInclude(typeof(TypeContainer))]
  public class TypeCollection : Collection<ILType>
  {
    public TypeCollection()
    {
    }

    /*
    public IOrderedEnumerable<TypeContainer> SortedList()
    {
      return this.OrderBy(mod => mod.Name);      
    }
    */

    public TypeCollection(TypeDefinitionCollection typeCol, ILType parent)
    {
      foreach(TypeDefinition type in typeCol)
      {
        if(type.Name != "<Module>"
           && type.Methods.Count > 0
           && !type.IsInterface
           && type.BaseType.FullName != "System.MulticastDelegate") //Todo: Remove hardcoded string
        {
          TypeContainer typeCon = new TypeContainer(type);
          typeCon.Parent = parent;
          Add(typeCon);
        }
      }

      Sort();
    }

    public void Sort()
    {
      CollectionTools.SortCollection(this);
    }
  }

  [Serializable]
  [XmlInclude(typeof(MemberContainer))]
  public class MemberCollection : Collection<ILType>
  {
    public MemberCollection()
    {
    }

    /* .Net 3.5 only
    public IOrderedEnumerable<MemberContainer> SortedList()
    {
      return this.OrderBy(mod => mod.Name);      
    }
    */

    public MemberCollection(TypeDefinition typeDef, ILType parent)
    {
      MemberContainer memCon = null;

      foreach(MethodDefinition method in typeDef.Constructors)
      {
        if(!method.IsAbstract && method.Body != null)
        {
          memCon = new MemberContainer(method);
          memCon.Parent = parent;
          Add(memCon);
        }
      }

      foreach(MethodDefinition method in typeDef.Methods)
      {
        if(!method.IsAbstract && method.Body != null)
        {
          memCon = new MemberContainer(method);
          memCon.Parent = parent;
          Add(memCon);
        }
      }

      Sort();
    }

    public void Sort()
    {
      CollectionTools.SortCollection(this);
    }
  }

  #endregion
}