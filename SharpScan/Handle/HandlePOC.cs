

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class HandlePOC
    {


        public async Task HandleDefault()
        {
            await ProcessPackets(Program.IpPortList, ServicePacket);
            await ProcessPackets(Program.IpPortList, PocPacket);
            await ProcessPackets(Program.IpPortList, BrotePacket);
        }

        private async Task ProcessPackets(List<string> ipPortList, Func<string, Task> packetProcessor)
        {
            List<Task> tasks = new List<Task>();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.maxConcurrency)))
            {
                foreach (var ipPort in ipPortList)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await packetProcessor(ipPort);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                    await Task.Delay(Convert.ToInt32(Program.delay));
                }

                await Task.WhenAll(tasks);
            }
        }

        public static async Task ServicePacket(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];
            switch (port)
            {
                case "445":
                    {
                        SMB smb = new SMB();
                        bool success = smb.Execute(ip, 445, Convert.ToInt32(Program.delay));
                        break;
                    }

                case "135":
                    {
                        WMI wmi = new WMI();
                        wmi.Execute(ip, 135, Convert.ToInt32(Program.delay));
                        break;
                    }
                case "137":
                    {
                        Dictionary<string, string> macdict = GetIP.GetMACDict();
                        NBNS nbns = new NBNS();
                        nbns.Execute(ip, 137, Convert.ToInt32(Program.delay), macdict);
                        break;
                    }
            }
        }

        public static async Task BrotePacket(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];
            switch (port)
            {
                case "22":
                    {
                        // SSH 弱口令
                        SshBrute.Run(ip);
                        break;
                    }
                case "3389":
                    {
                        new RdpBroute(ip);
                        break;

                    }
            }
        }

        public async Task ModPacket(string mode)
        {
            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.maxConcurrency)))
            {
                List<Task> tasks = new List<Task>();

                foreach (var ip in Program.IPlist)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(()=>ProcessIpAsync(mode, ip, semaphore)));
                    //await Task.Delay(Convert.ToInt32(Program.delay));
                }
                
                await Task.WhenAll(tasks);
            }
        }

        private static async Task ProcessIpAsync(string mode, string ip, SemaphoreSlim semaphore)
        {
            try
            {
                switch (mode.ToLower())
                {
                    case "ssh":
                        {
                            SshBrute.Run(ip);
                            break;
                        }
                        
                    case "rdp":
                        {
                            new RdpBroute(ip);
                            break;
                        }
                        
                    case "ms17010":
                        {
                            using (var client = new TcpClient())
                            {
                                var result = client.BeginConnect(ip, 22, null, null);
                                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1.5));
                                if (success)
                                {
                                    Console.WriteLine(ip);
                                    new ms17_010scanner().Run(ip);
                                }
                            }
                            //
                            break;

                        }
                    case "smb":
                        {
                            break;
                        }
                    case "ftp":
                        {
                            break;
                        }
                        // 对于 "smb" 和 "ftp"，没有操作的示例，所以跳过
                }
            }
            finally
            {
                //semaphore.Release();
            }
        }


        public static async Task PocPacket(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];
            switch (port)
            {
                case "445":
                    {
                        new ms17_010scanner().Run(ip);
                        new SMBGhost().SMBGhostScan(ip);
                        break;
                    }

                default:
                    {
                        // 获取 web 标签
                        string[] webPorts = Configuration.WebPort.Split(',');
                        if (webPorts.Contains(port))
                        {
                            string url = WebTitle.BuildUrl(ip, port);
                            WebTitle.Run(url);
                        }
                        break;
                    }
            }
        }
    }
}
