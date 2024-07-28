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
            text += GetRegCurrentUserMstsc();
            text += GetRegAllUserKeylist();
            text += EventLog_4624();
            text += EventLog_4625();
            Console.WriteLine(text);
        }

        public static string GetRegAllUserKeylist()
        {
            string text = "";
            text += "\r\n========== All Users cmdkey Cache Records ==========\r\n";
            foreach (ManagementBaseObject managementBaseObject in new ManagementClass("Win32_UserAccount").GetInstances())
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                string str = managementObject["Name"].ToString();
                text = text + "\r\n========== Username: " + str + " ==========\r\n";
                string text2 = managementObject["SID"].ToString();
                text = text + "SID: " + text2 + "\r\n";
                text = text + "\r\n" + GetRegUserKeylist(text2) + "\r\n";
            }
            return text;
        }

        public static string GetRegUserKeylist(string sid)
        {
            string text = "";
            try
            {
                RegistryKey users = Registry.Users;
                string name = sid + "\\SOFTWARE\\Microsoft\\Terminal Server Client\\Servers";
                RegistryKey registryKey = users.OpenSubKey(name, true);
                foreach (string text2 in registryKey.GetSubKeyNames())
                {
                    string str = registryKey.OpenSubKey(text2).GetValue("UsernameHint").ToString();
                    text = text + "Server: " + text2 + "\r\n";
                    text = text + "Username: " + str + "\r\n";
                    text += "\r\n-----------------------------------\r\n";
                }
            }
            catch (Exception)
            {
                text += "\r\nFail to load Registry, This User No RDP Connections History\r\n";
            }
            return text;
        }

        public static string GetRegRDPTcpPort()
        {
            string text = "";
            text += "\r\n========== Local RDP Port ==========\r\n";
            try
            {
                string str = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp", true).GetValue("PortNumber").ToString();
                text += str;
            }
            catch (Exception)
            {
                text += "Permission denied\r\n";
            }
            return text;
        }

        public static string GetRegCurrentUserMstsc()
        {
            string text = "";
            text += "\r\n========== Current User mstsc Connection History ==========\r\n";
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true);
                foreach (string name in registryKey.GetValueNames())
                {
                    string str = registryKey.GetValue(name).ToString();
                    text = text + "Server: " + str + "\r\n";
                }
            }
            catch (Exception)
            {
                text += "Permission denied\r\n";
            }
            return text;
        }

        public static string GetRegCurrentUserKeylist()
        {
            string text = "";
            text += "\r\n========== Current User cmdkey Cache Records ==========\r\n";
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Servers", true);
                foreach (string text2 in registryKey.GetSubKeyNames())
                {
                    string str = registryKey.OpenSubKey(text2).GetValue("UsernameHint").ToString();
                    text = text + "Server: " + text2 + "\r\n";
                    text = text + "Username: " + str + "\r\n";
                    text += "\r\n-----------------------------------\r\n";
                }
            }
            catch (Exception)
            {
                text += "\r\nPermission denied\r\n";
            }
            return text;
        }

        public static string EventLog_4624()
        {
            string text = "";
            try
            {
                EventLog eventLog = new EventLog("Security");
                text += "\r\n========== Logon Success Event ID: 4624 ==========\r\n";
                try
                {
                    IEnumerable<EventLogEntry> enumerable = from EventLogEntry x in eventLog.Entries
                                                            where x.InstanceId == 4624L
                                                            select x;
                    (from x in enumerable
                     select new
                     {
                         x.MachineName,
                         x.Site,
                         x.Source,
                         x.Message,
                         x.TimeGenerated
                     }).ToList();
                    foreach (EventLogEntry eventLogEntry in enumerable)
                    {
                        string message = eventLogEntry.Message;
                        string text2 = MidStrEx(message, "\tSource Network Address:\t", "\tSource Port:");
                        string sourse = MidStrEx(message, "New Logon:", "Process Information:");
                        string str = MidStrEx(sourse, "Account Domain:", "Logon ID:");
                        string text3 = MidStrEx(sourse, "Account Name:", "Account Domain:");
                        if (text3.Length == 0)
                        {
                            text3 = MidStrEx(sourse, "Account Name:", "Account Domain:");
                        }
                        DateTime timeGenerated = eventLogEntry.TimeGenerated;
                        if (text2.Length >= 7)
                        {
                            text += "\r\n-----------------------------------\r\n";
                            text = text + "\r\nTime: " + timeGenerated.ToString() + "\r\n";
                            text = text + "Domain: " + str + "\r\n";
                            text = text + "Username: " + text3.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "") + "\r\n";
                            text = text + "Remote ip: " + text2.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "") + "\r\n";
                        }
                    }
                }
                catch (Exception)
                {
                    text += "Permission denied\r\n";
                }
            }
            catch (Exception)
            {
                text += "Log is closed, no logs\r\n";
            }
            return text;
        }

        public static string EventLog_4625()
        {
            string text = "";
            try
            {
                EventLog eventLog = new EventLog("Security");
                text += "\r\n========== Logon Failure Event ID: 4625 ==========\r\n";
                try
                {
                    IEnumerable<EventLogEntry> enumerable = from EventLogEntry x in eventLog.Entries
                                                            where x.InstanceId == 4625L
                                                            select x;
                    (from x in enumerable
                     select new
                     {
                         x.MachineName,
                         x.Site,
                         x.Source,
                         x.Message,
                         x.TimeGenerated
                     }).ToList();
                    foreach (EventLogEntry eventLogEntry in enumerable)
                    {
                        string message = eventLogEntry.Message;
                        string text2 = MidStrEx(message, "\tSource Network Address:\t", "\tSource Port:");
                        string sourse = MidStrEx(message, "New Logon:", "Process Information:");
                        string str = MidStrEx(sourse, "Account Domain:", "Logon ID:");
                        string text3 = MidStrEx(sourse, "Account Name:", "Account Domain:");
                        if (text3.Length == 0)
                        {
                            text3 = MidStrEx(sourse, "Account Name:", "Account Domain:");
                        }
                        DateTime timeGenerated = eventLogEntry.TimeGenerated;
                        if (text2.Length >= 7)
                        {
                            text += "\r\n-----------------------------------\r\n";
                            text = text + "\r\nTime: " + timeGenerated.ToString() + "\r\n";
                            text = text + "Domain: " + str + "\r\n";
                            text = text + "Username: " + text3.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "") + "\r\n";
                            text = text + "Remote ip: " + text2.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "") + "\r\n";
                        }
                    }
                }
                catch (Exception)
                {
                    text += "Permission denied\r\n";
                }
            }
            catch (Exception)
            {
                text += "Log is closed, no logs\r\n";
            }
            return text;
        }

        public static string MidStrEx(string sourse, string startstr, string endstr)
        {
            string text = string.Empty;
            int num = sourse.IndexOf(startstr);
            if (num == -1)
            {
                return text.Trim();
            }
            string text2 = sourse.Substring(num + startstr.Length);
            int num2 = text2.IndexOf(endstr);
            if (num2 == -1)
            {
                return text.Trim();
            }
            text = text2.Remove(num2);
            return text.Trim();
        }

    }

}
