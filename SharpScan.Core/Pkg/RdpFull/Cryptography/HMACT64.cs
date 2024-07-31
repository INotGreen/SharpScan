using System;
using System.Collections.Generic;

namespace SharpRDPCheck
{
    internal class HMACT64
    {
        private const int BLOCK_LENGTH = 0x40;
        private List<byte> buffer = new List<byte>();
        private byte[] ipad = new byte[0x40];
        private const byte IPAD = 0x36;
        private byte[] opad = new byte[0x40];
        private const byte OPAD = 0x5c;

        public HMACT64(byte[] key)
        {
            int num = Math.Min(key.Length, 0x40);

            for (int i = 0; i < num; i++)
            {
                this.ipad[i] = (byte)(key[i] ^ 0x36);
                this.opad[i] = (byte)(key[i] ^ 0x5c);
            }

            for (int j = num; j < 0x40; j++)
            {
                this.ipad[j] = 0x36;
                this.opad[j] = 0x5c;
            }

            this.reset();
        }

        public byte[] digest()
        {
            byte[] collection = MD5.ComputeHash(this.buffer.ToArray());
            this.buffer.Clear();
            this.buffer.AddRange(this.opad);
            this.buffer.AddRange(collection);
            collection = MD5.ComputeHash(this.buffer.ToArray());
            this.reset();

            return collection;
        }

        public int digest(byte[] buf, int offset, int len)
        {
            byte[] collection = MD5.ComputeHash(this.buffer.ToArray());
            this.buffer.Clear();
            this.buffer.AddRange(this.opad);
            this.buffer.AddRange(collection);
            byte[] sourceArray = MD5.ComputeHash(this.buffer.ToArray());

            if (len > sourceArray.Length)
            {
                len = sourceArray.Length;
            }

            Array.Copy(sourceArray, 0, buf, offset, len);
            this.reset();

            return sourceArray.Length;
        }

        public void reset()
        {
            this.buffer.Clear();
            this.buffer.AddRange(this.ipad);
        }

        public void update(byte[] input)
        {
            this.buffer.AddRange(input);
        }

        public void update(byte[] input, int offset, int len)
        {
            for (int i = offset; i < (offset + len); i++)
            {
                this.buffer.Add(input[i]);
            }
        }

    }
}