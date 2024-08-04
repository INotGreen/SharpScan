// Copyright (c) 2004-2007 MySQL AB
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for MySqlSocket.
	/// </summary>
	internal sealed class SocketStream : Stream
	{
		private Socket	socket;

		public SocketStream(AddressFamily addressFamily, SocketType socketType, ProtocolType protocol)
		{
			socket = new Socket(addressFamily, socketType, protocol);
		}

		#region Properties

		public Socket Socket 
		{
			get { return socket; }
		}

		public override bool CanRead
		{
			get	{ return true;	}
		}

		public override bool CanSeek
		{
			get	{ return false;	}
		}

		public override bool CanWrite
		{
			get	{ return true; }
		}

		public override long Length
		{
			get	{ return 0;	}
		}

		public override long Position
		{
			get	{ return 0;	}
			set	{ throw new NotSupportedException("SocketStream does not support seek"); }
		}

		#endregion

		#region Stream Implementation

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return socket.Receive(buffer, offset, count, SocketFlags.None);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("SocketStream does not support seek");
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			socket.Send(buffer, offset, count, SocketFlags.None);
		}


		#endregion

		public bool Connect(EndPoint remoteEP, int timeout)
		{
            // set the socket to non blocking
            socket.Blocking = false;

			// then we star the connect
			SocketAddress addr = remoteEP.Serialize();
			byte[] buff = new byte[addr.Size];
			for (int i=0; i<addr.Size; i++)
				buff[i] = addr[i];

			NativeMethods.connect(socket.Handle, buff, addr.Size);
            int wsaerror = NativeMethods.WSAGetLastError();
            if (wsaerror != 10035)
            {
                //  this is probably an IPV6 address
                if (wsaerror == 10047)
                    return false;
                throw new SocketException(wsaerror);
            }

			// next we wait for our connect timeout or until the socket is connected
			ArrayList write = new ArrayList();
			write.Add(socket);
			ArrayList error = new ArrayList();
			error.Add(socket);

			Socket.Select(null, write, error, timeout*1000*1000);

			if (write.Count == 0) return false;

			// set socket back to blocking mode
            socket.Blocking = true;
			return true;
		}


	}
}
