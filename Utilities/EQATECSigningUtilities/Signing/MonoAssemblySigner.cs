using System;
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Reflection;

namespace EQATEC.SigningUtilities.Signing
{
  public class MonoAssemblySigner : IAssemblySigner
  {
    internal enum StrongNameOptions
    {
      Metadata,
      Signature
    }

		internal class StrongNameSignature {
			private byte[] hash;
			private byte[] signature;
			private UInt32 signaturePosition;
			private UInt32 signatureLength;
			private UInt32 metadataPosition;
			private UInt32 metadataLength;
			private byte cliFlag;
			private UInt32 cliFlagPosition;

			public byte[] Hash {
				get { return hash; }
				set { hash = value; }
			}

			public byte[] Signature {
				get { return signature; }
				set { signature = value; }
			}

			public UInt32 MetadataPosition {
				get { return metadataPosition; }
				set { metadataPosition = value; }
			}

			public UInt32 MetadataLength {
				get { return metadataLength; }
				set { metadataLength = value; }
			}

			public UInt32 SignaturePosition {
				get { return signaturePosition; }
				set { signaturePosition = value; }
			}

			public UInt32 SignatureLength {
				get { return signatureLength; }
				set { signatureLength = value; }
			}

			// delay signed -> flag = 0x01
			// strongsigned -> flag = 0x09
			public byte CliFlag {
				get { return cliFlag; }
				set { cliFlag = value; }
			}

			public UInt32 CliFlagPosition {
				get { return cliFlagPosition; }
				set { cliFlagPosition = value; }
			}
		}

    private string m_TokenAlgorithm = "SHA1";

    internal StrongNameSignature StrongHash(Stream stream, StrongNameOptions options)
    {
      StrongNameSignature info = new StrongNameSignature();

      HashAlgorithm hash = HashAlgorithm.Create(m_TokenAlgorithm);
      CryptoStream cs = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write);

      // MS-DOS Header - always 128 bytes
      // ref: Section 24.2.1, Partition II Metadata
      byte[] mz = new byte[128];
      stream.Read(mz, 0, 128);
      ushort dosHeader = BitConverterLE.ToUInt16(mz, 0);
      if ( dosHeader != 0x5a4d)
        return null;

      UInt32 peHeader = BitConverterLE.ToUInt32(mz, 60);
      cs.Write(mz, 0, 128);
      if (peHeader != 128)
      {
        byte[] mzextra = new byte[peHeader - 128];
        stream.Read(mzextra, 0, mzextra.Length);
        cs.Write(mzextra, 0, mzextra.Length);
      }

      // PE File Header - always 248 bytes
      // ref: Section 24.2.2, Partition II Metadata
      byte[] pe = new byte[248];
      stream.Read(pe, 0, 248);
      uint peHeader1 = BitConverterLE.ToUInt32(pe, 0);
      if (peHeader1 != 0x4550)
        return null;
      ushort peHeader2 = BitConverterLE.ToUInt16(pe, 4);
      if (peHeader2 != 0x14c)
        return null;

      // MUST zeroize both CheckSum and Security Directory
      byte[] v = new byte[8];
      Buffer.BlockCopy(v, 0, pe, 88, 4);
      Buffer.BlockCopy(v, 0, pe, 152, 8);
      cs.Write(pe, 0, 248);

      UInt16 numSection = BitConverterLE.ToUInt16(pe, 6);
      int sectionLength = (numSection * 40);
      byte[] sectionHeaders = new byte[sectionLength];
      stream.Read(sectionHeaders, 0, sectionLength);
      cs.Write(sectionHeaders, 0, sectionLength);

      UInt32 cliHeaderRVA = BitConverterLE.ToUInt32(pe, 232);
      UInt32 cliHeaderPos = RVAtoPosition(cliHeaderRVA, numSection, sectionHeaders);
      int cliHeaderSiz = (int)BitConverterLE.ToUInt32(pe, 236);

      // CLI Header
      // ref: Section 24.3.3, Partition II Metadata
      byte[] cli = new byte[cliHeaderSiz];
      stream.Position = cliHeaderPos;
      stream.Read(cli, 0, cliHeaderSiz);

