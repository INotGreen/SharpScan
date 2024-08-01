using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;


namespace SharpScan
{
    public static class Win32FileScanner
    {

        private static readonly IntPtr invalidHandle = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Win32FindData
        {
            public FileAttributes dwFileAttributes;
            public FileTime ftCreationTime;
            public FileTime ftLastAccessTime;
            public FileTime ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileTime
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFile(string lpFileName, out Win32FindData lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out Win32FindData lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);


        public static string GetlastModified(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                long fileSize = fileInfo.Length;
                DateTime lastModified = fileInfo.LastWriteTime;
                return lastModified.ToString();
            }
            else
            {
                Console.WriteLine("File does not exist.");
                return "NULL";
            }
        }
        /// <summary>
        /// Converts the provided Win32 FileTime struct into a .NET DateTime struct.
        /// </summary>



        public static IEnumerable<string> FindFilesByName(string path, string searchPattern, int maxDepth = -1)
        {
            return ScanFilesByName(Path.GetFullPath(path), searchPattern, maxDepth, 0);
        }

        public static IEnumerable<string> ScanFilesByName(string path, string searchPattern, int maxDepth, int depth)
        {
            IntPtr handle = FindFirstFile($@"{path}\*", out Win32FindData findData);

            if (handle != IntPtr.Zero)
            {
                try
                {
                    do
                    {
                        if (findData.cFileName.Equals(".") || findData.cFileName.Equals(".."))
                            continue;

                        string fullPath = Path.Combine(path, findData.cFileName);

                        if ((findData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (maxDepth >= 0 && depth + 1 > maxDepth)
                                continue;

                            foreach (string filePath in ScanFilesByName(fullPath, searchPattern, maxDepth, depth + 1))
                            {
                                yield return filePath;
                            }
                        }
                        else
                        {
                            if (Path.GetFileName(fullPath).IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                yield return fullPath;
                            }
                        }

                    } while (FindNextFile(handle, out findData));
                }
                finally
                {
                    FindClose(handle);
                }
            }
            else
            {
                Console.WriteLine($"Failed to access directory: {path}");
            }
        }
    } 
}
