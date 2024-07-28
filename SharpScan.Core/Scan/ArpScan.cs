
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

        public async Task ARPScanPC(List<string> IPlist)
        {
            List<Task> IcmpTasks = new List<Task>();
            foreach (var Ip in IPlist)
            {
                IcmpTasks.Add(Task.Run(() => ArpCheck(Ip)));
            }

            await Task.WhenAll(IcmpTasks);
        }
        public static void ArpCheck(string ip)
        {
            try
            {
                if (!Program.onlineHostList.Exists(onlinepc => onlinepc.IP == ip))
                {
                    if (IsHostAlive(ip))
                    {
                        Console.WriteLine(ip);
                        OnlinePC onlinePC = new OnlinePC();
                        onlinePC.IP = ip;
                        onlinePC.HostName = new GetOsInfos().GetHostName(ip);
                        onlinePC.OS = new GetOsInfos().GetOsVersion(ip);
                        string result = $"{ip + "(ARP)",-28} {onlinePC.HostName,-28} {onlinePC.OS,-40}";
                        Program.onlineHostList.Add(onlinePC);
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


    }
}
