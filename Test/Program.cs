using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Tamir.SharpSsh;

namespace SharpScan
{
    internal class SshBrute
    {
        public static Dictionary<string, List<string>> UserDict = new Dictionary<string, List<string>>
        {
            { "ssh", new List<string> { "root", "admin", "kali" } },
        };

        private static readonly ConcurrentBag<string> SuccessfulLogins = new ConcurrentBag<string>();
        private static readonly ConcurrentDictionary<string, bool> TriedCombinations = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, bool> SuccessfulCombinations = new ConcurrentDictionary<string, bool>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static int activeThreads = 0;
        private static readonly int maxDegreeOfParallelism = 20;

        

        static void Main(string[] args)
        {
            TryLogin("192.168.244.142", 22, "kali", "kali");
        }

        static  void TryLogin(string host, int port, string username, string password)
        {
            try
            {

                using (var client = new TcpClient())
                {
                    try
                    {

                        var result = client.BeginConnect(host, port, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1.5));
                        if (!success)
                        {
                            throw new Exception("Connection timed out");
                        }

                        client.EndConnect(result);
                        SshExec exec = new SshExec(host, username, password);
                        exec.Connect();
                        string output = exec.RunCommand("cat /etc/os-release");
                        if (output != null)
                        {
                            string loginInfo = $"[+] (SSH) {host}:{port}  User:{username}  Password:{password}   {ParseOsInfo(output)}";
                            string successKey = $"{username}@{host}:{port}";
                            Console.WriteLine(loginInfo);
                           
                            try
                            {
                                if (SuccessfulCombinations.TryAdd(successKey, true))
                                {
                                    SuccessfulLogins.Add(loginInfo);
                                    
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }

                        exec.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref activeThreads);
            }
        }

        static string ParseOsInfo(string output)
        {
            Dictionary<string, string> osInfo = new Dictionary<string, string>();
            Regex regex = new Regex(@"^(\w+)=""?([^""]*)""?$", RegexOptions.Multiline);

            foreach (Match match in regex.Matches(output))
            {
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    osInfo[key] = value;
                }
            }

            if (osInfo.ContainsKey("NAME") && osInfo.ContainsKey("VERSION"))
            {
                return $"OS: {osInfo["NAME"]}, Version: {osInfo["VERSION"]}";
            }
            else
            {
                return "Unable to determine the operating system version.";
            }
        }

        static void LogException(Exception ex)
        {
            // 这里可以将异常信息记录到日志文件中
            // 例如：
            // File.AppendAllText("error.log", $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
        }

        static void PrintResults()
        {
            foreach (var login in SuccessfulLogins)
            {
                Console.WriteLine(login);
            }
        }

    }
}
