

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public async Task ModPacket(string Mode)
        {

            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.maxConcurrency)))
            {
                switch (Mode.ToLower())
                {
                    case "ssh":
                        {
                            List<Task> tasks = new List<Task>();

                            foreach (var ip in Program.IPlist)
                            {
                                await semaphore.WaitAsync();
                                tasks.Add(Task.Run(async () =>
                                {
                                    try
                                    {

                                        BrotePacket($"{ip}:{22}");
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }));
                                await Task.Delay(Convert.ToInt32(Program.delay));
                            }
                            await Task.WhenAll(tasks);
                            break;
                        }
                    case "rdp":
                        {
                            List<Task> tasks = new List<Task>();

                            foreach (var ip in Program.IPlist)
                            {
                                await semaphore.WaitAsync();
                                tasks.Add(Task.Run(async () =>
                                {
                                    try
                                    {
                                        BrotePacket($"{ip}:{3389}");
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }));
                                await Task.Delay(Convert.ToInt32(Program.delay));
                            }
                            await Task.WhenAll(tasks);
                            break;
                        }
                    case "ms17010":
                        {
                            List<Task> tasks = new List<Task>();

                            foreach (var ip in Program.IPlist)
                            {
                                await semaphore.WaitAsync();
                                tasks.Add(Task.Run(async () =>
                                {
                                    try
                                    {
                                        new ms17_010scanner().Run(ip);
                                        // BrotePacket($"{ip}:{3389}");
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }));
                                await Task.Delay(Convert.ToInt32(Program.delay));
                            }
                            await Task.WhenAll(tasks);
                            break;
                        }

                    case "smb":
                        {
                            List<Task> tasks = new List<Task>();

                            foreach (var ip in Program.IPlist)
                            {
                                await semaphore.WaitAsync();
                                tasks.Add(Task.Run(async () =>
                                {
                                    try
                                    {
                                        SMBEnum.SMBLogin(ip);
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }));
                                await Task.Delay(Convert.ToInt32(Program.delay));
                            }
                            await Task.WhenAll(tasks);
                            break;
                            break;
                        }
                    case "ftp":
                        {
                            break;
                        }

                }
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
