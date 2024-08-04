using System;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace MySqlConnectionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: <hostname> <port> <username> <password>");
                return;
            }

            string hostname = args[0];
            string port = args[1];
            string username = args[2];
            string password = args[3];

            string connectionString = $"Server={hostname};Port={port};User ID={username};Password={password};";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("[+] MySQL logon success!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] MySQL logon failed: " + ex.Message);
            }
        }
    }
}