      UInt32 strongNameSignatureRVA = BitConverterLE.ToUInt32(cli, 32);
      info.SignaturePosition = RVAtoPosition(strongNameSignatureRVA, numSection, sectionHeaders);
      info.SignatureLength = BitConverterLE.ToUInt32(cli, 36);

      UInt32 metadataRVA = BitConverterLE.ToUInt32(cli, 8);
      info.MetadataPosition = RVAtoPosition(metadataRVA, numSection, sectionHeaders);
      info.MetadataLength = BitConverterLE.ToUInt32(cli, 12);

      if (options == StrongNameOptions.Metadata)
      {
        cs.Close();
        hash.Initialize();
        byte[] metadata = new byte[info.MetadataLength];
        stream.Position = info.MetadataPosition;
        stream.Read(metadata, 0, metadata.Length);
        info.Hash = hash.ComputeHash(metadata);
        return info;
      }

      // now we hash every section EXCEPT the signature block
      for (int i = 0; i < numSection; i++)
      {
        UInt32 start = BitConverterLE.ToUInt32(sectionHeaders, i * 40 + 20);
        int length = (int)BitConverterLE.ToUInt32(sectionHeaders, i * 40 + 16);
        byte[] section = new byte[length];
        stream.Position = start;
        stream.Read(section, 0, length);
        if ((start <= info.SignaturePosition) && (info.SignaturePosition < start + length))
        {
          // hash before the signature
          int before = (int)(info.SignaturePosition - start);
          if (before > 0)
          {
            cs.Write(section, 0, before);
          }
          // copy signature
          info.Signature = new byte[info.SignatureLength];
          Buffer.BlockCopy(section, before, info.Signature, 0, (int)info.SignatureLength);
          Array.Reverse(info.Signature);
          // hash after the signature
          int s = (int)(before + info.SignatureLength);
          int after = (int)(length - s);
          if (after > 0)
          {
            cs.Write(section, s, after);
          }
        }
        else
          cs.Write(section, 0, length);
      }

      cs.Close();
      info.Hash = hash.Hash;
      return info;
    }

    private UInt32 RVAtoPosition(UInt32 r, int sections, byte[] headers)
    {
      for (int i = 0; i < sections; i++)
      {
        UInt32 p = BitConverterLE.ToUInt32(headers, i * 40 + 20);
        UInt32 s = BitConverterLE.ToUInt32(headers, i * 40 + 12);
        int l = (int)BitConverterLE.ToUInt32(headers, i * 40 + 8);
        if ((s <= r) && (r < s + l))
        {
          return p + r - s;
        }
      }
      return 0;
    }

    private bool Sign(string fileName, RSA rsa)
    {
      bool result = false;
      StrongNameSignature sn;
      using (FileStream fs = File.OpenRead(fileName))
      {
        sn = StrongHash(fs, StrongNameOptions.Signature);
        fs.Close();
      }
      if (sn == null)
        return false;
      if (sn.Hash == null)
        return false;

      byte[] signature = null;
      try
      {
        RSAPKCS1SignatureFormatter sign = new RSAPKCS1SignatureFormatter(rsa);
        sign.SetHashAlgorithm(m_TokenAlgorithm);
        signature = sign.CreateSignature(sn.Hash);
        Array.Reverse(signature);
      }
      catch (CryptographicException ce)
      {
        Trace.WriteLine("CryptographicException signing file. Error is " + ce.Message);
        return false;
      }

      using (FileStream fs = File.OpenWrite(fileName))
      {
        fs.Position = sn.SignaturePosition;
        fs.Write(signature, 0, signature.Length);
        fs.Close();
        result = true;
      }
      return result;
    }

    public bool SignAssembly(string assemblyFilePath, string pathToKeyContainer)
    {
      RSA rsa = SignFileReader.ReadStrongNameKeyFile(pathToKeyContainer);
      return Sign(assemblyFilePath, rsa);
    }

    public bool SignAssembly(string assemblyFilePath, byte[] keyContainer)
    {
      RSA rsa = SignFileReader.ReadStrongNameKeyFile(keyContainer);
      return Sign(assemblyFilePath, rsa);
    }

  }
}
