// Copyright © 2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySql.Data.MySqlClient
{
  public delegate void StateChangeEventHandler(object sender, StateChangeEventArgs e);

  public sealed partial class MySqlConnection : RTConnection, ICloneable, IDisposable
  {
    public event StateChangeEventHandler StateChange;

    protected void OnStateChange(StateChangeEventArgs stateChange)
    {
      StateChangeEventHandler handler = StateChange;
      if (handler != null)
        handler(this, stateChange);
    }

    void IDisposable.Dispose()
    {
      if (State == ConnectionState.Open)
        Close();
    }

#if !RT
    ~MySqlConnection()
    {
      this.Dispose();
    }
#endif
  }

  public abstract class RTConnection
  {
    internal RTConnection() { }

    public abstract string DataSource { get; }
    public abstract int ConnectionTimeout { get; }
    public abstract string Database { get; }
    public abstract ConnectionState State { get; }
    public abstract string ServerVersion { get; }
    public abstract string ConnectionString { get; set;  }

    public abstract void ChangeDatabase(string database);
    public abstract void Open();
    public abstract void Close();
  }

  public enum ConnectionState
  {
    Closed,
    Open,
    Connecting,
    Executing,
    Fetching,
    Broken
  }

}
