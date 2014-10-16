using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace EQATEC.Utility
{
  internal class Decoder
  {
    public delegate void StringDecodedHandler(string s);

    public void Decode(byte[] encoded, StringDecodedHandler handler)
    {
      MemoryStream ms = new MemoryStream(encoded);
      BinaryReader br = new BinaryReader(ms);

      // Read the number of input tokens
      // Size=0 is encoded as the count and nothing more
      Int32 size = br.ReadInt32();
      if (size == 0)
        return;
      
      // Read the huffman-tree
      HuffmanNode root = HuffmanNode.ReadTree(br);

      // If root-node is a token then there is nothing encoded at all,
      // and the result is size x its token.
      if (root.Token != null)
      {
        for (int i = 0; i < size; i++)
          handler(root.Token);
        return;
      }

      // Decode the encoded tokens
      HuffmanNode node = root;
      for (; ; )
      {
        byte b = br.ReadByte();
        for (int i = 0; i < 8; i++)
        {
          byte mask = (byte)(1 << i);
          if ((b & mask) == 0)
            node = node.Bit0;
          else
            node = node.Bit1;
          if (node.Token != null)
          {
            handler(node.Token);
            if (--size == 0)
              return;
            node = root;
          }
        }
      }

    }
  }
}
