using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class SMBExploit
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: SMBExploit <IP> <Port>");
            return;
        }

        string ipAddress = args[0];
        int port = int.Parse(args[1]);

        byte[] header = new byte[64];
        Array.Copy(Encoding.ASCII.GetBytes("\xfeSMB"), 0, header, 0, 4);
        Array.Copy(BitConverter.GetBytes((ushort)64), 0, header, 4, 2);
        Array.Copy(BitConverter.GetBytes((ushort)0), 0, header, 6, 2);
        Array.Copy(BitConverter.GetBytes((ushort)0), 0, header, 8, 2);
        Array.Copy(BitConverter.GetBytes((ushort)0), 0, header, 10, 2);
        Array.Copy(BitConverter.GetBytes((ushort)31), 0, header, 12, 2);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, header, 14, 4);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, header, 18, 4);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, header, 22, 8);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, header, 30, 4);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, header, 34, 4);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, header, 38, 8);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, header, 46, 8);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, header, 54, 8);

        byte[] negotiation = new byte[120];
        Array.Copy(BitConverter.GetBytes((ushort)0x24), 0, negotiation, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)8), 0, negotiation, 2, 2);
        Array.Copy(BitConverter.GetBytes((ushort)1), 0, negotiation, 4, 2);
        Array.Copy(BitConverter.GetBytes((ushort)0), 0, negotiation, 6, 2);
        Array.Copy(BitConverter.GetBytes((uint)0x7f), 0, negotiation, 8, 4);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, negotiation, 12, 8);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, negotiation, 20, 8);
        Array.Copy(BitConverter.GetBytes((uint)0x78), 0, negotiation, 28, 4);
        Array.Copy(BitConverter.GetBytes((ushort)2), 0, negotiation, 32, 2);
        Array.Copy(BitConverter.GetBytes((ushort)0), 0, negotiation, 34, 2);

        ushort[] dialects = { 0x0202, 0x0210, 0x0222, 0x0224, 0x0300, 0x0302, 0x0310, 0x0311 };
        for (int i = 0; i < dialects.Length; i++)
        {
            Array.Copy(BitConverter.GetBytes(dialects[i]), 0, negotiation, 36 + (i * 2), 2);
        }

        Array.Copy(BitConverter.GetBytes((uint)0), 0, negotiation, 52, 4);
        Array.Copy(BitConverter.GetBytes((ushort)1), 0, negotiation, 56, 2);
        Array.Copy(BitConverter.GetBytes((ushort)38), 0, negotiation, 58, 2);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, negotiation, 60, 4);
        Array.Copy(BitConverter.GetBytes((ushort)1), 0, negotiation, 64, 2);
        Array.Copy(BitConverter.GetBytes((ushort)32), 0, negotiation, 66, 2);
        Array.Copy(BitConverter.GetBytes((ushort)1), 0, negotiation, 68, 2);
        Array.Copy(BitConverter.GetBytes((ushort)1), 0, negotiation, 70, 2);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, negotiation, 72, 8);
        Array.Copy(BitConverter.GetBytes((ulong)0), 0, negotiation, 80, 8);
        Array.Copy(BitConverter.GetBytes((ushort)3), 0, negotiation, 88, 2);
        Array.Copy(BitConverter.GetBytes((ushort)10), 0, negotiation, 90, 2);
        Array.Copy(BitConverter.GetBytes((uint)0), 0, negotiation, 92, 4);
        Array.Copy(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, negotiation, 96, 16);

        byte[] packet = new byte[header.Length + negotiation.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(negotiation, 0, packet, header.Length, negotiation.Length);

        byte[] netbios = new byte[4];
        netbios[0] = 0;
        netbios[1] = 0;
        netbios[2] = (byte)((packet.Length >> 8) & 0xff);
        netbios[3] = (byte)(packet.Length & 0xff);

        byte[] fullPacket = new byte[netbios.Length + packet.Length];
        Array.Copy(netbios, 0, fullPacket, 0, netbios.Length);
        Array.Copy(packet, 0, fullPacket, netbios.Length, packet.Length);

        Console.WriteLine($"NetBIOS ({netbios.Length}): {BitConverter.ToString(netbios).Replace("-", "")}");
        Console.WriteLine($"Header ({header.Length}): {BitConverter.ToString(header).Replace("-", "")}");
        Console.WriteLine($"Negotiation ({negotiation.Length}): {BitConverter.ToString(negotiation).Replace("-", "")}");
        Console.WriteLine($"Packet ({packet.Length}): {BitConverter.ToString(packet).Replace("-", "")}");

        HexDump(fullPacket);

        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(ipAddress, port);
                socket.Send(fullPacket);

                byte[] sizeBuffer = new byte[4];
                socket.Receive(sizeBuffer);
                int size = BitConverter.ToInt32(sizeBuffer, 0);
                Console.WriteLine($"Response length: {size}");

                byte[] response = new byte[size];
                int received = 0;
                while (received < size)
                {
                    int bytesRead = socket.Receive(response, received, size - received, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        throw new Exception("Connection closed prematurely.");
                    }
                    received += bytesRead;
                }

                Console.WriteLine($"Response: {BitConverter.ToString(response).Replace("-", "")}");

                HexDump(response);

                ushort version = BitConverter.ToUInt16(response, 68);
                ushort context = BitConverter.ToUInt16(response, 70);

                if (version != 0x0311)
                {
                    Console.WriteLine($"SMB version {version:X} was found which is not vulnerable!");
                }
                else if (context != 2)
                {
                    Console.WriteLine($"Server answered with context {context:X} which indicates that the target may not have SMB compression enabled and is therefore not vulnerable!");
                }
                else
                {
                    Console.WriteLine($"SMB version {version:X} with context {context:X} was found which indicates SMBv3.1.1 is being used and SMB compression is enabled, therefore being vulnerable to CVE-2020-0796!");
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    static void HexDump(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 16)
        {
            Console.Write($"{i:X8}  ");
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    Console.Write($"{data[i + j]:X2} ");
                }
                else
                {
                    Console.Write("   ");
                }
            }

            Console.Write(" ");
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    char ch = (char)data[i + j];
                    if (char.IsControl(ch))
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write(ch);
                    }
                }
            }

            Console.WriteLine();
        }
    }
}
