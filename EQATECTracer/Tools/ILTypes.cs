#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using EQATEC.CecilToolBox;
using EQATEC.SigningUtilities;
using Mono.Cecil;

#endregion

namespace EQATEC.Tracer.Tools
{

  #region IlTypeWrappers

  [Serializable]
  public abstract class ILType : ICustomTreeviewItem<ILType>, INotifyPropertyChanged
  {
    public virtual bool? GetState()
    {
      throw new NotImplementedException("Not to be called on base class");
    }

    #region Properties
    
    public event EventHandler EnabledChanged;

    public void Update()
    {
      int nullCount = 0;
      int trueCount = 0;
      Collection<ILType> col = GetSubItems();
      
      foreach (ILType type in col)
      {
        if (type.Enabled == null)
          nullCount++;
        if (type.Enabled == true)
          trueCount++;
      }

      bool? enabled = false;

      if (trueCount == col.Count)
        enabled = true;
      else if (trueCount > 0 || nullCount > 0)
        enabled = null;

      SetEnabledState(enabled);
    }

    protected bool? mEnabled = false;

    [XmlIgnore]
    public bool? Enabled
    {
      get { return mEnabled; }
      set
      {
        if(mEnabled == value)
          return;
        
        SetEnabledState(value);
        if (EnabledChanged != null && mEnabled != null)
          EnabledChanged(this, new EventArgs());
      }
    }

    public void SetEnabledState(bool? state)
    {
      if (mEnabled == state)
        return;

      mEnabled = state;      
      NotifyPropertyChanged("Enabled");
      
      ILType p = Parent;
      if(p != null)
        p.Update();
    }

    [XmlIgnore]
    public ViewerAssemblyState AssemblyState { get; set; }

    private int mId = -1;

    [XmlAttribute]
    public int ID
    {
      get { return mId; }
      set { mId = value; }
    }

    private string mName;

    [XmlAttribute("nm")]
    public string Name
    {
      get { return mName; }
      set { mName = value; }
    }

    private string mClassAndMethodName;

    [XmlIgnore]
    public string ClassAndMethodName
    {
      get
      {
        if(mClassAndMethodName == null)
        {
          mClassAndMethodName = Parent.Name + "." + Name;
        }

        return mClassAndMethodName;
      }
    }

    private string mFullName;

    [XmlIgnore]
    public string FullName
    {
      get
      {
        if(mFullName == null)
        {
          List<string> names = new List<string>();
          names.Add(mName);
          ILType parent = mParent;
          while(parent != null)
          {
            names.Add(parent.mName);
            parent = parent.mParent;
          }
          StringBuilder s = new StringBuilder();
          for(int i = names.Count - 2; i >= 0; i--)
          {
            if(s.Length > 0)
            {
              s.Append(".");
            }
            s.Append(names[i]);
          }
          mFullName = s.ToString();
        }
        return mFullName;
      }
      set { mFullName = value; }
    }

    [XmlIgnore]
    public string AssemblyName { get; set; }

    protected bool mExclude;

    protected ILType mParent;

    [XmlIgnore]
    public ILType Parent
    {
      set { mParent = value; }
      get { return mParent; }
    }
   
    #endregion

    public override string ToString()
    {
      return Name;
    }

    #region ICustomTreeviewItem<ILType> Members

    public abstract Collection<ILType> GetSubItems();

