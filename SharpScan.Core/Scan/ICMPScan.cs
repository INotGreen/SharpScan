
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static SharpScan.Program;
using System.Runtime;

namespace SharpScan
{
    internal class ICMPScan
    {
        public static int onlinePC = 0;
        public static bool isCheckPC = true;
        //public static int ScanOkNum = 0;
        private static List<Task> scanTasks = new List<Task>();


        static async Task Cping(string ip)
        {
            try
            {
                string enterIP = ip.Trim('.');
                string IPSegment = "192.168.1.";
                if (CheckIP.regCheckIP(enterIP))
                    IPSegment = enterIP.Substring(0, enterIP.LastIndexOf('.'));
                else if (CheckIP.regIPRegion(enterIP))
                    IPSegment = enterIP;

                var tasks = new List<Task>();
                for (int i = 1; i < 255; i++)
                {
                    string currentIP = IPSegment + "." + i.ToString();
                    tasks.Add(Task.Run(() => ICMPScanPC(currentIP)));
                }
                await Task.WhenAll(tasks);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        static async Task  Bping(string ip)
        {
            try
            {
                string enterIP = ip.Trim('.');
                string B_IPSegment = "192.168.";
                if (CheckIP.regCheckIP(enterIP))
                {
                    B_IPSegment = enterIP.Substring(0, enterIP.LastIndexOf('.'));
                    B_IPSegment = B_IPSegment.Substring(0, B_IPSegment.LastIndexOf('.'));
                }
                else if (CheckIP.regIPRegion(enterIP))
                    B_IPSegment = enterIP.Substring(0, enterIP.LastIndexOf('.'));
                else
                    B_IPSegment = enterIP;

                var tasks = new List<Task>();
                for (int i = 1; i < 255; i++)
                {
                    string currentIP = B_IPSegment + "." + i.ToString() + ".";
                    tasks.Add(Cping(currentIP));
                }

                await Task.WhenAll(tasks);

                GC.Collect();
                // Console.WriteLine("onlinePC:" + ScanOkNum);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        static async Task Aping(string ip)
        {
            try
            {
                string enterIP = ip.Trim('.');
                string A_IPSegment = "192.";
                if (CheckIP.regCheckIP(enterIP))
                {
                    A_IPSegment = enterIP.Substring(0, enterIP.LastIndexOf('.'));
                    A_IPSegment = A_IPSegment.Substring(0, A_IPSegment.LastIndexOf('.'));
                    A_IPSegment = A_IPSegment.Substring(0, A_IPSegment.LastIndexOf('.'));
                }
                else if (CheckIP.regIPRegion(enterIP))
                {
                    A_IPSegment = enterIP.Substring(0, enterIP.LastIndexOf('.'));
                    A_IPSegment = A_IPSegment.Substring(0, A_IPSegment.LastIndexOf('.'));
                }
                else
                    A_IPSegment = enterIP;

                var tasks = new List<Task>();
                for (int i = 1; i < 255; i++)
                {
                    string currentIP = A_IPSegment + "." + i.ToString() + ".";
                    tasks.Add(Bping(currentIP));
                }

                await Task.WhenAll(tasks);

                GC.Collect();
                //Console.WriteLine("===================================================================");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private static async void ICMPScanPC(string ip)
        {
            try
            {
                if (!Program.HostList.Exists(onlinepc => onlinepc.IP == ip))
                {
                    System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
                    System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
                    options.DontFragment = true;
                    string data = " ";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    int timeout = 1500; // Reduce timeout for faster scanning
                    System.Net.NetworkInformation.PingReply reply = pingSender.Send(ip, timeout, buffer, options);

                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        OnlinePC onlinePC = new OnlinePC();
                        onlinePC.IP = ip;
                        onlinePC.HostName = new GetOsInfos().GetHostName(ip);
                        onlinePC.OS = new GetOsInfos().GetOsVersion(ip);
                        string result = $"{ip + "(ICMP)",-28} {onlinePC.HostName,-28} {onlinePC.OS,-40}";
                        
                        Console.WriteLine(result);
                        Program.HostList.Add(onlinePC);
                        Program.onlinePC++;
                    }
                }

            }
            catch (Exception)
            {
                Console.WriteLine($"TCP 连接失败{ip}");
                // Handle exception if necessary
            }
        }

        public async Task ICMPScanIP(string ip)
        {
            if (isCheckPC)
            {
                if (ip.Contains("/8"))
                {
                    await Aping(ip.Replace("/8", ""));
                }
                else if (ip.Contains("/16"))
                {
                    await Bping(ip.Replace("/16", ""));
                }
                else if (ip.Contains("/24"))
                    await Cping(ip.Replace("/24", ""));
                else
                    ICMPScanPC(ip);
            }
            else
            {
                if (ip.Contains("/8"))
                {
                    await Aping(ip.Replace("/8", ""));
                }
                else if (ip.Contains("/16"))
                {
                    await Bping(ip.Replace("/16", ""));
                }
                else if (ip.Contains("/24"))
                    await Cping(ip.Replace("/24", ""));
            }
        }
    }
}
