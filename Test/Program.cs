using System;
using System.Net;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        // The packet data to be sent
        byte[] pkt = {   0x00, 0x00, 0x00, 0xC0, 0xFE, 0x53, 0x4D, 0x42, 0x40, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00,
    0x08, 0x00, 0x01, 0x00, 0x00, 0x00, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x02,
    0x10, 0x02, 0x22, 0x02, 0x24, 0x02, 0x00, 0x03, 0x02, 0x03, 0x10, 0x03,
    0x11, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x26, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x0A, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        // Argument handling for subnet
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: <program> <subnet>");
            return;
        }

        string subnet = args[0];
        Console.WriteLine(subnet);
        var subnetParts = subnet.Split('/');
        var baseIP = IPAddress.Parse(subnetParts[0]);
        int cidr = int.Parse(subnetParts[1]);
        uint mask = ~((1u << (32 - cidr)) - 1);
        uint ipBase = BitConverter.ToUInt32(baseIP.GetAddressBytes(), 0) & mask;

        uint ipStart = BitConverter.IsLittleEndian ? ReverseBytes(ipBase) : ipBase;
        uint ipEnd = ipStart + (1u << (32 - cidr)) - 1;

        for (uint ip = ipStart; ip <= ipEnd; ip++)
        {
            var targetIP = new IPAddress(BitConverter.IsLittleEndian ? ReverseBytes(ip) : ip);
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.ReceiveTimeout = 3000; // Timeout set to 3 seconds

                try
                {
                    sock.Connect(new IPEndPoint(targetIP, 445));
                    sock.Send(pkt);

                    byte[] buffer = new byte[4];
                    int received = sock.Receive(buffer);

                    if (received == 4)
                    {
                        int nb = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
                        byte[] response = new byte[nb];
                        received = sock.Receive(response);

                        // Check the response bytes for vulnerability
                        if (response.Length > 70 && !(response[68] == 0x11 && response[69] == 0x03 && response[70] == 0x02 && response[71] == 0x00))
                        {
                            Console.WriteLine($"{targetIP} Not vulnerable.");
                        }
                        else
                        {
                            Console.WriteLine($"{targetIP} Vulnerable");
                        }
                    }
                }
                catch (SocketException)
                {
                    // If there's a socket error (likely timeout or no connection), just continue with the next IP
                    continue;
                }
                finally
                {
                    sock.Close();
                }
            }
        }
    }

    // Utility method to reverse byte order; needed because of different endianess
    static uint ReverseBytes(uint value)
    {
        return (value >> 24) |
               ((value >> 8) & 0x0000FF00) |
               ((value << 8) & 0x00FF0000) |
               (value << 24);
    }
}
