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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Socks5.Core.TCP
{
    public class Client
    {
        private int _packetSize = 4096;
        private byte[] _buffer;
        private bool _disposed;
        public bool Receiving;

        public Client(Socket sock, int packetSize)
        {
            //start the data exchange.
            Sock = sock;
            OnClientDisconnected = delegate { };
            _buffer = new byte[packetSize];
            _packetSize = packetSize;
            sock.ReceiveBufferSize = packetSize;
        }

        public Socket Sock { get; set; }
        public event EventHandler<ClientEventArgs> OnClientDisconnected;

        public event EventHandler<DataEventArgs> OnDataReceived = delegate { };
        public event EventHandler<DataEventArgs> OnDataSent = delegate { };

        private void DataReceived(IAsyncResult res)
        {
            Receiving = false;
            try
            {
                var error = SocketError.Success;
                if (_disposed || res.AsyncState is null)
                {
                    return;
                }
                var received = ((Socket)res.AsyncState).EndReceive(res, out error);
                if (received <= 0 || error != SocketError.Success)
                {
                    Disconnect();
                    return;
                }

                var data = new DataEventArgs(this, _buffer, received);
                OnDataReceived(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
#endif
                Disconnect();
            }
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {
                var received = Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
                    Disconnect();
                    return -1;
                }

                return received;
            }
            catch (Exception ex)
            {
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
                Disconnect();
                return -1;
            }
        }

        private const int MaxStackSize = 300;
        private int _currentStackSize = 0;
        public void StartReceiveAsync(int bufferSize = -1)
        {
            if (++_currentStackSize > MaxStackSize)
            {
                _currentStackSize = 0;
                ThreadPool.QueueUserWorkItem(ReceiveAsync, bufferSize);
                return;
            }

            ReceiveAsync(bufferSize);
        }

        private void ReceiveAsync(object obj)
        {
            try
            {
                var bufferSize = (int)(obj ?? -1);
                if (bufferSize > -1) _buffer = new byte[bufferSize];
                Receiving = true;
                Sock.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataReceived, Sock);
            }
            catch (Exception ex)
            {
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
                Disconnect();
            }
        }


        public void Disconnect()
        {
            try
            {
                //while (Receiving) Thread.Sleep(10);
                if (!_disposed)
                {
                    if (Sock != null && Sock.Connected)
                    {
                        OnClientDisconnected(this, new ClientEventArgs(this));
                        Sock.Shutdown(SocketShutdown.Both);
                        Sock.Close();
                        //this.Sock = null;
                        return;
                    }

                    OnClientDisconnected(this, new ClientEventArgs(this));

                    Dispose();
                }
            }
            catch
            {
            }
        }

        private void DataSent(IAsyncResult res)
        {
            if (res.AsyncState == null)
            {
                throw new ArgumentNullException(nameof(res.AsyncState));
            }
            
            try
            {
                var sent = ((Socket)res.AsyncState).EndSend(res);
                if (sent < 0)
                {
                    Sock.Shutdown(SocketShutdown.Both);
                    Sock.Close();
                    return;
                }

                var data = new DataEventArgs(this, new byte[0] { }, sent);
                OnDataSent(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
            }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock != null && Sock.Connected) Sock.BeginSend(buff, offset, count, SocketFlags.None, DataSent, Sock);
            }
            catch (Exception ex)
            {
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
                Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock != null)
                {
                    if (Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                    {
                        Disconnect();
                        return false;
                    }

                    var data = new DataEventArgs(this, buff, count);
                    OnDataSent(this, data);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
#if DEBUG
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
#endif
                Disconnect();
                return false;
            }
        }

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
                //Sock = null;
                //buffer = null;
                //OnClientDisconnected = null;
                //OnDataReceived = null;
                //OnDataSent = null;
            }

            // Free any unmanaged objects here. 
            //
        }
    }
}

// public class Client
// {
//     private int _packetSize = 4096;
//     private byte[] _buffer;
//     private bool _disposed;
//     public Socket Sock { get; set; }
//     public event EventHandler<ClientEventArgs> OnClientDisconnected;
//
//     public event EventHandler<DataEventArgs> OnDataReceived = delegate { };
//     public event EventHandler<DataEventArgs> OnDataSent = delegate { };
//     public Client(Socket sock, int packetSize = 4096)
//     {
//         _packetSize = packetSize;
//         Sock = sock;
//         _buffer = new byte[_packetSize];
//         sock.ReceiveBufferSize = _packetSize;
//     }
//
//     public async Task Receive()
//     {
//         var received = await Sock.ReceiveAsync(_buffer, SocketFlags.None);
//     }
//     
//     public void Dispose()
//     {
//         Dispose(true);
//         GC.SuppressFinalize(this);
//     }
//     
//     protected virtual void Dispose(bool disposing)
//     {
//         if (_disposed)
//         {
//             return;
//         }
//          
//         _disposed = true;
//
//         if (disposing)
//         {
//             Sock = null;
//             _buffer = null;
//             OnClientDisconnected = null;
//             OnDataReceived = null;
//             OnDataSent = null;
//         }
//     }
// }

