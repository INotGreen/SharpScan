using SharpHostInfo.Services;
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
            List<Task> Task1 = new List<Task>();
            foreach (var IpPort in Program.IpPortList)
            {
                Task1.Add(Task.Run(() => ServicePacket(IpPort)));
            }

            await Task.WhenAll(Task1);



            List<Task> Task2 = new List<Task>();
            foreach (string IpPort in Program.IpPortList)
            {
                Task2.Add(Task.Run(() => PocPacket(IpPort)));
            }
            await Task.WhenAll(Task2);




            List<Task> Task3 = new List<Task>();
            foreach (string IpPort in Program.IpPortList)
            {
                Task3.Add(Task.Run(() => BrotePacket(IpPort)));
            }
            await Task.WhenAll(Task3);

            
        }

        public static async Task ServicePacket(string IpPort)
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

                case "135":
                    {
                        WMI wmi = new WMI();
                        wmi.Execute(IP, 135, Convert.ToInt32(Program.Delay));
                        break;
                    }
                case "137":
                    {
                        Dictionary<string, string> macdict = GetIP.GetMACDict();
                        NBNS nbns = new NBNS();
                        nbns.Execute(IP, 137, Convert.ToInt32(Program.Delay), macdict);
                        break;
                    }
            }
        }


        public static async Task BrotePacket(string IpPort)
        {
            string IP = IpPort.Split(':')[0];
            string Port = IpPort.Split(':')[1];
            switch (Port)
            {
                case "22":
                    {
                        //SSH弱口令
                        SshBrute.Run(IP);
                        break;
                    }
                //case "445":
                //    {
                //        Smblogin
                //    }
               
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
                        new ms17_010scanner().Run(IP);
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
