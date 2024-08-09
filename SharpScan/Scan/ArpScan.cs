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
        public async Task ARPScanPC(List<string> IPlist, int Delay, int maxConcurrency)
        {
            Console.WriteLine("\r\nC_Segment: " + hTarget + ".");
            Console.WriteLine(new string('-', 95));
            Console.WriteLine($"{"IP",-20} {"HostName",-28} {"OsVersion",-40}");
            Console.WriteLine(new string('-', 95));

            List<Task> ArpTasks = new List<Task>();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var Ip in IPlist)
                {
                    await semaphore.WaitAsync();
                    ArpTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            ArpCheck(Ip);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                    await Task.Delay(Delay);
                }

                await Task.WhenAll(ArpTasks);
            }
            Console.WriteLine(new string('-', 95));
            Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
            Console.WriteLine(new string('-', 95));
        }

        public static void ArpCheck(string ip)
        {
            try
            {
                if (!Program.onlineHostList.Exists(onlinepc => onlinepc.IP == ip))
                {
                    if (IsHostAlive(ip))
                    {
                        OnlinePC onlinePC = new OnlinePC();
                        onlinePC.IP = ip;
                        onlinePC.HostName = new GetOsInfos().GetHostName(ip);
                        onlinePC.OS = new GetOsInfos().GetOsVersion(onlinePC);
                        string result = $"{ip + "(ARP)",-20} {onlinePC.HostName,-28} {onlinePC.OS,-40}";
                        Program.onlineHostList.Add(onlinePC);
                        Console.WriteLine(result);
                        Program.onlinePC++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine($"Error checking IP {ip}: {ex.Message}");
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
