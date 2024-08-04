// Copyright © 2004, 2018, Oracle and/or its affiliates. All rights reserved.
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
#if !RT
using System.Data;
using System.Data.Common;
#endif
using System.Collections;
using MySql.Data.Types;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient.Properties;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using System.Threading;

namespace MySql.Data.MySqlClient
{
  /// <include file='docs/MySqlDataReader.xml' path='docs/ClassSummary/*'/>
  public sealed partial class MySqlDataReader : IDisposable
  {
    // The DataReader should always be open when returned to the user.
    private bool isOpen = true;

    private CommandBehavior commandBehavior;
    private MySqlCommand command;
    internal long affectedRows;
    internal Driver driver;
    private PreparableStatement statement;
    private ResultSet resultSet;
    private bool disposed = false;

    // Used in special circumstances with stored procs to avoid exceptions from DbDataAdapter
    // If set, AffectedRows returns -1 instead of 0.
    private bool disableZeroAffectedRows;

    /* 
     * Keep track of the connection in order to implement the
     * CommandBehavior.CloseConnection flag. A null reference means
     * normal behavior (do not automatically close).
     */
    private MySqlConnection connection;

    /*
     * Because the user should not be able to directly create a 
     * DataReader object, the constructors are
     * marked as internal.
     */
    internal MySqlDataReader(MySqlCommand cmd, PreparableStatement statement, CommandBehavior behavior)
    {
      this.command = cmd;
      connection = (MySqlConnection)command.Connection;
      commandBehavior = behavior;
      driver = connection.driver;
      affectedRows = -1;
      this.statement = statement;

#if !RT
      if (cmd.CommandType == CommandType.StoredProcedure
        && cmd.UpdatedRowSource == UpdateRowSource.FirstReturnedRecord
      )
      {
        disableZeroAffectedRows = true;
      }
#endif
    }

    #region Properties

    internal PreparableStatement Statement
    {
      get { return statement; }
    }

    internal MySqlCommand Command
    {
      get { return command; }
    }

    internal ResultSet ResultSet
    {
      get { return resultSet; }
    }

    internal CommandBehavior CommandBehavior
    {
      get { return commandBehavior; }
    }

    /// <summary>
    /// Gets the number of columns in the current row.
    /// </summary>
    public override int FieldCount
    {
      get { return resultSet == null ? 0 : resultSet.Size; }
    }

    /// <summary>
    /// Gets a value indicating whether the MySqlDataReader contains one or more rows.
    /// </summary>
    public override bool HasRows
    {
      get { return resultSet == null ? false : resultSet.HasRows; }
    }

    /// <summary>
    /// Gets a value indicating whether the data reader is closed.
    /// </summary>
    public override bool IsClosed
    {
      get { return !isOpen; }
    }

    /// <summary>
    /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
    /// </summary>
    public override int RecordsAffected
    {
      // RecordsAffected returns the number of rows affected in batch
      // statments from insert/delete/update statments.  This property
      // is not completely accurate until .Close() has been called.
      get
      {

        if (disableZeroAffectedRows)
        {
          // In special case of updating stored procedure called from 
          // within data adapter, we return -1 to avoid exceptions 
          // (s. Bug#54895)
          if (affectedRows == 0)
            return -1;

        }

        return (int)affectedRows;
      }
    }

    /// <summary>
    /// Overloaded. Gets the value of a column in its native format.
    /// In C#, this property is the indexer for the MySqlDataReader class.
    /// </summary>
    public override object this[int i]
    {
      get { return GetValue(i); }
    }

    /// <summary>
    /// Gets the value of a column in its native format.
    ///	[C#] In C#, this property is the indexer for the MySqlDataReader class.
    /// </summary>
    public override object this[String name]
    {
      // Look up the ordinal and return 
      // the value at that position.
      get { return this[GetOrdinal(name)]; }
    }

    #endregion

