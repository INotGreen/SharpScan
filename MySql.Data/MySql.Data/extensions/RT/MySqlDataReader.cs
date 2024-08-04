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

namespace MySql.Data.MySqlClient
{
  public sealed partial class MySqlDataReader : RTDataReader
  {
  }

  public abstract class RTDataReader
  {
    public abstract int FieldCount { get; }
    public abstract bool HasRows { get; }
    public abstract bool IsClosed { get; }
    public abstract int RecordsAffected { get; }
    public abstract object this[int i] { get; }
    public abstract object this[String name] { get; }

    public abstract void Close();
    public abstract bool GetBoolean(int i);
    public abstract byte GetByte(int i);
    public abstract char GetChar(int i);
    public abstract decimal GetDecimal(int i);
    public abstract double GetDouble(int i);
    public abstract float GetFloat(int i);
    public abstract Guid GetGuid(int i);
    public abstract Int16 GetInt16(int i);
    public abstract Int32 GetInt32(int i);
    public abstract Int64 GetInt64(int i);
    public abstract string GetString(int i);
    public abstract object GetValue(int i);
    public abstract int GetValues(object[] values);

    public abstract long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);
    public abstract long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);
    public abstract string GetDataTypeName(int ordinal);
    public abstract DateTime GetDateTime(int ordinal);
    public abstract Type GetFieldType(int ordinal);
    public abstract string GetName(int ordinal);
    public abstract int GetOrdinal(string name);
    public abstract bool IsDBNull(int i);
    public abstract bool NextResult();
    public abstract bool Read();
  }
}
