using System.Collections;
using System.IO;

namespace EQATEC.Tracer.Tools
{
  public class UUEncoder
  {
    const byte mAsciiBase = 0x32;

    public static string UUEncodeString(string data)
    {
      using (MemoryStream memStream = new MemoryStream(NoBullshitEncoder.GetBytes(data)))
      {
        using (MemoryStream outStream = new MemoryStream())
        {
          UUEncoder.UUEncode(memStream, outStream);
          outStream.Position = 0;
          byte[] buffer = new byte[outStream.Length];
          outStream.Read(buffer, 0, (int)outStream.Length);
          return NoBullshitEncoder.GetString(buffer);
        }
      }
    }

    public static string UUDecodeString(string data)
    {
      using (MemoryStream memStream = new MemoryStream(NoBullshitEncoder.GetBytes(data)))
      {
        using (MemoryStream outStream = new MemoryStream())
        {
          UUEncoder.UUDecode(memStream, outStream);
          outStream.Position = 0;
          byte[] buffer = new byte[outStream.Length];
          outStream.Read(buffer, 0, (int)outStream.Length);
          return NoBullshitEncoder.GetString(buffer);
        }
      }
    }

    /// <summary>
    /// UUencode stream
    /// </summary>
    /// <param name="input">input stream</param>
    /// <param name="output">output stream</param>
    public static void UUEncode(Stream input, Stream output)
    {
      byte[] data = new byte[input.Length];

      int i = 0;
      //put all data into a byte buffer
      while (true)
      {
        int d = input.ReadByte();
        if (d != -1)
          data[i++] = (byte)d;
        else
          break;
      }
      //Make one huge bitarray
      BitArray lotsOfBits = new BitArray(data);
      int bitCounter = 0;
      while (true)
      {
        byte d = 0;
        int chunkSize = 0;
        //Always take take chunksize when there is room to do so. Otherwize take remainder
        if (bitCounter + 6 < lotsOfBits.Length)
          chunkSize = 6;
        else
          chunkSize = lotsOfBits.Length - bitCounter;
        //Make new byte with only 6 bits from bitarray
        for (i = 0; i < chunkSize; i++)
          d |= (byte)((lotsOfBits[bitCounter + i] ? 1 : 0) << i);
        //Add ascii base to make sure its text
        d += mAsciiBase;
        //Write to outpuut
        output.WriteByte(d);
        //Are we there yet?
        if (chunkSize < 6)
          break;

        bitCounter += 6;
      }
    }

    /// <summary>
    /// Decodes UUEncoded data to output stream
    /// </summary>
    /// <param name="input">UUencoded stream</param>
    /// <param name="output">Output stream</param>
    public static void UUDecode(Stream input, Stream output)
    {
      int temp = 0;
      int c = 0;

      while (true)
      {
        int d = input.ReadByte();
        if (d == -1)
          break;
        //Subtract ascii base to get original data
        byte b = (byte)(d - mAsciiBase);
        //Add to integer holder
        temp |= b << (6 * c++);
        //Extract the original three bytes from integer if we're not at the end yet
        //4 encoded bytes gives us 3 decoded bytes.
        if (c == 4)
        {
          for (int i = 0; i < c - 1; i++)
          {
            b = (byte)(temp >> (8 * i));
            output.WriteByte(b);
          }
          c = 0;
          temp = 0;
        }

      }
      //Decode any remainder
      for (int i = 0; i < c - 1; i++)
      {
        byte b = (byte)(temp >> (8 * i));
        output.WriteByte(b);
      }

    }
  }
}