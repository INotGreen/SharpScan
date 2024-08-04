// Copyright © 2004, 2018 Oracle and/or its affiliates. All rights reserved.
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

using MySql.Data.MySqlClient.Properties;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Security;
using System.Security.Permissions;

namespace MySql.Data.MySqlClient
{
  [ToolboxBitmap(typeof(MySqlConnection), "MySqlClient.resources.connection.bmp")]
  [DesignerCategory("Code")]
  [ToolboxItem(true)]
  public sealed partial class MySqlConnection : DbConnection, ICloneable
  {
    private bool disposed = false;

    /// <summary>
    /// Returns schema information for the data source of this <see cref="DbConnection"/>. 
    /// </summary>
    /// <returns>A <see cref="DataTable"/> that contains schema information. </returns>
    public override DataTable GetSchema()
    {
      return GetSchema(null);
    }

    /// <summary>
    /// Returns schema information for the data source of this 
    /// <see cref="DbConnection"/> using the specified string for the schema name. 
    /// </summary>
    /// <param name="collectionName">Specifies the name of the schema to return. </param>
    /// <returns>A <see cref="DataTable"/> that contains schema information. </returns>
    public override DataTable GetSchema(string collectionName)
    {
      if (collectionName == null)
        collectionName = SchemaProvider.MetaCollection;

      return GetSchema(collectionName, null);
    }

    /// <summary>
    /// Returns schema information for the data source of this <see cref="DbConnection"/>
    /// using the specified string for the schema name and the specified string array 
    /// for the restriction values. 
    /// </summary>
    /// <param name="collectionName">Specifies the name of the schema to return.</param>
    /// <param name="restrictionValues">Specifies a set of restriction values for the requested schema.</param>
    /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
    public override DataTable GetSchema(string collectionName, string[] restrictionValues)
    {
      if (collectionName == null)
        collectionName = SchemaProvider.MetaCollection;

      string[] restrictions = schemaProvider.CleanRestrictions(restrictionValues);
      MySqlSchemaCollection c = schemaProvider.GetSchema(collectionName, restrictions);
      return c.AsDataTable();
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      if (isolationLevel == IsolationLevel.Unspecified)
        return BeginTransaction();
      return BeginTransaction(isolationLevel);
    }

    protected override DbCommand CreateDbCommand()
    {
      return CreateCommand();
    }

    partial void AssertPermissions()
    {
      // Security Asserts can only be done when the assemblies 
      // are put in the GAC as documented in 
      // http://msdn.microsoft.com/en-us/library/ff648665.aspx
      if (this.Settings.IncludeSecurityAsserts)
      {
        PermissionSet set = new PermissionSet(PermissionState.None);
        set.AddPermission(new MySqlClientPermission(ConnectionString));
        set.Demand();
        MySqlSecurityPermission.CreatePermissionSet(true).Assert();
      }
    }

    #region IDisposeable
    protected override void Dispose(bool disposing)
    {
      if (disposed)
        return;

      if (State == ConnectionState.Open)
        Close();

      disposed = true;
      base.Dispose(disposing);

    }
    #endregion
  }
}
