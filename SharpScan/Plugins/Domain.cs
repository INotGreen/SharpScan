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
using System.DirectoryServices.ActiveDirectory;

namespace SharpScan
{
    internal class Domain_c
    {
        public Domain_c()
        {
            try
            {
                SystemInfo();
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
                //Console. WriteLine($"\n[+] This host is in a domain! Domain name: {properties.DomainName}",ConsoleColor.Red );
                
                if(DoIt()) new GetDomainInfo();

                Console.WriteLine("\n");
               // ZeroLogon.ZeroLogonCheck();
            }
            else
            {
                Console.WriteLine("\n[-] This host is not in a domain, workgroup information collection completed~~");
            }
        }

        public static bool DoIt() // Locate Domain Controller IP and Domain Name
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
                string dnsHostname = dirEntry.Properties["dnsHostname"].Value.ToString();
                string defaultNamingContext = dirEntry.Properties["defaultNamingContext"].Value.ToString();
                Program.DomainName = GetDomainNameFromDN(defaultNamingContext);

                Helper.ColorfulConsole($"Domain Controller FQDN: {dnsHostname}", ConsoleColor.Red);
                Helper.ColorfulConsole($"Domain Name: {Program.DomainName}", ConsoleColor.Red);

                IPAddress[] ipAddresses = Dns.GetHostAddresses(dnsHostname);
                foreach (IPAddress ip in ipAddresses)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // Check for IPv4
                    {
                       
                        Helper.ColorfulConsole($"Domain Controller IP (IPv4): {ip}", ConsoleColor.Red);

                    }
                    else
                    {
                        Helper.ColorfulConsole($"Domain Controller IP (IPv6): {ip}", ConsoleColor.Red);
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                Helper.ColorfulConsole($"An error occurred: {ex.Message}", ConsoleColor.Red);
                return false;
            }
            return false;
        }

        public static string GetDomainNameFromDN(string distinguishedName)
        {
            string[] parts = distinguishedName.Split(',');
            string domainName = string.Empty;
            foreach (string part in parts)
            {
                if (part.StartsWith("DC=", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (domainName.Length > 0)
                    {
                        domainName += ".";
                    }
                    domainName += part.Substring(3);
                }
            }
            return domainName;
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

      
    }
}
