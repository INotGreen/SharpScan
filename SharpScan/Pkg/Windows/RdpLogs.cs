using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace SharpScan
{
    internal class RDPlog
    {
        public RDPlog()
        {
            try
            {
                Console.WriteLine("RDP Report");
                Console.WriteLine($"Time: {DateTime.Now}");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine(GetRegRDPTcpPort());
                Console.WriteLine(new string('-', 95));
                Console.WriteLine(GetRegAllUserKeylist());
                Console.WriteLine(new string('-', 95));
                Console.WriteLine(EventLog_4624());
                Console.WriteLine(new string('-', 95));
            }
            catch { }
        }

        public static string GetRegAllUserKeylist()
        {
            foreach (ManagementBaseObject managementBaseObject in new ManagementClass("Win32_UserAccount").GetInstances())
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                string username = managementObject["Name"].ToString();
                string sid = managementObject["SID"].ToString();

                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"SID: {sid}");
                Console.WriteLine(GetRegUserKeylist(sid));
            }
            return string.Empty;  // 返回空字符串
        }

        public static string GetRegUserKeylist(string sid)
        {
            try
            {
                RegistryKey users = Registry.Users;
                string name = $"{sid}\\SOFTWARE\\Microsoft\\Terminal Server Client\\Servers";
                RegistryKey registryKey = users.OpenSubKey(name, true);
                Console.WriteLine("Server IP                       : HostName");
                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    string username = registryKey.OpenSubKey(subKeyName).GetValue("UsernameHint").ToString();
                    Console.WriteLine($"{subKeyName,-30}: {username}");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load Registry. No RDP Connections History for this user.");
            }
            return string.Empty;  // 返回空字符串
        }

        public static string GetRegRDPTcpPort()
        {
            Console.WriteLine("Local RDP Port");
            try
            {
                string port = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp", true).GetValue("PortNumber").ToString();
                Console.WriteLine($"Port Number: {port}");
            }
            catch (Exception)
            {
                Console.WriteLine("Permission denied");
            }
            return string.Empty;  // 返回空字符串
        }

        public static string GetRegCurrentUserMstsc()
        {
            Console.WriteLine("Current User mstsc Connection History");
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true);
                foreach (string name in registryKey.GetValueNames())
                {
                    string server = registryKey.GetValue(name).ToString();
                    Console.WriteLine(server);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Permission denied");
            }
            return string.Empty;  // 返回空字符串
        }

        public static string GetRegCurrentUserKeylist()
        {
            Console.WriteLine("Current User cmdkey Cache Records");
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Servers", true);
                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    string username = registryKey.OpenSubKey(subKeyName).GetValue("UsernameHint").ToString();
                    Console.WriteLine($"{subKeyName,-30}: {username}");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Permission denied");
            }
            return string.Empty;  // 返回空字符串
        }

        public static string EventLog_4624()
        {
            try
            {
                EventLog eventLog = new EventLog("Security");
                Console.WriteLine("Logon Success Event ID: 4624");
                Console.WriteLine("TimeGenerated            AccountDomain          AccountName            SourceNetworkAddress");

                var logEntries = from EventLogEntry entry in eventLog.Entries
                                 where entry.InstanceId == 4624L
                                 select entry;

                foreach (var entry in logEntries)
                {
                    string message = entry.Message;
                    string sourceNetworkAddress = MidStrEx(message, "\tSource Network Address:\t", "\tSource Port:");
                    string logonDetails = MidStrEx(message, "New Logon:", "Process Information:");
                    string accountDomain = MidStrEx(logonDetails, "Account Domain:", "Logon ID:");
                    string accountName = MidStrEx(logonDetails, "Account Name:", "Account Domain:").Trim();

                    if (sourceNetworkAddress.Length >= 7)
                    {
                        Console.WriteLine($"{entry.TimeGenerated,-20} {accountDomain,-20} {accountName,-20} {sourceNetworkAddress}");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Log is closed, no logs or permission denied");
            }
            return string.Empty;  // 返回空字符串
        }

        public static string MidStrEx(string source, string startStr, string endStr)
        {
            string result = string.Empty;
            int startIndex = source.IndexOf(startStr);
            if (startIndex == -1) return result.Trim();

            string temp = source.Substring(startIndex + startStr.Length);
            int endIndex = temp.IndexOf(endStr);
            if (endIndex == -1) return result.Trim();

            result = temp.Remove(endIndex);
            return result.Trim();
        }
    }
}
