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

using System;
using System.IO;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Properties;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Security.Permissions;
using System.Security;



namespace MySql.Data.Common
{
  /// <summary>
  /// Summary description for API.
  /// </summary>
  [SuppressUnmanagedCodeSecurity()]
  internal class NamedPipeStream : Stream
  {
    SafeFileHandle handle;
    Stream fileStream;
    int readTimeout = Timeout.Infinite;
    int writeTimeout = Timeout.Infinite;
    const int ERROR_PIPE_BUSY = 231;
    const int ERROR_SEM_TIMEOUT = 121;

    public NamedPipeStream(string path, FileAccess mode, uint timeout)
    {
      Open(path, mode, timeout);
    }

    void CancelIo()
    {
      bool ok = NativeMethods.CancelIo(handle.DangerousGetHandle());
      if (!ok)
        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }

    public void Open(string path, FileAccess mode, uint timeout)
    {
      IntPtr nativeHandle;

      for (; ; )
      {
        NativeMethods.SecurityAttributes security = new NativeMethods.SecurityAttributes();
        security.inheritHandle = true;
        security.Length = Marshal.SizeOf(security);

        nativeHandle = NativeMethods.CreateFile(path, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                     0, security, NativeMethods.OPEN_EXISTING, NativeMethods.FILE_FLAG_OVERLAPPED, 0);

        if (nativeHandle != IntPtr.Zero)
          break;

        if (Marshal.GetLastWin32Error() != ERROR_PIPE_BUSY)
        {
          throw new Win32Exception(Marshal.GetLastWin32Error(),
              "Error opening pipe");
        }
        LowResolutionStopwatch sw = LowResolutionStopwatch.StartNew();
        bool success = NativeMethods.WaitNamedPipe(path, timeout);
        sw.Stop();
        if (!success)
        {
          if (timeout < sw.ElapsedMilliseconds ||
              Marshal.GetLastWin32Error() == ERROR_SEM_TIMEOUT)
          {
            throw new TimeoutException("Timeout waiting for named pipe");
          }
          else
          {
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "Error waiting for pipe");
          }
        }
        timeout -= (uint)sw.ElapsedMilliseconds;
      }
      handle = new SafeFileHandle(nativeHandle, true);
      fileStream = new FileStream(handle, mode, 4096, true);
    }

    public override bool CanRead
    {
      get { return fileStream.CanRead; }
    }

    public override bool CanWrite
    {
      get { return fileStream.CanWrite; }
    }

    public override bool CanSeek
    {
      get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
    }

    public override long Length
    {
      get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
    }

    public override long Position
    {
      get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
      set { }
    }

    public override void Flush()
    {
      fileStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (readTimeout == Timeout.Infinite)
      {
        return fileStream.Read(buffer, offset, count);
      }
      IAsyncResult result = fileStream.BeginRead(buffer, offset, count, null, null);
      if (result.CompletedSynchronously)
        return fileStream.EndRead(result);

      if (!result.AsyncWaitHandle.WaitOne(readTimeout))
      {
        CancelIo();
        throw new TimeoutException("Timeout in named pipe read");
      }
      return fileStream.EndRead(result);
    }


    public override void Write(byte[] buffer, int offset, int count)
    {
      if (writeTimeout == Timeout.Infinite)
      {
        fileStream.Write(buffer, offset, count);
        return;
      }
      IAsyncResult result = fileStream.BeginWrite(buffer, offset, count, null, null);
      if (result.CompletedSynchronously)
      {
        fileStream.EndWrite(result);
      }

      if (!result.AsyncWaitHandle.WaitOne(readTimeout))
      {
        CancelIo();
        throw new TimeoutException("Timeout in named pipe write");
      }
      fileStream.EndWrite(result);
    }

    public override void Close()
    {
      if (handle != null && !handle.IsInvalid && !handle.IsClosed)
      {
        fileStream.Close();
        try
        {
          handle.Close();
        }
        catch (Exception)
        {
        }
      }
    }

    public override void SetLength(long length)
    {
      throw new NotSupportedException(Resources.NamedPipeNoSetLength);
    }


    public override bool CanTimeout
    {
      get
      {
        return true;
      }
    }

    public override int ReadTimeout
    {
      get
      {
        return readTimeout;
      }
      set
      {
        readTimeout = value;
      }
    }

    public override int WriteTimeout
    {
      get
      {
        return writeTimeout;
      }
      set
      {
        writeTimeout = value;
      }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException(Resources.NamedPipeNoSeek);
    }

    internal static Stream Create(string pipeName, string hostname, uint timeout)
    {
      string pipePath;
      if (0 == String.Compare(hostname, "localhost", true))
        pipePath = @"\\.\pipe\" + pipeName;
      else
        pipePath = String.Format(@"\\{0}\pipe\{1}", hostname, pipeName);
      return new NamedPipeStream(pipePath, FileAccess.ReadWrite, timeout);
    }
  }
}


