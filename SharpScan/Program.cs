using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using SharpScan.Plugins;
using Tamir.SharpSsh.Sharp.io;
using static System.Net.WebRequestMethods;

namespace SharpScan
{
    public class Program
    {
        public static int onlinePC = 0;
        private static List<Task> scanTasks = new List<Task>();
        public static List<OnlinePC> onlineHostList = new List<OnlinePC>();
        public static List<string> IpPortList = new List<string>();
        public static int alivePort = 0;
        private static StreamWriter fileWriter;
        public static bool showHelp = false;
        public static bool icmpScan = false;
        public static bool arpScan = false;
        public static bool isUDP = false;
        public static bool nopoc = false;
        public static string mode { get; set; }
        public static string hTarget { get; set; }

        public static List<string> IPlist;
        public static string portRange { get; set; }
        public static string socks5 { get; set; }
        public static string command { get; set; }
        public static string maxConcurrency = "600";
        public static string delay = "10";
        public static string userName { get; set; }
        public static string userNameFile { get; set; }
        public static string passWord { get; set; }
        public static string passWordFile { get; set; }
        public static List<string> userList { get; set; }
        public static List<string> passwordList { get; set; }
        public static string outputFile { get; set; }


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
            Console.WriteLine("Usage: SharpScan [OPTIONS]\n");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            Console.WriteLine("\nExample:");
            Console.WriteLine("  SharpScan.exe -t 192.168.1.1/24");
            Console.WriteLine("  SharpScan.exe -t 192.168.1.107 -p 100-1024");
        }

        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(StringPating);

            var options = new OptionSet
            {
                { "i|icmp", "Perform icmp scan", i => icmpScan = i != null },
                { "a|arp", "Perform arp scan", a => arpScan = a != null },
                { "U|udp", "Perform udp scan", udp => isUDP = udp != null },
                { "h|hTarget=", "Target segment to scan", h => hTarget = h },
                { "p|ports=", "Ports to scan (e.g. \"0-1024\" or \"80,443,8080\")", p => portRange = p },
                { "u|username=", "Username for authentication", u => userName = u },
                { "pw|password=", "Password for authentication", pwd => passWord = pwd },
                { "uf|ufile=", "Username file for authentication", uf => userNameFile = uf },
                { "pwf|pwdfile=", "Password file for authentication", pwdf => passWordFile = pwdf },
                { "m|mode=", "Scanning poc mode(e.g. ssh/smb/rdp/ms17010)", m => Program.mode = m },
                { "c|command=", "Command Execution", c => command = c },
                { "d|delay=", "Scan delay(ms),Defalt:1000", p => delay = p },
                { "t|thread=", "Maximum num of concurrent scans,Defalt:600", t => maxConcurrency = t },
                { "socks5=", "Open socks5 port", socks5 => Program.socks5 = socks5 },
                { "nopoc", "Not using proof of concept(POC)", nopoc => Program.nopoc =nopoc!= null },
                { "o|output=", "Output file to save console output", o => outputFile = o },
                 { "help|show", "Show this usage and help", h => showHelp = h != null },
            };

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

            Console.WriteLine($"Delay:{delay}   MaxConcurrency:{maxConcurrency}");
            new SetTls12UserRegistryKeys();


            if (string.IsNullOrEmpty(hTarget))
            {
                ShowHelp(options);
                return;
            }

            if (!string.IsNullOrEmpty(hTarget))
            {
                IPlist = SharpScan.GetIP.IPList(hTarget);
            }



            if (!string.IsNullOrEmpty(socks5))
            {
                new Socks5().Run(Convert.ToInt32(socks5), Program.userName, Program.passWord);
                return;
            }

            if (!string.IsNullOrEmpty(userNameFile))
            {
                if (System.IO.File.Exists(userNameFile))
                {
                    string[] lines = System.IO.File.ReadAllLines(userNameFile);

                    // 将 string[] 转换为 List<string>
                    userList = new List<string>(lines);
                    //foreach (string line in userList)
                    //{
                    //    Console.WriteLine(line);
                    //}
                }

            }
            if (!string.IsNullOrEmpty(passWordFile))
            {
                if (System.IO.File.Exists(passWordFile))
                {
                    string[] lines = System.IO.File.ReadAllLines(passWordFile);

                    // 将 string[] 转换为 List<string>
                    passwordList = new List<string>(lines);
                    //foreach (string line in passwordList)
                    //{
                    //    Console.WriteLine(line);
                    //}
                }
            }


            if (!string.IsNullOrEmpty(outputFile))
            {
                fileWriter = new StreamWriter(outputFile, false) { AutoFlush = true };
                var multiTextWriter = new MultiTextWriter(Console.Out, fileWriter);
                Console.SetOut(multiTextWriter);
                Console.SetError(multiTextWriter);
            }

            if (!string.IsNullOrEmpty(portRange))
            {
                if (isUDP)
                {
                    await new UdpPortscan().ScanPortRange(hTarget, portRange, Convert.ToInt32(delay), Convert.ToInt32(maxConcurrency));

                }
                else { await new TcpPortscan().ScanPortRange(hTarget, portRange, Convert.ToInt32(delay), Convert.ToInt32(maxConcurrency)); }

                return;
            }

            if (!string.IsNullOrEmpty(mode))
            {

                await new HandlePOC().ModPacket(mode);

                return;
            }


            Console.WriteLine("\r\nC_Segment: " + hTarget + ".");
            Console.WriteLine("===================================================================");
            Console.WriteLine($"{"IP",-28} {"HostName",-28} {"OsVersion",-40}");

            if (icmpScan)
            {
                await Task.Run(() => new ICMPScan().ICMPScanPC(Program.IPlist, Convert.ToInt32(delay), Convert.ToInt32(maxConcurrency)));
                Console.WriteLine("===================================================================");
                Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
                Console.WriteLine("===================================================================");
            }

            if (arpScan)
            {
                var arpTask = Task.Run(() => new ARPScan().ARPScanPC(Program.IPlist, Convert.ToInt32(delay), Convert.ToInt32(maxConcurrency)));
                await Task.WhenAll(arpTask);
                Console.WriteLine("===================================================================");
                Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
                Console.WriteLine("===================================================================");
            }

            await new TcpPortscan().ScanPortDefault(Convert.ToInt32(delay), Configuration.PortList, Convert.ToInt32(maxConcurrency));

            Console.WriteLine("===================================================================");
            Console.WriteLine($"[+] alive ports len is: {alivePort}");
            Console.WriteLine("===================================================================");
            GC.Collect();



            if (!nopoc)
            {
                new DomainCollect();
                await new HandlePOC().HandleDefault();
            }
            Console.ResetColor();

            if (fileWriter != null)
            {
                fileWriter.Close();
            }

        }
    }
}
