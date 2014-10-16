using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.Tracer.Tools
{
  public class LineHolder
  {

    //public LineHolder Self
    //{
    //  get
    //  {
    //    return this;
    //  }
    //}

    public delegate void LineSelectedHandler(bool selected);
    private event LineSelectedHandler OnLineSelectedInternal;
    LineSelectedHandler mCurrentHandler = null;

    /// <summary>
    /// Only one handler is allowed on this event. If another is added the old one is removed
    /// </summary>
    public event LineSelectedHandler OnLineSelected
    {
      add
      {
        if (mCurrentHandler != null)
          OnLineSelectedInternal -= mCurrentHandler;

        OnLineSelectedInternal += value;
        mCurrentHandler = value;
      }

      remove
      {
        OnLineSelectedInternal -= value;
        if (mCurrentHandler != null)
          mCurrentHandler = null;
      }
    }

    public delegate void LineSelectedHandlerBySearch(bool selected);
    private event LineSelectedHandlerBySearch OnLineSelectedInternalBySearch;
    LineSelectedHandlerBySearch mCurrentHandlerBySearch = null;

    /// <summary>
    /// Only one handler is allowed on this event. If another is added the old one is removed
    /// </summary>
    public event LineSelectedHandlerBySearch OnLineSelectedBySearch
    {
      add
      {
        if (mCurrentHandlerBySearch != null)
          OnLineSelectedInternalBySearch -= mCurrentHandlerBySearch;

        OnLineSelectedInternalBySearch += value;
        mCurrentHandlerBySearch = value;
      }

      remove
      {
        OnLineSelectedInternalBySearch -= value;
        if (mCurrentHandlerBySearch != null)
          mCurrentHandlerBySearch = null;
      }
    }


    #region Properties

    int mLevel = 0;
    public int Level
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mLevel; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mLevel = value; }
    }


    bool mSelected = false;

    public bool Selected
    {
      [System.Diagnostics.DebuggerStepThrough]
      get
      {
        return mSelected;
      }
      set
      {
        if (OnLineSelectedInternal != null)
          OnLineSelectedInternal(value);
        mSelected = value;
      }
    }

    bool mSelectedBySearch = false;
    public bool SelectedBySearch
    {
      [System.Diagnostics.DebuggerStepThrough]
      get
      {
        return mSelectedBySearch;
      }
      set
      {
        if (OnLineSelectedInternalBySearch != null)
          OnLineSelectedInternalBySearch(value);
        mSelectedBySearch = value;
      }
    }

    WordHolder mName;
    public WordHolder Name
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mName; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mName = value; }
    }

    WordHolder mFullName = new WordHolder();
    public WordHolder FullName
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mFullName; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mFullName = value; }
    }

    ParameterHolder[] mParams;
    public ParameterHolder[] Params
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mParams; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mParams = value; }
    }

    WordHolder mData;
    public WordHolder Data
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mData; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mData = value; }
    }

    string mTraceLevel;
    public string TraceLevel
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mTraceLevel; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mTraceLevel = value; }
    }

    int mThreadID;
    public int IThreadID
    {
      [System.Diagnostics.DebuggerStepThrough]
      get
      {
        return mThreadID;
      }
    }

    string mThreadIdString = null;
    public string ThreadID
    {
      [System.Diagnostics.DebuggerStepThrough]
      get
      {
        if (mThreadIdString == null)
          mThreadIdString = mThreadID.ToString();

        return mThreadIdString;
      }
    }

    DateTime mTime;
    string mTimeString = null;
    public string Time
    {
      get
      {
        if (mTimeString == null)
          mTimeString = mTime.ToString("HH:mm:ss:fff");

        return mTimeString;
      }
    }

    int mID;
    public int ID
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mID; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mID = value; }
    }

    MemberContainer mMember;
    public MemberContainer Member
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mMember; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mMember = value; }
    }

    LineType mType;
    public LineType Type
    {
      [System.Diagnostics.DebuggerStepThrough]
      get { return mType; }
      [System.Diagnostics.DebuggerStepThrough]
      set { mType = value; }
    }

    #endregion

    public enum LineType
    {
      EnterTrace,
      ReturnTrace,
      EnterLeaveTrace,
      Exception,
      CaughtException,
      FlushInfo,
      Log4NetTrace
    }

    public LineHolder(int id, int threadID, DateTime time, List<string> data)
    {
      mID = id;
      mTime = time;
      mThreadID = threadID;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < data.Count; i++)
      {
        sb.Append(data[i]);

        if (i == 0)
        {
          if (data[i] != "")
            mName = new WordHolder(data[i]);
          else
            mName = new WordHolder("No information available");
          sb.Append(Environment.NewLine);
        }
      }

      mData = new WordHolder(sb.ToString());
    }

    public LineHolder(int id, string text, int threadID, DateTime time, string data, string returnType)
    {
      mID = id;
      mName = new WordHolder(text);
      mTime = time;
      mParams = new ParameterHolder[0];
      mThreadID = threadID;
      if (data != null)
      {
        mParams = new ParameterHolder[1];
        mData = new WordHolder(data);
        mParams[0] = new ParameterHolder(mData, new WordHolder(returnType), new WordHolder("(return)"));
      }
    }

    public LineHolder(DateTime time)
    {
      mTime = time;
    }

    public LineHolder(DateTime time, string data)
    {
      mTime = time;
      mData = new WordHolder(data);
    }

    public LineHolder(int id, string text, int threadID, DateTime time, List<string> paramList, List<string> paramTypeList, List<string> paramNameList)
    {
      mID = id;
      mName = new WordHolder(text);
      mTime = time;
      mParams = new ParameterHolder[paramList.Count];

      for (int i = 0; i < paramList.Count; i++)
      {
        WordHolder data;
        int index = paramList[i].IndexOf(RemoveGenericMarkers(paramTypeList[i]));
        if (index != -1)
        {
          data = new WordHolder(paramList[i].Remove(0, index));
        }
        else if (paramTypeList[i] == "String")
        {
          data = new WordHolder('"' + paramList[i] + '"');
        }
        else
        {
          data = new WordHolder(paramList[i]);
        }

        mParams[i] = new ParameterHolder(data, new WordHolder(paramTypeList[i]), new WordHolder(paramNameList[i]));
      }

      mThreadID = threadID;
    }

    public string RemoveGenericMarkers(string data)
    {
      int index = data.IndexOf("<");
      if (index != -1)
        return data.Remove(index, data.Length - index);
      else
        return data;
    }

    public override string ToString()
    {
      string temp = string.Format("{0} ({1}) ", mTime.ToString("HH:mm:ss:fff"), mThreadID);
      StringBuilder sb = new StringBuilder();
      sb.Append(temp);

      switch (mType)
      {
        case LineType.EnterTrace:
          sb.Append("enter ");
          break;
        case LineType.ReturnTrace:
          sb.Append("leave ");
          break;
        case LineType.Log4NetTrace:
          sb.Append("log4net ");
          break;
        case LineType.Exception:
          sb.Append("exception ");
          sb.Append(mData.Text);
          return sb.ToString();
        case LineType.CaughtException:
          sb.Append("caught exception ");
          sb.Append(mData.Text);
          return sb.ToString();
        case LineType.FlushInfo:
          sb.Append("Line flush ");
          sb.Append(mData.Text);
          return sb.ToString();
        case LineType.EnterLeaveTrace:
          sb.Append("enter+leave ");
          break;
      }
      
      if(mName != null)
        sb.Append(mName.Text + " ");

      if (mParams != null && mParams.Length > 0)
      {
        sb.Append("( ");
        for (int i = 0; i < mParams.Length; i++)
        {
          sb.Append(mParams[i].Data.Text + ", ");
        }
        sb.Append(" )");
      }
      else
        sb.Append("()");

      return sb.ToString();
    }
  }
}