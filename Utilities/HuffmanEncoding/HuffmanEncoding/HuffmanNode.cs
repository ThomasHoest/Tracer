using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EQATEC.Utility
{
  internal class HuffmanNode : IComparable
  {
    public HuffmanNode(string token, int count)
    {
      Token = token;
      Count = count;
      Bit0 = Bit1 = null;
    }

    public HuffmanNode(HuffmanNode bit0, HuffmanNode bit1)
    {
      Token = null;
      Count = bit0.Count + bit1.Count;
      Bit0 = bit0;
      Bit1 = bit1;
    }

    public readonly string Token;
    public readonly int Count;
    public HuffmanNode Bit0;
    public HuffmanNode Bit1;


    public int CompareTo(object obj)
    {
      HuffmanNode arg = obj as HuffmanNode;
      if (arg == null)
        throw new Exception("HuffmanNode.CompareTo called with " + obj.GetType());
      if (Count != arg.Count)
        return Count.CompareTo(arg.Count);
      if (Token != null && arg.Token != null)
        return Token.CompareTo(arg.Token);
      if (Token == null && arg.Token == null)
        return Bit0.CompareTo(arg.Bit0);
      if (Token == null)
        return 1;
      return -1;
    }

    static public void WriteTree(BinaryWriter bw, HuffmanNode node)
    {
      bool isToken = (node.Token != null);
      bw.Write(isToken);
      if (isToken)
      {
        char[] tokenRaw = node.Token.ToCharArray();
        // Write the length of the data first; is needed for decoding.
        // If length can be encoded in one byte then settle for that; else
        // use the full 32-bit value to describe the length.
        if (tokenRaw.Length < Byte.MaxValue)
        {
          bw.Write((Byte)tokenRaw.Length);
        }
        else
        {
          bw.Write(Byte.MaxValue);
          bw.Write(tokenRaw.Length);
        }
        // Write the actual binary token-data
        bw.Write(tokenRaw);
      }
      else
      {
        WriteTree(bw, node.Bit0);
        WriteTree(bw, node.Bit1);
      }
    }

    static public HuffmanNode ReadTree(BinaryReader br)
    {
      bool isToken = br.ReadBoolean();
      if (isToken)
      {
        int len = br.ReadByte();
        if (len == Byte.MaxValue)
          len = br.ReadInt32();
        char[] tokenRaw = br.ReadChars(len);
        string token = new String(tokenRaw);
        HuffmanNode node = new HuffmanNode(token, 0/*don't care*/);
        return node;
      }
      else
      {
        HuffmanNode bit0 = ReadTree(br);
        HuffmanNode bit1 = ReadTree(br);
        HuffmanNode node = new HuffmanNode(bit0, bit1);
        return node;
      }
    }

  }

}