    /// <summary>
    /// Closes the MySqlDataReader object.
    /// </summary>
    public override void Close()
    {
      if (!isOpen) return;

      bool shouldCloseConnection = (commandBehavior & CommandBehavior.CloseConnection) != 0;
      CommandBehavior originalBehavior = commandBehavior;

      // clear all remaining resultsets
      try
      {
        // Temporarily change to Default behavior to allow NextResult to finish properly.
        if (!originalBehavior.Equals(CommandBehavior.SchemaOnly))
          commandBehavior = CommandBehavior.Default;
        while (NextResult()) { }
      }
      catch (MySqlException ex)
      {
        // Ignore aborted queries
        if (!ex.IsQueryAborted)
        {
          // ignore IO exceptions.
          // We are closing or disposing reader, and  do not
          // want exception to be propagated to used. If socket is
          // is closed on the server side, next query will run into
          // IO exception. If reader is closed by GC, we also would 
          // like to avoid any exception here. 
          bool isIOException = false;
          for (Exception exception = ex; exception != null;
            exception = exception.InnerException)
          {
            if (exception is System.IO.IOException)
            {
              isIOException = true;
              break;
            }
          }
          if (!isIOException)
          {
            // Ordinary exception (neither IO nor query aborted)
            throw;
          }
        }
      }
      catch (System.IO.IOException)
      {
        // eat, on the same reason we eat IO exceptions wrapped into 
        // MySqlExceptions reasons, described above.
      }
      finally
      {
        // always ensure internal reader is null (Bug #55558)
        connection.Reader = null;
        commandBehavior = originalBehavior;
      }
      // we now give the command a chance to terminate.  In the case of
      // stored procedures it needs to update out and inout parameters
      command.Close(this);
      commandBehavior = CommandBehavior.Default;

      if (this.command.Canceled && connection.driver.Version.isAtLeast(5, 1, 0))
      {
        // Issue dummy command to clear kill flag
        ClearKillFlag();
      }

      if (shouldCloseConnection)
        connection.Close();

      command = null;
      connection.IsInUse = false;
      connection = null;
      isOpen = false;
    }

    #region TypeSafe Accessors

    /// <summary>
    /// Gets the value of the specified column as a Boolean.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBoolean(string name)
    {
      return GetBoolean(GetOrdinal(name));
    }

    /// <summary>
    /// Gets the value of the specified column as a Boolean.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override bool GetBoolean(int i)
    {
      return Convert.ToBoolean(GetValue(i));
    }

    /// <summary>
    /// Gets the value of the specified column as a byte.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte GetByte(string name)
    {
      return GetByte(GetOrdinal(name));
    }

    /// <summary>
    /// Gets the value of the specified column as a byte.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override byte GetByte(int i)
    {
      IMySqlValue v = GetFieldValue(i, false);
      if (v is MySqlUByte)
        return ((MySqlUByte)v).Value;
      else
        return (byte)((MySqlByte)v).Value;
    }

    /// <summary>
    /// Gets the value of the specified column as a sbyte.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public sbyte GetSByte(string name)
    {
      return GetSByte(GetOrdinal(name));
    }

    /// <summary>
    /// Gets the value of the specified column as a sbyte.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public sbyte GetSByte(int i)
    {
      IMySqlValue v = GetFieldValue(i, false);
      if (v is MySqlByte)
        return ((MySqlByte)v).Value;
      else
        return (sbyte)((MySqlByte)v).Value;
    }

    /// <summary>
    /// Reads a stream of bytes from the specified column offset into the buffer an array starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal. </param>
    /// <param name="fieldOffset">The index within the field from which to begin the read operation. </param>
    /// <param name="buffer">The buffer into which to read the stream of bytes. </param>
    /// <param name="bufferoffset">The index for buffer to begin the read operation. </param>
    /// <param name="length">The maximum length to copy into the buffer. </param>
    /// <returns>The actual number of bytes read.</returns>
    /// <include file='docs/MySqlDataReader.xml' path='MyDocs/MyMembers[@name="GetBytes"]/*'/>
    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      IMySqlValue val = GetFieldValue(i, false);

      if (!(val is MySqlBinary) && !(val is MySqlGuid))
        Throw(new MySqlException("GetBytes can only be called on binary or guid columns"));

      byte[] bytes = null;
      if (val is MySqlBinary)
        bytes = ((MySqlBinary)val).Value;
      else
        bytes = ((MySqlGuid)val).Bytes;

      if (buffer == null)
        return bytes.Length;

