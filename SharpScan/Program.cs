using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using Tamir.SharpSsh.java.io;

namespace SharpScan
{
    public class Program
    {
        public static int onlinePC = 0;
        private static List<Task> scanTasks = new List<Task>();
        public static List<OnlinePC> HostList = new List<OnlinePC>();
        public static List<string> IpPortList = new List<string>();
        public static int alivePort = 0;
        private static StreamWriter fileWriter;
        public static bool showHelp = false;
        public static bool icmpScan = false;
        public static bool arpScan = false;
        public static string targetSegment = "";
        public static string outputFile = "";
        public static List<string> IPlist;
        public static string portRange = "";
        public static string MaxConcurrency = "600";
        public static string Delay = "1000";
        public static string Username = "";
        public static string Password = "";

        public class OnlinePC
        {
            public string IP { get; set; }
            public List<int> Port = new List<int>();
            public string Url { get; set; }
            public List<string> Service { get; set; }
            public string HostName { get; set; }
            public string OS { get; set; }
            public string Infostr { get; set; }
        }
        public static string StringPating = @"
  ______   __                                       ______                                
 /      \ /  |                                     /      \                               
/$$$$$$  |$$ |____    ______    ______    ______  /$$$$$$  |  _______   ______   _______  
$$ \__$$/ $$      \  /      \  /      \  /      \ $$ \__$$/  /       | /      \ /       \ 
$$      \ $$$$$$$  | $$$$$$  |/$$$$$$  |/$$$$$$  |$$      \ /$$$$$$$/  $$$$$$  |$$$$$$$  |
 $$$$$$  |$$ |  $$ | /    $$ |$$ |  $$/ $$ |  $$ | $$$$$$  |$$ |       /    $$ |$$ |  $$ |
/  \__$$ |$$ |  $$ |/$$$$$$$ |$$ |      $$ |__$$ |/  \__$$ |$$ \_____ /$$$$$$$ |$$ |  $$ |
$$    $$/ $$ |  $$ |$$    $$ |$$ |      $$    $$/ $$    $$/ $$       |$$    $$ |$$ |  $$ |
 $$$$$$/  $$/   $$/  $$$$$$$/ $$/       $$$$$$$/   $$$$$$/   $$$$$$$/  $$$$$$$/ $$/   $$/ 
                                        $$ |                                              
                                        $$ |                                              
                                        $$/                                               
";
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SharpScan [OPTIONS]");
            Console.WriteLine("Perform network scans using different protocols.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine(StringPating);

            var options = new OptionSet
            {
                { "i|icmp", "Perform ICMP scan", i => icmpScan = i != null },
                { "a|arp", "Perform ARP scan", a => arpScan = a != null },
                { "t|Target=", "Target segment to scan", t => targetSegment = t },
                { "p|ports=", "Ports to scan (e.g. \"0-1024\" or \"80,443,8080\")", p => portRange = p },
                { "d|delay=", "Scan Delay(ms),Defalt:1000", p => Delay = p },
                { "m|maxconcurrency=", "Maximum number of concurrent scans,Defalt:600", m => MaxConcurrency = m },
                { "u|username=", "Username for authentication", u => Username = u },
                { "pw|password=", "Password for authentication", pw => Password = pw },
                { "h|help", "Show this usage and help", h => showHelp = h != null },
                { "o|output=", "Output file to save console output", o => outputFile = o }
            };
            Dictionary<string, string> macdict = Options.GetMACDict();
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine($"SharpScan: {e.Message}");
                Console.WriteLine("Try `SharpScan --help` for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(options);
                return;
            }
            if (!icmpScan && !arpScan)
            {
                icmpScan = true;
            }
            Console.WriteLine($"Delay:{Delay}   MaxConcurrency:{MaxConcurrency}");

            if (!string.IsNullOrEmpty(targetSegment))
            {
                IPlist = SharpScan.GetIP.IPList(targetSegment);
            }
            if (!string.IsNullOrEmpty(portRange))
            {
                await new Portscan().ScanPortRange(targetSegment, portRange, Convert.ToInt32(Delay), Convert.ToInt32(MaxConcurrency));
                return;
            }

            if (string.IsNullOrEmpty(targetSegment))
            {
                Console.WriteLine("Target segment must be specified using -s or --segment.");
                ShowHelp(options);
                return;
            }

            if (!string.IsNullOrEmpty(outputFile))
            {
                fileWriter = new StreamWriter(outputFile, false) { AutoFlush = true };
                var multiTextWriter = new MultiTextWriter(Console.Out, fileWriter);
                Console.SetOut(multiTextWriter);
                Console.SetError(multiTextWriter);
            }

            Console.WriteLine("\r\nC_Segment: " + targetSegment + ".");
            Console.WriteLine("===================================================================");
            Console.WriteLine($"{"IP",-28} {"HostName",-28} {"OsVersion",-40}");

            if (icmpScan)
            {
                await Task.Run(() => new ICMPScan().ICMPScanPC(Program.IPlist));

                Console.WriteLine("===================================================================");
                Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
                Console.WriteLine("===================================================================");
            }

            if (arpScan)
            {
                var arpTask = Task.Run(() => new ARPScan().ARPScanPC(Program.IPlist));
                await Task.WhenAll(arpTask);
                Console.WriteLine("===================================================================");
                Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
                Console.WriteLine("===================================================================");
            }

            await new Portscan().ScanPortAsync(Convert.ToInt32(Delay), Convert.ToInt32(MaxConcurrency));

            Console.WriteLine("===================================================================");
            Console.WriteLine($"[+] alive ports len is: {alivePort}");
            Console.WriteLine("===================================================================");
            GC.Collect();
            await new ScanPacket().HandlePacket();

            if (fileWriter != null)
            {
                fileWriter.Close();
            }
        }
    }
}
