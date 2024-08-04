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
using System.IO;
using Windows.Storage;

namespace System.IO
{
  class FileStream : Stream
  {
    private Stream stream;

    public FileStream(string path, FileMode mode, FileAccess access)
    {
      OpenFile(path, access);
    }

    private async void OpenFile(string path, FileAccess access)
    {
      StorageFile file = await StorageFile.GetFileFromPathAsync(path);
      if (access == FileAccess.Read)
        stream = await file.OpenStreamForReadAsync();
      else
        stream = await file.OpenStreamForWriteAsync();
    }

    public override bool CanRead
    {
      get { return stream.CanRead; }
    }

    public override bool CanSeek
    {
      get { return stream.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return stream.CanWrite; }
    }

    public override void Flush()
    {
      stream.Flush();
    }

    public override long Length
    {
      get { return stream.Length; }
    }

    public override long Position
    {
      get { return stream.Position; }
      set { stream.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      stream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      stream.Dispose();
      stream = null;
    }
  }
}
