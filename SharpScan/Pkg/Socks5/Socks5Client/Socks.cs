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

using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using Socks5.Core.Encryption;
using Socks5.Core.Socks;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks5Client
{
    public class Socks
    {
        public static AuthTypes Greet(Client client, IList<AuthTypes> supportedAuthTypes = null)
        {
            if (supportedAuthTypes == null)
            {
                supportedAuthTypes = new List<AuthTypes> { AuthTypes.None, AuthTypes.Login, AuthTypes.SocksEncrypt };
            }

            // https://www.ietf.org/rfc/rfc1928.txt [Page 3]
            var bytes = new byte[supportedAuthTypes.Count + 2];
            bytes[0] = 0x05; // protocol version - socks5
            bytes[1] = (byte)supportedAuthTypes.Count;
            for (var i = 0; i < supportedAuthTypes.Count; i++) bytes[i + 2] = (byte)supportedAuthTypes[i];
            client.Send(bytes);

            var buffer = new byte[512];
            var received = client.Receive(buffer, 0, buffer.Length);
            if (received > 0)
            {
                // Check for server version.
                if (buffer[0] == 0x05)
                {
                    return (AuthTypes)buffer[1];
                }
            }

            return 0;
        }


        public static int SendLogin(Client cli, string username, string password)
        {
            var x = new byte[username.Length + password.Length + 3];
            var total = 0;
            x[total++] = 0x01;
            x[total++] = Convert.ToByte(username.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(username), 0, x, 2, username.Length);
            total += username.Length;
            x[total++] = Convert.ToByte(password.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(password), 0, x, total, password.Length);
            // Send request.
            cli.Send(x);
            var buffer = new byte[512];
            cli.Receive(buffer, 0, buffer.Length);

            switch (buffer[1])
            {
                case 0x00:
                    return 1;
                case 0xFF:
                    return 0;
                default:
                    return 0;
            }
        }


        public static SocksError SendRequest(Client cli, SocksEncryption enc, string ipOrDomain, int port)
        {
            AddressType type;
            // It's a domain. :D (hopefully).
            type = !IPAddress.TryParse(ipOrDomain, out _) ? AddressType.Domain : AddressType.Ip;
            var sr = new SocksRequest(StreamTypes.Stream, type, ipOrDomain, port);
            // Send data.
            var p = sr.GetData(false);
            p[1] = 0x01;
            // Process data.
            var encOutput = enc.ProcessOutputData(p, 0, p.Length);
            if (encOutput == null)
            {
                throw new ArgumentNullException(nameof(encOutput));
            }
            cli.Send(encOutput);
            var buffer = new byte[512];
            // Process input data.
            var recv = cli.Receive(buffer, 0, buffer.Length);
            if (recv == -1) return SocksError.Failure;
            var buff = enc.ProcessInputData(buffer, 0, recv);
            if (buff == null)
            {
                throw new ArgumentNullException(nameof(buff));
            }

            return (SocksError)buff[1];
        }


        public static bool DoSocksAuth(Socks5Client client, string username, string password)
        {
            if (client.Client == null)
            {
                throw new ArgumentNullException(nameof(client.Client));
            }

            var auth = Greet(client.Client, client.UseAuthTypes);
            if (auth == AuthTypes.Unsupported)
            {
                client.Client.Disconnect();
                return false;
            }

            client.enc = new SocksEncryption();
            if (auth != AuthTypes.None)
            {
                switch (auth)
                {
                    case AuthTypes.Login:
                        // Logged in.
                        client.enc.SetType(AuthTypes.Login);
                        if (username == null)
                        {
                            throw new ArgumentNullException(nameof(username));
                        }
                        if (password == null)
                        {
                            throw new ArgumentNullException(nameof(password));
                        }
                        break;
                    case AuthTypes.SocksBoth:
                        //socksboth.
                        client.enc.SetType(AuthTypes.SocksBoth);
                        client.enc.GenerateKeys();
                        //send public key.
                        client.Client.Send(client.enc.GetPublicKey());
                        //now receive key.

                        var buffer = new byte[4096];
                        var keysize = client.Client.Receive(buffer, 0, buffer.Length);
                        client.enc.SetKey(buffer, 0, keysize);
                        //let them know we got it
                        //now receive our encryption key.
                        var enckeysize = client.Client.Receive(buffer, 0, buffer.Length);
                        //decrypt with our public key.
                        var newkey = new byte[enckeysize];
                        Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
                        client.enc.SetEncKey(client.enc.key.Decrypt(newkey, false));
                        //now we share our encryption key.
                        client.Client.Send(client.enc.ShareEncryptionKey());

                        break;
                    case AuthTypes.SocksEncrypt:
                        client.enc.SetType(AuthTypes.SocksEncrypt);
                        client.enc.GenerateKeys();
                        //send public key.
                        client.Client.Send(client.enc.GetPublicKey());
                        //now receive key.

                        buffer = new byte[4096];
                        keysize = client.Client.Receive(buffer, 0, buffer.Length);
                        client.enc.SetKey(buffer, 0, keysize);
                        //now receive our encryption key.
                        enckeysize = client.Client.Receive(buffer, 0, buffer.Length);
                        //decrypt with our public key.
                        newkey = new byte[enckeysize];
                        Buffer.BlockCopy(buffer, 0, newkey, 0, enckeysize);
                        client.enc.SetEncKey(client.enc.key.Decrypt(newkey, false));
                        //now we share our encryption key.

                        client.Client.Send(client.enc.ShareEncryptionKey());

                        //socksencrypt.
                        break;
                    case AuthTypes.SocksCompress:
                        client.enc.SetType(AuthTypes.SocksCompress);
                        //sockscompress.
                        break;
                    default:
                        client.Client.Disconnect();
                        return false;
                }

                if (client.enc.GetAuthType() != AuthTypes.Login)
                {
                    //now receive login params.
                    var buff = new byte[1024];
                    var recv = client.Client.Receive(buff, 0, buff.Length);
                    //check for 
                    if (recv > 0)
                    {
                        //check if socks5 version is 5
                        if (buff[0] == 0x05)
                        {
                            //good.
                            if (buff[1] == (byte)AuthTypes.Login)
                            {
                                if (username == null || password == null)
                                {
                                    client.Client.Sock.Close();
                                    return false;
                                }

                                var ret = SendLogin(client.Client, username, password);
                                if (ret != 1)
                                {
                                    client.Client.Sock.Close();
                                    return false;
                                }
                            }
                            else
                            {
                                //idk? close for now.
                                client.Client.Disconnect();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        client.Client.Disconnect();
                        return false;
                    }
                }
                else
                {
                    if (username == null || password == null)
                    {
                        client.Client.Sock.Close();
                        return false;
                    }

                    var ret = SendLogin(client.Client, username, password);
                    if (ret != 1)
                    {
                        client.Client.Sock.Close();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

