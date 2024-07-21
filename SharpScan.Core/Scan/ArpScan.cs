
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static SharpScan.Program;


namespace SharpScan
{
    internal class ARPScan
    {
        public static int onlinePC = 0;
        public static bool isCheckPC = true;
        //public static int ScanOkNum = 0;

        static async Task ArpCping(string ip)
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
                    tasks.Add(Task.Run(() => ArpScanPC(currentIP)));
                }
                await Task.WhenAll(tasks);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        static async Task ArpBping(string ip)
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

                    tasks.Add(ArpCping(currentIP));
                }
                await Task.WhenAll(tasks);
                GC.Collect();
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
                    tasks.Add(ArpBping(currentIP));
                }

                await Task.WhenAll(tasks);

                GC.Collect();

            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        public static  void ArpScanPC(string ip)
        {
            try
            {
                if (IsHostAlive(ip))
                {
                    Console.WriteLine(ip);
                    if (!Program.HostList.Exists(onlinepc=>onlinepc.IP == ip))
                    {
                         OnlinePC onlinePC = new OnlinePC();
                        onlinePC.IP = ip;
                        onlinePC.HostName = new GetOsInfos().GetHostName(ip);
                        onlinePC.OS = new GetOsInfos().GetOsVersion(ip);
                        string result = $"{ip + "(ARP)",-28} {onlinePC.HostName,-28} {onlinePC.OS,-40}";
                        Program.HostList.Add(onlinePC);
                        Console.WriteLine(result);
                        Program.onlinePC++;
                    }
                   
                }
            }
            catch (Exception)
            {
                // Handle exception if necessary
            }
        }

        public static bool IsHostAlive(string ipAddress)
        {
            try
            {
                uint destIP = inet_addr(ipAddress);
                uint srcIP = 0;
                byte[] macAddr = new byte[6];
                uint macAddrLen = (uint)macAddr.Length;

                int result = SendARP(destIP, srcIP, macAddr, ref macAddrLen);

                if (result == 0)
                {
                    // Check if MAC address is not empty
                    return macAddr.Any(b => b != 0);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        [DllImport("Iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint destIP, uint srcIP, byte[] pMacAddr, ref uint phyAddrLen);

        [DllImport("Ws2_32.dll")]
        public static extern uint inet_addr(string ipaddr);

        public async Task ArpScanIP(string ip)
        {
            if (isCheckPC)
            {
                if (ip.Contains("/8"))
                {
                    await Aping(ip.Replace("/8", ""));
                }
                else if (ip.Contains("/16"))
                {
                    await ArpBping(ip.Replace("/16", ""));
                }
                else if (ip.Contains("/24"))
                    await ArpCping(ip.Replace("/24", ""));
                else
                    ArpScanPC(ip);
            }
            else
            {
                if (ip.Contains("/8"))
                {
                    await Aping(ip.Replace("/8", ""));
                }
                else if (ip.Contains("/16"))
                {
                    await ArpBping(ip.Replace("/16", ""));
                }
                else if (ip.Contains("/24"))
                    await ArpCping(ip.Replace("/24", ""));
            }
        }
    }
}
