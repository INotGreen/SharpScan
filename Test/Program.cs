
using System;

namespace SharpScan
{
    public class MySqlBroute
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: SharpScan.exe <host> <username> <password>");
                return;
            }

            string host = args[0];
            string username = args[1];
            string password = args[2];

            bool isConnected = MySQL_Connect(host, username, password);

            if (isConnected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] MySQL login successful.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] MySQL login failed.");
            }

            Console.ResetColor();
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
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }
        }
    }
}
