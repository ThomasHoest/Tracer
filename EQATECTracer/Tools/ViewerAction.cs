using System.Collections.Generic;

namespace EQATEC.Tracer.Tools
{
  public class ViewerAction
  {
    public enum ActionType
    {
      Enable,
      Disable/*,
      EnableMany,
      DisableMany*/
    }

    List<ILType> mActionTargets;

    public List<ILType> ActionTargets
    {
      get
      {
        if (mActionTargets == null)
        {
          mActionTargets = new List<ILType>();
          /*
          if (mAction == ActionType.Disable)
            mAction = ActionType.DisableMany;
          else if (mAction == ActionType.Enable)
            mAction = ActionType.EnableMany;*/
        }

        return mActionTargets;
      }
      set { mActionTargets = value; }
    }
    /*
    ILType mActionTarget;

    public ILType ActionTarget
    {
      get { return mActionTarget; }
      set { mActionTarget = value; }
    }*/

    ActionType mAction;

    public ActionType Action
    {
      get { return mAction; }
      set { mAction = value; }
    }

    public ViewerAction(ILType target, ActionType action)
    {
      mAction = action;
      ActionTargets.Add(target);
      //mActionTarget = target;
    }

    public ViewerAction(ActionType action)
    {
      mAction = action;
    }

    public ViewerAction(bool enable)
    {
      if (enable)
        mAction = ActionType.Enable;
      else
        mAction = ActionType.Disable;
    }

  }
}