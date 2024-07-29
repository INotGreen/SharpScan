using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace SharpScan
{
    internal class RDPlog
    {
        public RDPlog()
        {
            string text = "========== RDP Report ==========\r\n";
            text = text + "Time: " + DateTime.Now.ToString() + "\r\n";
            text = text + GetRegRDPTcpPort() + "\r\n";
            //text += GetRegCurrentUserMstsc();
            text += GetRegAllUserKeylist();
            text += EventLog_4624();
            // text += EventLog_4625();
            Console.WriteLine(text);
        }

        public static string GetRegAllUserKeylist()
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("\r\n========== All Users cmdkey Cache Records ==========\r\n");
            foreach (ManagementBaseObject managementBaseObject in new ManagementClass("Win32_UserAccount").GetInstances())
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                string username = managementObject["Name"].ToString();
                string sid = managementObject["SID"].ToString();

                sb.AppendLine($"========== Username: {username} ==========");
                sb.AppendLine($"SID: {sid}\r\n");
                sb.AppendLine(GetRegUserKeylist(sid));
            }
            return sb.ToString();
        }
        protected static string Format(string args_1, string args_2) => String.Format("  [>] {0,-28}: {1}\r", args_1, args_2);
        public static string GetRegUserKeylist(string sid)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                RegistryKey users = Registry.Users;
                string name = sid + "\\SOFTWARE\\Microsoft\\Terminal Server Client\\Servers";
                RegistryKey registryKey = users.OpenSubKey(name, true);
                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    string username = registryKey.OpenSubKey(subKeyName).GetValue("UsernameHint").ToString();
                    sb.AppendLine(String.Format("[>] {0,-28}: {1}\r", subKeyName, username));
                }
            }
            catch (Exception)
            {
                sb.AppendLine("Fail to load Registry, This User No RDP Connections History");
            }
            return sb.ToString();
        }

        public static string GetRegRDPTcpPort()
        {
            string text = "\r\n========== Local RDP Port ==========\r\n";
            try
            {
                string port = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp", true).GetValue("PortNumber").ToString();
                text += port;
            }
            catch (Exception)
            {
                text += "Permission denied\r\n";
            }
            return text;
        }

        //public static string GetRegCurrentUserMstsc()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("\r\n========== Current User mstsc Connection History ==========\r\n");
        //    try
        //    {
        //        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true);
        //        foreach (string name in registryKey.GetValueNames())
        //        {
        //            string server = registryKey.GetValue(name).ToString();
        //            sb.AppendLine(server);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        sb.AppendLine("Permission denied");
        //    }
        //    return sb.ToString();
        //}

        public static string GetRegCurrentUserKeylist()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\r\n========== Current User cmdkey Cache Records ==========\r\n");
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Servers", true);
                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    string username = registryKey.OpenSubKey(subKeyName).GetValue("UsernameHint").ToString();
                    sb.AppendLine($"{subKeyName,-30} {username}");
                }
            }
            catch (Exception)
            {
                sb.AppendLine("Permission denied");
            }
            return sb.ToString();
        }

        public static string EventLog_4624()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                EventLog eventLog = new EventLog("Security");
                sb.AppendLine("\r\n========== Logon Success Event ID: 4624 ==========\r\n");

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
                        sb.AppendLine($"{entry.TimeGenerated,-20} {accountDomain,-20} {accountName,-20} {sourceNetworkAddress}");
                    }
                }
            }
            catch (Exception)
            {
                sb.AppendLine("Log is closed, no logs or permission denied");
            }
            return sb.ToString();
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
