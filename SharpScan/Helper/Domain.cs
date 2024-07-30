using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace SharpScan.Helper
{
    internal class Domain
    {

        public Domain()
        {
            try
            {
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
                string q = null;
                q = "(&(objectClass=group)(cn=Domain Admins))";
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = q;
                Console.WriteLine("\n[+]All Domain Admins");
                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domain_users_count = 0;
                    string domain_users = "";
                    int len = 0;
                    domain_users_count = r.Properties["member"].Count;
                    while (len < domain_users_count)
                    {
                        domain_users = r.Properties["member"][len].ToString();
                        len++;
                        if (domain_users.Contains("CN"))
                        {
                            Console.WriteLine(domain_users);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
            }
        }
        static void GetAllMachine()
        {
            try
            {
                string q = null;
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = "(&(objectCategory=computer))";
                SearchResultCollection rs = ds.FindAll();
                Console.WriteLine("\n[+]All Domain Machine");
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine(r.GetDirectoryEntry().Name.ToString());
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
                string q = null;
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = "(&(objectCategory=group))";
                SearchResultCollection rs = ds.FindAll();
                Console.WriteLine("\n[+]All Domain Groups");
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine(r.GetDirectoryEntry().Name.ToString());
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
                string q = null;
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = "(&(objectCategory=organizationalUnit))";
                SearchResultCollection rs = ds.FindAll();
                Console.WriteLine("\n[+]All Domain OU");
                foreach (SearchResult r in rs)
                {
                    Console.WriteLine(r.GetDirectoryEntry().Name.ToString());
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
                string q = null;
                q = "(&(objectClass=group)(cn=Enterprise Admins))";
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = q;
                Console.WriteLine("\n[+]All Domain Enterprise Admins");
                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domain_users_count = 0;
                    string domain_users = "";
                    int len = 0;
                    domain_users_count = r.Properties["member"].Count;
                    while (len < domain_users_count)
                    {
                        domain_users = r.Properties["member"][len].ToString();
                        len++;
                        if (domain_users.Contains("CN"))
                        {
                            Console.WriteLine(domain_users);
                        }
                        else
                        {
                            continue;
                        }
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
                string q = null;
                q = "(&(objectClass=group)(cn=Domain Controllers))";
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = q;
                Console.WriteLine("\n[+]All Domain Controllers");
                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domain_users_count = 0;
                    string domain_users = "";
                    int len = 0;
                    domain_users_count = r.Properties["member"].Count;
                    while (len < domain_users_count)
                    {
                        domain_users = r.Properties["member"][len].ToString();
                        len++;
                        if (domain_users.Contains("CN"))
                        {
                            Console.WriteLine(domain_users);
                        }
                        else
                        {
                            continue;
                        }
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
                string q = null;
                q = "(&(objectClass=group)(cn=administrators))";
                DirectoryEntry de = new DirectoryEntry();
                DirectorySearcher ds = new DirectorySearcher();
                ds.Filter = q;
                Console.WriteLine("\n[+]All Domain Administrators");
                SearchResultCollection rs = ds.FindAll();
                foreach (SearchResult r in rs)
                {
                    int domain_users_count = 0;
                    string domain_users = "";
                    int len = 0;
                    domain_users_count = r.Properties["member"].Count;
                    while (len < domain_users_count)
                    {
                        domain_users = r.Properties["member"][len].ToString();
                        len++;
                        if (domain_users.Contains("CN"))
                        {
                            Console.WriteLine(domain_users);
                        }
                        else
                        {
                            continue;
                        }
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
