using System.IO;
using System.Text;

namespace EQATEC.Tracer.Tools
{
  public class NoBullshitEncoder
  {

    public static string GetString(byte[] data)
    {
      StringBuilder temp = new StringBuilder(data.Length);

      for (int i = 0; i < data.Length; i++)
        temp.Append((char)data[i]);

      return temp.ToString();
    }

    public static string GetString(Stream stream)
    {
      StringBuilder temp = new StringBuilder((int)stream.Length);
      while (true)
      {
        int d = stream.ReadByte();
        if (d != -1)
          temp.Append((char)d);
        else
          break;
      }

      return temp.ToString();
    }

    public static byte[] GetBytes(string data)
    {
      byte[] temp = new byte[data.Length];

      for (int i = 0; i < data.Length; i++)
        temp[i] = (byte)data[i];

      return temp;
    }

    public static byte[] GetBytes(Stream stream)
    {
      byte[] temp = new byte[stream.Length];
      int i = 0;
      while (true)
      {
        int d = stream.ReadByte();
        if (d != -1)
          temp[i++] = (byte)d;
        else
          break;
      }

      return temp;
    }

  }
}