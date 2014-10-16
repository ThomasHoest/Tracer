using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EQATEC.CecilToolBox
{

  public class CecilTools
  {
    public static AssemblyDefinition OpenAssembly(string assemblyPath)
    {      
      return AssemblyFactory.GetAssembly(assemblyPath);      
    }

    public static bool SaveModifiedAssembly(AssemblyDefinition assem, string path, ref string failure)
    {
      try
      {
        AssemblyFactory.SaveAssembly(assem, path);
        return true;
      }
      catch (Exception ex)
      {
        failure = ex.Message;
        return false;
      }
    }

    /// <summary>
    /// Creates function body returning a constant string. Existing body will be cleared.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="data"></param>
    public static void InsertBodyReturningString(Mono.Cecil.Cil.MethodBody body, string data)
    {
      body.Instructions.Clear();
      body.InitLocals = true;
      BodyWorker worker = new BodyWorker(body);      

      //Insert the following
      //
      //return "constant string";
      //
      //IL code
      //
      //ldstr ""
      //ret 
      //

      //IL could be transferred to tool box
      Instruction pushName = worker.Create(OpCodes.Ldstr, data);
      worker.Append(pushName);

      Instruction returnInst = worker.Create(OpCodes.Ret);
      worker.Append(returnInst);
    }

    /// <summary>
    /// Creates function body returning a constant string. Existing body will be cleared.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="data"></param>
    public static void InsertBodyReturningBool(Mono.Cecil.Cil.MethodBody body, bool data)
    {
      body.Instructions.Clear();
      body.InitLocals = true;
      BodyWorker worker = new BodyWorker(body);

      //Insert the following
      //
      //return "constant string";
      //
      //IL code
      //
      //ldstr ""
      //ldc.i4.1 or ldc.i4.0 
      //ret 
      //

      //IL could be transferred to tool box

      Instruction inst = data ? worker.Create(OpCodes.Ldc_I4_1) : worker.Create(OpCodes.Ldc_I4_0);
      worker.Append(inst);
            
      Instruction returnInst = worker.Create(OpCodes.Ret);
      worker.Append(returnInst);
    }
    
    public static string ShowParameters(MethodDefinition md)
    {
      StringBuilder s = new StringBuilder();

      s.Append("Method Parameters. Enjoy....");
      s.Append(Environment.NewLine);
      s.Append("---------------------");
      s.Append(Environment.NewLine);
    
      foreach (ParameterDefinition pd in md.Parameters)
      {
        s.Append(pd.ParameterType);
        s.Append(" ");
        s.Append(pd.Name);
        s.Append(Environment.NewLine);
      }
      
      return s.ToString();
    }

    public static string DumpIL(MethodDefinition md)
    {
      StringBuilder s = new StringBuilder();

      s.Append("Method IL. Enjoy....");
      s.Append(Environment.NewLine);
      s.Append("---------------------");
      s.Append(Environment.NewLine);
      s.Append("MaxStack: " + md.Body.MaxStack);
      s.Append(Environment.NewLine);
      s.Append("Code size: " + md.Body.CodeSize);
      s.Append(Environment.NewLine);

      foreach (VariableDefinition var in md.Body.Variables)
      {
        s.Append("Local var: " + var.Name);
        s.Append(" of type: " + var.VariableType);
        s.Append(" at index: " + var.Index);
        s.Append(Environment.NewLine);
      }

      s.Append(Environment.NewLine);
      s.Append("---------------------");
      s.Append(Environment.NewLine);
      foreach (Instruction inst in md.Body.Instructions)
      {
        s.Append(inst.OpCode);
        s.Append(" ");
        s.Append(inst.Operand);
        s.Append(Environment.NewLine);
      }
      
      return s.ToString();
    }

  }
}
