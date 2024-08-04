using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class Ftp
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(20); // 合理设置并发数，防止过载

        public static async Task Run(string IP)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                try
                {
                    int Port = 21;
                    if (!Helper.TestPort(IP, Port))
                    {
                        return;
                    }

                    Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is open");

                    List<Task> tasks = new List<Task>();

                    if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
                    {
                        tasks.Add(ExecuteWithSemaphore(IP, Program.userName, Program.passWord, cts.Token, cts));
                    }
                    else if (Program.userList != null && Program.passwordList != null)
                    {
                        foreach (var user in Program.userList)
                        {
                            foreach (var pass in Program.passwordList)
                            {
                                tasks.Add(ExecuteWithSemaphore(IP, user, pass, cts.Token, cts));
                            }
                        }
                    }
                    else if (Configuration.UserDictionary.TryGetValue("ftp", out List<string> ftpUsers))
                    {
                        foreach (var user in ftpUsers)
                        {
                            foreach (var pass in GeneratePasswords(user))
                            {
                                tasks.Add(ExecuteWithSemaphore(IP, user, pass, cts.Token, cts));
                                await Task.Delay(50); // 添加适当延迟，避免过载
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Username list for FTP service not found.");
                    }

                    await Task.WhenAll(tasks); // 等待所有任务完成
                }
                catch (Exception exception)
                {
                    Console.WriteLine("[!] " + exception.Message);
                    if (exception.InnerException != null)
                    {
                        Console.WriteLine("InnerException: " + exception.InnerException);
                    }
                }
            }
        }

        private static Task ExecuteWithSemaphore(string IP, string userName, string passWord, CancellationToken token, CancellationTokenSource cts)
        {
            return Task.Run(async () =>
            {
                await Semaphore.WaitAsync(token);
                try
                {
                    if (!token.IsCancellationRequested)
                    {
                        bool success = await TestFtpConnection(IP, userName, passWord, token);
                        if (success)
                        {
                            cts.Cancel(); // 取消所有其他任务
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 捕获任务取消异常
                }
                catch (Exception ex)
                {
                    // 处理异常，但不输出错误信息以减少日志干扰
                }
                finally
                {
                    Semaphore.Release();
                    // 手动进行垃圾回收
                    GC.Collect();
                }
            }, token);
        }

        private static async Task<bool> TestFtpConnection(string IP, string userName, string passWord, CancellationToken token)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            try
            {
                if (token.IsCancellationRequested)
                {
                    return false; // 如果取消了任务，则直接返回
                }

                string ftpUri = $"ftp://{IP}/";

                // 创建FtpWebRequest对象
                request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.KeepAlive = false;
                // 设置登录凭据
                request.Credentials = new NetworkCredential(userName, passWord);

                // 使用Task.Factory.FromAsync模拟异步操作
                var task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
                response = (FtpWebResponse)await task;

                Console.WriteLine($"[+] (FTP){IP}:{21} logon success! - User: {userName}, Password: {passWord}");
                return true; // 连接成功
            }
            catch (WebException ex)
            {
                // 捕获WebException但不记录以减少日志干扰
            }
            catch (Exception ex)
            {
                // 捕获其他异常但不记录以减少日志干扰
            }
            finally
            {
                // 断开连接并释放资源
                request?.Abort();
                response?.Close();
                
            }

            return false; // 连接失败
        }

        private static IEnumerable<string> GeneratePasswords(string user)
        {
            foreach (var pass in Configuration.Passwords)
            {
                yield return pass.Replace("{user}", user);
            }
        }
    }
}
