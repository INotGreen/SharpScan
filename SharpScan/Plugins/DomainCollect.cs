using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.DirectoryServices.AccountManagement;
using Microsoft.VisualBasic.Devices;
using System.DirectoryServices;
using System.Management;

namespace SharpScan.Plugins
{
    internal class DomainCollect
    {
        public DomainCollect()
        {
            try
            {
                SystemInfo();
                NetworkConnections();
                ReadRegistry();
                Domain_p();
            }
            catch (Exception ex) { }
        }


        public static void SystemInfo()
        {
            // Get system information
            Console.WriteLine("==========Basic Information==========\n");
            var operating_system = Environment.OSVersion;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            Console.WriteLine("[+] Machine Name: " + Environment.MachineName);
            Console.WriteLine("[+] Domain Name: " + Environment.UserDomainName);
            Console.WriteLine("[+] Current User: " + Environment.UserName);
            Console.WriteLine("[+] .NET Version: {0}", Environment.Version.ToString());
            Console.WriteLine("[+] Operating System: " + new ComputerInfo().OSFullName + (Is64Bit() ? " 64-bit" : " 32-bit")); // Operating System
            new EDRCheck();
            List<string> users = GetLocalUsers();
            Console.Write("[+] Existing Users: ");
            foreach (var user in users)
            {
                Console.Write(user + " | ");
            }
            Console.WriteLine("\n");
            new RDPlog();
        }

        public static bool Is64Bit()
        {
            return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").Contains("64");
        }

        static List<string> GetLocalUsers()
        {
            List<string> users = new List<string>();
            try
            {
                string query = "SELECT * FROM Win32_UserAccount WHERE LocalAccount=True";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject obj in searcher.Get())
                {
                    users.Add(obj["Name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving users: {ex.Message}");
                return null;
            }
            return users;
        }

        public static void Domain_p() // Invoke Domain to detect information within the domain
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            if (properties.DomainName.Length > 0)
            {
                Console.WriteLine("\n[+] This host is in a domain! Domain name: {0}", properties.DomainName);
                DoIt();
                new Domain();
                Console.WriteLine("\n");
                // ZeroLogon.ZeroLogonCheck();
            }
            else
            {
                Console.WriteLine("\n[-] This host is not in a domain, workgroup information collection completed~~");
            }
        }

        public static void DoIt() // Locate Domain Controller IP
        {
            DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
            string dnsHostname = dirEntry.Properties["dnsHostname"].Value.ToString();
            Console.WriteLine("[+] Domain Controller FQDN: " + dnsHostname);
            IPAddress[] ipAddresses = Dns.GetHostAddresses(dnsHostname);
            Console.WriteLine("\n[+] Domain Controller IP: ");
            foreach (IPAddress i in ipAddresses)
            {
                Console.WriteLine(i);
            }
        }

        public static void ReadRegistry()
        {
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey SYS = rk.OpenSubKey("system").OpenSubKey("CurrentControlSet").OpenSubKey("Control").OpenSubKey("Terminal Server");
            Console.Write("[+] RDP Information:");
            foreach (string b in SYS.GetValueNames()) // Use shell.getvaluenames() here instead of shell.getsubkeynames() 
            {
                string a = SYS.GetValue(b).ToString();
                if (b == "fDenyTSConnections")
                {
                    string e = SYS.GetValue(b).ToString();
                    int num = int.Parse(e);
                    if (num == 1)
                    {
                        Console.WriteLine("\t RDP is not enabled");
                    }
                    else
                    {
                        Console.WriteLine("\t RDP is enabled");
                    }
                }
            }
        }

        public static void NetworkConnections()
        {
            // NETWORK CONNECTIONS
            Console.WriteLine("\n[+] Network Connection Status:");
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = ipProperties.GetActiveTcpListeners();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation info in tcpConnections)
            {
                String str = info.LocalEndPoint.Address.ToString();
                if (str.StartsWith("127.0.0.1"))
                {
                    continue;
                }
                Console.WriteLine("\tLocal: " + info.LocalEndPoint.Address.ToString() + ":" + info.LocalEndPoint.Port.ToString() + " - Remote: " + info.RemoteEndPoint.Address.ToString() + ":" + info.RemoteEndPoint.Port.ToString());
            }
        }


    }
}
