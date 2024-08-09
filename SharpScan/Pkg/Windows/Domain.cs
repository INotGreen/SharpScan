using System;
using System.DirectoryServices;
using System.Net;

namespace SharpScan
{
    internal class GetDomainInfo
    {
        public GetDomainInfo()
        {
            try
            {
                ListAllUsers(Program.DomainName);
                GetAllGroup();
                GetAllOu();
                GetAllAdmins();
                GetAllEnt();
                GetAllCollers();
                GetAllAdministrators();
                
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllAdmins()
        {
            try
            {
                string q = "(&(objectClass=group)(cn=Domain Admins))";
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher(de, q);

                Console.WriteLine("\n[+] All Domain Admins");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50} {1,-30}", "Member", "CN");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domainUsersCount = r.Properties["member"].Count;
                    for (int len = 0; len < domainUsersCount; len++)
                    {
                        string domainUser = r.Properties["member"][len].ToString();
                        Console.WriteLine("{0,-50} {1,-30}", domainUser, domainUser.Contains("CN") ? domainUser : "N/A");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void ListAllUsers(string domainName)
        {
            try
            {
                
                if (string.IsNullOrEmpty(domainName))
                {
                    Console.WriteLine("[!] ERROR: Unable to determine the domain name.");
                    return;
                }

                using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{domainName}"))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = "(objectClass=user)";
                        searcher.PropertiesToLoad.Add("samaccountname");
                        searcher.PropertiesToLoad.Add("displayName");
                        searcher.PropertiesToLoad.Add("mail");
                        searcher.PropertiesToLoad.Add("telephoneNumber");

                        Console.WriteLine("\n[+] All Domain Users");
                        Console.WriteLine(new string('-', 95));
                        Console.WriteLine("{0,-20} {1,-30} {2,-30} {3,-15}", "Username", "Display Name", "Email", "Phone");
                        Console.WriteLine(new string('-', 95));

                        foreach (SearchResult result in searcher.FindAll())
                        {
                            if (result.Properties["samaccountname"].Count > 0)
                            {
                                string username = result.Properties["samaccountname"][0].ToString();
                                Configuration.UserDictionary["smb"].Add(username);
                                string displayName = result.Properties["displayName"].Count > 0 ? result.Properties["displayName"][0].ToString() : "N/A";
                                string email = result.Properties["mail"].Count > 0 ? result.Properties["mail"][0].ToString() : "N/A";
                                string phone = result.Properties["telephoneNumber"].Count > 0 ? result.Properties["telephoneNumber"][0].ToString() : "N/A";

                                Console.WriteLine("{0,-20} {1,-30} {2,-30} {3,-15}", username, displayName, email, phone);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void GetAllMachine()
        {
            try
            {
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), "(&(objectCategory=computer))");

                Console.WriteLine("\n[+] All Domain Machines");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50}", "Computer Name");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine("{0,-50}", r.GetDirectoryEntry().Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllGroup()
        {
            try
            {
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), "(&(objectCategory=group))");

                Console.WriteLine("\n[+] All Domain Groups");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50}", "Group Name");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine("{0,-50}", r.GetDirectoryEntry().Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllOu()
        {
            try
            {
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), "(&(objectCategory=organizationalUnit))");

                Console.WriteLine("\n[+] All Domain OUs");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50}", "OU Name");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine("{0,-50}", r.GetDirectoryEntry().Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllEnt()
        {
            try
            {
                string q = "(&(objectClass=group)(cn=Enterprise Admins))";
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), q);

                Console.WriteLine("\n[+] All Domain Enterprise Admins");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50} {1,-30}", "Member", "CN");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domainUsersCount = r.Properties["member"].Count;
                    for (int len = 0; len < domainUsersCount; len++)
                    {
                        string domainUser = r.Properties["member"][len].ToString();
                        Console.WriteLine("{0,-50} {1,-30}", domainUser, domainUser.Contains("CN") ? domainUser : "N/A");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllCollers()
        {
            try
            {
                string q = "(&(objectClass=group)(cn=Domain Controllers))";
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), q);

                Console.WriteLine("\n[+] All Domain Controllers");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50} {1,-30}", "Member", "CN");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domainUsersCount = r.Properties["member"].Count;
                    for (int len = 0; len < domainUsersCount; len++)
                    {
                        string domainUser = r.Properties["member"][len].ToString();
                        Console.WriteLine("{0,-50} {1,-30}", domainUser, domainUser.Contains("CN") ? domainUser : "N/A");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }

        static void GetAllAdministrators()
        {
            try
            {
                string q = "(&(objectClass=group)(cn=administrators))";
                DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(), q);

                Console.WriteLine("\n[+] All Domain Administrators");
                Console.WriteLine(new string('-', 95));
                Console.WriteLine("{0,-50} {1,-30}", "Member", "CN");
                Console.WriteLine(new string('-', 95));

                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domainUsersCount = r.Properties["member"].Count;
                    for (int len = 0; len < domainUsersCount; len++)
                    {
                        string domainUser = r.Properties["member"][len].ToString();
                        Console.WriteLine("{0,-50} {1,-30}", domainUser, domainUser.Contains("CN") ? domainUser : "N/A");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }



    }
}
