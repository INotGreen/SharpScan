using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpScan.Helper
{
    internal class EDRCheck
    {
        protected static string Format(string args_1, string args_2) => String.Format("  [>] {0,-28}: {1}\r", args_1, args_2);
        public EDRCheck()
        {
            // 获取当前系统的进程列表
            List<string> currentProcesses = Process.GetProcesses()
                                         .Select(p => p.ProcessName.ToLower() + ".exe")
                                          .Distinct()
                                          .ToList();

            // 查找匹配的杀毒软件
            var matchedProcesses = new Dictionary<string, string>();

            foreach (var process in currentProcesses)
            {
                if (Configuration.EDRQueryDictionary.ContainsKey(process))
                {
                    matchedProcesses[process] = Configuration.EDRQueryDictionary[process];
                }
            }

            // 打印已安装的杀毒软件及对应的进程
            if (matchedProcesses.Any())
            {
                Console.WriteLine("[+] Installed AV, EDR and corresponding processes:\n");
                foreach (var kvp in matchedProcesses)
                {
                    Console.WriteLine(Format(kvp.Key, kvp.Value));
                }
            }
            else
            {
                Console.WriteLine("No installed anti-virus software was detected.");
            }
            Console.WriteLine("\n");
        }
    }
}
