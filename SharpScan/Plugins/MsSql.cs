using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SharpScan
{
    internal class MsSqlBroute
    {

        public static void Run(string host)
        {
            int Port = 1433;
            if (!Helper.TestPort(host, Port))
            {
                return;
            }

           // Console.WriteLine($"[*] {host}:{Port}{Helper.GetServiceByPort(Port)} is brute force cracking in progress");

            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                bool isConnected = MsSQL_Connect(host, Program.userName, Program.passWord);
                if (isConnected)
                {
                    Helper.ColorfulConsole($"MSSQL login successful with user: {Program.userName} ,  password: {Program.passWord}", ConsoleColor.Green);
                    return;
                }
            }
            else if (Program.userList != null && Program.passwordList != null)
            {
                foreach (var user in Program.userList)
                {
                    foreach (var password in Program.passwordList)
                    {
                        
                        bool isConnected = MsSQL_Connect(host, user, password);
                        if (isConnected)
                        {
                            Helper.ColorfulConsole($"MSSQL login successful with user: {user} ,  password: {password}", ConsoleColor.Green);
                            return;
                        }
                    }
                }
            }
            else if (Configuration.UserDictionary.TryGetValue("mssql", out List<string> mssqlUsers))
            {
                foreach (var user in mssqlUsers)
                {
                    foreach (var password in Configuration.Passwords)
                    {
                        string formattedPassword = password.Replace("{user}", user);
                        bool isConnected = MsSQL_Connect(host, user, formattedPassword);
                        if (isConnected)
                        {
                            Helper.ColorfulConsole($"MSSQL login successful with user: {user} ,  password: {formattedPassword}", ConsoleColor.Green);
                            return;
                        }
                    }
                }
               
            }
           
            Helper.ColorfulConsole("[-] MSSQL login failed with all provided credentials.", ConsoleColor.Red);
        }

        public static bool MsSQL_Connect(string Server, string User, string Password)
        {
            string connectionString = $"Server={Server};Database=master;User ID={User};Password={Password};";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch (Exception e)
                {
                    // Console.WriteLine(e.ToString());
                    return false;
                }
            }
        }
    }
}
