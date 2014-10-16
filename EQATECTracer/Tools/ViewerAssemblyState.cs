using System;
using System.Collections.Generic;

namespace EQATEC.Tracer.Tools
{
  [Serializable]
  public class ViewerAssemblyState
  {
    Dictionary<string, MemberStateDescriptor> mEnabledMembers = new Dictionary<string, MemberStateDescriptor>();

    public Dictionary<string, MemberStateDescriptor> EnabledMembers
    {
      get { return mEnabledMembers; }
    }

    public void AddEnabled(MemberContainer member)
    {
      System.Diagnostics.Debug.Assert(!mEnabledMembers.ContainsKey(member.FullNameWithParams), "Member already enabled");

      if (!mEnabledMembers.ContainsKey(member.FullNameWithParams))
        mEnabledMembers.Add(member.FullNameWithParams, MemberStateDescriptor.Enabled);
    }

    public bool Exists(MemberContainer member)
    {
      return mEnabledMembers.ContainsKey(member.FullNameWithParams);
    }

    public void RemovedEnabled(MemberContainer member)
    {
      System.Diagnostics.Debug.Assert(mEnabledMembers.ContainsKey(member.FullNameWithParams), "Member not enabled");

      mEnabledMembers.Remove(member.FullNameWithParams);
    }
  }
}