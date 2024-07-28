
using SSharpScan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class HandlePOC
    {


        public async Task HandlePacket()
        {
            await ProcessPackets(Program.IpPortList, ServicePacket);
            await ProcessPackets(Program.IpPortList, PocPacket);
            await ProcessPackets(Program.IpPortList, BrotePacket);
        }

        private async Task ProcessPackets(List<string> ipPortList, Func<string, Task> packetProcessor)
        {
            List<Task> tasks = new List<Task>();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.MaxConcurrency)))
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
                    await Task.Delay(Convert.ToInt32(Program.Delay));
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
                        bool success = smb.Execute(ip, 445, Convert.ToInt32(Program.Delay));
                        break;
                    }

                case "135":
                    {
                        WMI wmi = new WMI();
                        wmi.Execute(ip, 135, Convert.ToInt32(Program.Delay));
                        break;
                    }
                case "137":
                    {
                        Dictionary<string, string> macdict = GetIP.GetMACDict();
                        NBNS nbns = new NBNS();
                        nbns.Execute(ip, 137, Convert.ToInt32(Program.Delay), macdict);
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
                    // case "445":
                    //     {
                    //         Smblogin
                    //     }
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
