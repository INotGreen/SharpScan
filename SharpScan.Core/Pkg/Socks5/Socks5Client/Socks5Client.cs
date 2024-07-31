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
using System.Net.Sockets;
using Socks5.Core.Encryption;
using Socks5.Core.Socks;
using Socks5.Core.Socks5Client.Events;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks5Client
{
    public class Socks5Client
    {
        private readonly byte[] _halfReceiveBuffer = new byte[4200];
        public Client Client;
        private string _dest;
        private int _destport;

        public SocksEncryption enc = new SocksEncryption();
        private int _halfReceivedBufferLength;
        private IPAddress _ipAddress;

        private Socket _p;
        private string _password;
        private int _port;
        public bool reqPass;

        private string _username;

        private Socks5Client()
        {
            UseAuthTypes = new List<AuthTypes>(new[] { AuthTypes.None, AuthTypes.Login, AuthTypes.SocksEncrypt });
        }

        public Socks5Client(string ipOrDomain, int port, string dest, int destport, string username = null,
                            string password = null)
            : this()
        {
            //Parse IP?
            if (!IPAddress.TryParse(ipOrDomain, out _ipAddress))
            {
                //not connected.
                try
                {
                    foreach (var p in Dns.GetHostAddresses(ipOrDomain))
                    {
                        if (p.AddressFamily == AddressFamily.InterNetwork)
                        {
                            DoSocks(p, port, dest, destport, username, password);
                            return;
                        }
                    }
                }
                catch
                {
                    throw new NullReferenceException();
                }
            }

            DoSocks(_ipAddress, port, dest, destport, username, password);
        }

        public Socks5Client(IPAddress ip, int port, string dest, int destport, string username = null,
                            string password = null)
            : this()
        {
            DoSocks(ip, port, dest, destport, username, password);
        }

        public IList<AuthTypes> UseAuthTypes { get; set; }


        public bool Connected => Client != null ? Client.Sock.Connected : false;

        public event EventHandler<Socks5ClientArgs> OnConnected = delegate { };
        public event EventHandler<Socks5ClientDataArgs> OnDataReceived = delegate { };
        public event EventHandler<Socks5ClientDataArgs> OnDataSent = delegate { };
        public event EventHandler<Socks5ClientArgs> OnDisconnected = delegate { };

        private void DoSocks(IPAddress ip, int port, string dest, int destport, string username = null,
                             string password = null)
        {
            _ipAddress = ip;
            _port = port;
            //check for username & pw.
            if (username != null && password != null)
            {
                _username = username;
                _password = password;
                reqPass = true;
            }

            _dest = dest;
            _destport = destport;
        }

        public void ConnectAsync()
        {
            //
            if (_ipAddress == null)
            {
                throw new ArgumentNullException(nameof(_ipAddress));
            }

            _p = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client = new Client(_p, 4200);
            Client.OnClientDisconnected += Client_onClientDisconnected;
            Client.Sock.BeginConnect(new IPEndPoint(_ipAddress, _port), ClientOnConnected, Client);
            //return status?
        }

        private void Client_onClientDisconnected(object sender, ClientEventArgs e)
        {
            OnDisconnected(this, new Socks5ClientArgs(this, SocksError.Expired));
        }

        public bool Send(byte[] buffer, int offset, int length)
        {
            //buffer sending.
            var offst = 0;
            while (true)
            {
                var outputData = enc.ProcessOutputData(buffer, offst, length - offst > 4092 ? 4092 : length - offst);
                if (outputData is null)
                {
                    return false;
                }

                offst += length - offst > 4092 ? 4092 : length - offst;
                //craft headers & shit.
                //send outputdata's length first.
                if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                {
                    var dataToSend = new byte[outputData.Length + 4];
                    Buffer.BlockCopy(outputData, 0, dataToSend, 4, outputData.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(outputData.Length), 0, dataToSend, 0, 4);
                    outputData = dataToSend;
                }

                Client?.Send(outputData, 0, outputData.Length);
                if (offst >= buffer.Length)
                {
                    return true;
                }
            }
        }

        public bool Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length);
        }

        public int? Receive(byte[] buffer, int offset, int count)
        {
            //this should be packet header.
            try
            {
                if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                {
                    if (_halfReceivedBufferLength > 0)
                    {
                        if (_halfReceivedBufferLength <= count)
                        {
                            Buffer.BlockCopy(_halfReceiveBuffer, 0, buffer, offset, _halfReceivedBufferLength);
                            _halfReceivedBufferLength = 0;
                            return _halfReceivedBufferLength;
                        }

                        Buffer.BlockCopy(_halfReceiveBuffer, 0, buffer, offset, count);
                        _halfReceivedBufferLength = _halfReceivedBufferLength - count;
                        Buffer.BlockCopy(_halfReceiveBuffer, count, _halfReceiveBuffer, 0, count);

                        return count;
                    }

                    count = Math.Min(4200, count);

                    var databuf = new byte[4200];
                    var got = Client?.Receive(databuf, 0, 4200);
                    if (got <= 0)
                    {
                        return 0;
                    }


                    var packetsize = BitConverter.ToInt32(databuf, 0);
                    var processed = enc.ProcessInputData(databuf, 4, packetsize);

                    Buffer.BlockCopy(databuf, 0, buffer, offset, count);
                    Buffer.BlockCopy(databuf, count, _halfReceiveBuffer, 0, packetsize - count);
                    _halfReceivedBufferLength = packetsize - count;
                    return count;
                }

                return Client?.Receive(buffer, offset, count);
            }
            catch (Exception)
            {
                //disconnect.
                Client?.Disconnect();
                throw;
            }
        }

        private void Client_onDataReceived(object sender, DataEventArgs e)
        {
            // This should be packet header.
            try
            {
                if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                {
                    // Get total number of bytes.
                    var torecv = BitConverter.ToInt32(e.Buffer, 0);
                    var newbuff = new byte[torecv];

                    var recv = Client != null ? Client.Receive(newbuff, 0, torecv) : (int?)null;
                    if (recv != null && recv.Value == torecv)
                    {
                        var output = enc.ProcessInputData(newbuff, 0, recv.Value);
                        if (output == null)
                        {
                            return;
                        }

                        // Receive full packet.
                        e.Buffer = output;
                        e.Offset = 0;
                        e.Count = output.Length;
                        OnDataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
                    }
                }
                else
                {
                    OnDataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
                }
            }
            catch (Exception)
            {
                // Disconnect.
                Client?.Disconnect();
                throw;
            }
        }


        public bool Connect()
        {
            if (_ipAddress == null)
            {
                throw new ArgumentNullException(nameof(_ipAddress));
            }
            if (_dest == null)
            {
                throw new ArgumentNullException(nameof(_dest));
            }

            try
            {
                _p = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client = new Client(_p, 65535);
                Client.Sock.Connect(new IPEndPoint(_ipAddress, _port));
                // Try the greeting.
                // Client.onDataReceived += Client_onDataReceived;
                if (Socks.DoSocksAuth(this, _username, _password))
                {
                    if (Socks.SendRequest(Client, enc, _dest, _destport) == SocksError.Granted)
                    {
                        Client.OnDataReceived += Client_onDataReceived;
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ClientOnConnected(IAsyncResult res)
        {
            if (res.AsyncState == null)
            {
                throw new ArgumentNullException(nameof(res.AsyncState));
            }
            Client = (Client)res.AsyncState;
            try
            {
                Client.Sock.EndConnect(res);
            }
            catch
            {
                OnConnected(this, new Socks5ClientArgs(this, SocksError.Failure));
                return;
            }

            if (Socks.DoSocksAuth(this, _username, _password))
            {
                if (_dest == null)
                {
                    throw new ArgumentNullException(nameof(_dest));
                }
                var sockError = Socks.SendRequest(Client, enc, _dest, _destport);
                Client.OnDataReceived += Client_onDataReceived;
                OnConnected(this, new Socks5ClientArgs(this, sockError));
            }
            else
            {
                OnConnected(this, new Socks5ClientArgs(this, SocksError.Failure));
            }
        }

        //send.
    }
}

