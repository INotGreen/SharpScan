using SharpScan.Helper.SMB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpScan.Plugins
{
    internal class SMB
    {
        static List<string> usernames = new List<string> { "liukaifeng01" };
        static List<string> passwords = new List<string> { "Lang123456789", "password123" };

        public  void SMBScan(string target)
        {
            string shareName = "cwwin7jhgj_downcc";
            foreach (var user in usernames)
            {
                foreach (var pass in passwords)
                {
                    SmbClient client = new SmbClient(target, shareName)
                    {
                        User = user,
                        Password = pass,
                        NetBiosOverTCP = false,
                        Port = 445
                    };

                    try
                    {
                        if (client.Connect())
                        {
                            Console.WriteLine($"[+] SMB logon Success: {user}:{pass}");
                            //client.SetWorkingDirectory("cwjhgj");

                            //var files = client.GetFiles("");
                            //Console.WriteLine("Files:");
                            //foreach (var file in files)
                            //{
                            //    Console.WriteLine($"  {file}");
                            //}

                            //var directories = client.GetDirectories("");
                            //Console.WriteLine("Directories:");
                            //foreach (var directory in directories)
                            //{
                            //    Console.WriteLine($"  {directory}");
                            //}

                            return; // Exit after first success
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[-] Failed: {user}:{pass} - {ex.Message}");
                    }
                }
            }
        }
    }
}
