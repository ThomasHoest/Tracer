namespace EQATEC.Tracer.Tools
{
  public class TraceLineMenuAction
  {
    public enum MenuActionType
    {
      EnableDisable,
      DisableAndRemove,
      EnableCallees,
      FindInTree,
    }

    MenuActionType mAction;
    public MenuActionType Action
    {
      get { return mAction; }
    }

    int mLevel;
    public int Level
    {
      get { return mLevel; }
    }

    bool mEnable;
    public bool Enable
    {
      get { return mEnable; }
      set { mEnable = value; }
    }

    int mId;
    public int ID
    {
      get { return mId; }
    }

    public TraceLineMenuAction(int id, int level, MenuActionType action)
    {
      mId = id;
      mLevel = level;
      mAction = action;
    }
  }
}