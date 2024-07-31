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
using Socks5.Core.Plugin;
using Socks5.Core.TCP;
using Socks5.Core.Socks;
using Socks5.Core;

internal class SocksSpecialTunnel
{
    private readonly int _packetSize = 4096;

    private readonly List<DataHandler> _plugins = new List<DataHandler>();
    private readonly SocksEncryption _se;
    public SocksClient Client;
    private bool _disconnected;
    public SocksRequest ModifiedReq;
    public Client RemoteClient;
    public SocksRequest Req;

    private int _timeout;

    public SocksSpecialTunnel(SocksClient p, SocksEncryption ph, SocksRequest req, SocksRequest req1, int packetSize,
                              int timeout = 10000)
    {
        RemoteClient = new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                  _packetSize);
        Client = p;
        Req = req;
        ModifiedReq = req1;
        _packetSize = packetSize;
        _timeout = timeout;
        _se = ph;
    }

    public void Open(IPAddress outboundIp)
    {
        if (ModifiedReq.Port <= -1)
        {
            Client.Client.Disconnect();
            return;
        }
#if DEBUG
        Console.WriteLine("{0}:{1}", ModifiedReq.Address, ModifiedReq.Port);
#endif
        foreach (ConnectSocketOverrideHandler conn in PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)))
        {
            var pm = conn.OnConnectOverride(ModifiedReq);
            if (!pm.Sock.Connected)
            {
                continue;
            }

            RemoteClient = pm;
            //send request right here.
            var shit = Req.GetData(true);
            shit[1] = 0x00;
            //process packet.
            var output = _se.ProcessOutputData(shit, 0, shit.Length);
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
           
            //gucci let's go.
            Client.Client.Send(output);
            ConnectHandler(new SocketAsyncEventArgs());
            return;
        }

        
        if (ModifiedReq.Ip == null)
        {
            throw new ArgumentNullException(nameof(ModifiedReq.Ip));
        }
        var socketArgs = new SocketAsyncEventArgs { RemoteEndPoint = new IPEndPoint(ModifiedReq.Ip, ModifiedReq.Port) };
        socketArgs.Completed += socketArgs_Completed;
        RemoteClient.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        RemoteClient.Sock.Bind(new IPEndPoint(outboundIp, 0));
        if (!RemoteClient.Sock.ConnectAsync(socketArgs))
        {
            ConnectHandler(socketArgs);
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

        var encreq = _se.ProcessOutputData(request, 0, request.Length);
        if (encreq != null)
        {
            Client.Client.Send(encreq);
        }

        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Connect:
                //connected;
                ConnectHandler(e);
                break;
        }
    }

    private void ConnectHandler(SocketAsyncEventArgs e)
    {
        //start receiving from both endpoints.
        try
        {
            //all plugins get the event thrown.
            Client.Client.Sock.ReceiveBufferSize = 4200;
            Client.Client.Sock.SendBufferSize = 4200;
            foreach (DataHandler data in PluginLoader.LoadPlugin(typeof(DataHandler)))
            {
                _plugins.Push(data);
            }
                
            Client.Client.OnDataReceived += Client_onDataReceived;
            RemoteClient.OnDataReceived += RemoteClient_onDataReceived;
            RemoteClient.OnClientDisconnected += RemoteClient_onClientDisconnected;
            Client.Client.OnClientDisconnected += Client_onClientDisconnected;
            Client.Client.StartReceiveAsync();
            RemoteClient.StartReceiveAsync();
        }
        catch
        {
        }
    }

    private void Client_onClientDisconnected(object sender, ClientEventArgs e)
    {
#if DEBUG
        //Console.WriteLine("Client DC'd");
#endif
        if (_disconnected) return;
        _disconnected = true;
        RemoteClient.Disconnect();
    }

    private void RemoteClient_onClientDisconnected(object sender, ClientEventArgs e)
    {
#if DEBUG
        //Console.WriteLine("Remote DC'd");
#endif
        /* if (disconnected) return;
         disconnected = true;
         //Client.Client.Disconnect();
         disconnected = true;*/
    }

    private void RemoteClient_onDataReceived(object sender, DataEventArgs e)
    {
        e.Request = ModifiedReq;
        try
        {
            foreach (var f in _plugins)
                f.OnServerDataReceived(this, e);
            //craft headers & shit.
            if (e.Count > 0)
            {
                var outputData = _se.ProcessOutputData(e.Buffer, e.Offset, e.Count);
                if (outputData != null)
                {
                    var dataToSend = new byte[outputData.Length + 4];
                    Buffer.BlockCopy(outputData, 0, dataToSend, 4, outputData.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(outputData.Length), 0, dataToSend, 0, 4);
                    //send outputdata's length first.
                    Client.Client.Send(dataToSend);
                }
            }

            if (!RemoteClient.Receiving)
            {
                RemoteClient.StartReceiveAsync();
            }
        }
        catch
        {
            Client.Client.Disconnect();
            RemoteClient.Disconnect();
        }
    }

    private void Client_onDataReceived(object sender, DataEventArgs e)
    {
        e.Request = ModifiedReq;
        //this should be packet header.
        try
        {
            var packetSize = BitConverter.ToInt32(e.Buffer, 0);
            var newBuff = new byte[packetSize];
            //yey
            //process packet.
            var output = _se.ProcessInputData(e.Buffer, 4, packetSize);
            if (output != null)
            {
                e.Buffer = output;
                e.Offset = 0;
                e.Count = output.Length;
                //receive full packet.
                foreach (var f in _plugins)
                    f.OnClientDataReceived(this, e);
                if (e.Count > 0)
                {
                    RemoteClient.SendAsync(e.Buffer, e.Offset, e.Count);
                }
            }

            if (!Client.Client.Receiving)
            {
                Client.Client.StartReceiveAsync();
            }
        }
        catch
        {
            //disconnect.
            Client.Client.Disconnect();
            RemoteClient.Disconnect();
        }
    }
}
namespace Socks5.Core.Socks { }

