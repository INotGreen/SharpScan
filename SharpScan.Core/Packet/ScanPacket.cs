using SharpHostInfo.Services;
using SharpScan.Plugins;
using SSharpScan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class ScanPacket
    {
        private static readonly int maxConcurrency = 10; // 最大并发数
        private static readonly int delay = 1000; // 延迟时间，单位为毫秒
        private static SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency);

        public async Task HandlePacket()
        {
            List<Task> portscanTasks = new List<Task>();

            // 先扫描135端口和445端口
            var highPriorityPorts = new[] { "135", "445" };
            var highPriorityIpPorts = Program.IpPortList
                .Where(ipPort => highPriorityPorts.Contains(ipPort.Split(':')[1]))
                .ToList();

            foreach (var IpPort in highPriorityIpPorts)
            {
                portscanTasks.Add(Task.Run(() => PacketWithSemaphore(IpPort)));
            }

            await Task.WhenAll(portscanTasks);

            // 清空任务列表以便处理其他端口
            portscanTasks.Clear();

            // 再扫描其他端口
            List<string> otherIpPorts = Program.IpPortList .Where(ipPort => !highPriorityPorts.Contains(ipPort.Split(':')[1])).ToList();

            foreach (string IpPort in otherIpPorts)
            {
                portscanTasks.Add(Task.Run(() => PacketWithSemaphore(IpPort)));
            }

            await Task.WhenAll(portscanTasks);
        }

        public static async Task PacketWithSemaphore(string IpPort)
        {
            await semaphore.WaitAsync(); // 等待信号量
            try
            {
                await PocPacket(IpPort);
            }
            finally
            {
                semaphore.Release(); // 释放信号量
                await Task.Delay(delay); // 延时
            }
        }

        public static async Task PocPacket(string IpPort)
        {
            string IP = IpPort.Split(':')[0];
            string Port = IpPort.Split(':')[1];
            switch (Port)
            {
                case "445":
                    {
                        SMB smb = new SMB();
                        bool success = smb.Execute(IP, 445, Convert.ToInt32(Program.Delay));
                        break;
                    }
                case "22":
                    {
                        //SSH弱口令
                        SshBrute.Run(IP);
                        break;
                    }
                case "135":
                    {
                        WMI wmi = new WMI();
                        wmi.Execute(IP, 135, Convert.ToInt32(Program.Delay));
                        break;
                    }
                case "137":
                    {
                        //NBNS nbns = new NBNS();
                        //nbns.Execute(IP, 137, Convert.ToInt32(Program.Delay), macdict);
                        break;
                    }
                default:
                    {
                        //获取web标签
                        string[] webPorts = Configuration.WebPort.Split(',');
                        if (webPorts.Contains(Port))
                        {
                            string url = WebTitle.BuildUrl(IP, Port);
                            WebTitle.Run(url);
                        }
                        break;
                    }
            }
        }
    }
}