      if (bufferoffset >= buffer.Length || bufferoffset < 0)
        Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
      if (buffer.Length < (bufferoffset + length))
        Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
      if (fieldOffset < 0 ||
        ((ulong)fieldOffset >= (ulong)bytes.Length && (ulong)bytes.Length > 0))
        Throw(new IndexOutOfRangeException("Data index must be a valid index in the field"));

      // adjust the length so we don't run off the end
      if ((ulong)bytes.Length < (ulong)(fieldOffset + length))
      {
        length = (int)((ulong)bytes.Length - (ulong)fieldOffset);
      }

      Buffer.BlockCopy(bytes, (int)fieldOffset, buffer, (int)bufferoffset, (int)length);

      return length;
    }

    private object ChangeType(IMySqlValue value, int fieldIndex, Type newType)
    {
#if !RT
      resultSet.Fields[fieldIndex].AddTypeConversion(newType);
#endif
      return Convert.ChangeType(value.Value, newType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the value of the specified column as a single character.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public char GetChar(string name)
    {
      return GetChar(GetOrdinal(name));
    }

    /// <summary>
    /// Gets the value of the specified column as a single character.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override char GetChar(int i)
    {
      string s = GetString(i);
      return s[0];
    }

    /// <summary>
    /// Reads a stream of characters from the specified column offset into the buffer as an array starting at the given buffer offset.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="fieldoffset"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferoffset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      string valAsString = GetString(i);

      if (buffer == null) return valAsString.Length;

      if (bufferoffset >= buffer.Length || bufferoffset < 0)
        Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
      if (buffer.Length < (bufferoffset + length))
        Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
      if (fieldoffset < 0 || fieldoffset >= valAsString.Length)
        Throw(new IndexOutOfRangeException("Field offset must be a valid index in the field"));

      if (valAsString.Length < length)
        length = valAsString.Length;
      valAsString.CopyTo((int)fieldoffset, buffer, bufferoffset, length);
      return length;
    }

