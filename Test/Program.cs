using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        string target = "192.168.244.139"; // 目标IP地址
        int startPort = 80;
        int endPort = 1024;

        var openPorts = ScanUdpPorts(target, startPort, endPort).Result;

        foreach (var port in openPorts)
        {
            Console.WriteLine($"Port {port} is open or filtered");
        }
    }

    static async Task<List<int>> ScanUdpPorts(string targetIp, int startPort, int endPort)
    {
        List<int> openPorts = new List<int>();
        List<Task> tasks = new List<Task>();

        for (int port = startPort; port <= endPort; port++)
        {
            int currentPort = port; // Capture the current port in a local variable for closure
            tasks.Add(Task.Run(() =>
            {
                if (IsUdpPortOpen(targetIp, currentPort))
                {
                    lock (openPorts)
                    {
                        openPorts.Add(currentPort);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        return openPorts;
    }

    static bool IsUdpPortOpen(string host, int port)
    {
        try
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.ReceiveTimeout = 1000; // 设置超时为1秒
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);

                byte[] sendBytes = new byte[] { 0x00 }; // 发送一个字节的数据
                udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);

                // 尝试接收响应
                try
                {
                    byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                    return true; // 如果接收到响应，表示端口开放
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        // 超时，可能端口开放或被防火墙过滤
                        return true;
                    }
                    else if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // 目标端口不可达，端口关闭
                        return false;
                    }
                }
            }
        }
        catch
        {
            return false;
        }
        return false;
    }
}
