using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;

namespace SMBLogin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: smblogin <ip> <user> <password>");
                Console.WriteLine(" e.g.: smblogin 192.168.1.1 .\\Administrator P@ssw0rd");
                return;
            }

            string ip = args[0];
            string user = args[1];
            string pass = args[2];

            string result = SMBLogin(ip, user, pass);
            Console.WriteLine(result);
        }

        static string SMBLogin(string ip, string user, string pass)
        {
            if (!TestPort(ip, 445))
            {
                return $"{ip},445,Port unreachable";
            }

            string output = $"{ip},{user},{pass},";
            output += SMBLoginWorker(ip, user, pass);
            return output;
        }

        static bool TestPort(string remoteHost, int remotePort)
        {
            int timeout = 3000; // 3 seconds
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult result = client.BeginConnect(remoteHost, remotePort, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeout, false);
                    if (!success)
                    {
                        client.Close();
                        return false;
                    }
                    client.EndConnect(result);
                }
                return true;
            }
            catch
            {
                return false;
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

            // Convert SecureString to IntPtr
            IntPtr unmanagedPassword = IntPtr.Zero;
            try
            {
                unmanagedPassword = Marshal.SecureStringToGlobalAllocUnicode(securePass);

                // Attempt to create a network connection
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

        // P/Invoke for WNetAddConnection2
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
