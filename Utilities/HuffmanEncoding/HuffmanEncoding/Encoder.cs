using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EQATEC.Utility
{
  internal class Encoder
  {
    public byte[] Encode(IList<string> input)
    {
      MemoryStream ms = new MemoryStream();
      BinaryWriter bw = new BinaryWriter(ms);

      // Write the number of input-tokens. It will make reading
      // back the encoded stream a bit easier.
      bw.Write(input.Count);

      // Size=0 is encoded as the count and nothing more
      if (input.Count == 0)
        return ms.ToArray();

      // Count token frequency
      IDictionary<string, int> freqTable = new SortedDictionary<string, int>();
      foreach (string token in input)
      {
        if (freqTable.ContainsKey(token))
          freqTable[token]++;
        else
          freqTable[token] = 1;
      }

      // Build a sorted yet fast-lookup version of the frequency table
      SortedDictionary<HuffmanNode,bool> tokenSet = new SortedDictionary<HuffmanNode, bool>();
      foreach (KeyValuePair<string, int> kvp in freqTable)
      {
        HuffmanNode node = new HuffmanNode(kvp.Key, kvp.Value);
        tokenSet.Add(node, true);
      }

      // Reduce the tree by examining the two lowest frequencies and replace them with
      // a single node containing their combined count. Each loop will therefore remove
      // two and add one node, so it will stop after count-1 iterations.
      while (tokenSet.Count > 1)
      {
        HuffmanNode bit0 = GetFirstNode(tokenSet); tokenSet.Remove(bit0);
        HuffmanNode bit1 = GetFirstNode(tokenSet); tokenSet.Remove(bit1);
        HuffmanNode parent = new HuffmanNode(bit0, bit1);
        tokenSet.Add(parent, true);
      }

      // Huffman-tree is now fully built. Next step is to serialize it and
      // the input as a binary stream.
      HuffmanNode root = GetFirstNode(tokenSet);

      // Write the huffman-tree
      HuffmanNode.WriteTree(bw, root);

      // Now output the encoded bitstream for all the input.
      // Create a fast lookup table of each token and its corresponding bit-encoding,
      // to make encoding faster.
      IDictionary<string, List<bool>> tokenMap = new Dictionary<string, List<bool>>();
      BuildBitEncoding(root, tokenMap, new List<bool>());
      byte bytevalue = 0;
      int bitnumber = 0;
      foreach (string token in input)
      {
        IList<bool> bits = tokenMap[token];
        foreach (bool bit in bits)
        {
          if (bit)
            bytevalue |= (byte)(1 << bitnumber);
          if (++bitnumber == 8)
          {
            bw.Write(bytevalue);
            bytevalue = 0;
            bitnumber = 0;
          }
        }
      }
      if (bitnumber > 0)
        bw.Write(bytevalue);

      // All done
      byte[] encoded = ms.ToArray();
      return encoded;
    }

    private HuffmanNode GetFirstNode(SortedDictionary<HuffmanNode, bool> dict)
    {
      foreach (HuffmanNode node in dict.Keys)
        return node;
      return null;
    }

    private void BuildBitEncoding(HuffmanNode node, IDictionary<string, List<bool>> map, List<bool> bits)
    {
      if (node.Token != null)
      {
        map[node.Token] = new List<bool>(bits);
      }
      else
      {
        int top = bits.Count;
        bits.Add(false); BuildBitEncoding(node.Bit0, map, bits); bits.RemoveAt(top);
        bits.Add(true);  BuildBitEncoding(node.Bit1, map, bits); bits.RemoveAt(top);
      }
    }

  }
}
