using System;
using System.Net.Sockets;
using System.Security;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

class Program
{
    static void Main(string[] args)
    {
        // 示例调用
        Smblogin("192.168.244.153", @"desktop-pesl5dr\administrator", "a");
    }

    public static void Smblogin(string ip, string user, string pass)
    {
        if (string.IsNullOrEmpty(pass))
        {
            Console.WriteLine("usage: smblogin <ip> <user> <password>");
            Console.WriteLine(" e.g.: smblogin 192.168.1.1 .\\Administrator P@ssw0rd");
            return;
        }

        string userm = user.Replace("\\", "\\\\").Replace(".", "\\.");

        if (!WorkerTestPort(ip, 445))
        {
            Console.WriteLine($"{ip},445,Port unreachable");
            return;
        }

        string output = $"{ip},{user},{pass},";
        output += SmbloginWorker(ip, user, pass);
        Console.WriteLine(output);
    }

    public static string SmbloginWorker(string host, string user, string pass)
    {
        user = user.Replace("^.\\", $"{host}\\");
        SecureString securePassword = new SecureString();
        foreach (char c in pass)
        {
            securePassword.AppendChar(c);
        }

        PSCredential credential = new PSCredential(user, securePassword);

        using (PowerShell ps = PowerShell.Create())
        {
            ps.AddCommand("New-PSDrive")
                .AddParameter("Name", "Share")
                .AddParameter("PSProvider", "FileSystem")
                .AddParameter("Root", $"\\\\{host}\\Admin$")
                .AddParameter("Credential", credential)
                .AddParameter("ErrorAction", "SilentlyContinue");

            try
            {
                var result = ps.Invoke();
                if (result.Count > 0)
                {
                    ps.Commands.Clear();
                    ps.AddCommand("Remove-PSDrive")
                        .AddParameter("Name", "Share");
                    ps.Invoke();

                    return "True,admin";
                }
                else
                {
                    string errorMessage = ps.Streams.Error[0].Exception.Message;
                    if (errorMessage.Contains("password is incorrect"))
                    {
                        return "False";
                    }
                    else if (errorMessage.Contains("Access is denied"))
                    {
                        return "True";
                    }
                }
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        return "Error";
    }

    public static bool WorkerTestPort(string remoteHost, int remotePort)
    {
        int timeout = 3000; // 3 seconds
        try
        {
            using (TcpClient t = new TcpClient())
            {
                var result = t.BeginConnect(remoteHost, remotePort, null, null);
                var success = result.AsyncWaitHandle.WaitOne(timeout);
                if (!success)
                {
                    t.Close();
                    return false;
                }

                t.EndConnect(result);
                t.Close();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
