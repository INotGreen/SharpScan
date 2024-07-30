using System;
using System.Threading;
using System.Diagnostics;

namespace SharpRDPCheck
{
    internal class ISO
    {   
        internal static RdpPacket Receive()
        {
            byte[] buffer = new byte[0x3000];
            int count = Network.Receive(buffer);
            RdpPacket packet = new RdpPacket();
            packet.Write(buffer, 0, count);
            packet.Position = 0L;
            int num2 = 0;

            if (packet.ReadByte() == 3)
            {
                packet.ReadByte();
                num2 = packet.ReadBigEndian16();
                long position = packet.Position;

                while (num2 > count)
                {
                    int num4 = Network.Receive(buffer);
                    packet.Position = count;
                    packet.Write(buffer, 0, num4);
                    count += num4;
                }

                packet.Position = position;

                return packet;
            }
            num2 = packet.ReadByte();

            if ((num2 & 0x80) != 0)
            {
                num2 &= -129;
                num2 = num2 << (8 + packet.ReadByte());
            }

            return packet;
        }


    }
}