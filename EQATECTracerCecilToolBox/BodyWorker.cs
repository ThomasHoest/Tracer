///////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2008, EQATEC A/S. All rights reserved.
// 
// This software remains the property of EQATEC A/S. The software must
// not be disclosed, split, or merged in any form without prior written
// approval by EQATEC A/S.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;


namespace EQATEC.CecilToolBox
{
  /// <summary>
  /// Helper for manipulating instructions in Cecil-parsed assemblies.
  /// </summary>
  public class BodyWorker
  {
    private readonly MethodBody m_Body;
    private readonly CilWorker m_CilWorker;

    public BodyWorker(MethodBody body)
    {
      m_Body = body;
      m_CilWorker = body.CilWorker;

      // Initialize the original instructions for use in remapping etc
      m_OriginalInstructions = new List<Instruction>(body.Instructions.Count);
      foreach (Instruction i in body.Instructions)
        m_OriginalInstructions.Add(i);

      InitOpCodeRemap();
    }

    #region Original instructions
    private readonly IList<Instruction> m_OriginalInstructions;
    public IList<Instruction> OriginalInstructions
    {
      get
      {
        List<Instruction> oi = new List<Instruction>(m_OriginalInstructions.Count);
        oi.AddRange(m_OriginalInstructions);
        return oi;
      }    
    }

    public Instruction LastInstruction
    {
      get
      {
        return m_OriginalInstructions[m_OriginalInstructions.Count - 1];
      }
    }

    public Instruction FirstInstruction
    {
      get
      {
        return m_OriginalInstructions[0];
      }
    }

    #endregion

    #region Current instructions

    public InstructionCollection CurrentInstructions
    {
      get
      {
        return m_Body.Instructions;
      }
    }

    private Instruction mCurrentInstruction = null;

    public Instruction CurrentInstruction
    {
      get { return mCurrentInstruction; }
      set { mCurrentInstruction = value; }
    }

    #endregion 

    #region Create methods
    public Instruction Create (OpCode opcode)
    {
      return m_CilWorker.Create(opcode);
    }

		public Instruction Create (OpCode opcode, TypeReference type)
		{
      return m_CilWorker.Create(opcode, type);
    }
    /*
		public Instruction Create (OpCode opcode, CallSite site)
		{
      return m_CilWorker.Create(opcode, site);
		}*/

		public Instruction Create (OpCode opcode, MethodReference method)
		{
			return m_CilWorker.Create(opcode, method);
		}

		public Instruction Create (OpCode opcode, FieldReference field)
		{
			return m_CilWorker.Create(opcode, field);
		}

		public Instruction Create (OpCode opcode, string str)
		{
			return m_CilWorker.Create(opcode, str);
		}

		public Instruction Create (OpCode opcode, sbyte b)
		{
			return m_CilWorker.Create(opcode, b);
		}

		public Instruction Create (OpCode opcode, byte b)
		{
			return m_CilWorker.Create(opcode, b);
		}

		public Instruction Create (OpCode opcode, int i)
		{
			return m_CilWorker.Create(opcode, i);
		}

		public Instruction Create (OpCode opcode, long l)
		{
			return m_CilWorker.Create(opcode, l);
		}

		public Instruction Create (OpCode opcode, float f)
		{
			return m_CilWorker.Create(opcode, f);
		}

		public Instruction Create (OpCode opcode, double d)
		{
			return m_CilWorker.Create(opcode, d);
		}

		public Instruction Create (OpCode opcode, Instruction label)
		{
			return m_CilWorker.Create(opcode, label);
		}

		public Instruction Create (OpCode opcode, Instruction [] labels)
		{
			return m_CilWorker.Create(opcode, labels);
		}

		public Instruction Create (OpCode opcode, VariableDefinition var)
		{
			return m_CilWorker.Create(opcode, var);
		}

		public Instruction Create (OpCode opcode, ParameterDefinition param)
		{
			return m_CilWorker.Create(opcode, param);
    }
    #endregion

    #region Instruction manipulation

    

    /// <summary>
    /// Inject a new instruction just before an existing instruction, so that
    /// jumps to the old instruction will now become jumps to this new instruction.
    /// </summary>
    /// <param name="atIns">the instruction to insert right before</param>
    /// <param name="ins">the new instruction to insert</param>
    /// <returns></returns>
    public Instruction InsertAt(Instruction atIns, Instruction ins)
    {
      m_CilWorker.InsertBefore(atIns, ins);
      m_FixupMap.Add(atIns, ins);
      return ins;
    }

    /// <summary>
    /// Replace an existing instruction with another instruction. Existing
    /// jumps to the old instruction will now jump to this new instruction
    /// instead.
    /// </summary>
    /// <param name="oldIns">the existing instruction</param>
    /// <param name="ins">the new instruction to replace it</param>
    /// <returns></returns>
    public Instruction Replace(Instruction oldIns, Instruction ins)
    {
      m_CilWorker.Replace(oldIns, ins);
      m_FixupMap.Add(oldIns, ins);
      return ins;
    }

    /// <summary>
    /// Append a new instruction right after another existing instruction.
    /// </summary>
    /// <param name="afterIns"></param>
    /// <param name="ins"></param>
    /// <returns></returns>
    public Instruction AppendAfter(Instruction afterIns, Instruction ins)
    {      
      m_CilWorker.InsertAfter(afterIns, ins);
      return ins;
    }

