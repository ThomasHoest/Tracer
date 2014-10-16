using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using EQATEC.Tracer.Tools;
using EQATEC.Tracer.TracerRuntime;
using EQATEC.CecilToolBox;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace EQATEC.Tracer
{ 
  public class InstrumentationHandler
  {
    AssemblyCollection mAssemblyList;
    SigningUtilities.AssemblySigningActionCallback mAssemblySigningCallback;
    private static string mVSHostsExt = ".vshost.exe";

    public delegate void InstrumentationActionHandler(InstrumentationActionEventArgs e);
    /// <summary>
    /// Event fired on instrumentation events, started, ended, finished
    /// </summary>
    public event InstrumentationActionHandler OnInstrumentationAction;

    public AssemblyCollection AssemblyList
    {
      get
      {
        return mAssemblyList;
      }
    }

    public bool InstrumentProperties { get; set; }
    public bool TraceToFile { get; set; }

    List<string> mErrorList;
    public List<string> ErrorList
    {
      get { return mErrorList; }
      set { mErrorList = value; }
    }

    List<string> mMessageList;
    public List<string> MessageList
    {
      get { return mMessageList; }
      set { mMessageList = value; }
    }

    private string mSourceBase = null;
    public string SourceBase
    {
      get { return mSourceBase; }
      set { mSourceBase = value; }
    }

    public InstrumentationHandler(SigningUtilities.AssemblySigningActionCallback callback )
    {
      mAssemblySigningCallback = callback;
      mAssemblyList = new AssemblyCollection();     
      mErrorList = new List<string>();
      mMessageList = new List<string>();
      InstrumentProperties = false;
      TraceToFile = false;
    }
    List<AssemblyContainer> mExecutableList = null;

    /// <summary>
    /// List of all executables in the added assemblies
    /// </summary>
    public List<AssemblyContainer> ExecutableList
    {
      get
      {
        if (mExecutableList == null)
        {
          mExecutableList = new List<AssemblyContainer>();
          foreach (AssemblyContainer assemCon in mAssemblyList)
            if (assemCon.AssemblyDef.Kind == AssemblyKind.Console || assemCon.AssemblyDef.Kind == AssemblyKind.Windows)
              mExecutableList.Add(assemCon);
        }

        return mExecutableList;
      }
    }

    public void Reset()
    {
      mExecutableList = null;
      mAssemblyList.Clear();
      
    }

    /// <summary>
    /// Add assembly to be instrumented
    /// </summary>
    /// <param name="path">Path of the assembly</param>
    /// <returns>True if .net assembly</returns>
    public bool AddAssembly(string path)
    {
      if (mSourceBase == null)
        throw new InvalidOperationException("Source base must be set before adding assemblies");

      AssemblyContainer assemContainer = null;

      try
      {
        AssemblyDefinition assemDefinition = CecilTools.OpenAssembly(path);
        assemContainer = new AssemblyContainer(assemDefinition);
        assemContainer.AssemblyName = path.Remove(0, mSourceBase.Length + 1);
        assemContainer.AssemblyPath = path;
        
        if(assemContainer.AssemblyName.Contains(mVSHostsExt))
          assemContainer.SetEnabledState(false);
        else
          assemContainer.SetEnabledState(true);

        if (assemDefinition.Name.HasPublicKey)
        {
          if (!ContinueWithSignedAssembly(assemContainer))
          {
            mErrorList.Add(string.Format("Skipping signed assembly {0}", assemContainer.AssemblyName));
            return false;
          }
        }
      }
      catch (Exception)
      { }

      if (assemContainer != null)
      {
        // We inject a class named "EQATEC.Profiler.UserModule" into profiled assemblies
        // as a way of tracking whether they've been profiled or not.
        // So, if we come acress an assembly that already has this class then it must
        // have been previously profiled and we should warn about that, and skip it.
        // Otherwise, add the type to the new assembly.
        if (assemContainer.AssemblyDef.MainModule.TypeReferences["EQATEC.Tracer.TracerRuntime.TracerRuntime"] != null)
        {
          mErrorList.Add(string.Format("Assembly {0} is already profiled; you should do a full app rebuild", assemContainer.AssemblyName));
          return false;
        }

        if (assemContainer.AssemblyDef.MainModule.TypeReferences["log4net.LogManager"] != null)
        {
          assemContainer.HasLog4Net = true;
          mMessageList.Add("Log4Net detected. Adding TracerLog4NetAppender");          
        }

        mAssemblyList.Add(assemContainer);
        return true;
      }
      return false;
    }

    private bool ContinueWithSignedAssembly( AssemblyContainer assemContainer )
    {
      Debug.Assert(assemContainer.AssemblyDef.Name.HasPublicKey, "Called without having a public key");
      bool canContinue = false;

      SigningUtilities.SigningSetting signingSettings = null;
      if (mAssemblySigningCallback != null)
      {
        signingSettings = mAssemblySigningCallback(assemContainer.Name, assemContainer.AssemblyDef.Name.PublicKey);
        if (signingSettings != null)
        {
          switch (signingSettings.SigningAction)
          {
            case EQATEC.SigningUtilities.SigningAction.Sign:
              assemContainer.SigningSettings = signingSettings;
              canContinue = true;
              break;

            case EQATEC.SigningUtilities.SigningAction.Skip:
              canContinue = false;
              break;

            case EQATEC.SigningUtilities.SigningAction.Strip:
              assemContainer.AssemblyDef.Name.HasPublicKey = false;
              canContinue = true;
              break;
          }
        }
      }

      return canContinue;
    }
    /*
    public bool AddSerializedAssembly(string data)
    {
      XmlSerializer serializer = new XmlSerializer(typeof(AssemblyCollection));
      UTF8Encoding enc = new UTF8Encoding();
      byte[] buffer = enc.GetBytes(data);
      MemoryStream mStream = new MemoryStream(buffer);
      mAssemblyList = serializer.Deserialize(mStream) as AssemblyCollection;
      return true;
    }*/

    /// <summary>
    /// Instrument all added assemblies 
    /// </summary>
    /// <param name="path">Output directory</param>
    public void Instrument(string path)
    {      
      AssemblyInstrumenter instrumenter = new AssemblyInstrumenter(mAssemblySigningCallback);
      instrumenter.TraceToFile = TraceToFile;
      instrumenter.InstrumentProperties = InstrumentProperties;
      instrumenter.OnAssemblyInstrumentationEnded += new AssemblyInstrumenter.AssemblyInstrumentationEnded(instrumenter_OnAssemblyInstrumentationEnded);
      instrumenter.OnAssemblyInstrumentationStarted += new AssemblyInstrumenter.AssemblyInstrumentationStarted(instrumenter_OnAssemblyInstrumentationStarted);
      mAssemblyList.ID = Guid.NewGuid();      
      
      
      instrumenter.InstrumentCollection(mAssemblyList);      
      instrumenter.FinishInstrumentation(path);

      Save(instrumenter, path);

      if (instrumenter.ErrorList.Count > 0)
      {
        foreach (string s in instrumenter.ErrorList)
          mErrorList.Add(s);

        return;
      }

      if (OnInstrumentationAction != null)
      {
        OnInstrumentationAction(
          new InstrumentationActionEventArgs(InstrumentationActionEventArgs.InstrumentationAction.InstrumentationFinished, instrumenter.InstrumentedMethodCounter));
      }
    }

    void instrumenter_OnAssemblyInstrumentationStarted(string name)
    {
      if (OnInstrumentationAction != null)
      {
        OnInstrumentationAction(
          new InstrumentationActionEventArgs(InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationStarted, name));
      }
    }

    void instrumenter_OnAssemblyInstrumentationEnded(string name, int methodCounter, bool success)
    {
      if (OnInstrumentationAction != null)
      {
        OnInstrumentationAction(
          new InstrumentationActionEventArgs(InstrumentationActionEventArgs.InstrumentationAction.AssemblyInstrumentationEnded, name, methodCounter));
      }

    }

    private void Save(AssemblyInstrumenter instrumenter, string path)
    {
      try
      {
        foreach (AssemblyContainer assemCon in mAssemblyList)
        {
          //only save the modified assemblies to the new path
          if (assemCon.Enabled == true)
          {
            string assemPath = System.IO.Path.Combine(path, assemCon.AssemblyName);

            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(assemPath));

            if (!dir.Exists)
              dir.Create();

            FileInfo fi = new FileInfo(assemPath);
            if (fi.Exists)
              fi.Delete();

            string failure = "";

            if (!CecilTools.SaveModifiedAssembly(assemCon.AssemblyDef, assemPath, ref failure))
            {
              mErrorList.Add(string.Format("Unable to save instrumented assembly: {0} Deselect and try again", assemCon.AssemblyName));
              if (failure != "")
                mErrorList.Add("Reason: " + failure);

              return;
            }

            //sign saved assembly, if applicable
            if (assemCon.SigningSettings != null)
            {
              bool wasSigned = instrumenter.SignAssemblyContainer(assemCon, assemPath);
              if (!wasSigned)
                mErrorList.Add(string.Format("Unable to sign instrumented assembly: {0}", assemCon.AssemblyName));
            }            
          }
        }
      }      
      catch (Exception)
      {
        mErrorList.Add("Unable to save instrumented assemblies");
      }
    }

  }
}
