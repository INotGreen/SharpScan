using SharpScan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class TcpPortscan
    {
        
        public async Task ScanPortDefault(int delay, Dictionary<string, int> PortList,int maxConcurrency)
        {
            List<Task> portscanTasks = new List<Task>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var onlineHost in Program.onlineHostList)
            {
                portscanTasks.Add(Task.Run(() => ScanPorts(onlineHost.IP, PortList, delay, maxConcurrency)));
            }

            await Task.WhenAll(portscanTasks);
            Console.WriteLine($"\n[+] Port Scanning completed in {(stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds\n");
            // 开始扫描Web端口
            await ScanWebPorts(delay, maxConcurrency);
            stopwatch.Stop();
            Console.WriteLine($"\n[+] WebPort Scanning completed in {(stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds\n");
        }


        public static async Task ScanPorts(string ip, Dictionary<string, int> PortList, int delay, int maxConcurrency)
        {
            List<Task> tasks = new List<Task>();

            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var portGroup in PortList)
                {
                    int port = portGroup.Value;
                    await semaphore.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using (TcpClient tcpClient = new TcpClient())
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
                                            Program.IpPortList.Add($"{ip}:{port}");
                                            Console.WriteLine($"[*] {ip}:{port} ({portGroup.Key}) is open");

                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Port is closed or not reachable
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
            }
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
                                        Program.alivePort++;
                                        Program.IpPortList.Add($"{IP}:{port}");
                                        Console.WriteLine($"[*] {IP}:{port}{Helper.GetServiceByPort(port)} is open");
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
            else if (int.TryParse(portRange, out int singlePort))
            {
                ports.Add(singlePort);
            }

            return ports.ToArray();
        }




        public static async Task ScanWebPorts(int delay, int maxConcurrency)
        {
            List<Task> webScanTasks = new List<Task>();
            string[] webPorts = Configuration.WebPort.Split(',');
            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var onlineHost in Program.onlineHostList)
                {
                    foreach (var port in webPorts)
                    {
                        if (int.TryParse(port, out int portNumber))
                        {
                            await semaphore.WaitAsync();

                            webScanTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    using (TcpClient tcpClient = new TcpClient())
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
                                                    Program.IpPortList.Add($"{onlineHost.IP}:{portNumber}");
                                                    Console.WriteLine($"[*] {onlineHost.IP}:{portNumber} (web) is open");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // Port is closed or not reachable
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
                    }
                }
                await Task.WhenAll(webScanTasks);
            }


        }
    }
}
