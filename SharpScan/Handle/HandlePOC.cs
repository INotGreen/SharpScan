using SharpScan.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
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
            await ProcessBrotePackets(Program.IpPortList);
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
                        catch (Exception ex)
                        {
                            // Console.WriteLine($"[!] Error processing {ipPort}: {ex.Message}");
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

        private async Task ProcessBrotePackets(List<string> ipPortList)
        {
            await ProcessSpecificBrotePackets(ipPortList, "22"); // SSH
            await ProcessSpecificBrotePackets(ipPortList, "445"); // SMB
            await ProcessSpecificBrotePackets(ipPortList, "1433"); // RDP
            await ProcessSpecificBrotePackets(ipPortList, "21"); // FTP
        }

        private async Task ProcessSpecificBrotePackets(List<string> ipPortList, string port)
        {
            List<Task> tasks = new List<Task>();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.maxConcurrency)))
            {
                foreach (var ipPort in ipPortList.Where(ipPort => ipPort.EndsWith($":{port}")))
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await BrotePacket(ipPort);
                        }
                        catch (Exception ex)
                        {
                            // Console.WriteLine($"[!] Error processing {ipPort}: {ex.Message}");
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
                case "21":
                    {
                        await Task.Run(() => Ftp.Run(ip)); // 使用 Task.Run 包装同步方法
                        break;
                    }
                case "22":
                    {
                        //SSH 弱口令
                        await Task.Run(() => SshBrute.Run(ip)); // 使用 Task.Run 包装同步方法
                        break;
                    }
                case "445":
                    {
                        await Task.Run(() => SMBEnum.SMBLogin(ip)); // 使用 Task.Run 包装同步方法
                        break;
                    }
                case "3389":
                    {
                        await Task.Run(() => Rdp.Run(ip)); // 使用 Task.Run 包装同步方法
                        break;
                    }
                case "1433":
                    {
                        await Task.Run(() => MsSqlBroute.Run(ip));
                        break;
                    }
            }
        }

        public static async Task ModPacket(string mode)
        {
            using (SemaphoreSlim semaphore = new SemaphoreSlim(Convert.ToInt32(Program.maxConcurrency)))
            {
                List<Task> tasks = new List<Task>();

                foreach (var ip in Program.IPlist)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            switch (mode.ToLower())
                            {
                                case "ssh":
                                    {
                                        await Task.Run(() => SshBrute.Run(ip));
                                        break;
                                    }
                                   
                                case "rdp":
                                    {
                                        await Task.Run(() => Rdp.Run(ip));
                                        
                                        break;
                                    }
                                   
                                case "ms17010":
                                    {
                                        await Task.Run(() => new ms17_010scanner().Run(ip));
                                        break;
                                    }
                                   
                                case "smb":
                                    {
                                        await Task.Run(() => SMBEnum.SMBLogin(ip));
                                        break;
                                    }

                                case "ftp":
                                    {
                                        await Ftp.Run(ip);
                                        break;
                                    }

                                case "wmiexec":
                                    {
                                        await Task.Run(() => WmiExec.Run(ip));
                                        break;
                                    }
                                case "mssql":
                                    {
                                        await Task.Run(() => MsSqlBroute.Run(ip));
                                        break;
                                    }

                            }
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

        public static async Task PocPacket(string ipPort)
        {
            string ip = ipPort.Split(':')[0];
            string port = ipPort.Split(':')[1];
            switch (port)
            {
                case "445":
                    {
                        if (Program.onlineHostList.Exists(onlinepc => onlinepc.IP == ip && onlinepc.buildNumber >= 18362))
                        {
                            await Task.Run(() => new SmbGhost().Run(ip));
                        }
                        else
                        {
                            await Task.Run(() => new ms17_010scanner().Run(ip));
                        }

                        break;
                    }
                default:
                    {
                        // 获取 web 标签
                        string[] webPorts = Configuration.WebPort.Split(',');
                        if (webPorts.Contains(port))
                        {
                            string url = WebTitle.BuildUrl(ip, port);
                            await Task.Run(() => WebTitle.Run(url));
                        }
                        break;
                    }
            }
        }
    }
}
