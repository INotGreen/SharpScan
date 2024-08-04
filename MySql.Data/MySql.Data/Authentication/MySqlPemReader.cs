// Copyright © 2017, 2018, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#if NETSTANDARD1_3
using AliasText = MySql.Data.MySqlClient.Framework.NetStandard1_3;
#else
using AliasText = System.Text;
#endif

public class MySqlPemReader
{
#if NETSTANDARD1_3
    public static RSA ConvertPemToRSAProvider(byte[] rawPublicKey)
#else
    public static RSACryptoServiceProvider ConvertPemToRSAProvider(byte[] rawPublicKey)
#endif
    {
      byte[] decodedKey = DecodeOpenSslKey(rawPublicKey);
      return DecodeX509Key(decodedKey);
    }

#if NETSTANDARD1_3
    static RSA DecodeX509Key(byte[] key)
#else
    static RSACryptoServiceProvider DecodeX509Key(byte[] key)
#endif
    {
      if (key == null) return null;

      byte[] oidSequence = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
      using (var memoryStream = new MemoryStream(key))
      {
        using (var reader = new BinaryReader(memoryStream))
        {
          try
          {
            var bytes = reader.ReadUInt16();
            switch (bytes)
            {
              case 0x8130:
                reader.ReadByte();
                break;
              case 0x8230:
                reader.ReadInt16();
                break;
              default:
                return null;
            }

            var sequence = reader.ReadBytes(15);

            // Compare arrays.
            bool arraysAreEqual = true;
            if (sequence.Length == oidSequence.Length)
            {
              for (int i=0; i<oidSequence.Length; i++)
                if (sequence[i] != oidSequence[i])
                {
                  arraysAreEqual = false;
                }
            }

            if (!arraysAreEqual) return null;

            bytes = reader.ReadUInt16();
            if (bytes == 0x8103) reader.ReadByte();
            else if (bytes == 0x8203) reader.ReadInt16();
            else return null;

            if (reader.ReadByte() != 0x00) return null;

            bytes = reader.ReadUInt16();
            if (bytes == 0x8130) reader.ReadByte();
            else if (bytes == 0x8230) reader.ReadInt16();
            else return null;

            bytes = reader.ReadUInt16();
            byte lowByte = 0x00;
            byte highByte = 0x00;
            if (bytes == 0x8102) lowByte = reader.ReadByte();
            else if (bytes == 0x8202)
            {
              highByte = reader.ReadByte();
              lowByte = reader.ReadByte();
            }
            else return null;

            int modulusSize = BitConverter.ToInt32(new byte[] { lowByte, highByte, 0x00, 0x00 }, 0);
            byte firstByte = reader.ReadByte();
            reader.BaseStream.Seek(-1, SeekOrigin.Current);

            if (firstByte == 0x00)
            {
              reader.ReadByte();
              modulusSize -= 1;
            }

            // Read modulus.
            byte[] modulus = reader.ReadBytes(modulusSize);
            if (reader.ReadByte() != 0x02) return null;

            // Read exponent.
            byte[] exponent = reader.ReadBytes(reader.ReadByte());
#if NETSTANDARD1_3
            RSA rsa = RSA.Create();
#else
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
#endif
            RSAParameters rsaKeyInfo = new RSAParameters
            {
              Modulus = modulus,
              Exponent = exponent
            };
            rsa.ImportParameters(rsaKeyInfo);
            return rsa;
          }
          catch (Exception)
          {
            return null;
          }
        }
      }
    }

    static byte[] DecodeOpenSslKey(byte[] rawPublicKey)
    {
      if (rawPublicKey == null) return null;

      // Remove line breaks and carriage returns.
      rawPublicKey = rawPublicKey.Where(b => b != 0x0D).ToArray();
      rawPublicKey = rawPublicKey.Where(b => b != 0x0A).ToArray();

      // Trim.
      rawPublicKey = TrimByteArray(rawPublicKey);

      // Remove header and footer
      byte[] headerSequence = { 0x2D, 0x2D, 0x2D, 0x2D, 0x2D, 0x42, 0x45, 0x47, 0x49, 0x4E, 0x20, 0x50, 0x55, 0x42, 0x4C, 0x49, 0x43, 0x20, 0x4B, 0x45, 0x59, 0x2D, 0x2D, 0x2D, 0x2D, 0x2D };
      byte[] footerSequence = { 0x2D, 0x2D, 0x2D, 0x2D, 0x2D, 0x45, 0x4E, 0x44, 0x20, 0x50, 0x55, 0x42, 0x4C, 0x49, 0x43, 0x20, 0x4B, 0x45, 0x59, 0x2D, 0x2D, 0x2D, 0x2D, 0x2D };
      if (!StartsWith(rawPublicKey, headerSequence) || !EndsWith(rawPublicKey, footerSequence)) return null;
      byte[] formattedRawPublicKey = new byte[rawPublicKey.Length - headerSequence.Length - footerSequence.Length];
      Array.Copy(rawPublicKey, headerSequence.Length, formattedRawPublicKey, 0, formattedRawPublicKey.Length);
      byte[] binaryKey;

      try
      {
        binaryKey = Convert.FromBase64String(AliasText.Encoding.Default.GetString(formattedRawPublicKey));
      }
      catch (FormatException)
      {
        return null;
      }

      return binaryKey;
    }

    static byte[] DecodeOpenSslKey2(byte[] rawPublicKey)
    {
      if (rawPublicKey == null) return null;

      var pem = AliasText.Encoding.Default.GetString(rawPublicKey);
      pem = pem.Replace(Environment.NewLine, "");
      const string header = "-----BEGIN PUBLIC KEY-----";
      const string footer = "-----END PUBLIC KEY-----";
      pem = pem.Trim();
      byte[] binaryKey;
      if (!pem.StartsWith(header) || !pem.EndsWith(footer))
        return null;

      StringBuilder builder = new StringBuilder(pem);
      builder.Replace(header, "");
      builder.Replace(footer, "");
      string formattedPem = builder.ToString().Trim();

      try
      {
        binaryKey = Convert.FromBase64String(formattedPem);
      }
      catch (FormatException)
      {
        return null;
      }

      return binaryKey;
    }

    private static byte[] TrimByteArray(byte[] array)
    {
      List<byte> trimmedArray = new List<byte>();
      bool startCopying = false;

      foreach (var item in array)
      {
        if (!startCopying)
        {
          if (item == 0x20) continue;
          else startCopying = true;
        }

        trimmedArray.Add(item);
      }
      array = trimmedArray.ToArray();
      trimmedArray = new List<byte>();

      for(int i=array.Length-1; i>=0; i--)
      {
        if (!startCopying)
        {
          if (array[i] == 0x20) continue;
          else startCopying = true;
        }

        trimmedArray.Add(array[i]);
      }

      return trimmedArray.ToArray().Reverse().ToArray();
    }

    private static bool StartsWith(byte[] array, byte[] containedArray)
    {
      for (int i = 0; i < array.Length; i++)
      {
        if (i == containedArray.Length) break;
        if (array[i] != containedArray[i]) return false;
      }

      return true;
    }

    private static bool EndsWith(byte[] array, byte[] containedArray)
    {
      for (int i = array.Length-1, j=0 ; i >= 0; i--, j++)
      {
        if (j == containedArray.Length) break;
        if (array[i] != containedArray[containedArray.Length - j - 1]) return false;
      }

      return true;
    }
}