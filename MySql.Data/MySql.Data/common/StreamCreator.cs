// Copyright (c) 2004-2008 MySQL AB, 2008-2009 Sun Microsystems, Inc.
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

using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Properties;
using System;
using System.IO;


namespace MySql.Data.Common
{
  /// <summary>
  /// Summary description for StreamCreator.
  /// </summary>
  internal class StreamCreator
  {
    string hostList;
    uint port;
    string pipeName;
    uint timeOut;
    uint keepalive;
    DBVersion driverVersion;

    public StreamCreator(string hosts, uint port, string pipeName, uint keepalive, DBVersion driverVersion)
    {
      hostList = hosts;
      if (hostList == null || hostList.Length == 0)
        hostList = "localhost";
      this.port = port;
      this.pipeName = pipeName;
      this.keepalive = keepalive;
      this.driverVersion = driverVersion;
    }

    public static Stream GetStream(string server, uint port, string pipename, uint keepalive, DBVersion v, uint timeout)
    {
      MySqlConnectionStringBuilder settings = new MySqlConnectionStringBuilder();
      settings.Server = server;
      settings.Port = port;
      settings.PipeName = pipename;
      settings.Keepalive = keepalive;
      settings.ConnectionTimeout = timeout;
      return GetStream(settings);
    }

    public static Stream GetStream(MySqlConnectionStringBuilder settings)
    {
      switch (settings.ConnectionProtocol)
      {
        case MySqlConnectionProtocol.Tcp: return GetTcpStream(settings);
#if RT
        case MySqlConnectionProtocol.UnixSocket: throw new NotImplementedException();
        case MySqlConnectionProtocol.SharedMemory: throw new NotImplementedException();
#else
        case MySqlConnectionProtocol.UnixSocket: return GetUnixSocketStream(settings);        
        case MySqlConnectionProtocol.SharedMemory: return GetSharedMemoryStream(settings);
        case MySqlConnectionProtocol.NamedPipe: return GetNamedPipeStream(settings);
#endif
      }
      throw new InvalidOperationException(Resources.UnknownConnectionProtocol);
    }

    private static Stream GetTcpStream(MySqlConnectionStringBuilder settings)
    {
      MyNetworkStream s = MyNetworkStream.CreateStream(settings, false);
      return s;
    }

#if !RT
    private static Stream GetUnixSocketStream(MySqlConnectionStringBuilder settings)
    {
      if (Platform.IsWindows())
        throw new InvalidOperationException(Resources.NoUnixSocketsOnWindows);

      MyNetworkStream s = MyNetworkStream.CreateStream(settings, true);
      return s;
    }

    private static Stream GetSharedMemoryStream(MySqlConnectionStringBuilder settings)
    {
      SharedMemoryStream str = new SharedMemoryStream(settings.SharedMemoryName);
      str.Open(settings.ConnectionTimeout);
      return str;
    }

    private static Stream GetNamedPipeStream(MySqlConnectionStringBuilder settings)
    {
      Stream stream = NamedPipeStream.Create(settings.PipeName, settings.Server, settings.ConnectionTimeout);
      return stream;
    }
#endif

  }
}
