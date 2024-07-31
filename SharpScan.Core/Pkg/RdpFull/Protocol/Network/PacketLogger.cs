using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpRDPCheck
{
    internal class PacketLogger : IDisposable
    {
        private Dictionary<int, PacketBlob> m_Blobs = new Dictionary<int, PacketBlob>();
        private BinaryReader m_ReadStream;
        private Dictionary<int, Queue<ReceivedPacket>> m_ReceivedPackets = new Dictionary<int, Queue<ReceivedPacket>>();
        private Dictionary<string, byte> m_Sockets = new Dictionary<string, byte>();
        private Dictionary<int, DateTime> m_SocketsDelay = new Dictionary<int, DateTime>();
        private Dictionary<int, long> m_SocketsLastReceivedTick = new Dictionary<int, long>();
        private BinaryWriter m_WriteStream;

        public void Dispose()
        {
            if (this.m_WriteStream != null)
            {
                this.m_WriteStream.Close();
                //this.m_WriteStream.Dispose();
                this.m_WriteStream = null;
            }
            if (this.m_ReadStream != null)
            {
                this.m_ReadStream.Close();
                //this.m_ReadStream.Dispose();
                this.m_ReadStream = null;
            }
        }

        public void AddBlob(PacketType type, byte[] data, string socket)
        {
            if (!this.m_Blobs.ContainsKey(this.BuildBlobId(type, socket)))
            {
                this.m_Blobs.Add(this.BuildBlobId(type, socket), new PacketBlob(data, type));
            }
        }



        private int BuildBlobId(PacketType type, string socket)
        {
            byte num;
            if (!this.m_Sockets.TryGetValue(socket, out num))
            {
                if (this.Reading)
                {
                    throw new Exception("Unknown socket - " + socket);
                }
                num = (byte) (this.m_Sockets.Count + 1);
                this.m_Sockets.Add(socket, num);
                byte[] bytes = ASCIIEncoding.GetBytes(socket);
            }
            return (((int) type) | (num << 0x10));
        }

        public byte[] GetBlob(PacketType type, string socket)
        {
            PacketBlob blob;
            if (this.m_Blobs.TryGetValue(this.BuildBlobId(type, socket), out blob))
            {
                return blob.Data;
            }
            return null;
        }

        public byte[] GetSSLPublicKey(string socket)
        {
            PacketBlob blob;
            if (this.m_Blobs.TryGetValue(this.BuildBlobId(PacketType.PublicKey, socket), out blob))
            {
                return blob.Data;
            }
            return null;
        }

        public void SSLPublicKey(byte[] data, string socket)
        {
            if (this.GetSSLPublicKey(socket) == null)
            {
                this.m_Blobs.Add(this.BuildBlobId(PacketType.PublicKey, socket), new PacketBlob(data, PacketType.PublicKey));
            }
        }

        public bool Reading
        {
            get
            {
                return (this.m_ReadStream != null);
            }
        }

        private class PacketBlob
        {
            private byte[] m_Data;
            private PacketLogger.PacketType m_Type;

            public PacketBlob(byte[] data, PacketLogger.PacketType type)
            {
                this.m_Data = data;
                this.m_Type = type;
            }

            public byte[] Data
            {
                get
                {
                    return this.m_Data;
                }
            }

        }

        public enum PacketType
        {
            BitmapCache = 7,
            ClientRandom = 8,
            Exception = 9,
            HostSettings = 6,
            None = -1,
            NTLM_ClientChallenge = 4,
            NTLM_ExportedSessionKey = 5,
            NTLM_KeyExchangeKey = 11,
            NTLM_ResponseKeyNT = 10,
            PublicKey = 3,
            Received = 1,
            Sent = 2,
            SocketName = 12
        }

        private class ReceivedPacket
        {
            private byte[] m_Data;
            private uint m_Ticks;

            public ReceivedPacket(byte[] data, uint ticks)
            {
                this.m_Data = data;
                this.m_Ticks = ticks;
            }

            public byte[] Data
            {
                get
                {
                    return this.m_Data;
                }
            }

            public uint Ticks
            {
                get
                {
                    return this.m_Ticks;
                }
            }
        }

    }
}