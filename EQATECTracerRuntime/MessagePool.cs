using System.Collections.Generic;
using EQATEC.Tracer.TracerRuntime.Communication;

namespace EQATEC.Tracer.TracerRuntime
{
  public class MessagePool<T> where T : new()
  {
    object mQueueSync = new object();
    Queue<T> mFreeMessages;

    int mPoolSize;

    public MessagePool(int size)
    {
      mPoolSize = size;

      mFreeMessages = new Queue<T>(size);

      for (int i = 0; i < size; i++)
      {
        T ms = new T();
        //ms.mValueList = new List<string>();
        mFreeMessages.Enqueue(ms);
      }
    }   

    public void PutMessage(T ms)
    {
      //ms.mValueList.Clear();
      lock (mQueueSync)
        mFreeMessages.Enqueue(ms);
      //Console.WriteLine("Putting. Count: " + mFreeMessages.Count);
    }

    public T GetMessage()
    {
      T ms;
      lock (mQueueSync)
      {
        if (mFreeMessages.Count == 0)
        {
          ms = new T();
          //ms.mValueList = new List<string>();
          mPoolSize++;
        }
        else
          ms = mFreeMessages.Dequeue();
      }

      //Console.WriteLine("Getting. Count: " + mFreeMessages.Count + " Length: " + mPoolSize);
      return ms;
    }
  }
}