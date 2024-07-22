using SharpScan.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class ScanPacket
    {
        public async Task HandlePacket()
        {
            List<Task> portscanTasks = new List<Task>();

            foreach (var IpPort in Program.IpPortList)
            {
                portscanTasks.Add(Task.Run(() => ScanPacket.Packet(IpPort)));
            }
            await Task.WhenAll(portscanTasks);
        }


        public static async Task Packet(string IpPort)
        {

            string IP = IpPort.Split(':')[0];
            string Port = IpPort.Split(':')[1];
            switch (Port)
            {
                case "445":
                    {
                        //验证永恒之蓝漏洞
                        new ms17_010scanner().Run(IP);
                       // new SMB().SMBScan();
                        break;
                    }
                case "22":
                    {
                        //SSH弱口令
                        SshBrute.Run(IP);
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
