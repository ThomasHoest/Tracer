using System.Threading;

namespace EQATEC.Tracer.TracerRuntime
{
  public class Semaphore
  {
    ManualResetEvent mWaitEvent = new ManualResetEvent(false);
    object mSync = new object();
    int mItemCounter = 0;

    public void Release()
    {
      mWaitEvent.Set();
    }

    public int Count
    {
      get
      {
        return mItemCounter;
      }      
    }
        
    public void Clear()
    {
      mItemCounter = 0;
      mWaitEvent.Reset();
    }

    public void Signal()
    {
      lock (mSync)
      {
        mItemCounter++;
        mWaitEvent.Set();
      }
    }

    public bool Wait(int timeout)
    {
      bool success = false;
     
      if (timeout == 0)
        success = mWaitEvent.WaitOne();
      else
        success = mWaitEvent.WaitOne(timeout, false);

      if (success)
      {
        lock (mSync)
        {
          mItemCounter--;

          if (mItemCounter == 0)
            mWaitEvent.Reset();
        }
      }

      return success;
    }
  }
}