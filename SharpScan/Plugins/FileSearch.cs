using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpScan
{
    internal class FileSearch
    {
        public static volatile bool stopRequested = false;
        private static Thread thread { get; set; }
        public static void Start(string[] drives)
        {
            if(!string.IsNullOrEmpty(Program.search))
            {
                string searchPattern = Program.search; ; // Replace with the filename you're searching for
                int maxDepth = -1; // Use -1 for unlimited depth

                if (string.IsNullOrEmpty(searchPattern) || searchPattern == "" || searchPattern == null)
                {
                    thread.Abort();
                    return;
                }
                if (thread != null)
                {
                    thread.Abort();
                }
                FileSearch.ThreadMethod(drives, searchPattern);
            }
            
            //thread = new Thread(() => FileSearch.ThreadMethod(drives, searchPattern, Controler_HWID));
            //thread.IsBackground = true;
            //thread.Start();
        }

        public static void ThreadMethod(string[] drives, string searchPattern)
        {
            foreach (string drive in drives)
            {
                if (stopRequested) break;

                Console.WriteLine($"Searching in {drive}");
                
                var files = Win32FileScanner.ScanFilesByName(drive, searchPattern, -1, 0);
                foreach (var file in files)
                {
                    if (stopRequested) break;

                   
                    Console.WriteLine(file);
                }
            }
        }
       
    }
}
