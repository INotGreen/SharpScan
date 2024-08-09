using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SharpScan.Program;

namespace SharpScan
{
    internal class ICMPScan
    {
        public async Task ICMPScanPC(List<string> IPlist, int Delay, int maxConcurrency)
        {
            Console.WriteLine("\r\nC_Segment: " + hTarget + ".");
            Console.WriteLine(new string('-', 95));
            Console.WriteLine($"{"IP",-20} {"HostName",-28} {"OsVersion",-40}");
            Console.WriteLine(new string('-', 95));

            List<Task> IcmpTasks = new List<Task>();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var Ip in IPlist)
                {
                    await semaphore.WaitAsync();
                    IcmpTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            Ping(Ip);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                    await Task.Delay(Delay);
                }

                await Task.WhenAll(IcmpTasks);
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine("[+] onlinePC: " + Program.onlinePC);
            Console.WriteLine(new string('-', 95));
        }

        private static void Ping(string ip)
        {
            try
            {
                if (!Program.onlineHostList.Exists(onlinepc => onlinepc.IP == ip))
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
                        onlinePC.OS = new GetOsInfos().GetOsVersion(onlinePC);
                        string result = $"{ip + "(ICMP)",-20} {onlinePC.HostName,-28} {onlinePC.OS,-40}";

                        Console.WriteLine(result);
                        Program.onlineHostList.Add(onlinePC);
                        Program.onlinePC++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                //Console.WriteLine($"Error pinging IP {ip}: {ex.Message}");
            }
        }
    }
}
