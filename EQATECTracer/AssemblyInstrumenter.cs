using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.TracerRuntime;
using EQATEC.CecilToolBox;
using EQATEC.SigningUtilities;
using EQATEC.SigningUtilities.Signing;
using log4net.Repository.Hierarchy;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;


namespace EQATEC.Tracer
{
  public class AssemblyInstrumenter
  {
    #region Members

    IAssemblySigner mAssemblySigner;
    AssemblySigningActionCallback mSigningActionCallback;

    AssemblyDefinition mRuntimeAssemDef;
    AssemblyNameReference mRuntimeAssemRef;
    TypeDefinition mRuntimeType;    
    MethodDefinition mRegisterFunction;
    MethodDefinition mRegisterFunctionName;
    BodyWorker mRuntimeWorker = null;
    
    MethodInfo mDoTraceMethod;
    MethodReference mDoTraceMethodImported;

    MethodInfo mTraceNoParamMethod;
    MethodReference mTraceNoParamMethodImported;

    MethodInfo mTraceExitMethod;
    MethodReference mTraceExitMethodImported;

    MethodInfo mTraceCaughtException;
    MethodReference mTraceCaughtExceptionImported;

    MethodInfo mTraceExitMethodWithData;
    MethodReference mTraceExitMethodWithDataImported;

    PropertyInfo mGetExceptionMessage;
    MethodReference mGetExceptionMessageImported;

    PropertyInfo mGetExceptionStack;
    MethodReference mGetExceptionStackImported;

    MethodReference mTraceInternal;

    ArrayType mObjectArrayTypeRef = null;

    //Dictionary<string, TypeDefinition> mSystemTypeDefs = new Dictionary<string, TypeDefinition>();
    Dictionary<string, TypeReference> mSystemTypeRefs = new Dictionary<string, TypeReference>();
    Dictionary<string, MethodDefinition> mMethodList = new Dictionary<string, MethodDefinition>();

    int mRuntimeMethodCounter = 0;
    //Function id counter
    int mMemberID;

    #endregion

    #region ValueType object names

    //Constant strings used to retrieve system types and for comparison
    const string OBJECT = "System.Object";
    const string VOID = "System.Void";
    const string INT = "System.Int32";
    const string UINT = "System.UInt32";
    const string FLOAT = "System.Single";
    const string DOUBLE = "System.Double";
    const string BYTE = "System.Byte";
    const string SHORT = "System.Int16";
    const string USHORT = "System.UInt16";
    const string CHAR = "System.Char";
    const string BOOL = "System.Boolean";
    const string INTPTR = "System.IntPtr";
    const string UINTPTR = "System.UIntPtr";
    const string DELEGATE = "System.Delegate";
    const string MULTDELEGATE = "System.MulticastDelegate";

    #endregion

    #region Properties

    public bool InstrumentProperties { get; set; }
    public bool TraceToFile { get; set; }
    
    List<string> mErrorList;
    public List<string> ErrorList
    {
      get { return mErrorList; }
      set { mErrorList = value; }
    }

    int mInstrumentedMethodCounter = 0;
    public int InstrumentedMethodCounter
    {
      get { return mInstrumentedMethodCounter; }
      set { mInstrumentedMethodCounter = value; }
    }

    #endregion

    #region Events

    public delegate void AssemblyInstrumentationStarted(string name);
    public event AssemblyInstrumentationStarted OnAssemblyInstrumentationStarted;
    
    public delegate void AssemblyInstrumentationEnded(string name, int methodCount, bool success);
    public event AssemblyInstrumentationEnded OnAssemblyInstrumentationEnded;

    #endregion

    #region Constructors

    public AssemblyInstrumenter()
      :this(null)
    {
    }
    public AssemblyInstrumenter( AssemblySigningActionCallback signingCallback )
      :this(new MonoAssemblySigner(), signingCallback)
    {
    }
    public AssemblyInstrumenter(
      IAssemblySigner assemblySigner,
      AssemblySigningActionCallback signingCallback )
    {
      GetRuntimeMethods();
      mErrorList = new List<string>();
      mSigningActionCallback = signingCallback;
      mAssemblySigner = assemblySigner;
    }

    #endregion

    #region Initialization

    //Retrieve all methodreferences and types necessary for isntrumentation
    private void GetRuntimeMethods()
    {
      Assembly runtimeAssembly = null;
      GetRuntimeAssemblyFromResource(ref runtimeAssembly, ref mRuntimeAssemDef);
      
      //remembering to add the publickey to the assembly reference
      mRuntimeAssemRef = new AssemblyNameReference(mRuntimeAssemDef.Name.Name, mRuntimeAssemDef.Name.Culture, mRuntimeAssemDef.Name.Version);
      mRuntimeAssemRef.PublicKeyToken = mRuntimeAssemDef.Name.PublicKeyToken;

      //Get tracerruntime type
      mRuntimeType = mRuntimeAssemDef.MainModule.Types["EQATEC.Tracer.TracerRuntime.TracerRuntime"];
      //Get ref to register function 
      mRegisterFunction = mRuntimeType.Methods.GetMethod("RegisterMethod", new Type[] { typeof(int) });
      //Get ref to log4net add adapter function 
      mRegisterFunctionName = mRuntimeType.Methods.GetMethod("RegisterMethodName", new Type[] { typeof(int), typeof(string) });
      // Get ref to internal trace used for building custom templates
      mTraceInternal = mRuntimeType.Methods.GetMethod("Trace", new Type[] { typeof(int), typeof(object[]) });
      //Clear register function and make a worker for it 
      MethodDefinition[] registerAllMethod = mRuntimeType.Methods.GetMethod("RegisterAllMethods");
      registerAllMethod[0].Body.Instructions.Clear();
      mRuntimeWorker = new BodyWorker(registerAllMethod[0].Body);

      Type tracerType = runtimeAssembly.GetType("EQATEC.Tracer.TracerRuntime.TracerRuntime");
      
      //Get trace methodinfos from traceruntime. Use methodinfos for injecting into instrumented assemblies
      mDoTraceMethod = tracerType.GetMethod("DoFunctionTrace",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
        System.Type.DefaultBinder, new Type[] { typeof(System.Int32) }, null);

      mTraceNoParamMethod = tracerType.GetMethod("Trace",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
        System.Type.DefaultBinder, new Type[] { typeof(System.Int32) }, null);

      mTraceExitMethod = tracerType.GetMethod("TraceExit",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
        System.Type.DefaultBinder, new Type[] { typeof(System.Int32) }, null);

      mTraceExitMethodWithData = tracerType.GetMethod("TraceExit",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
        System.Type.DefaultBinder, new Type[] { typeof(System.Object), typeof(System.Int32) }, null);

      mTraceCaughtException = tracerType.GetMethod("TraceException",
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
        System.Type.DefaultBinder, new Type[] { typeof(System.Int32), typeof(System.String), typeof(System.String) }, null);

      Type exceptionType = typeof(System.Exception);
      mGetExceptionMessage = exceptionType.GetProperty("Message");
      mGetExceptionStack = exceptionType.GetProperty("StackTrace");
          
      ImportSystemTypes();
    }

