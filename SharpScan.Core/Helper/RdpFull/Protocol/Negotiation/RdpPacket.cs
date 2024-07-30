using System;
using System.IO;
using System.Text;

namespace SharpRDPCheck
{
    internal class RdpPacket : MemoryStream
    {
        public const byte DATA_TRANSFER = 240;
        public const byte DISCONNECT_REQUEST = 0x80;
        public const byte EOT = 0x80;
        public const byte FAST_PATH_OUTPUT = 0xff;
        public const byte FAST_PATH_OUTPUT_ENCRYPTED = 0xfe;
        private byte[] m_Buffer;
        private NetworkSocket m_Socket;

        public RdpPacket()
        {
        }

        public RdpPacket(NetworkSocket Socket, byte[] buffer)
        {
            this.m_Socket = Socket;
            this.m_Buffer = buffer;
        }


        public void copyToByteArray(RdpPacket packet)
        {
            byte[] buffer = new byte[packet.Length];
            packet.Position = 0L;
            packet.Read(buffer, 0, (int) packet.Length);
            this.Write(buffer, 0, buffer.Length);
        }

        public void InsertByte(byte value)
        {
            long position = this.Position;
            this.Seek(0L, SeekOrigin.End);
            this.WriteByte(0);
            byte[] sourceArray = this.GetBuffer();
            Array.Copy(sourceArray, (int) position, sourceArray, ((int) position) + 1, (int) ((sourceArray.Length - position) - 1L));
            this.Position = position;
            this.WriteByte(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if ((this.Position + count) > this.Length)
            {
                this.ReadFromSocket(count);
            }

            return base.Read(buffer, offset, count);
        }

        public int ReadLittleEndian16()
        {
            byte[] buffer = new byte[2];
            this.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public int ReadLittleEndian32()
        {
            byte[] buffer = new byte[4];
            this.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }


        public int ReadBigEndian16()
        {
            byte[] buffer = new byte[2];
            this.Read(buffer, 0, 2);
            return BitConverter.ToInt16(this.Reverse(buffer), 0);
        }


        public override int ReadByte()
        {
            if (this.Position >= this.Length)
            {
                this.ReadFromSocket(1);
            }
            return base.ReadByte();
        }

        private void ReadFromSocket(int lengthRequired)
        {
            if (this.m_Socket == null)
            {
                throw new IOException("RdpPacket - Overrun!");
            }
            int count = this.m_Socket.Receive(this.m_Buffer, this.m_Buffer.Length);
            if (count <= 0)
            {
                throw new IOException("RdpPacket - Overrun!");
            }
            lengthRequired -= count;
            if (lengthRequired > 0)
            {
                throw new IOException("RdpPacket - Overrun!");
            }
            long position = this.Position;
            this.Write(this.m_Buffer, 0, count);
            this.Position = position;
        }

        public string ReadString(int Length)
        {
            byte[] buffer = new byte[Length];
            this.Read(buffer, 0, Length);
            return ASCIIEncoding.GetString(buffer, 0, Length);
        }


        private byte[] Reverse(byte[] data)
        {
            Array.Reverse(data);
            return data;
        }

        public void WriteBigEndian16(short Value)
        {
            base.Write(this.Reverse(BitConverter.GetBytes(Value)), 0, 2);
        }


        override public void WriteByte(byte value)
        {
            base.WriteByte(value);
        }

        public void WriteLittleEndian16(short Value)
        {
            base.Write(BitConverter.GetBytes(Value), 0, 2);
        }

        public void WriteLittleEndian16(ushort Value)
        {
            base.Write(BitConverter.GetBytes(Value), 0, 2);
        }

        public void WriteLittleEndian32(int value)
        {
            base.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteLittleEndian32(uint value)
        {
            base.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WritePadding(int bytes)
        {
            for (int i = 0; i < bytes; i++)
            {
                this.WriteByte(0);
            }
        }

        public void WriteString(string sString, bool bQuiet)
        {
            byte[] bytes = ASCIIEncoding.GetBytes(sString, bQuiet);
            this.Write(bytes, 0, bytes.Length);
        }

    }
}