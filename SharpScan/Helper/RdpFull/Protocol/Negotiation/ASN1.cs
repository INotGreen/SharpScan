using System;
using System.Collections.Generic;

namespace SharpRDPCheck
{
    internal class ASN1
    {
        private static Dictionary<string, Fixup> m_Fixup = new Dictionary<string, Fixup>();

        protected static void CloseTag(RdpPacket packet, string Identifier)
        {
            UpdateLength(packet, Identifier);
        }

        protected static int ContextTag(int type)
        {
            return (160 + type);
        }

        protected static void Init()
        {
            m_Fixup.Clear();
        }

        protected static int OctetStringTag()
        {
            return 4;
        }

        protected static int ReadInteger(RdpPacket packet)
        {
            if (packet.ReadByte() != 2)
            {
                throw new Exception("Data Error!");
            }
            int num2 = packet.ReadByte();
            byte[] buffer = new byte[4];

            switch (num2)
            {
                case 4:
                    packet.Read(buffer, 0, 4);
                    return BitConverter.ToInt32(buffer, 0);

                case 3:
                    packet.Read(buffer, 0, 3);
                    return BitConverter.ToInt32(buffer, 0);

                case 2:
                    packet.Read(buffer, 0, 2);
                    return BitConverter.ToInt32(buffer, 0);
            }

            packet.Read(buffer, 0, 1);

            return BitConverter.ToInt32(buffer, 0);
        }

        protected static int ReadLength(RdpPacket packet, string Identifier)
        {
            int num;
            byte[] buffer = new byte[4];
            int num2 = packet.ReadByte();

            switch (num2)
            {
                case 0x84:
                    buffer[3] = (byte) packet.ReadByte();
                    buffer[2] = (byte) packet.ReadByte();
                    buffer[1] = (byte) packet.ReadByte();
                    buffer[0] = (byte) packet.ReadByte();
                    num = BitConverter.ToInt32(buffer, 0);
                    break;

                case 0x83:
                    buffer[2] = (byte) packet.ReadByte();
                    buffer[1] = (byte) packet.ReadByte();
                    buffer[0] = (byte) packet.ReadByte();
                    num = BitConverter.ToInt32(buffer, 0);
                    break;

                case 130:
                    buffer[1] = (byte) packet.ReadByte();
                    buffer[0] = (byte) packet.ReadByte();
                    num = BitConverter.ToInt32(buffer, 0);
                    break;

                case 0x81:
                    num = packet.ReadByte();
                    break;

                default:
                    num = num2;
                    break;
            }

            m_Fixup.Add(Identifier, new Fixup(Identifier, packet.Position, num));

            return num;
        }

        protected static int ReadTag(RdpPacket packet, string Identifier)
        {
            int num = packet.ReadByte();
            ReadLength(packet, Identifier);

            return num;
        }

        protected static int ReadTag(RdpPacket packet, int ExpectedTag, string Identifier)
        {
            int num = packet.ReadByte();

            if (num != ExpectedTag)
            {
                throw new Exception(string.Concat(new object[] { "Expected DER tag ", ExpectedTag, " but got ", num }));
            }

            return ReadLength(packet, Identifier);
        }

        protected static int SequenceTag(int type)
        {
            return (0x30 + type);
        }

        protected static void UpdateLength(RdpPacket packet, string Identifier)
        {
            Fixup fixup = m_Fixup[Identifier];
            m_Fixup.Remove(Identifier);
            long position = packet.Position;

            if (fixup.Length != -1)
            {
                long num2 = packet.Position - fixup.Offset;

                if (num2 != fixup.Length)
                {
                    throw new Exception("DER Tag length invalid");
                }
            }
            else
            {
                long num3 = packet.Position - (fixup.Offset + 1L);
                byte[] bytes = BitConverter.GetBytes(num3);
                packet.Position = fixup.Offset;

                if (num3 > 0xffffffL)
                {
                    packet.WriteByte(0x84);
                    packet.InsertByte(bytes[3]);
                    position += 1L;
                    packet.InsertByte(bytes[2]);
                    position += 1L;
                    packet.InsertByte(bytes[1]);
                    position += 1L;
                    packet.InsertByte(bytes[0]);
                    position += 1L;
                }
                else if (num3 > 0xffffL)
                {
                    packet.WriteByte(0x83);
                    packet.InsertByte(bytes[2]);
                    position += 1L;
                    packet.InsertByte(bytes[1]);
                    position += 1L;
                    packet.InsertByte(bytes[0]);
                    position += 1L;
                }
                else if (num3 > 0xffL)
                {
                    packet.WriteByte(130);
                    packet.InsertByte(bytes[1]);
                    position += 1L;
                    packet.InsertByte(bytes[0]);
                    position += 1L;
                }
                else if (num3 > 0x7fL)
                {
                    packet.WriteByte(0x81);
                    packet.InsertByte(bytes[0]);
                    position += 1L;
                }
                else
                {
                    packet.WriteByte(bytes[0]);
                }

                packet.Position = position;
            }
        }

        protected static void WriteByte(RdpPacket packet, int value)
        {
            packet.WriteByte((byte) value);
        }

        protected static void WriteInteger(RdpPacket packet, int value)
        {
            packet.WriteByte(2);
            byte[] bytes = BitConverter.GetBytes(value);

            if (value > 0xffffff)
            {
                packet.WriteByte(4);
                packet.WriteByte(bytes[3]);
                packet.WriteByte(bytes[2]);
                packet.WriteByte(bytes[1]);
                packet.WriteByte(bytes[0]);
            }
            else if (value > 0xffff)
            {
                packet.WriteByte(3);
                packet.WriteByte(bytes[2]);
                packet.WriteByte(bytes[1]);
                packet.WriteByte(bytes[0]);
            }
            else if (value > 0xff)
            {
                packet.WriteByte(2);
                packet.WriteByte(bytes[1]);
                packet.WriteByte(bytes[0]);
            }
            else
            {
                packet.WriteByte(1);
                packet.WriteByte(bytes[0]);
            }
        }

        protected static void WriteLength(RdpPacket packet, string Identifier)
        {
            m_Fixup.Add(Identifier, new Fixup(Identifier, packet.Position));
            WriteByte(packet, 0xff);
        }

        protected static void WriteTag(RdpPacket packet, int Tag, string Identifier)
        {
            WriteByte(packet, Tag);
            WriteLength(packet, Identifier);
        }

        private class Fixup
        {
            public string Identifer;
            public int Length;
            public long Offset;

            public Fixup(string sIdentifier, long Offset)
            {
                this.Identifer = sIdentifier;
                this.Offset = Offset;
                this.Length = -1;
            }

            public Fixup(string sIdentifier, long Offset, int Length)
            {
                this.Identifer = sIdentifier;
                this.Offset = Offset;
                this.Length = Length;
            }
        }

    }
}