    //Import all simple system types
    private void ImportSystemTypes()
    {
      TypeReference typeRef = mRuntimeAssemDef.MainModule.TypeReferences[OBJECT];//mRuntimeAssemDef.MainModule.Import(typeDef as TypeReference);
      mObjectArrayTypeRef = new ArrayType(typeRef);
      mSystemTypeRefs.Add(OBJECT, typeRef);
      
      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[VOID];//mRuntimeAssemDef.MainModule.Import(typeDef as TypeReference);      
      mSystemTypeRefs.Add(VOID, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[INT];//mRuntimeAssemDef.MainModule.Import(typeDef as TypeReference);
      mSystemTypeRefs.Add(INT, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[UINT];
      mSystemTypeRefs.Add(UINT, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[FLOAT];
      mSystemTypeRefs.Add(FLOAT, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[DOUBLE];
      mSystemTypeRefs.Add(DOUBLE, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[BYTE];
      mSystemTypeRefs.Add(BYTE, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[SHORT];
      mSystemTypeRefs.Add(SHORT, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[USHORT];
      mSystemTypeRefs.Add(USHORT, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[CHAR];
      mSystemTypeRefs.Add(CHAR, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[BOOL];
      mSystemTypeRefs.Add(BOOL, typeRef);
    
      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[DELEGATE];
      mSystemTypeRefs.Add(DELEGATE, typeRef);

      typeRef = mRuntimeAssemDef.MainModule.TypeReferences[MULTDELEGATE];
      mSystemTypeRefs.Add(MULTDELEGATE, typeRef);
    }


    System.Reflection.Assembly mAssembly;
    //Get runtime from embedded resource
    private void GetRuntimeAssemblyFromResource(ref System.Reflection.Assembly rtAss, ref AssemblyDefinition rtAssDef)
    {
      mAssembly = System.Reflection.Assembly.GetExecutingAssembly();

      using (Stream stream = (Stream)mAssembly.GetManifestResourceStream("EQATEC.Tracer.Resources.EQATECTracerRuntime.dll"))
      {
        byte[] contents = new byte[stream.Length];
        stream.Read(contents, 0, contents.Length);
        rtAss = System.Reflection.Assembly.Load(contents);
        rtAssDef = AssemblyFactory.GetAssembly(contents);
      }
    }

    #endregion

    #region Public interface

    /// <summary>
    /// Finishes the instrumentation by saving the runtime dll
    /// </summary>
    /// <param name="path">Destination for the runtime</param>
    public void FinishInstrumentation(string path)
    {
      //Appen return statement to register function
      mRuntimeWorker.Append(mRuntimeWorker.Create(OpCodes.Ret));
      mRuntimeWorker.Done(); //Let the bodyworker do its body workout
      string assemPath = System.IO.Path.Combine(path, "EQATECTracerRuntime.dll");
      string failure = "";
      if (!CecilTools.SaveModifiedAssembly(mRuntimeAssemDef, assemPath, ref failure))
      {
        mErrorList.Add("Unable to save TracerRuntime.dll to output directory.");
        if(failure != "")
          mErrorList.Add("Reason: " + failure);
        return;
      }

      byte[] keyBytes = null;
      using (Stream stream = (Stream)mAssembly.GetManifestResourceStream("EQATEC.Tracer.Resources.runtime.snk"))
      {
        keyBytes = new byte[stream.Length];
        stream.Read(keyBytes, 0, keyBytes.Length);
      }

      if(!mAssemblySigner.SignAssembly(assemPath, keyBytes))
        mErrorList.Add("Unable to sign TracerRuntime.dll. File is properly in use");
    }

    /// <summary>
    /// Instrument all assemblies in the passed collection. On return the collection will hold all modified assemblies
    /// </summary>
    /// <param name="assemCollection">Collection of assemblies</param>
    public void InstrumentCollection(AssemblyCollection assemCollection)
    {
      mMemberID = 0;
      Dictionary<MemberReference, int> methodNameDictionary = new Dictionary<MemberReference, int>();
      List<MemberContainer> allMembers = new List<MemberContainer>();
      mInstrumentedMethodCounter = 0;
      assemCollection.Sort();
      bool hasLog4Net = false;
      //Run through all assemblies 
      foreach (AssemblyContainer assemCon in assemCollection)
      { 

       
        hasLog4Net |= assemCon.HasLog4Net;
        int assemblyMethodCounter = 0;
        if (assemCon.Enabled == true)
        {
          //Reload from scratch to enable changes to be added to instrumentation and to prevent duplicate code insertions
          assemCon.ReloadAssembly();
          //checking for assembly signing
          bool continueProcessing = CheckAssemblySigning(assemCon);
          if (!continueProcessing)
            continue;
          System.Diagnostics.Trace.WriteLine("Starting instrumentation of: " + assemCon.AssemblyName);
          OnAssemblyInstrumentationStarted(assemCon.FullName);
          assemCon.AssemblyDef.MainModule.AssemblyReferences.Add(mRuntimeAssemRef);
          //Import the necessary methods
          mDoTraceMethodImported = assemCon.AssemblyDef.MainModule.Import(mDoTraceMethod);
          mTraceNoParamMethodImported = assemCon.AssemblyDef.MainModule.Import(mTraceNoParamMethod);
          mTraceExitMethodImported = assemCon.AssemblyDef.MainModule.Import(mTraceExitMethod);
          mTraceExitMethodWithDataImported = assemCon.AssemblyDef.MainModule.Import(mTraceExitMethodWithData);
          mTraceCaughtExceptionImported = assemCon.AssemblyDef.MainModule.Import(mTraceCaughtException);
          mGetExceptionMessageImported = assemCon.AssemblyDef.MainModule.Import(mGetExceptionMessage.GetGetMethod());
          mGetExceptionStackImported = assemCon.AssemblyDef.MainModule.Import(mGetExceptionStack.GetGetMethod());
          //Run through all modules
          foreach (ModuleContainer moduleCon in assemCon.Modules)
          {            
            moduleCon.Parent = assemCon;
            //and the types
            foreach (TypeContainer typeCon in moduleCon.Types)
            {              
              typeCon.Parent = moduleCon;
               /*
              if (typeCon.Name.IndexOf("UnaryPixelOps") != -1)
                System.Diagnostics.Debugger.Break();  */
              List<MemberContainer> getSetMembersToRemove = new List<MemberContainer>();
              //Do the members
              foreach (MemberContainer memberCon in typeCon.Members)
              {
                if (!InstrumentProperties && (memberCon.MemberDef.IsGetter || memberCon.MemberDef.IsSetter))
                {
                  getSetMembersToRemove.Add(memberCon);
                  continue;
                }

                memberCon.Parent = typeCon;
                memberCon.ID = mMemberID;

                /*if (memberCon.FullName.IndexOf("...") != -1)
                  System.Diagnostics.Debugger.Break();*/
                
                if (InstrumentMethod(memberCon, assemCon))
                {
                  assemblyMethodCounter++;
                  UpdateRuntimeRegister(memberCon);
                  //string signature = string.Concat(typeCon.FullName, ".", memberCon.GetFullNameWithParametes());
                  methodNameDictionary.Add(memberCon.MemberDef.GetOriginalMethod(), memberCon.ID);
                  allMembers.Add(memberCon);
                  mMemberID++;
                }
                else
                  memberCon.ID = -1;
              }

              foreach (MemberContainer member in getSetMembersToRemove)
              {
                typeCon.Members.Remove(member);
              }
            }
          }
          mInstrumentedMethodCounter += assemblyMethodCounter;
          OnAssemblyInstrumentationEnded(assemCon.AssemblyName, assemblyMethodCounter, true);
        }
      }

      //Convert callee names to nifty numbers
      foreach (MemberContainer mcon in allMembers)
      {
        foreach (MemberReference mf in mcon.Calls)
        {
          if (methodNameDictionary.ContainsKey(mf))
            mcon.mCl.Add(methodNameDictionary[mf]);
        }
      }

      InsertAssemblyStructureInRuntime(assemCollection);
      SetLog4NetStatus(hasLog4Net);
      SetTraceToFileStatus(TraceToFile);
    }

    /// <summary>
    /// Signs assembly with key pointed to by path
    /// </summary>
    /// <param name="assemblyContainer">Assembly to sign</param>
    /// <param name="assemblyFilePath">Path to key</param>
    /// <returns></returns>
    public bool SignAssemblyContainer(AssemblyContainer assemblyContainer, string assemblyFilePath)
    {
      if (assemblyContainer.SigningSettings != null)
      {
        Debug.Assert(assemblyContainer.SigningSettings.SigningAction == SigningAction.Sign);
        return mAssemblySigner.SignAssembly(assemblyFilePath, assemblyContainer.SigningSettings.PathToKeyContainer);
      }
      return false;
    }

    #endregion

    #region Serialization and signing

    private bool CheckAssemblySigning( AssemblyContainer assemCon)
    {
      bool continueProcessing = true;
      if (assemCon.AssemblyDef.Name.HasPublicKey)
      {
        continueProcessing = assemCon.SigningSettings != null && assemCon.SigningSettings.SigningAction == SigningAction.Sign;
      }
      return continueProcessing;
    }

    private string SerializeCollectionInformation(AssemblyCollection assemCollection)
    {
      string temp = "";
      ASCIIEncoding enc = new ASCIIEncoding();
      //Tree step slam bam collection serialization and compression

      //Test uuencoding all strings
      //foreach (AssemblyContainer assemCon in assemCollection)
      //{
      //  assemCon.Name = UUEncoder.UUEncodeString(assemCon.Name);

      //  foreach(ModuleContainer modCon in assemCon.Modules)
      //  {
      //    modCon.Name = UUEncoder.UUEncodeString(modCon.Name);

      //    foreach(TypeContainer typeCon in modCon.Types)
      //    {
      //      typeCon.Name = UUEncoder.UUEncodeString(typeCon.Name);
      //      typeCon.Namespace = UUEncoder.UUEncodeString(typeCon.Namespace);

      //      foreach(MemberContainer memCon in typeCon.Members)
      //      {
      //        memCon.Name = UUEncoder.UUEncodeString(memCon.Name);
      //        memCon.ReturnType = UUEncoder.UUEncodeString(memCon.ReturnType);
      //        for (int i = 0; i < memCon.mTn.Count; i++)
      //          memCon.mTn[i] = UUEncoder.UUEncodeString(memCon.mTn[i]);

      //      }
      //    }
      //  }
      //}
      //Test end

      using (MemoryStream memStream = new MemoryStream())
      {
        AssemblyIDWrapper wrapper = new AssemblyIDWrapper(assemCollection.ID, assemCollection);        
        XmlSerializer xmlSer = new XmlSerializer(typeof(AssemblyIDWrapper)); // Xml serialization leaves us with a large amount of data
        xmlSer.Serialize(memStream, wrapper);
        memStream.Flush();
        //Convert data to byte array
        memStream.Position = 0;
        byte[] data = new byte[memStream.Length];
        memStream.Read(data, 0, (int)memStream.Length);

        
        /*temp = enc.GetString(data);
        System.Diagnostics.Trace.WriteLine(temp);*/
        
        System.Diagnostics.Trace.WriteLine("raw length: " + data.Length);
        using (MemoryStream deflMemStream = new MemoryStream())
        {
          //Use delflate stream to compress data to a much smaller binary format
          DeflateStream ds = new DeflateStream(deflMemStream, CompressionMode.Compress, true);
          ds.Write(data, 0, data.Length);
          ds.Close();
          deflMemStream.Flush();
          deflMemStream.Position = 0;
          System.Diagnostics.Trace.WriteLine("compressed length: " + deflMemStream.Length);
          using(MemoryStream uuStream = new MemoryStream())
          {
            //Since we're using a text based xml connection to the client all non text bytes needs to be removed
            //This is done with UUEncoding
            UUEncoder.UUEncode(deflMemStream, uuStream);
            uuStream.Position = 0;
            //Again convert to byte array and then convert to a string used to insert into runtime
            byte[] deflData = new byte[uuStream.Length];
            uuStream.Read(deflData, 0, (int)uuStream.Length);
            System.Diagnostics.Trace.WriteLine("UU length: " + uuStream.Length);
            temp = NoBullshitEncoder.GetString(deflData);
            System.Diagnostics.Trace.WriteLine("string length: " + temp.Length);
          }          
        }        
        
      }
      return temp;
    }

    #endregion

    #region SerializeAndCompress Test

    private void TestSerializeCollectionInformation(AssemblyCollection assemCollection)
    {
      string temp = "";
      byte[] deflData;
      UTF8Encoding enc = new UTF8Encoding();
      XmlSerializer xmlSer = new XmlSerializer(typeof(AssemblyCollection));
      //Tree step slam bam collection serialization and compression

      using (MemoryStream memStream = new MemoryStream())
      {
         // Xml serialization leaves us with a large amount of data
        xmlSer.Serialize(memStream, assemCollection);
        memStream.Flush();
        //Convert data to byte array
        memStream.Position = 0;
        byte[] data = new byte[memStream.Length];
        memStream.Read(data, 0, (int)memStream.Length);

        /*
        temp = enc.GetString(data);
        System.Diagnostics.Trace.WriteLine(temp);
        */
        System.Diagnostics.Trace.WriteLine("raw length: " + data.Length);
        using (MemoryStream deflMemStream = new MemoryStream())
        {
          //Use delflate stream to compress data to a much smaller binary format
          DeflateStream ds = new DeflateStream(deflMemStream, CompressionMode.Compress, true);
          ds.Write(data, 0, data.Length);
          ds.Close();
          deflMemStream.Flush();
          deflMemStream.Position = 0;
          deflData = new byte[deflMemStream.Length];
          deflMemStream.Read(deflData, 0, (int)deflMemStream.Length);          
          //temp = enc.GetString(deflData);
          temp = NoBullshitEncoder.GetString(deflData);
          System.Diagnostics.Trace.WriteLine("string length: " + temp.Length);          
        }

      }

      byte[] buffer = NoBullshitEncoder.GetBytes(temp);
      AssemblyCollection assemCol;      
      //Use memory stream to hold ingoing data
      using (MemoryStream inflMemStream = new MemoryStream(buffer))
      {
        inflMemStream.Position = 0;

        //The deflate the stuff
        System.Diagnostics.Trace.WriteLine("compressed length: " + inflMemStream.Length);
        DeflateStream ds = new DeflateStream(inflMemStream, CompressionMode.Decompress, true);
        inflMemStream.Position = 0;

        List<byte> bytes = new List<byte>();
        while (true)
        {
          int d = ds.ReadByte();

          if (d != -1)
            bytes.Add((byte)d);
          else
            break;
        }

        ds.Close();

        byte[] desBuffer = new byte[bytes.Count];
        desBuffer = bytes.ToArray();

        MemoryStream desStream = new MemoryStream(desBuffer);
        desStream.Position = 0;

        assemCol = xmlSer.Deserialize(desStream) as AssemblyCollection;
      }
    }

    #endregion

    #region Method Instrumentation

    //Inserts call to register function in runtime updating the lookup table with member id
    private void UpdateRuntimeRegister(MemberContainer memberCon)
    {
      Instruction pushID = mRuntimeWorker.Create(OpCodes.Ldc_I4, memberCon.ID);
      Instruction callRegister = mRuntimeWorker.Create(OpCodes.Call, mRegisterFunction);

      mRuntimeWorker.Append(pushID);
      mRuntimeWorker.Append(callRegister);

      if (TraceToFile)
      {
        //pushID = mRuntimeWorker.Create(OpCodes.Ldc_I4, memberCon.ID);
        Instruction pushName = mRuntimeWorker.Create(OpCodes.Ldstr, memberCon.FullName);
        callRegister = mRuntimeWorker.Create(OpCodes.Call, mRegisterFunctionName);

        mRuntimeWorker.Append(pushID);
        mRuntimeWorker.Append(pushName);
        mRuntimeWorker.Append(callRegister);
      }
    }

    private void SetLog4NetStatus(bool status)
    {
      MethodDefinition[] method = mRuntimeType.Methods.GetMethod("DoAddLog4NetAppender");
      EQATEC.CecilToolBox.CecilTools.InsertBodyReturningBool(method[0].Body, status);
    }

    private void SetTraceToFileStatus(bool status)
    {
      MethodDefinition[] method = mRuntimeType.Methods.GetMethod("TraceToFile");
      EQATEC.CecilToolBox.CecilTools.InsertBodyReturningBool(method[0].Body, status);
    }
    
    //Inserts constant string in runtime describing the assembly structure 
    private void InsertAssemblyStructureInRuntime(AssemblyCollection assemCollection)
    {

      MethodDefinition[] assemStructure = mRuntimeType.Methods.GetMethod("GetAssemblyStructure");
      //TestSerializeCollectionInformation(assemCollection);

      AssemblyCollection enabledCollection = new AssemblyCollection();
      //Dump the not enabled before serialization
      foreach (AssemblyContainer assemCon in assemCollection)
        if (assemCon.Enabled == true)
          enabledCollection.Add(assemCon);

      enabledCollection.ID = assemCollection.ID;

      string assemInfo = SerializeCollectionInformation(enabledCollection);

      EQATEC.CecilToolBox.CecilTools.InsertBodyReturningString(assemStructure[0].Body, assemInfo);
      
      /*
      //Why is this short branch necessary???      
      Instruction branch = worker.Create(OpCodes.Br_S, worker.CurrentInstructions[2]);
      worker.AppendAfter(push, branch);
      */
    }

    private bool InstrumentMethod(MemberContainer memberCon,AssemblyContainer assemCon)
    {
      if (memberCon.MemberDef != null && memberCon.MemberDef.Body != null)
      {
        BodyWorker worker = new BodyWorker(memberCon.MemberDef.Body);
        
        //STARTTRACE
        //
        //Inserts the following
        //
        //if(ConditionMethod(int id))
        //  ResultMethod(int id, ...)
        //
        //IL code
        //
        // ldc.i4 0x87
        // call bool [TracerRuntime]Runtime.TracerRuntime::DoFunctionTrace(int32)
        // ldc.i4.0 
        // beq L_001c
        // ldc.i4 0x87  //Load instructions will depend on ResultMethod signature
        // ldarg.s path
        // call void [TracerRuntime]Runtime.TracerRuntime::T1(int32, object)
        //

        Instruction loadInt = worker.Create(OpCodes.Ldc_I4, memberCon.ID);
        worker.CurrentInstruction = worker.InsertBefore(worker.FirstInstruction, loadInt);

        Instruction callDoTrace = worker.Create(OpCodes.Call, mDoTraceMethodImported);
        worker.ContinuesInsert(callDoTrace);

        Instruction loadInt0 = worker.Create(OpCodes.Ldc_I4_0);
        worker.ContinuesInsert(loadInt0);

        Instruction branchifequal = worker.Create(OpCodes.Beq, loadInt0);
        worker.ContinuesInsert(branchifequal);

        Instruction loadID = worker.Create(OpCodes.Ldc_I4, memberCon.ID);
        worker.ContinuesInsert(loadID);
        /*
        if (memberCon.Name.IndexOf("Parameters2") != -1)
          System.Diagnostics.Debugger.Break();*/

        //Find the correct trace method
        MethodDefinition traceMethod = GetTraceStartMethod(worker, memberCon.MemberDef, memberCon.ID);
        Instruction callTrace = null;
        
        //Did we find a matching trace method or is the method without parameters        
        if(traceMethod != null)
        {          
          MethodReference methRef = null;
          methRef = assemCon.AssemblyDef.MainModule.Import(traceMethod);          
          callTrace = worker.Create(OpCodes.Call, methRef);
        }
        else
          callTrace = worker.Create(OpCodes.Call, mTraceNoParamMethodImported);

        worker.ContinuesInsert(callTrace);
        
        //Insert branch instruction target
        Instruction jmpTarget = worker.FirstInstruction;
        branchifequal.Operand = jmpTarget;

        //Trace all caught exceptions
        foreach (ExceptionHandler exHandler in memberCon.MemberDef.Body.ExceptionHandlers)
        {
          TraceCaughtException(exHandler, worker, memberCon.ID);
        }
       
        //Check all original instructions
        foreach (Instruction inst in worker.OriginalInstructions)
        {
          //Save all callees
          /*if (inst.OpCode == OpCodes.Calli)
            System.Diagnostics.Debugger.Break();*/

          if (inst.OpCode == OpCodes.Call || inst.OpCode == OpCodes.Callvirt) //TODO: what is Calli??
          {
            MemberReference callee = inst.Operand as MemberReference;            
            memberCon.Calls.Add(callee);
          }
          //Trace all returns
          if (inst.OpCode == OpCodes.Ret)
            InsertReturnTrace(worker, inst, memberCon.ID, memberCon.MemberDef.ReturnType.ReturnType);
        }

        worker.Done();
        return true;
      }
      return false;
    }

    private void TraceCaughtException(ExceptionHandler exHandler, BodyWorker worker, int id)
    {
      if (exHandler.Type == ExceptionHandlerType.Catch)
      {
        if (exHandler.HandlerStart.OpCode == OpCodes.Pop)
        {
          Instruction loadInt = worker.Create(OpCodes.Ldc_I4, id);
          worker.CurrentInstruction = worker.InsertAt(exHandler.HandlerStart, loadInt);

          Instruction loadString1 = worker.Create(OpCodes.Ldstr, "");
          worker.ContinuesInsert(loadString1);

          Instruction loadString2 = worker.Create(OpCodes.Ldstr, "");
          worker.ContinuesInsert(loadString2);
        }
        else if (exHandler.HandlerStart.OpCode == OpCodes.Stloc_0)
        {
          worker.CurrentInstruction = exHandler.HandlerStart;

          Instruction loadInt = worker.Create(OpCodes.Ldc_I4, id);
          worker.ContinuesInsert(loadInt);

          Instruction loadLocal1 = worker.Create(OpCodes.Ldloc_0);
          worker.ContinuesInsert(loadLocal1);

          Instruction callGetMessage = worker.Create(OpCodes.Callvirt, mGetExceptionMessageImported);
          worker.ContinuesInsert(callGetMessage);

          Instruction loadLocal2 = worker.Create(OpCodes.Ldloc_0);
          worker.ContinuesInsert(loadLocal2);

          Instruction callGetStack = worker.Create(OpCodes.Callvirt, mGetExceptionStackImported);
          worker.ContinuesInsert(callGetStack);

        }
        else
          return;
      }
      else
        return;

      Instruction callDoTrace = worker.Create(OpCodes.Call, mTraceCaughtExceptionImported);
      worker.ContinuesInsert(callDoTrace);
    }

    private void InsertReturnTrace(BodyWorker worker, Instruction retInst, int id, TypeReference returnType)
    {
      //ENDTRACE
      //
      //Inserts the following
      //
      //if(ConditionMethod(int id))
      //  ResultMethod(int id, ...)
      //
      //IL code
      //
      // ldc.i4 0x87
      // call bool [TracerRuntime]Runtime.TracerRuntime::DoFunctionTrace(int32)
      // ldc.i4.0 
      // beq L_001c
      // ldc.i4 0x87  //Load instructions will depend on ResultMethod signature
      // ldarg.s path
      // call void [TracerRuntime]Runtime.TracerRuntime::T1(int32, object)

      Instruction loadInt = worker.Create(OpCodes.Ldc_I4, id);
      //worker.CurrentInstruction = worker.InsertBefore(retInst, loadInt);
      worker.CurrentInstruction = worker.InsertAt(retInst, loadInt);

      Instruction callDoTrace = worker.Create(OpCodes.Call, mDoTraceMethodImported);
      worker.ContinuesInsert(callDoTrace);

      Instruction loadInt0 = worker.Create(OpCodes.Ldc_I4_0);
      worker.ContinuesInsert(loadInt0);

      Instruction branchifequal = worker.Create(OpCodes.Beq, loadInt0);      
      worker.ContinuesInsert(branchifequal);

      MethodReference traceMeth = null;

      if (returnType.FullName != VOID)
      {
        /*if (returnType.Name.IndexOf("Resource") != -1)
          System.Diagnostics.Debugger.Break();*/

        Instruction dup = worker.Create(OpCodes.Dup);
        worker.ContinuesInsert(dup);

        ParseReturnType(worker, ref returnType);
        
        traceMeth = mTraceExitMethodWithDataImported;
      }
      else
        traceMeth = mTraceExitMethodImported;

      Instruction loadID = worker.Create(OpCodes.Ldc_I4, id);
      worker.ContinuesInsert(loadID);

      Instruction callTrace = worker.Create(OpCodes.Call, traceMeth);
      worker.ContinuesInsert(callTrace);
      //Insert branch instruction target
      Instruction jmpTarget = retInst;
      branchifequal.Operand = jmpTarget;
    }

    #endregion

    #region Parameter Handling

    //Creates signature based on the parameters of the given parameter collection
    private string CreateParameterSignature(ParameterDefinitionCollection parameters)
    {
      string signature = "";
      
      foreach (ParameterDefinition param in parameters)
      {
        TypeReference typeRef = param.ParameterType;

        //pointer and reference types is stripped to its elements type
        if (typeRef is ReferenceType)
        {
          ReferenceType refType = typeRef as ReferenceType;
          typeRef = refType.ElementType;
        }
        else if (typeRef is PointerType)
        {
          PointerType pType = typeRef as PointerType;
          if (mSystemTypeRefs[VOID].Name == pType.ElementType.Name)
            typeRef = mSystemTypeRefs[UINT];
        }

        /*if (typeRef == null)
          System.Diagnostics.Debugger.Break();*/

        //Only use system types otherwize use object
        if (mSystemTypeRefs.ContainsKey(typeRef.FullName))
        {
          signature += typeRef.Name + ",";
        }
        else
        {
          signature += "object,";
        }
      }

      return signature;
    }

    //Creates new signature method in runtime
    private MethodDefinition CreateNewSignatureMethod()
    {
      MethodDefinition meth = null;
      //Create a new trace method to use for unknown method signature      
      meth = new MethodDefinition("T" + mRuntimeMethodCounter,
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig,
            mSystemTypeRefs[VOID]);
      //Yes yes init them locals
      meth.Body.InitLocals = true;
      //First param is always the ID
      ParameterDefinition intParam = new ParameterDefinition(mSystemTypeRefs[INT]);
      meth.Parameters.Add(intParam);
      //It uses object array to store params before it calls trace in the runtime
      VariableDefinition newVar = new VariableDefinition(mObjectArrayTypeRef);
      newVar.Name = "localObjArray";
      meth.Body.Variables.Add(newVar);

      return meth;
    }

    //Run through method params and create new signature method if necessary
    private void ParseMethodParameters(BodyWorker worker, MethodDefinition meth, ParameterDefinitionCollection parameters, GenericParameterCollection genericParameters)
    {
      //The methods is fresh if it only has 1 parameter yet and that is the ID
      bool creatingMethod = meth.Parameters.Count == 1;

      for (int i = 0; i < parameters.Count; i++)
      {
        ParameterDefinition param = parameters[i];

        //Load method parameter on the stack
        Instruction pushParam = worker.Create(OpCodes.Ldarg_S, param);
        worker.ContinuesInsert(pushParam);

        bool useObject = ParseParameter(worker, ref param);
        //If we're creating a new runtime method as well we need to update this with the correct type as well
        if (creatingMethod)
        {
          if (!useObject)
          {
            meth.Parameters.Add(param);
          }
          else
          {
            ParameterDefinition objectParam;
            objectParam = new ParameterDefinition(mSystemTypeRefs[OBJECT]);
            objectParam.Name = "P" + i.ToString();
            meth.Parameters.Add(objectParam);
          }
        }
      }
    }

    [Flags]
    public enum ParameterType : int
    {
      None = 0x0, 
      ValueType = 0x1,	
      Generic = 0x2,	
      Pointer = 0x4,	
      Reference = 0x8,
      VoidPointer = 0x10     
    }

    private ParameterType DetectParameterType(ref ParameterDefinition param, ref TypeReference paramType)
    {
      ParameterType type = ParameterType.None;

      if (paramType is PointerType)
      {
        //If its a pointer type replace with element type and set flag
        PointerType pType = param.ParameterType as PointerType;
        paramType = pType.ElementType;
        param = new ParameterDefinition(paramType);

        if (mSystemTypeRefs[VOID].Name == paramType.Name)
        {
          //Void pointers are realy just adresses, so we treat them as uint
          paramType = mSystemTypeRefs[UINT];
          param = new ParameterDefinition(paramType);
          type |= ParameterType.VoidPointer;
        }
        //System.Diagnostics.Trace.WriteLine("pointer type");
        type |= ParameterType.Pointer;
      }
      else if (paramType is ReferenceType)
      {

        //Same as with pointer types we replace the type with the element type
        ReferenceType reftype = param.ParameterType as ReferenceType;
        paramType = reftype.ElementType;

        param = new ParameterDefinition(paramType);
        type |= ParameterType.Reference;
      }

      //Always check this after we have stripped down to element type of ref and pointer
      if (paramType is GenericParameter)
      {
        type |= ParameterType.Generic;
        /*if (paramType.Name.IndexOf("&") != -1)
          System.Diagnostics.Debugger.Break();*/
      }

      return type;
    }

    private void ParseReturnType(BodyWorker worker, ref TypeReference typeRef)
    {      
      ParameterDefinition param = new ParameterDefinition(typeRef);
      ParameterType detectedType = DetectParameterType(ref param, ref typeRef);

      /*if (param.ParameterType.Name.IndexOf("FDE_SHAREVIOLATION") != -1)
        System.Diagnostics.Debugger.Break();*/
      if (typeRef.IsValueType)
      {
        //If its either of these we want the value not the adress, so insert the load object instruction. It
        //loads the object pointed to on the eval stack
        if (((detectedType & ParameterType.Reference) == ParameterType.Reference) ||
          ((detectedType & ParameterType.Pointer) == ParameterType.Pointer))
        {
          //Instruction ld_ind = worker.Create(OpCodes.Ldobj, paramType);
          Instruction ld_ind = GetIndirectLoadInst(worker, typeRef);
          if (ld_ind == null)
            ld_ind = worker.Create(OpCodes.Ldobj, typeRef);
          worker.ContinuesInsert(ld_ind);
        }
      }
      else
      {

        //If we're dealing with a reference load the object pointed to with load ref instruction
        if ((detectedType & ParameterType.Reference) == ParameterType.Reference)
        {
          if ((detectedType & ParameterType.Generic) == ParameterType.Generic)
          {
            Instruction ld_ind = worker.Create(OpCodes.Ldobj, typeRef);
            worker.ContinuesInsert(ld_ind);

          }
          else
          {
            Instruction ld_ind = worker.Create(OpCodes.Ldind_Ref);
            worker.ContinuesInsert(ld_ind);
          }
        }
      }

      Instruction box = worker.Create(OpCodes.Box, typeRef);
      worker.ContinuesInsert(box);        
      
    }

    private bool ParseParameter(BodyWorker worker, ref ParameterDefinition param)
    {
      bool useObject = true;
           
      string typeName = param.ParameterType.FullName;
      TypeReference paramType = param.ParameterType;

      ParameterType detectedType = DetectParameterType(ref param, ref paramType);
            
      /*if (param.ParameterType.Name.IndexOf("FDE_SHAREVIOLATION") != -1)
        System.Diagnostics.Debugger.Break();*/

      //If its a value type it needs to be boxed
      if (paramType.IsValueType)
      {
        //If its either of these we want the value not the adress, so insert the load object instruction. It
        //loads the object pointed to on the eval stack
        if ( ( ( detectedType & ParameterType.Reference ) == ParameterType.Reference ) ||
          ((detectedType & ParameterType.Pointer) == ParameterType.Pointer))
        {
          //Instruction ld_ind = worker.Create(OpCodes.Ldobj, paramType);
          Instruction ld_ind = GetIndirectLoadInst(worker, paramType);
          if (ld_ind == null)
            ld_ind = worker.Create(OpCodes.Ldobj, paramType);
          worker.ContinuesInsert(ld_ind);
        }
        //Check for system type
        if (!mSystemTypeRefs.ContainsKey(paramType.FullName))
        {
          //Otherwize box            
          Instruction box = worker.Create(OpCodes.Box, paramType);
          worker.ContinuesInsert(box);          
        }
        else
        {
          //Our special friend the void pointer. The attentive reader would have noticed that we treat this as
          //a uint. Here is were we convert the pointer to a uint32
          if ((detectedType & ParameterType.VoidPointer) == ParameterType.VoidPointer)
          {
            Instruction conv = worker.Create(OpCodes.Conv_U4);
            worker.ContinuesInsert(conv);
          }

          useObject = false;
        }
      }
      else
      {
        //Todo: pointer to reference types??

        //If we're dealing with a reference load the object pointed to with load ref instruction
        if ((detectedType & ParameterType.Reference) == ParameterType.Reference)
        {
          if ((detectedType & ParameterType.Generic) == ParameterType.Generic)
          {
            Instruction ld_ind = worker.Create(OpCodes.Ldobj, paramType);
            worker.ContinuesInsert(ld_ind);

          }
          else
          {
            Instruction ld_ind = worker.Create(OpCodes.Ldind_Ref);
            worker.ContinuesInsert(ld_ind);
          }
        }
        //Box any generics
        if ((detectedType & ParameterType.Generic) == ParameterType.Generic)
        {
          Instruction box = worker.Create(OpCodes.Box, paramType);
          worker.ContinuesInsert(box);          
        }
      }

      return useObject;
    }
    
    //Gets runtime trace method or creates a new one
    private MethodDefinition GetTraceStartMethod(BodyWorker worker, MethodDefinition methDef, int id)
    {
      string signature = CreateParameterSignature(methDef.Parameters);
      
      if (signature != "")
      {
        MethodDefinition traceMeth = null;
        BodyWorker newSigWorker = null;
        bool creatingMethod = false;

        //Check if we have a signature match
        if (mMethodList.ContainsKey(signature))
        {
          traceMeth = mMethodList[signature] as MethodDefinition;
        }
        else
        {
          //Else create a new one 
          creatingMethod = true;
          traceMeth = CreateNewSignatureMethod();
          
          //Start body with the following
          //ldarg.0
          //ldc.i4     0x2
          //newarr     [mscorlib]System.Object
          //stloc.0

          newSigWorker = new BodyWorker(traceMeth.Body);
          newSigWorker.Append(newSigWorker.Create(OpCodes.Ldarg_0));
          newSigWorker.Append(newSigWorker.Create(OpCodes.Ldc_I4, methDef.Parameters.Count));
          newSigWorker.Append(newSigWorker.Create(OpCodes.Newarr, mSystemTypeRefs[OBJECT]));
          newSigWorker.Append(newSigWorker.Create(OpCodes.Stloc_0)); 
        }

        //Parse params and insert instruction in method and new signature method if need be
        ParseMethodParameters(worker, traceMeth, methDef.Parameters, methDef.GenericParameters);        
        
        if (creatingMethod)
        {
          for (int i = 1; i < traceMeth.Parameters.Count; i++)
          {
            //Add instructions to put every parameter on the stack
            ParameterDefinition traceParam = traceMeth.Parameters[i];
            AddStackInstructions(traceParam, i - 1, newSigWorker);
          }
          //Last add the call to internal runtime trace
          newSigWorker.Append(newSigWorker.Create(OpCodes.Ldloc_0));
          newSigWorker.Append(newSigWorker.Create(OpCodes.Call, mTraceInternal));
          newSigWorker.Append(newSigWorker.Create(OpCodes.Ret));
          //Add to list and do some housekeeping
          mMethodList.Add(signature, traceMeth);
          mRuntimeMethodCounter++;
          mRuntimeType.Methods.Add(traceMeth);          
        }
        return traceMeth;        
      }
      else
        return null;
    }

    //Adds instruction for loading parameters into array in internal trace in runtime 
    private void AddStackInstructions(ParameterDefinition param, int count, BodyWorker worker)
    {
      //Add array counter
      worker.Append(worker.Create(OpCodes.Ldloc_0));
      worker.Append(worker.Create(OpCodes.Ldc_I4, count));
      worker.Append(worker.Create(OpCodes.Ldarg_S, param));

      TypeReference type = param.ParameterType;
      
      //Do the ref stuff as the previous times
      bool isReference = false;
      if (type is ReferenceType)
      {
        ReferenceType reftype = param.ParameterType as ReferenceType;        
        type = reftype.ElementType;
        isReference = true;
      }

      type = InsertLoadReference(worker, type, isReference);
      
      //If not object then box
      if(type != null)
        worker.Append(worker.Create(OpCodes.Box, type));

      worker.Append(worker.Create(OpCodes.Stelem_Ref));      
    }

    public TypeReference InsertLoadReference(BodyWorker worker, TypeReference typeIn, bool refType)
    {
      TypeReference type = null;

      if (mSystemTypeRefs.ContainsKey(typeIn.FullName))
        type = mSystemTypeRefs[typeIn.FullName];

      if (refType)
        worker.Append(GetIndirectLoadInst(worker, type));

      return type;
    }

    public Instruction GetIndirectLoadInst(BodyWorker worker, TypeReference typeIn)
    {
      switch (typeIn.FullName)
      {
        case INT:
          return worker.Create(OpCodes.Ldind_I4);
        case UINT:
          return worker.Create(OpCodes.Ldind_U4);          
        case FLOAT:
          return worker.Create(OpCodes.Ldind_R4);
        case DOUBLE:
          return worker.Create(OpCodes.Ldind_R8);
        case SHORT:
          return worker.Create(OpCodes.Ldind_I2);
        case USHORT:
          return worker.Create(OpCodes.Ldind_U2);
        case BYTE:
          return worker.Create(OpCodes.Ldind_I1);
        case CHAR:
          return worker.Create(OpCodes.Ldind_I1);
        case BOOL:
          return worker.Create(OpCodes.Ldind_I1);
        default:
          //System.Diagnostics.Trace.WriteLine("Unkown type" + typeIn.FullName);
          return null;
      }
    }

    #endregion
  } 
}
