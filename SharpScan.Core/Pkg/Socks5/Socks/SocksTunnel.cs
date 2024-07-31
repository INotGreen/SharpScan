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
using Socks5.Core.Plugin;
using Socks5.Core.TCP;

namespace Socks5.Core.Socks {



    internal class SocksTunnel
    {
        private readonly int _packetSize = 4096;

        private readonly List<DataHandler> _plugins = new List<DataHandler>();
        public SocksClient Client;
        private bool _disconnected;
        public SocksRequest ModifiedReq;
        public Client RemoteClient;
        public SocksRequest Req;

        private SocketAsyncEventArgs _socketArgs;

        private int _timeout = 10000;

        public SocksTunnel(SocksClient p, SocksRequest req, SocksRequest req1, int packetSize, int timeout)
        {
            RemoteClient = new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                      _packetSize);
            Client = p;
            Req = req;
            ModifiedReq = req1;
            _packetSize = packetSize;
            _timeout = timeout;
        }

        public void Open(IPAddress outbound)
        {
            if (ModifiedReq.Address == null || ModifiedReq.Port <= -1)
            {
                Client.Client.Disconnect();
                return;
            }
#if DEBUG
        //Console.WriteLine("{0}:{1}", ModifiedReq.Address, ModifiedReq.Port);
#endif
            foreach (ConnectSocketOverrideHandler conn in PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)))
            {
                if (conn.Enabled)
                {
                    var pm = conn.OnConnectOverride(ModifiedReq);
                    if (pm != null && pm.Sock != null && pm.Sock.Connected)
                    //check if it's connected.
                    {
                        RemoteClient = pm;
                        //send request right here.
                        var data = Req.GetData(true);
                        data[1] = 0x00;
                        //gucci let's go.
                        Client.Client.Send(data);
                        ConnectHandler();
                        return;
                    }
                }
            }

            if (ModifiedReq.Error != SocksError.Granted)
            {
                Client.Client.Send(Req.GetData(true));
                Client.Client.Disconnect();
                return;
            }
            if (ModifiedReq.Ip == null)
            {
                throw new ArgumentNullException(nameof(ModifiedReq.Ip));
            }
       

            _socketArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(ModifiedReq.Ip, ModifiedReq.Port)
            };
            _socketArgs.Completed += socketArgs_Completed;
            RemoteClient.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RemoteClient.Sock.Bind(new IPEndPoint(outbound, 0));
            if (!RemoteClient.Sock.ConnectAsync(_socketArgs))
            {
                ConnectHandler();
            }
        }

        private void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            var request = Req.GetData(true); // Client.Client.Send(Req.GetData());
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine("Error while connecting: {0}", e.SocketError.ToString());
                request[1] = (byte)SocksError.Unreachable;
            }
            else
            {
                request[1] = 0x00;
            }

            Client.Client.Send(request);

            if (_socketArgs != null)
            {
                _socketArgs.Completed -= socketArgs_Completed;
                _socketArgs.Dispose();
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    //connected;
                    ConnectHandler();
                    break;
            }
        }

        private void ConnectHandler()
        {
            //start receiving from both endpoints.
            try
            {
                //all plugins get the event thrown.
                foreach (DataHandler data in PluginLoader.LoadPlugin(typeof(DataHandler)))
                {
                    _plugins.Push(data);
                }

                Client.Client.OnDataReceived += Client_onDataReceived;
                RemoteClient.OnDataReceived += RemoteClient_onDataReceived;

                Client.Client.OnClientDisconnected += Client_onClientDisconnected;
                RemoteClient.OnClientDisconnected += RemoteClient_onClientDisconnected;

                Client.Client.StartReceiveAsync();
                RemoteClient.StartReceiveAsync();
            }
            catch
            {
                RemoteClient.Disconnect();
                Client.Client.Disconnect();
            }
        }

        private void Client_onClientDisconnected(object sender, ClientEventArgs e)
        {
            if (_disconnected)
            {
                return;
            }
            //Console.WriteLine("Client DC'd");
            _disconnected = true;

            RemoteClient.OnDataReceived -= RemoteClient_onDataReceived;
            RemoteClient.OnClientDisconnected -= RemoteClient_onClientDisconnected;
            RemoteClient.Disconnect();
        }

        private void RemoteClient_onClientDisconnected(object sender, ClientEventArgs e)
        {
#if DEBUG
        //Console.WriteLine("Remote DC'd");
#endif
            if (_disconnected) return;
            //Console.WriteLine("Remote DC'd");
            _disconnected = true;
            Client.Client.OnDataReceived -= Client_onDataReceived;
            Client.Client.OnClientDisconnected -= Client_onClientDisconnected;
            Client.Client.Disconnect();
        }

        private void RemoteClient_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = ModifiedReq;
            foreach (var f in _plugins)
                f.OnServerDataReceived(this, e);
            if (e.Count > 0)
                Client.Client.Send(e.Buffer, e.Offset, e.Count);
            if (!RemoteClient.Receiving)
                RemoteClient.StartReceiveAsync();
        }

        private void Client_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = ModifiedReq;
            foreach (var f in _plugins)
                f.OnClientDataReceived(this, e);
            if (e.Count > 0)
                RemoteClient.Send(e.Buffer, e.Offset, e.Count);
            if (!Client.Client.Receiving)
                Client.Client.StartReceiveAsync();
        }
    }
}

