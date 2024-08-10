using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpScan
{
    internal class EDRCheck
    {
        protected static string Format(string args_1, string args_2) => String.Format("  [>] {0,-28}: {1}", args_1, args_2);
        public EDRCheck()
        {
           
            List<string> currentProcesses = Process.GetProcesses()
                                          .Select(p => p.ProcessName.ToLower() + ".exe")
                                          .Distinct()
                                          .ToList();


            Dictionary<string, string> matchedProcesses = new Dictionary<string, string>();

            foreach (string process in currentProcesses)
            {
                if (Configuration.EDRQueryDictionary.ContainsKey(process))
                {
                    matchedProcesses[process] = Configuration.EDRQueryDictionary[process];
                }
            }

      
            if (matchedProcesses.Any())
            {
                Console.WriteLine("[+] Installed AV, EDR and corresponding processes:\n");
                foreach (KeyValuePair<string,string> kvp in matchedProcesses)
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
