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
using System.Runtime.InteropServices;
using System.Threading;

namespace MySql.Data.Common
{
  internal class NativeMethods
  {
    // Keep the compiler from generating a default ctor
    private NativeMethods()
    {
    }

    //Constants for dwDesiredAccess:
    public const UInt32 GENERIC_READ = 0x80000000;
    public const UInt32 GENERIC_WRITE = 0x40000000;

    //Constants for return value:
    public const Int32 INVALIDpipeHandle_VALUE = -1;

    //Constants for dwFlagsAndAttributes:
    public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
    public const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

    //Constants for dwCreationDisposition:
    public const UInt32 OPEN_EXISTING = 3;

    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
      public SecurityAttributes()
      {
        Length = Marshal.SizeOf(typeof(SecurityAttributes));
      }
      public int Length;
      public IntPtr securityDescriptor = IntPtr.Zero;
      public bool inheritHandle;
    }

    [DllImport("Kernel32", CharSet = CharSet.Unicode)]
    static extern public IntPtr CreateFile(
            String fileName,
      uint desiredAccess,
      uint shareMode,
      SecurityAttributes securityAttributes,
      uint creationDisposition,
      uint flagsAndAttributes,
      uint templateFile);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
    static extern public bool PeekNamedPipe(IntPtr handle,
      byte[] buffer,
      uint nBufferSize,
      ref uint bytesRead,
      ref uint bytesAvail,
      ref uint BytesLeftThisMessage);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern public bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead,
  out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("Kernel32")]
    public static extern bool WriteFile(IntPtr hFile, [In]byte[] buffer,
  uint numberOfBytesToWrite, out uint numberOfBytesWritten, IntPtr lpOverlapped);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CancelIo(IntPtr handle);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FlushFileBuffers(IntPtr handle);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr OpenEvent(uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)]bool bInheritHandle,
        string lpName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr OpenFileMapping(uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)]bool bInheritHandle,
        string lpName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint
        dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,
        IntPtr dwNumberOfBytesToMap);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int FlushViewOfFile(IntPtr address, uint numBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WaitNamedPipe(string namedPipeName, uint timeOut);
    #region Winsock functions

    // SOcket routines
    [DllImport("ws2_32.dll", SetLastError = true)]
    static extern public IntPtr socket(int af, int type, int protocol);

    [DllImport("ws2_32.dll", SetLastError = true)]
    static extern public int ioctlsocket(IntPtr socket, uint cmd, ref UInt32 arg);

    [DllImport("ws2_32.dll", SetLastError = true)]
    public static extern int WSAIoctl(IntPtr s, uint dwIoControlCode, byte[] inBuffer, uint cbInBuffer,
      byte[] outBuffer, uint cbOutBuffer, IntPtr lpcbBytesReturned, IntPtr lpOverlapped,
      IntPtr lpCompletionRoutine);

    [DllImport("ws2_32.dll", SetLastError = true)]
    static extern public int WSAGetLastError();

    [DllImport("ws2_32.dll", SetLastError = true)]
    static extern public int connect(IntPtr socket, byte[] addr, int addrlen);

    [DllImport("ws2_32.dll", SetLastError = true)]
    static extern public int recv(IntPtr socket, byte[] buff, int len, int flags);

    [DllImport("ws2_32.Dll", SetLastError = true)]
    static extern public int send(IntPtr socket, byte[] buff, int len, int flags);

    #endregion

  }
}
