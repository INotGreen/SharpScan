using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading.Tasks;

namespace SharpScan
{
    public class SMBEnum
    {
        public async Task Smblogin(List<string> IPlist)
        {
            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                List<Task> IcmpTasks = new List<Task>();
                foreach (var Ip in IPlist)
                {
                    IcmpTasks.Add(Task.Run(() => Smblogin(Ip, Program.userName, Program.passWord)));
                }

                await Task.WhenAll(IcmpTasks);
            }
        }
        public static async Task Smblogin(string ip, string user, string pass)
        {
            string userm = user.Replace("\\", "\\\\").Replace(".", "\\.");

            if (!await WorkerTestPort(ip, 445))
            {
                Console.WriteLine($"{ip},445,Port unreachable");
                return;
            }

            string output = $"{ip},{user},{pass},";
            output += await SmbloginWorker(ip, user, pass);
            Console.WriteLine(output);
        }

        public static async Task<string> SmbloginWorker(string host, string user, string pass)
        {
            user = user.Replace("^.\\", $"{host}\\");
            SecureString securePassword = new SecureString();
            foreach (char c in pass)
            {
                securePassword.AppendChar(c);
            }

            string userName = user.Contains("\\") ? user.Split('\\')[1] : user;
            string domainName = user.Contains("\\") ? user.Split('\\')[0] : string.Empty;

            try
            {
                ConnectionOptions options = new ConnectionOptions
                {
                    Username = userName,
                    Password = new NetworkCredential("", pass).Password,
                    Authority = $"ntlmdomain:{domainName}",
                    Impersonation = ImpersonationLevel.Impersonate
                };

                ManagementScope scope = new ManagementScope($@"\\{host}\root\cimv2", options);
                await Task.Run(() => scope.Connect());

                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Share WHERE Name = 'Admin$'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = await Task.Run(() => searcher.Get());

                if (queryCollection.Count > 0)
                {
                    return "True,admin";
                }
                else
                {
                    return "False";
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[UnauthorizedAccessException] {ex.Message}");
                return "False"; // Access is denied and credentials are incorrect
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] {ex.Message}");
                return "Error";
            }
        }

        public static async Task<bool> WorkerTestPort(string remoteHost, int remotePort)
        {
            int timeout = 500; // 0.5 second
            try
            {
                using (TcpClient t = new TcpClient())
                {
                    var result = t.BeginConnect(remoteHost, remotePort, null, null);
                    var success = await Task.Run(() => result.AsyncWaitHandle.WaitOne(timeout));
                    if (!success)
                    {
                        t.Close();
                        return false;
                    }

                    t.EndConnect(result);
                    t.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorkerTestPort Exception] {ex.Message}");
                return false;
            }
        }
    }

}
