
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
using System.Collections;

namespace SharpScan
{
    internal class ICMPScan
    {
        public static int onlinePC = 0;
        public static bool isCheckPC = true;
        //public static int ScanOkNum = 0;
        private static List<Task> scanTasks = new List<Task>();


        public async Task ICMPScanPC(List<string> IPlist)
        {
            List<Task> IcmpTasks = new List<Task>();
            foreach (var Ip in IPlist)
            {
                IcmpTasks.Add(Task.Run(() => ping(Ip)));
            }

            await Task.WhenAll(IcmpTasks);
        }

        private static async void ping(string ip)
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
            catch (Exception ex)
            {
                //Console.WriteLine($");
                // Handle exception if necessary
            }
        }

    }
}
