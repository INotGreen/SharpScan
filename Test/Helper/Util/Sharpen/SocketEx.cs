// SocketEx.cs implementation by J. Arturo <webmaster at komodosoft dot net>
//  
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharpCifs.Util.Sharpen
{
    /// <summary>
    /// Extended Socket
    /// </summary>
    /// <remarks>
    /// System.Net.Scokets.Socket
    /// https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket?view=netcore-1.1
    /// </remarks>
    public class SocketEx : Socket
    {
        private int _soTimeOut = -1;

        public int SoTimeOut
        {
            get
            {
                return _soTimeOut;
            }

            set
            {
                if (value > 0)
                {
                    _soTimeOut = value;
                }
                else
                {
                    _soTimeOut = -1;
                }

            }
        }

        public SocketEx(AddressFamily addressFamily, 
                        SocketType socketType, 
                        ProtocolType protocolType)
            : base(addressFamily, socketType, protocolType)
        {
        }

        public void Connect(IPEndPoint endPoint, int timeOut)
        {
            using (var evt = new ManualResetEventSlim(false))
            {
                using (var args = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = endPoint
                })
                {
                    var isEvtSetted = false;

                    args.Completed += delegate
                    {
                        if (!isEvtSetted)
                        {
                            evt.Set();
                            isEvtSetted = true;
                        }
                    };

                    if (ConnectAsync(args))
                    {
                        //asynchronous action.
                        if (!evt.Wait(timeOut))
                        {
                            ConnectAsync(args);
                            throw new ConnectException("Can't connect to end point.");
                        }

                        if (args.SocketError != SocketError.Success)
                        {
                            throw new ConnectException("Can't connect to end point.");
                        }
                    }
                    else
                    {
                        //synchronous action, and it completed.
                        //evt.Completed event not be raised.
                        //https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.connectasync?view=netcore-1.1#System_Net_Sockets_Socket_ConnectAsync_System_Net_Sockets_SocketAsyncEventArgs_
                    }

                    if (!isEvtSetted)
                    {
                        evt.Set();
                        isEvtSetted = true;
                    }
                }
            }
        }

        public void Bind2(EndPoint ep)
        {
            if (ep == null)
                Bind(new IPEndPoint(IPAddress.Any, 0));
            else
                Bind(ep);
        }


        public int Receive(byte[] buffer, int offset, int count)
        {
            using (var evt = new ManualResetEventSlim(false))
            {
                using (var args = new SocketAsyncEventArgs
                {
                    UserToken = this
                })
                {
                    var isEvtSetted = false;

                    args.SetBuffer(buffer, offset, count);

                    args.Completed += delegate
                    {
                        if (!isEvtSetted)
                        {
                            evt.Set();
                            isEvtSetted = true;
                        }
                    };

                    if (ReceiveAsync(args))
                    {
                        //asynchronous action.
                        if (!evt.Wait(_soTimeOut))
                            throw new TimeoutException("No data received.");
                    }
                    else
                    {
                        //synchronous action, and it completed.
                        //evt.Completed event not be raised.
                        //https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.receiveasync?view=netcore-1.1#System_Net_Sockets_Socket_ReceiveAsync_System_Net_Sockets_SocketAsyncEventArgs_
                    }

                    if (!isEvtSetted)
                    {
                        evt.Set();
                        isEvtSetted = true;
                    }

                    return args.BytesTransferred;
                }
            }
        }

        public void Send(byte[] buffer, int offset, int length, EndPoint destination = null)
        {
            using (var evt = new ManualResetEventSlim(false))
            {
                using (SocketAsyncEventArgs args = new SocketAsyncEventArgs
                {
                    UserToken = this
                })
                {
                    var isEvtSetted = false;

                    args.SetBuffer(buffer, offset, length);

                    args.Completed += delegate
                    {
                        if (!isEvtSetted)
                        {
                            evt.Set();
                            isEvtSetted = true;
                        }
                    };

                    args.RemoteEndPoint = destination ?? RemoteEndPoint;

                    if (SendToAsync(args))
                    {
                        //asynchronous action.
                        if (!evt.Wait(_soTimeOut))
                            throw new TimeoutException("No data sent.");
                    }
                    else
                    {
                        //synchronous action, and it completed.
                        //evt.Completed event not be raised.
                        //https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.sendasync?view=netcore-1.1#System_Net_Sockets_Socket_SendAsync_System_Net_Sockets_SocketAsyncEventArgs_
                    }

                    if (!isEvtSetted)
                    {
                        evt.Set();
                        isEvtSetted = true;
                    }
                }
            }
        }

        public InputStream GetInputStream()
        {
            return new NetworkStream(this);
        }

        public OutputStream GetOutputStream()
        {
            return new NetworkStream(this);
        }

    }
}