    /// <summary>
    /// Gets the name of the source data type.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override String GetDataTypeName(int i)
    {
      if (!isOpen)
        Throw(new Exception("No current query in data reader"));
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      // return the name of the type used on the backend
      IMySqlValue v = resultSet.Values[i];
      return v.MySqlTypeName;
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetMySqlDateTime/*'/>
    public MySqlDateTime GetMySqlDateTime(string column)
    {
      return GetMySqlDateTime(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetMySqlDateTime/*'/>
    public MySqlDateTime GetMySqlDateTime(int column)
    {
      return (MySqlDateTime)GetFieldValue(column, true);
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDateTimeS/*'/>
    public DateTime GetDateTime(string column)
    {
      return GetDateTime(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDateTime/*'/>
    public override DateTime GetDateTime(int i)
    {
      IMySqlValue val = GetFieldValue(i, true);
      MySqlDateTime dt;

      if (val is MySqlDateTime)
        dt = (MySqlDateTime)val;
      else
      {
        // we need to do this because functions like date_add return string
        string s = GetString(i);
        dt = MySqlDateTime.Parse(s);
      }

      dt.TimezoneOffset = driver.timeZoneOffset;
      if (connection.Settings.ConvertZeroDateTime && !dt.IsValidDateTime)
        return DateTime.MinValue;
      else
        return dt.GetDateTime();
    }

    public MySqlDecimal GetMySqlDecimal(string column)
    {
      return GetMySqlDecimal(GetOrdinal(column));
    }

    public MySqlDecimal GetMySqlDecimal(int i)
    {
      return (MySqlDecimal)GetFieldValue(i, false);
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDecimalS/*'/>
    public Decimal GetDecimal(string column)
    {
      return GetDecimal(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDecimal/*'/>
    public override Decimal GetDecimal(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlDecimal)
        return ((MySqlDecimal)v).Value;
      return Convert.ToDecimal(v.Value);
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDoubleS/*'/>
    public double GetDouble(string column)
    {
      return GetDouble(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetDouble/*'/>
    public override double GetDouble(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlDouble)
        return ((MySqlDouble)v).Value;
      return Convert.ToDouble(v.Value);
    }

    public Type GetFieldType(string column)
    {
      return GetFieldType(GetOrdinal(column));
    }

    /// <summary>
    /// Gets the Type that is the data type of the object.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override Type GetFieldType(int i)
    {
      if (!isOpen)
        Throw(new Exception("No current query in data reader"));
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      // we have to use the values array directly because we can't go through
      // GetValue
      IMySqlValue v = resultSet.Values[i];
      if (v is MySqlDateTime)
      {
        if (!connection.Settings.AllowZeroDateTime)
          return typeof(DateTime);
        return typeof(MySqlDateTime);
      }
      return v.SystemType;
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetFloatS/*'/>
    public float GetFloat(string column)
    {
      return GetFloat(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetFloat/*'/>
    public override float GetFloat(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlSingle)
        return ((MySqlSingle)v).Value;
      return Convert.ToSingle(v.Value);
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetGuidS/*'/>
    public Guid GetGuid(string column)
    {
      return GetGuid(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetGuid/*'/>
    public override Guid GetGuid(int i)
    {
      object v = GetValue(i);
      if (v is Guid)
        return (Guid)v;
      if (v is string)
        return new Guid(v as string);
      if (v is byte[])
      {
        byte[] bytes = (byte[])v;
        if (bytes.Length == 16)
          return new Guid(bytes);
      }
      Throw(new MySqlException(Resources.ValueNotSupportedForGuid));
      return Guid.Empty; // just to silence compiler
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt16S/*'/>
    public Int16 GetInt16(string column)
    {
      return GetInt16(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt16/*'/>
    public override Int16 GetInt16(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlInt16)
        return ((MySqlInt16)v).Value;

      return (short)ChangeType(v, i, typeof(short));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt32S/*'/>
    public Int32 GetInt32(string column)
    {
      return GetInt32(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt32/*'/>
    public override Int32 GetInt32(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlInt32)
        return ((MySqlInt32)v).Value;

      return (Int32)ChangeType(v, i, typeof(Int32));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt64S/*'/>
    public Int64 GetInt64(string column)
    {
      return GetInt64(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetInt64/*'/>
    public override Int64 GetInt64(int i)
    {
      IMySqlValue v = GetFieldValue(i, true);
      if (v is MySqlInt64)
        return ((MySqlInt64)v).Value;

      return (Int64)ChangeType(v, i, typeof(Int64));
    }

    /// <summary>
    /// Gets the name of the specified column.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override String GetName(int i)
    {
      if (!isOpen)
        Throw(new Exception("No current query in data reader"));
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      return resultSet.Fields[i].ColumnName;
    }

    /// <summary>
    /// Gets the column ordinal, given the name of the column.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override int GetOrdinal(string name)
    {
      if (!isOpen || resultSet == null)
        Throw(new Exception("No current query in data reader"));

      return resultSet.GetOrdinal(name);
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetStringS/*'/>
    public string GetString(string column)
    {
      return GetString(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetString/*'/>
    public override String GetString(int i)
    {
      IMySqlValue val = GetFieldValue(i, true);

      if (val is MySqlBinary)
      {
        byte[] v = ((MySqlBinary)val).Value;
        return resultSet.Fields[i].Encoding.GetString(v, 0, v.Length);
      }

      return val.Value.ToString();
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetTimeSpan/*'/>
    public TimeSpan GetTimeSpan(string column)
    {
      return GetTimeSpan(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetTimeSpan/*'/>
    public TimeSpan GetTimeSpan(int column)
    {
      IMySqlValue val = GetFieldValue(column, true);

      MySqlTimeSpan ts = (MySqlTimeSpan)val;
      return ts.Value;
    }

    /// <summary>
    /// Gets the value of the specified column in its native format.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override object GetValue(int i)
    {
      if (!isOpen)
        Throw(new Exception("No current query in data reader"));
      if (i >= FieldCount)
        Throw(new IndexOutOfRangeException());

      IMySqlValue val = GetFieldValue(i, false);
      if (val.IsNull)
        return DBNull.Value;

      // if the column is a date/time, then we return a MySqlDateTime
      // so .ToString() will print '0000-00-00' correctly
      if (val is MySqlDateTime)
      {
        MySqlDateTime dt = (MySqlDateTime)val;
        if (!dt.IsValidDateTime && connection.Settings.ConvertZeroDateTime)
          return DateTime.MinValue;
        else if (connection.Settings.AllowZeroDateTime)
          return val;
        else
          return dt.GetDateTime();
      }

      return val.Value;
    }

    /// <summary>
    /// Gets all attribute columns in the collection for the current row.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public override int GetValues(object[] values)
    {
      int numCols = Math.Min(values.Length, FieldCount);
      for (int i = 0; i < numCols; i++)
        values[i] = GetValue(i);

      return numCols;
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt16/*'/>
    public UInt16 GetUInt16(string column)
    {
      return GetUInt16(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt16/*'/>
    public UInt16 GetUInt16(int column)
    {
      IMySqlValue v = GetFieldValue(column, true);
      if (v is MySqlUInt16)
        return ((MySqlUInt16)v).Value;

      return (UInt16)ChangeType(v, column, typeof(UInt16));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt32/*'/>
    public UInt32 GetUInt32(string column)
    {
      return GetUInt32(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt32/*'/>
    public UInt32 GetUInt32(int column)
    {
      IMySqlValue v = GetFieldValue(column, true);
      if (v is MySqlUInt32)
        return ((MySqlUInt32)v).Value;
      return (uint)ChangeType(v, column, typeof(UInt32));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt64/*'/>
    public UInt64 GetUInt64(string column)
    {
      return GetUInt64(GetOrdinal(column));
    }

    /// <include file='docs/MySqlDataReader.xml' path='docs/GetUInt64/*'/>
    public UInt64 GetUInt64(int column)
    {
      IMySqlValue v = GetFieldValue(column, true);
      if (v is MySqlUInt64)
        return ((MySqlUInt64)v).Value;

      return (UInt64)ChangeType(v, column, typeof(UInt64));
    }


    #endregion

#if !RT
    IDataReader IDataRecord.GetData(int i)
    {
      return base.GetData(i);
    }
#endif

    /// <summary>
    /// Gets a value indicating whether the column contains non-existent or missing values.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public override bool IsDBNull(int i)
    {
      return DBNull.Value == GetValue(i);
    }

    /// <summary>
    /// Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns></returns>
    public override bool NextResult()
    {
      if (!isOpen)
        Throw(new MySqlException(Resources.NextResultIsClosed));

      bool isCaching = command.CommandType == CommandType.TableDirect && command.EnableCaching &&
        (commandBehavior & CommandBehavior.SequentialAccess) == 0;

      // this will clear out any unread data
      if (resultSet != null)
      {
        resultSet.Close();
        if (isCaching)
          TableCache.AddToCache(command.CommandText, resultSet);
      }

      // single result means we only return a single resultset.  If we have already
      // returned one, then we return false
      // TableDirect is basically a select * from a single table so it will generate
      // a single result also
      if (resultSet != null &&
        ((commandBehavior & CommandBehavior.SingleResult) != 0 || isCaching))
        return false;

      // next load up the next resultset if any
      try
      {
        do
        {
          resultSet = null;
          // if we are table caching, then try to retrieve the resultSet from the cache
          if (isCaching)
            resultSet = TableCache.RetrieveFromCache(command.CommandText,
              command.CacheAge);

          if (resultSet == null)
          {
            resultSet = driver.NextResult(Statement.StatementId, false);
            if (resultSet == null) return false;
            if (resultSet.IsOutputParameters && command.CommandType == CommandType.StoredProcedure)
            {
              StoredProcedure sp = statement as StoredProcedure;
              sp.ProcessOutputParameters(this);
              resultSet.Close();
              if (!sp.ServerProvidingOutputParameters) return false;
              // if we are using server side output parameters then we will get our ok packet
              // *after* the output parameters resultset
              resultSet = driver.NextResult(Statement.StatementId, true);
            }
            resultSet.Cached = isCaching;
          }

          if (resultSet.Size == 0)
          {
            Command.lastInsertedId = resultSet.InsertedId;
            if (affectedRows == -1)
              affectedRows = resultSet.AffectedRows;
            else
              affectedRows += resultSet.AffectedRows;
          }
        } while (resultSet.Size == 0);

        return true;
      }
      catch (MySqlException ex)
      {
        if (ex.IsFatal)
          connection.Abort();
        if (ex.Number == 0)
          throw new MySqlException(Resources.FatalErrorReadingResult, ex);
        if ((commandBehavior & CommandBehavior.CloseConnection) != 0)
          Close();
        throw;
      }
    }

    /// <summary>
    /// Advances the MySqlDataReader to the next record.
    /// </summary>
    /// <returns></returns>
    public override bool Read()
    {
      if (!isOpen)
        Throw(new MySqlException("Invalid attempt to Read when reader is closed."));
      if (resultSet == null)
        return false;

      try
      {
        return resultSet.NextRow(commandBehavior);
      }
      catch (TimeoutException tex)
      {
        connection.HandleTimeoutOrThreadAbort(tex);
        throw; // unreached
      }
      catch (ThreadAbortException taex)
      {
        connection.HandleTimeoutOrThreadAbort(taex);
        throw;
      }
      catch (MySqlException ex)
      {
        if (ex.IsFatal)
          connection.Abort();

        if (ex.IsQueryAborted)
        {
          throw;
        }

        throw new MySqlException(Resources.FatalErrorDuringRead, ex);
      }
    }


    private IMySqlValue GetFieldValue(int index, bool checkNull)
    {
      if (index < 0 || index >= FieldCount)
        Throw(new ArgumentException(Resources.InvalidColumnOrdinal));

      IMySqlValue v = resultSet[index];

      if (checkNull && v.IsNull)
#if RT
        throw new MySqlNullValueException();
#else
        throw new System.Data.SqlTypes.SqlNullValueException();
#endif

      return v;
    }


    private void ClearKillFlag()
    {
      // This query will silently crash because of the Kill call that happened before.
      string dummyStatement = "SELECT * FROM bogus_table LIMIT 0"; /* dummy query used to clear kill flag */
      MySqlCommand dummyCommand = new MySqlCommand(dummyStatement, connection);
      dummyCommand.InternallyCreated = true;
      try
      {
        dummyCommand.ExecuteReader(); // ExecuteReader catches the exception and returns null, which is expected.
      }
      catch (MySqlException ex)
      {
        if (ex.Number != (int)MySqlErrorCode.NoSuchTable) throw;
      }
    }

