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
using System.Text;
using System.Security.Cryptography;
using MySql.Data.MySqlClient.Properties;
#if NETSTANDARD1_3
using AliasText = MySql.Data.MySqlClient.Framework.NetStandard1_3;
#else
using AliasText = System.Text;
#endif

namespace MySql.Data.MySqlClient.Authentication
{
  /// <summary>
  /// The implementation of the caching_sha2_password authentication plugin.
  /// </summary>
  public class CachingSha2AuthenticationPlugin : Sha256AuthenticationPlugin
  {
    internal static AuthStage _authStage;

    public override string PluginName => "caching_sha2_password";

    protected override void SetAuthData(byte[] data)
    {
      // If the data given to us is a null terminated string, we need to trim off the trailing zero.
      if (data[data.Length - 1] == 0)
      {
        byte[] b = new byte[data.Length - 1];
        Buffer.BlockCopy(data, 0, b, 0, data.Length - 1);
        base.SetAuthData(b);
      }
      else base.SetAuthData(data);
    }

    protected override byte[] MoreData(byte[] data)
    {
      rawPubkey = data;

      // Generate scramble.
      if (data == null)
      {
        byte[] scramble = GetPassword() as byte[];
        byte[] buffer = new byte[scramble.Length - 1];
        Array.Copy(scramble, 1, buffer, 0, scramble.Length-1);
        return buffer;
      }
      // Fast authentication.
      else if (data[0] == 3)
      {
        _authStage = AuthStage.FAST_AUTH;
        return null;
      }
      else
        return GeneratePassword() as byte[];
    }

    protected byte[] GeneratePassword()
    {
      // If connection is secure perform full authentication.
      if (Settings.SslMode != MySqlSslMode.None)
      {
        _authStage = AuthStage.FULL_AUTH;

        // Send as clear text since the channel is already encrypted.
        byte[] passBytes = Encoding.GetBytes(Settings.Password);
        byte[] buffer = new byte[passBytes.Length + 1];
        Array.Copy(passBytes, 0, buffer, 0, passBytes.Length);
        buffer[passBytes.Length] = 0;
        return buffer;
      }
      else
      {
        // Request RSA key from server.
        if (rawPubkey != null && rawPubkey[0] == 4)
        {
          _authStage = AuthStage.REQUEST_RSA_KEY;
          return new byte[] { 0x02 };
        }
        else if (!Settings.AllowPublicKeyRetrieval)
          throw new MySqlException(Resources.RSAPublicKeyRetrievalNotEnabled);
        // Full authentication.
        else
        {
          _authStage = AuthStage.FULL_AUTH;
          byte[] bytes = GetRsaPassword(Settings.Password, AuthenticationData, rawPubkey);
          if (bytes != null && bytes.Length == 1 && bytes[0] == 0) return null;
          return bytes;
        }
      }
    }

    private byte[] GetRsaPassword(string password, byte[] seedBytes, byte[] rawPublicKey)
    {
      if (password.Length == 0) return new byte[1];
      if (rawPubkey == null) return null;
      // Obfuscate the plain text password with the session scramble.
      byte[] obfuscated = GetXor(AliasText.Encoding.Default.GetBytes(password), seedBytes);

      // Encrypt the password and send it to the server.
      if (this.ServerVersion >= new Version("8.0.5"))
      {
#if NETSTANDARD1_3
        RSA rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
        if (rsa == null) throw new MySqlException(Resources.UnableToReadRSAKey);

        return rsa.Encrypt(obfuscated, RSAEncryptionPadding.OaepSHA1);
#else
        RSACryptoServiceProvider rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
        if (rsa == null) throw new MySqlException(Resources.UnableToReadRSAKey);

        return rsa.Encrypt(obfuscated, true);
#endif
      }
      else
      {
#if NETSTANDARD1_3
        RSA rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
        if (rsa == null) throw new MySqlException(Resources.UnableToReadRSAKey);

        return rsa.Encrypt(obfuscated, RSAEncryptionPadding.Pkcs1);
#else
        RSACryptoServiceProvider rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
        if (rsa == null) throw new MySqlException(Resources.UnableToReadRSAKey);

        return rsa.Encrypt(obfuscated, false);
#endif
      }
    }

    public override object GetPassword()
    {
      _authStage = AuthStage.GENERATE_SCRAMBLE;

      // If we have no password then we just return 1 zero byte.
      if (Settings.Password.Length == 0) return new byte[1];

      SHA256 sha = SHA256.Create();

      byte[] firstHash = sha.ComputeHash(AliasText.Encoding.Default.GetBytes(Settings.Password));
      byte[] secondHash = sha.ComputeHash(firstHash);

      byte[] input = new byte[AuthenticationData.Length + secondHash.Length];
      Array.Copy(secondHash, 0, input, 0, secondHash.Length);
      Array.Copy(AuthenticationData, 0, input, secondHash.Length, AuthenticationData.Length);
      byte[] thirdHash = sha.ComputeHash(input);

      byte[] finalHash = new byte[thirdHash.Length];
      for (int i = 0; i < firstHash.Length; i++)
        finalHash[i] = (byte)(firstHash[i] ^ thirdHash[i]);

      byte[] buffer = new byte[finalHash.Length + 1];
      Array.Copy(finalHash, 0, buffer, 1, finalHash.Length);
      buffer[0] = 0x20;
      return buffer;
    }
  }

  /// <summary>
  /// Defines the stage of the authentication.
  /// </summary>
  internal enum AuthStage
  {
    GENERATE_SCRAMBLE = 0,
    REQUEST_RSA_KEY = 1,
    FAST_AUTH = 2,
    FULL_AUTH = 3
  }
}