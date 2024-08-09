using System;
using System.DirectoryServices;

namespace ADUserQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the domain name as an argument.");
                return;
            }

            string domainName = args[0];
            ListAllUsers(domainName);
        }

        static void ListAllUsers(string domainName)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{domainName}"))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = "(objectClass=user)";
                        searcher.PropertiesToLoad.Add("samaccountname");
                        searcher.PropertiesToLoad.Add("displayName"); // 加载显示名称属性
                        searcher.PropertiesToLoad.Add("mail"); // 加载邮箱属性
                        searcher.PropertiesToLoad.Add("telephoneNumber"); // 加载电话号码属性

                        Console.WriteLine("{0,-20} {1,-30} {2,-30} {3,-15}", "Username", "Display Name", "Email", "Phone");
                        Console.WriteLine(new string('-', 95));

                        foreach (SearchResult result in searcher.FindAll())
                        {
                            if (result.Properties["samaccountname"].Count > 0)
                            {
                                string username = result.Properties["samaccountname"][0].ToString();
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
    }
}
