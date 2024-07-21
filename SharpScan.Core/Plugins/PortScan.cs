using SharpScan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class Portscan
    {
        public static Dictionary<string, int> PortGroup = new Dictionary<string, int>
        {
            { "ftp", 21 }, { "ssh", 22 }, { "findnet", 135 }, { "netbios", 139 }, { "smb", 445 },
            { "mssql", 1433 }, { "oracle", 1521 }, { "mysql", 3306 }, { "rdp", 3389 }, { "psql", 5432 },
            { "redis", 6379 }, { "fcgi", 9000 }, { "mem", 11211 }, { "mgo", 27017 }
        };

        public async Task ScanPortAsync()
        {
            List<Task> portscanTasks = new List<Task>();

            foreach (var onlineHost in Program.HostList)
            {
                portscanTasks.Add(Task.Run(() => ScanPorts(onlineHost.IP)));
            }

            await Task.WhenAll(portscanTasks);

            // 开始扫描Web端口
            await ScanWebPorts();
        }

        public static async Task ScanPorts(string ip)
        {
            List<Task> tasks = new List<Task>();

            foreach (var portGroup in PortGroup)
            {

                int port = portGroup.Value;
                tasks.Add(Task.Run(async () =>
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        try
                        {
                            var connectTask = Task.Factory.FromAsync(
                                tcpClient.BeginConnect(ip, port, null, null),
                                tcpClient.EndConnect);
                            if (await Task.WhenAny(connectTask, Task.Delay(1200)) == connectTask)
                            {
                                if (tcpClient.Connected)
                                {
                                    if (!Program.IpPortList.Exists(client => client == $"{ip}:{port}"))
                                    {
                                        Program.alivePort++;
                                        Console.WriteLine($"{ip}:{port} ({portGroup.Key}) is open");
                                        Program.IpPortList.Add($"{ip}:{port}");
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Port is closed or not reachable
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
     

        
        public async Task ScanPortRange(string IP, string PortRange, int delay, int MaxConcurrency)
        {
            List<Task> tasks = new List<Task>();
            int[] Ports = ParsePortRange(PortRange);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrency))
            {
                foreach (var port in Ports)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using (TcpClient tcpClient = new TcpClient())
                            {
                                var connectTask = Task.Factory.FromAsync(
                                    tcpClient.BeginConnect(IP, port, null, null),
                                    tcpClient.EndConnect);

                                if (await Task.WhenAny(connectTask, Task.Delay(1200)) == connectTask)
                                {
                                    if (tcpClient.Connected)
                                    {
                                        Console.WriteLine($"[+] {IP}:{port} is open");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exception (if necessary)
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                        if (delay > 0)
                        {
                            await Task.Delay(delay);
                        }
                    }));
                }
                await Task.WhenAll(tasks);
                stopwatch.Stop();
                Console.WriteLine($"\n[+] Scanning completed in {(stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds");
            }
        }


        static int[] ParsePortRange(string portRange)
        {
            List<int> ports = new List<int>();

            if (string.IsNullOrEmpty(portRange))
                return ports.ToArray();

            if (portRange.Contains("-"))
            {
                var parts = portRange.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        ports.Add(i);
                    }
                }
            }
            else if (portRange.Contains(","))
            {
                var parts = portRange.Split(',');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int port))
                    {
                        ports.Add(port);
                    }
                }
            }

            return ports.ToArray();
        }



        private static async Task ScanWebPorts()
        {
            List<Task> webScanTasks = new List<Task>();
            string[] webPorts = Configuration.WebPort.Split(',');

            foreach (var onlineHost in Program.HostList)
            {
                foreach (var port in webPorts)
                {
                    int portNumber;
                    if (int.TryParse(port, out portNumber))
                    {
                        webScanTasks.Add(Task.Run(async () =>
                        {
                            using (TcpClient tcpClient = new TcpClient())
                            {
                                try
                                {
                                    var connectTask = Task.Factory.FromAsync(
                                        tcpClient.BeginConnect(onlineHost.IP, portNumber, null, null),
                                        tcpClient.EndConnect);
                                    if (await Task.WhenAny(connectTask, Task.Delay(1200)) == connectTask)
                                    {
                                        if (tcpClient.Connected)
                                        {
                                            if (!Program.IpPortList.Exists(client => client == $"{onlineHost.IP}:{portNumber}"))
                                            {
                                                Program.alivePort++;
                                                Program.IpPortList.Add($"{onlineHost.IP}:{portNumber.ToString()}");
                                                Console.WriteLine($"{onlineHost.IP}:{portNumber} (web) is open");
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // Port is closed or not reachable
                                }
                            }
                        }));
                    }
                }
            }

            await Task.WhenAll(webScanTasks);
        }
    }
}