    public string MyName()
    {
      throw new NotImplementedException();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(String info)
    {
      if(PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion
  }

  [Serializable]
  public class ApplicationContainer : ILType
  {
    private ModuleCollection mModules;

    #region Properties

    public ModuleCollection Modules
    {
      get
      {
        if(mModules == null)
        {
          mModules = new ModuleCollection();
        }
        return mModules;
      }
      set { mModules = value; }
    }

    #endregion

    public ApplicationContainer()
    {
      Parent = null;
    }
        
    public override string ToString()
    {
      return Name;
    }

    public override Collection<ILType> GetSubItems()
    {
      return mModules;
    }
  }

  [Serializable]
  [XmlType("ac")]
  public class AssemblyContainer : ILType
  {
    private AssemblyDefinition mAssemblyDef;
    private ModuleCollection mModules;

    [NonSerialized]
    private SigningSetting mSigningSettings;

    #region Properties

    public string AssemblyPath { get; set; }

    [XmlIgnore]
    public SigningSetting SigningSettings
    {
      get { return mSigningSettings; }
      set { mSigningSettings = value; }
    }

    public AssemblyDefinition AssemblyDef
    {
      get { return mAssemblyDef; }
    }

    [XmlArray("modCol")]
    public ModuleCollection Modules
    {
      get
      {
        if(mModules == null)
        {
          if(mAssemblyDef != null)
          {
            mModules = new ModuleCollection(mAssemblyDef.Modules, this);
          }
        }
        return mModules;
      }
      set { mModules = value; }
    }

    public bool HasLog4Net { get; set; }

    #endregion

    public AssemblyContainer()
    {
    }

    public AssemblyContainer(AssemblyDefinition assem)
    {
      Name = assem.Name.Name;
      mAssemblyDef = assem;
      Parent = null;
    }

    public void ReloadAssembly()
    {
      mModules = null;
      mAssemblyDef = CecilTools.OpenAssembly(AssemblyPath);
    }

    public override bool? GetState()
    {
      return false;
    }

    public override string ToString()
    {
      return AssemblyName;
    }

    public override Collection<ILType> GetSubItems()
    {
      return mModules;
    }
  }

  [Serializable]
  [XmlType("mcon")]
  public class ModuleContainer : ILType
  {
    private TypeCollection mTypes;
    private NamespaceCollection mNamespaces;
    private readonly Dictionary<string, NamespaceContainer> mNamespaceLookup;
    private readonly ModuleDefinition mModule;

    #region Properties

    public string ModuleName
    {
      get { return Name; }
    }

    [XmlIgnore]
    public NamespaceCollection Namespaces
    {
      get { return mNamespaces; }
      set { mNamespaces = value; }
    }

    /*
    public IOrderedEnumerable<NamespaceContainer> SortedNamespaces
    {
      get { return mNamespaces.SortedList(); }
    }*/

    [XmlArray("tcol")]
    public TypeCollection Types
    {
      get
      {
        if(mTypes == null)
        {
          if(mModule != null)
          {
            mTypes = new TypeCollection(mModule.Types, this);
          }
        }

        return mTypes;
      }
      set { mTypes = value; }
    }

    #endregion

    public ModuleContainer()
    {
      mNamespaceLookup = new Dictionary<string, NamespaceContainer>();
      mNamespaces = new NamespaceCollection();
    }

    public ModuleContainer(ModuleDefinition module)
    {
      Name = module.Name;
      mModule = module;
      mNamespaceLookup = new Dictionary<string, NamespaceContainer>();
      mNamespaces = new NamespaceCollection();
    }

    public NamespaceContainer AddToNamespace(TypeContainer type)
    {
      NamespaceContainer nameCon;

      if(!mNamespaceLookup.ContainsKey(type.Namespace))
      {
        nameCon = new NamespaceContainer(type.Namespace);
        mNamespaceLookup.Add(type.Namespace, nameCon);
        mNamespaces.Add(nameCon);
      }
      else
      {
        nameCon = mNamespaceLookup[type.Namespace];
      }

      nameCon.Types.Add(type);

      return nameCon;
    }

    public override Collection<ILType> GetSubItems()
    {
      return mNamespaces;
    }
  }

  [Serializable]
  [XmlType("nmsc")]
  public class NamespaceContainer : ILType
  {
    private TypeCollection mTypes;

    #region Properties

    public string ModuleName
    {
      get { return Name; }
    }

    [XmlArray("tcol")]
    public TypeCollection Types
    {
      get { return mTypes; }
      set { mTypes = value; }
    }

/*
    public IOrderedEnumerable<TypeContainer> SortedTypes
    {
      get{return mTypes.SortedList();}
    }
    */

    #endregion

    public NamespaceContainer()
    {
      mTypes = new TypeCollection();
    }

    public NamespaceContainer(string name)
    {
      Name = name;
      mTypes = new TypeCollection();
    }
        
    public override Collection<ILType> GetSubItems()
    {
      return mTypes;
    }
  }

  [Serializable]
  [XmlType("tc")]
  public class TypeContainer : ILType
  {
    private readonly TypeDefinition mTypeDef;

    public TypeDefinition TypeDef
    {
      get { return mTypeDef; }
    }

    private MemberCollection mMemberCol;

    #region Properties

    [XmlAttribute("nmsn")]
    public string Namespace { get; set; }

    public string TypeName
    {
      get { return Name; }
    }

    [XmlArray("mCol")]
    public MemberCollection Members
    {
      get
      {
        if(mMemberCol == null)
        {
          if(mTypeDef != null)
          {
            mMemberCol = new MemberCollection(mTypeDef, this);
          }
        }

        return mMemberCol;
      }
      set { mMemberCol = value; }
    }

/*
    public IOrderedEnumerable<MemberContainer> SortedMembers
    {
      get { return mMemberCol.SortedList(); }
    }
    */

    #endregion

    public TypeContainer()
    {
    }

    public TypeContainer(TypeDefinition type)
    {
      /*
      if (type.Name.IndexOf("TripleComparer") != -1)
        System.Diagnostics.Debugger.Break();*/

      if(type.IsNestedPrivate || type.IsNestedPublic || type.IsNestedFamily || type.IsNestedAssembly)
      {
        int index = type.FullName.LastIndexOf(".");
        if(index != -1)
        {
          index++;
          Name = type.FullName.Substring(index, type.FullName.Length - index);
          Namespace = type.FullName.Substring(0, index - 1);
          ;
        }
        else
        {
          Name = type.FullName;
          Namespace = "";
        }
      }
      else
      {
        Name = type.Name;
        Namespace = type.Namespace;
      }

      if(type.GenericParameters.Count > 0)
      {
        int index = Name.IndexOf("`");

        if(index != -1)
        {
          string temp = Name.Remove(index, 2);
          StringBuilder sb = new StringBuilder();
          sb.Append("<");
          for(int i = 0; i < type.GenericParameters.Count; i++)
          {
            GenericParameter gParam = type.GenericParameters[i];
            if(i > 0)
            {
              sb.Append(",");
            }
            sb.Append(gParam.Name);
          }
          sb.Append(">");
          temp = temp.Insert(index, sb.ToString());
          Name = temp;
        }
      }

      /*if (Name == null)
        System.Diagnostics.Debugger.Break();*/

      mTypeDef = type;
    }
    
    public override string ToString()
    {
      return Namespace + "." + Name;
    }

    public override Collection<ILType> GetSubItems()
    {
      return mMemberCol;
    }
  }

  [Serializable]
  [XmlType("mc")]
  public class MemberContainer : ILType
  {
    private readonly MethodDefinition mMember;

    #region Properties

    public MethodDefinition MemberDef
    {
      get { return mMember; }
    }

    public string MemberName
    {
      get { return Name; }
    }

    //private object mContextMenu;

    //public object ContextMenu
    //{
    //  get { return mContextMenu; }
    //  set { mContextMenu = value; }
    //}

    private readonly List<MemberReference> mCalls;

    [XmlIgnore]
    public List<MemberReference> Calls
    {
      get { return mCalls; }
    }

    private readonly List<MemberContainer> mCallees;

    [XmlIgnore]
    public List<MemberContainer> Callees
    {
      get { return mCallees; }
    }

    public override Collection<ILType> GetSubItems()
    {
      return new Collection<ILType>();
    }

    private string mFullNameWithParams;

    [XmlIgnore]
    public string FullNameWithParams
    {
      get
      {
        if(mFullNameWithParams == null)
        {
          /*
          if (FullName.IndexOf("AsBindable") != -1)
            System.Diagnostics.Debugger.Break();*/
          string temp = FullName.Substring(0, FullName.LastIndexOf(".") + 1);

          if(mReturnType != null)
          {
            mFullNameWithParams = string.Concat(temp, NameWithParams, " : ", mReturnType);
          }
          else
          {
            mFullNameWithParams = string.Concat(temp, NameWithParams);
          }
        }

        return mFullNameWithParams;
      }
    }

    private string mReturnType;

    [XmlAttribute("rt")]
    public string ReturnType
    {
      get { return mReturnType; }
      set { mReturnType = value; }
    }

    private string mNameWithParams;

    [XmlIgnore]
    public string NameWithParams
    {
      get
      {
        if(mNameWithParams == null)
        {
          StringBuilder s = new StringBuilder();
          s.Append(Name);
          s.Append("(");
          for(int i = 0; i < mTn.Count; i++)
          {
            if(i > 0)
            {
              s.Append(",");
            }

            s.Append(mTn[i]);
          }
          s.Append(")");
          mNameWithParams = s.ToString();
        }

        return mNameWithParams;
      }
    }

    #endregion

    public MemberContainer()
    {
      mCallees = new List<MemberContainer>();
    }

    public List<string> mTn;
    public List<string> mPn;
    public List<int> mCl;      

    public MemberContainer(MethodDefinition method)
    {      
      mMember = method;
      Name = method.Name;

      if(method.CallingConvention == MethodCallingConvention.Generic)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append("<");
        for(int i = 0; i < method.GenericParameters.Count; i++)
        {
          if(i > 0)
          {
            sb.Append(",");
          }
          GenericParameter genParam = method.GenericParameters[i];
          sb.Append(genParam.Name);
        }
        sb.Append(">");

        Name += sb.ToString();
      }

      mCalls = new List<MemberReference>();
      mCallees = new List<MemberContainer>();
      mReturnType = method.ReturnType.ReturnType.Name;

      mTn = new List<string>();
      mPn = new List<string>();
      mCl = new List<int>();

      foreach(ParameterDefinition param in method.Parameters)
      {
        if(param.ParameterType is GenericInstanceType)
        {
          GenericInstanceType genType = param.ParameterType as GenericInstanceType;
          int index = genType.Name.IndexOf("`");
          string temp;

          if(index != -1)
          {
            temp = genType.Name.Remove(index, genType.Name.Length - index);
          }
          else
          {
            temp = genType.Name;
          }

          index = genType.FullName.IndexOf("<");
          temp += genType.FullName.Substring(index, genType.FullName.Length - index);
          mTn.Add(temp);
          /*
          StringBuilder sb = new StringBuilder();
          sb.Append(temp);
          sb.Append("<");
          for (int i = 0; i < genType.GenericArguments.Count; i++)
          {
            if (i > 0)
              sb.Append(",");
            sb.Append(genType.GenericArguments[i].FullName);
          }
          sb.Append(">");          
          mTn.Add(sb.ToString());*/
        }
        else
        {
          mTn.Add(param.ParameterType.Name);
        }

        mPn.Add(param.Name);
      }
    }

    public int GetCallees(int level)
    {
      if(level == 0)
      {
        return 0;
      }
      /*
      if (level < 0)
        System.Diagnostics.Debugger.Break();
      */
      int temp = mCallees.Count;

      --level;

      foreach(MemberContainer cl in Callees)
      {
        temp += cl.GetCallees(level);
      }

      return temp;
    }

    public string Dump()
    {
      if(mMember is MethodDefinition)
      {
        return string.Concat(CecilTools.ShowParameters(mMember), Environment.NewLine, CecilTools.DumpIL(mMember));
      }
      else
      {
        return "";
      }
    }

    public override bool? GetState()
    {
      return mEnabled;
    }

    public override string ToString()
    {
      return FullNameWithParams;
    }
  }

  #endregion
}