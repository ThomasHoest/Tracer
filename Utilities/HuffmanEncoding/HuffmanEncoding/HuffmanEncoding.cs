using System;
using System.Collections.Generic;
using System.Text;

namespace EQATEC.Utility
{
  public class HuffmanEncoding
  {
    public static byte[] Encode(IList<string> input)
    {
      Encoder encoder = new Encoder();
      byte[] ba = encoder.Encode(input);
      return ba;
    }

    public delegate void StringDecodedHandler(string s);

    public static void Decode(byte[] encoded, StringDecodedHandler handler)
    {
      m_handler = handler;
      Decoder decoder = new Decoder();
      decoder.Decode(encoded, new Decoder.StringDecodedHandler(OnStringDecoded));
    }
    private static StringDecodedHandler m_handler;
    private static void OnStringDecoded(string s)
    {
      m_handler(s);
    }
  }
}