    private void ProcessOutputParameters()
    {
      // if we are not 5.5 or later or we are not prepared then we are simulating output parameters
      // with user variables and they are also string so we have to work some magic with out
      // column types before we read the data
      if (!driver.SupportsOutputParameters || !command.IsPrepared)
        AdjustOutputTypes();

      // now read the output parameters data row
      if ((commandBehavior & CommandBehavior.SchemaOnly) != 0) return;
      resultSet.NextRow(commandBehavior);

      string prefix = "@" + StoredProcedure.ParameterPrefix;

      for (int i = 0; i < FieldCount; i++)
      {
        string fieldName = GetName(i);
        if (fieldName.StartsWith(prefix))
          fieldName = fieldName.Remove(0, prefix.Length);
        MySqlParameter parameter = command.Parameters.GetParameterFlexible(fieldName, true);
        parameter.Value = GetValue(i);
      }
    }

    private void AdjustOutputTypes()
    {
      // since MySQL likes to return user variables as strings
      // we reset the types of the readers internal value objects
      // this will allow those value objects to parse the string based
      // return values
      for (int i = 0; i < FieldCount; i++)
      {
        string fieldName = GetName(i);
        fieldName = fieldName.Remove(0, StoredProcedure.ParameterPrefix.Length + 1);
        MySqlParameter parameter = command.Parameters.GetParameterFlexible(fieldName, true);

        IMySqlValue v = MySqlField.GetIMySqlValue(parameter.MySqlDbType);
        if (v is MySqlBit)
        {
          MySqlBit bit = (MySqlBit)v;
          bit.ReadAsString = true;
          resultSet.SetValueObject(i, bit);
        }
        else
          resultSet.SetValueObject(i, v);
      }
    }

    private void Throw(Exception ex)
    {
      if (connection != null)
        connection.Throw(ex);
      throw ex;
    }

    public new void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposed)
        return;

      if (disposing)
        Close();

      base.Dispose(disposing);
      disposed = true;
    }

    #region Destructor
    ~MySqlDataReader()
    {
      Dispose(false);
    }
    #endregion
  }
}
