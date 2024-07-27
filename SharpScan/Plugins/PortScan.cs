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
    internal class Portscan
    {
        public static Dictionary<string, int> PortGroup = new Dictionary<string, int>
        {
            { "ftp", 21 }, { "ssh", 22 }, { "telnet", 23 }, { "smtp", 25 },
            { "dns", 53 }, { "http", 80 }, { "pop3", 110 }, { "ntp", 123 },
            { "imap", 143 }, { "snmp", 161 }, { "ldap", 389 }, { "https", 443 },
            { "smb", 445 }, { "mssql", 1433 }, { "oracle", 1521 }, { "mysql", 3306 },
            { "rdp", 3389 }, { "psql", 5432 }, { "redis", 6379 }, { "fcgi", 9000 },
            { "mem", 11211 }, { "mgo", 27017 }, { "vnc", 5900 }, { "sip", 5060 },
            { "mqtt", 1883 }, { "nfs", 2049 }, { "msrpc", 135 }, { "netbios", 139 },
            { "rpcbind", 111 }, { "snmptrap", 162 }, { "syslog", 514 }, { "tftp", 69 },
            { "kerberos", 88 }, { "smtps", 465 }, { "imaps", 993 }, { "pop3s", 995 },
            { "socks", 1080 }, { "ldaps", 636 }, { "ftps", 990 }, { "nntp", 119 },
            { "rsh", 514 }, { "exec", 512 }, { "login", 513 }, { "printer", 515 },
            { "nntps", 563 }, { "rtsp", 554 }, { "sip-tls", 5061 }, { "cifs", 3020 },
            { "msp", 18 }, { "bgp", 179 }, { "isakmp", 500 }, { "ldp", 646 },
            { "kpasswd", 464 }, { "submissions", 587 }, { "ircd", 6667 }, { "afs", 7000 },
            { "kerberos-adm", 749 }, { "kerberos-sec", 750 }, { "kerberos-pwd", 751 },
            { "klogin", 543 }, { "kshell", 544 }, { "knetd", 2053 }, { "dhcpv6-client", 546 },
            { "dhcpv6-server", 547 }, { "ntalk", 518 }, { "rtelnet", 107 }, { "sip-tcp", 5061 },
            { "sip-udp", 5060 }, { "ftam", 62 }, { "nfa", 115 }, { "imap3", 220 },
            { "sqlnet", 1500 }, { "vxlan", 4789 }, { "cmip-agent", 164 }, { "cmip-man", 163 },
            { "rsync", 873 }, { "alt-https", 8443 }, { "oracle-oms", 1158 }, { "apple-sasl", 3659 }
        };


        public async Task ScanPortAsync(int delay, int maxConcurrency)
        {
            List<Task> portscanTasks = new List<Task>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var onlineHost in Program.HostList)
            {
                portscanTasks.Add(Task.Run(() => ScanPorts(onlineHost.IP, delay, maxConcurrency)));
            }

            await Task.WhenAll(portscanTasks);
            Console.WriteLine($"\n[+] Port Scanning completed in {(stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds\n");
            // 开始扫描Web端口
            await ScanWebPorts(delay, maxConcurrency);
            stopwatch.Stop();
            Console.WriteLine($"\n[+] WebPort Scanning completed in {(stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds\n");
        }


        public static async Task ScanPorts(string ip, int delay, int maxConcurrency)
        {
            List<Task> tasks = new List<Task>();

            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var portGroup in PortGroup)
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
                                            Console.WriteLine($"{ip}:{port} ({portGroup.Key}) is open");

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
                                        Console.WriteLine($"[+] {IP}:{port}{GetServiceByPort(port)} is open");
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

        public static string GetServiceByPort(int port)
        {
            var service = Configuration.PortList.FirstOrDefault(p => p.Value == port).Key;
            if (service != null)
            {
                return $" ({service})";
            }
            return "";


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



        public static async Task ScanWebPorts(int delay, int maxConcurrency)
        {
            List<Task> webScanTasks = new List<Task>();
            string[] webPorts = Configuration.WebPort.Split(',');
            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
            {
                foreach (var onlineHost in Program.HostList)
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
                                                    Console.WriteLine($"{onlineHost.IP}:{portNumber} (web) is open");
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
