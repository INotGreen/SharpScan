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
#if !CF
using System.Linq;
#endif
using System.Text;

namespace System.Data
{
#if !CF
    public enum UpdateRowSource
    {
      None,
      OutputParameters,
      FirstReturnedRecord,
      Both
    }

    public enum DataRowVersion
    {
      Original = 256,
      Current = 512,
      Proposed = 1024,
      Default = 1536,
    }
#endif

}

namespace System.ComponentModel
{
    public enum DesignerSerializationVisibility
    {
      Hidden, Visible, Content
    }


    public enum RefreshProperties
    {
      None, All, Repaint
    }
}

namespace System.IO
{
#if !CF
  public enum FileMode
  {
    CreateNew = 1,
    Create = 2,
    Open = 3,
    OpenOrCreate = 4,
    Truncate = 5,
    Append = 6,
  }

  [Flags]
  public enum FileAccess
  {
    Read = 1,
    Write = 2,
    ReadWrite = 3,
  }
#endif
}
