using SharpRDPCheck;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
                    //IcmpTasks.Add(Task.Run(() => Smblogin(Ip, Program.userName, Program.passWord)));
                }

                await Task.WhenAll(IcmpTasks);
            }
        }

        public static void SMBLogin(string ip)
        {
            int Port = 445;

            if (!Helper.TestPort(ip, Port))
            {
                return;
            }

            Console.WriteLine($"[*] {ip}:{Port}{Helper.GetServiceByPort(Port)} is brute force cracking in progress");

            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                string result = SMBLoginWorker(ip, Program.userName, Program.passWord);
                if (result.StartsWith("True"))
                {
                    string output = $"[*] (SMB)IP:{ip}  User:{Program.userName}  Password:{Program.passWord}, Result:{result}";
                    Console.WriteLine(output);
                    return;
                }
                
            }

            if (Program.userList != null && Program.passwordList != null)
            {
                foreach (var user in Program.userList)
                {
                    foreach (var pass in Program.passwordList)
                    {
                        string result = SMBLoginWorker(ip, user, pass);
                        if (result.StartsWith("True"))
                        {
                            string output = $"[*] (SMB)IP:{ip}  User:{user}  Password:{pass}, Result:{result}";
                            if (!Program.Service.Exists(service => service == output))
                            {
                                Program.Service.Add(output);
                                Console.WriteLine(output);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Configuration.UserDictionary.TryGetValue("smb", out List<string> smbUsers))
                {
                    foreach (var user in smbUsers)
                    {
                        foreach (var pass in Configuration.Passwords)
                        {
                            string result = SMBLoginWorker(ip, user, pass);
                            if (result.StartsWith("True"))
                            {
                                string output = $"[*] (SMB)IP:{ip}  User:{user}  Password:{pass}, Result:{result}";
                                if (!Program.Service.Exists(service => service == output))
                                {
                                    Program.Service.Add(output);
                                    Console.WriteLine(output);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Username list for SMB service not found.");
                }
            }
        }

        static string SMBLoginWorker(string host, string user, string pass)
        {
            user = user.Replace("^\\.\\", $"{host}\\");
            SecureString securePass = new SecureString();
            foreach (char c in pass)
            {
                securePass.AppendChar(c);
            }

            IntPtr unmanagedPassword = IntPtr.Zero;
            try
            {
                unmanagedPassword = Marshal.SecureStringToGlobalAllocUnicode(securePass);

                string networkPath = $"\\\\{host}\\Admin$";
                int result = WNetAddConnection2(new NETRESOURCE
                {
                    lpRemoteName = networkPath,
                    lpProvider = null
                }, Marshal.PtrToStringUni(unmanagedPassword), user, 0);

                if (result == 0)
                {
                    WNetCancelConnection2(networkPath, 0, true);
                    return "True,admin";
                }
                else
                {
                    switch (result)
                    {
                        case 86: // ERROR_INVALID_PASSWORD
                            return "False";
                        case 5:  // ERROR_ACCESS_DENIED
                            return "True";
                        default:
                            return "Error";
                    }
                }
            }
            catch
            {
                return "Error";
            }
            finally
            {
                if (unmanagedPassword != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedPassword);
                }
            }
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NETRESOURCE netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 1; // RESOURCETYPE_DISK
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName;
            public string lpComment = null;
            public string lpProvider = null;
        }
    }
}