    /// <summary>
    /// Append an instruction after the last appended instruction. Can only be called after setting currentinstruction
    /// </summary>
    /// <param name="ins"></param>
    public void ContinuesInsert(Instruction ins)
    {
      if (mCurrentInstruction != null)
      {
        AppendAfter(mCurrentInstruction, ins);
        mCurrentInstruction = ins;
      }
    }

    /// <summary>
    /// Insert a new instruction at the end of the current body.
    /// </summary>
    /// <param name="ins"></param>
    /// <returns></returns>
    public Instruction Append(Instruction ins)
    {
      m_CilWorker.Append(ins);
      return ins;
    }

    /// <summary>
    /// Insert a new instruction just before an existing instruction, so that
    /// existing jumps to the existing instructions will still jump to that,
    /// not to this new instruction.
    /// </summary>
    /// <param name="beforeIns"></param>
    /// <param name="ins"></param>
    /// <returns></returns>
    public Instruction InsertBefore(Instruction beforeIns, Instruction ins)
    {
      m_CilWorker.InsertBefore(beforeIns, ins);
      return ins;
    }

    /// <summary>
    /// Remove specific instruction from method body
    /// </summary>
    /// <returns></returns>
    public void Remove(Instruction ins)
    {
      m_CilWorker.Remove(ins);      
    }

    /// <summary>
    /// Called to indicate that we are appending an instruction after the
    /// body's original instructions.
    /// </summary>
    /// <param name="ins"></param>
    public void RemapBodyOutside(Instruction ins)
    {
      m_FixupMap.Add(m_Body.Instructions.Outside, ins);
    }

    /// <summary>
    /// Should be called when all instruction-manipulation is completed. The
    /// body-worker is no longer valid after this call has been performed.
    /// </summary>
    public void Done()
    {
      PerformFixupMapping();
      PerformOpCodeRemapping();
    }

    #endregion


    #region Fixup-remapping
    private readonly IDictionary<Instruction, Instruction> m_FixupMap = new Dictionary<Instruction, Instruction>(10);
    private void PerformFixupMapping()
    {
      // Now do a fixup of all affected event-handler-instruction-pointers
      foreach (KeyValuePair<Instruction, Instruction> fixup in m_FixupMap)
      {
        //method.Body.Scopes[0].
        Instruction before = fixup.Key;
        Instruction after = fixup.Value;
        foreach (ExceptionHandler eh in m_Body.ExceptionHandlers)
        {
          if (eh.TryStart == before) eh.TryStart = after;
          if (eh.TryEnd == before) eh.TryEnd = after;
          if (eh.FilterStart == before) eh.FilterStart = after;
          if (eh.FilterEnd == before) eh.FilterEnd = after;
          if (eh.HandlerStart == before) eh.HandlerStart = after;
          if (eh.HandlerEnd == before) eh.HandlerEnd = after;
        }
        foreach (Scope s in m_Body.Scopes)
        {
          if (s.Start == before)
            s.Start = after;
          if (s.End == before)
            s.End = after;
        }
        foreach (Instruction ins in m_OriginalInstructions)
        {
          OperandType optype = ins.OpCode.OperandType;
          switch (optype)
          {
            case OperandType.InlineBrTarget:
            case OperandType.ShortInlineBrTarget:
              if (ins.Operand == before)
                ins.Operand = after;
              break;
            case OperandType.InlineSwitch:
              {
                Instruction[] labels = (Instruction[])ins.Operand;
                for (int i = 0; i < labels.Length; i++)
                {
                  if (labels[i] == before)
                    labels[i] = after;
                }
              }
              break;
          }
        }
      }
    }
    #endregion

    #region Op-code remapping
    private readonly IDictionary<OpCode, OpCode> m_OpCodeRemap = new Dictionary<OpCode, OpCode>(20);
    private void InitOpCodeRemap()
    {
      // Init opcode mapping table
      m_OpCodeRemap[OpCodes.Br_S] = OpCodes.Br;
      m_OpCodeRemap[OpCodes.Brfalse_S] = OpCodes.Brfalse;
      m_OpCodeRemap[OpCodes.Brtrue_S] = OpCodes.Brtrue;
      m_OpCodeRemap[OpCodes.Beq_S] = OpCodes.Beq;
      m_OpCodeRemap[OpCodes.Bge_S] = OpCodes.Bge;
      m_OpCodeRemap[OpCodes.Bgt_S] = OpCodes.Bgt;
      m_OpCodeRemap[OpCodes.Ble_S] = OpCodes.Ble;
      m_OpCodeRemap[OpCodes.Blt_S] = OpCodes.Blt;
      m_OpCodeRemap[OpCodes.Bne_Un_S] = OpCodes.Bne_Un;
      m_OpCodeRemap[OpCodes.Bge_Un_S] = OpCodes.Bge_Un;
      m_OpCodeRemap[OpCodes.Bgt_Un_S] = OpCodes.Bgt_Un;
      m_OpCodeRemap[OpCodes.Ble_Un_S] = OpCodes.Ble_Un;
      m_OpCodeRemap[OpCodes.Blt_Un_S] = OpCodes.Blt_Un;
      m_OpCodeRemap[OpCodes.Leave_S] = OpCodes.Leave;
    }

    private void PerformOpCodeRemapping()
    {
      // Finally, change unsafe short-jumps into long-jumps
      foreach (Instruction ins in m_OriginalInstructions)
      {
        if (m_OpCodeRemap.ContainsKey(ins.OpCode))
          ins.OpCode = m_OpCodeRemap[ins.OpCode];
      }
    }
    #endregion

  }
}
