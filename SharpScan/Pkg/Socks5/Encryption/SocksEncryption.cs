/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Security.Cryptography;
using System.Text;
using Socks5.Core.Socks;

namespace Socks5.Core.Encryption
{
    public class SocksEncryption
    {
        private AuthTypes _auth;
        private DarthEncrypt _dc;
        private DarthEncrypt _dcc;

        public RSACryptoServiceProvider key;
        private RSACryptoServiceProvider _remotepubkey;

        public SocksEncryption()
        {
            key = new RSACryptoServiceProvider(1024);
            _remotepubkey = new RSACryptoServiceProvider(1024);
            _remotepubkey.PersistKeyInCsp = false;
            key.PersistKeyInCsp = false;
            _dc = new DarthEncrypt
            {
                PassPhrase = Utils.RandStr(20)
            };
            _dcc = new DarthEncrypt();
        }

        public void GenerateKeys()
        {
            key = new RSACryptoServiceProvider(1024);
            _remotepubkey = new RSACryptoServiceProvider(1024);
            _remotepubkey.PersistKeyInCsp = false;
            key.PersistKeyInCsp = false;
            _dc = new DarthEncrypt();
            _dc.PassPhrase = Utils.RandStr(20);
            _dcc = new DarthEncrypt();
        }

        public byte[] ShareEncryptionKey()
        {
            //share public key.
            return _remotepubkey.Encrypt(Encoding.ASCII.GetBytes(_dc.PassPhrase), false);
        }

        public byte[] GetPublicKey()
        {
            return Encoding.ASCII.GetBytes(key.ToXmlString(false));
        }

        public void SetEncKey(byte[] key)
        {
            _dcc.PassPhrase = Encoding.ASCII.GetString(key);
        }

        public void SetKey(byte[] key, int offset, int len)
        {
            var e = Encoding.ASCII.GetString(key, offset, len);
            _remotepubkey.FromXmlString(e);
        }

        public void SetType(AuthTypes k)
        {
            _auth = k;
        }

        public AuthTypes GetAuthType()
        {
            return _auth;
        }

        public byte[] ProcessInputData(byte[] buffer, int offset, int count)
        {
            //realign buffer.
            try
            {
                var buff = new byte[count];
                Buffer.BlockCopy(buffer, offset, buff, 0, count);
                switch (_auth)
                {
                    case AuthTypes.SocksBoth:
                        //decrypt, then decompress.
                        var data = _dcc.DecryptBytes(buff);
                        return _dcc.DecompressBytes(data);
                    case AuthTypes.SocksCompress:
                        //compress data.
                        return _dcc.DecompressBytes(buff);
                    case AuthTypes.SocksEncrypt:
                        return _dcc.DecryptBytes(buff);
                    default:
                        return buffer;
                }
            }
            catch
            {
                return null;
            }
        }

        public byte[] ProcessOutputData(byte[] buffer, int offset, int count)
        {
            //realign buffer.
            try
            {
                var buff = new byte[count - offset];
                Buffer.BlockCopy(buffer, offset, buff, 0, count);
                switch (_auth)
                {
                    case AuthTypes.SocksBoth:
                        //compress, then encrypt.
                        var data = _dc.CompressBytes(buff, 0, count);
                        return _dc.EncryptBytes(data);
                    case AuthTypes.SocksCompress:
                        //compress data.
                        return _dc.CompressBytes(buff, 0, count);
                    case AuthTypes.SocksEncrypt:
                        return _dc.EncryptBytes(buff);
                    case AuthTypes.Login:
                    case AuthTypes.Unsupported:
                    case AuthTypes.None:
                    default:
                        return buffer;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

