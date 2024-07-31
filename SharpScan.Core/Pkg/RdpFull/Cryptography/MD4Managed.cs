using System;
using System.Security.Cryptography;

namespace SharpRDPCheck
{
    internal class MD4Managed : HashAlgorithm
    {
        private byte[] buffer = new byte[0x40];
        private uint[] count = new uint[2];
        private byte[] digest = new byte[0x10];
        private const int S11 = 3;
        private const int S12 = 7;
        private const int S13 = 11;
        private const int S14 = 0x13;
        private const int S21 = 3;
        private const int S22 = 5;
        private const int S23 = 9;
        private const int S24 = 13;
        private const int S31 = 3;
        private const int S32 = 9;
        private const int S33 = 11;
        private const int S34 = 15;
        private uint[] state = new uint[4];
        private uint[] x = new uint[0x10];

        public MD4Managed()
        {
            this.Initialize();
        }

        protected override byte[] HashFinal()
        {
            byte[] output = new byte[8];
            this.Encode(output, this.count);
            uint num = (this.count[0] >> 3) & 0x3f;
            int cbSize = (num < 0x38) ? (0x38 - ((int)num)) : (120 - ((int)num));
            this.HashCore(this.Padding(cbSize), 0, cbSize);
            this.HashCore(output, 0, 8);
            this.Encode(this.digest, this.state);
            this.Initialize();

            return this.digest;
        }

        private void Decode(uint[] output, byte[] input, int index)
        {
            int num = 0;

            for (int i = index; num < output.Length; i += 4)
            {
                output[num] = (uint)(((input[i] | (input[i + 1] << 8)) | (input[i + 2] << 0x10)) | (input[i + 3] << 0x18));
                num++;
            }
        }

        private void Encode(byte[] output, uint[] input)
        {
            int index = 0;

            for (int i = 0; i < output.Length; i += 4)
            {
                output[i] = (byte)input[index];
                output[i + 1] = (byte)(input[index] >> 8);
                output[i + 2] = (byte)(input[index] >> 0x10);
                output[i + 3] = (byte)(input[index] >> 0x18);
                index++;
            }
        }

        private uint F(uint x, uint y, uint z)
        {
            return ((x & y) | (~x & z));
        }

        private void FF(ref uint a, uint b, uint c, uint d, uint x, byte s)
        {
            a += this.F(b, c, d) + x;
            a = this.ROL(a, s);
        }

        private uint G(uint x, uint y, uint z)
        {
            return (((x & y) | (x & z)) | (y & z));
        }

        private void GG(ref uint a, uint b, uint c, uint d, uint x, byte s)
        {
            a += (this.G(b, c, d) + x) + 0x5a827999;
            a = this.ROL(a, s);
        }

        private uint H(uint x, uint y, uint z)
        {
            return ((x ^ y) ^ z);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            int dstOffset = ((int)(this.count[0] >> 3)) & 0x3f;
            this.count[0] += (uint)(cbSize << 3);

            if (this.count[0] < (cbSize << 3))
            {
                this.count[1]++;
            }

            this.count[1] += (uint)(cbSize >> 0x1d);
            int count = 0x40 - dstOffset;
            int index = 0;

            if (cbSize >= count)
            {
                Buffer.BlockCopy(array, ibStart, this.buffer, dstOffset, count);
                this.MD4Transform(this.state, this.buffer, 0);
                index = count;

                while ((index + 0x3f) < cbSize)
                {
                    this.MD4Transform(this.state, array, index);
                    index += 0x40;
                }

                dstOffset = 0;
            }

            Buffer.BlockCopy(array, ibStart + index, this.buffer, dstOffset, cbSize - index);
        }

        private void HH(ref uint a, uint b, uint c, uint d, uint x, byte s)
        {
            a += (this.H(b, c, d) + x) + 0x6ed9eba1;
            a = this.ROL(a, s);
        }

        public override void Initialize()
        {
            this.count[0] = 0;
            this.count[1] = 0;
            this.state[0] = 0x67452301;
            this.state[1] = 0xefcdab89;
            this.state[2] = 0x98badcfe;
            this.state[3] = 0x10325476;
            Array.Clear(this.buffer, 0, 0x40);
            Array.Clear(this.x, 0, 0x10);
        }

        private void MD4Transform(uint[] state, byte[] block, int index)
        {
            uint a = state[0];
            uint b = state[1];
            uint c = state[2];
            uint d = state[3];
            this.Decode(this.x, block, index);
            this.FF(ref a, b, c, d, this.x[0], 3);
            this.FF(ref d, a, b, c, this.x[1], 7);
            this.FF(ref c, d, a, b, this.x[2], 11);
            this.FF(ref b, c, d, a, this.x[3], 0x13);
            this.FF(ref a, b, c, d, this.x[4], 3);
            this.FF(ref d, a, b, c, this.x[5], 7);
            this.FF(ref c, d, a, b, this.x[6], 11);
            this.FF(ref b, c, d, a, this.x[7], 0x13);
            this.FF(ref a, b, c, d, this.x[8], 3);
            this.FF(ref d, a, b, c, this.x[9], 7);
            this.FF(ref c, d, a, b, this.x[10], 11);
            this.FF(ref b, c, d, a, this.x[11], 0x13);
            this.FF(ref a, b, c, d, this.x[12], 3);
            this.FF(ref d, a, b, c, this.x[13], 7);
            this.FF(ref c, d, a, b, this.x[14], 11);
            this.FF(ref b, c, d, a, this.x[15], 0x13);
            this.GG(ref a, b, c, d, this.x[0], 3);
            this.GG(ref d, a, b, c, this.x[4], 5);
            this.GG(ref c, d, a, b, this.x[8], 9);
            this.GG(ref b, c, d, a, this.x[12], 13);
            this.GG(ref a, b, c, d, this.x[1], 3);
            this.GG(ref d, a, b, c, this.x[5], 5);
            this.GG(ref c, d, a, b, this.x[9], 9);
            this.GG(ref b, c, d, a, this.x[13], 13);
            this.GG(ref a, b, c, d, this.x[2], 3);
            this.GG(ref d, a, b, c, this.x[6], 5);
            this.GG(ref c, d, a, b, this.x[10], 9);
            this.GG(ref b, c, d, a, this.x[14], 13);
            this.GG(ref a, b, c, d, this.x[3], 3);
            this.GG(ref d, a, b, c, this.x[7], 5);
            this.GG(ref c, d, a, b, this.x[11], 9);
            this.GG(ref b, c, d, a, this.x[15], 13);
            this.HH(ref a, b, c, d, this.x[0], 3);
            this.HH(ref d, a, b, c, this.x[8], 9);
            this.HH(ref c, d, a, b, this.x[4], 11);
            this.HH(ref b, c, d, a, this.x[12], 15);
            this.HH(ref a, b, c, d, this.x[2], 3);
            this.HH(ref d, a, b, c, this.x[10], 9);
            this.HH(ref c, d, a, b, this.x[6], 11);
            this.HH(ref b, c, d, a, this.x[14], 15);
            this.HH(ref a, b, c, d, this.x[1], 3);
            this.HH(ref d, a, b, c, this.x[9], 9);
            this.HH(ref c, d, a, b, this.x[5], 11);
            this.HH(ref b, c, d, a, this.x[13], 15);
            this.HH(ref a, b, c, d, this.x[3], 3);
            this.HH(ref d, a, b, c, this.x[11], 9);
            this.HH(ref c, d, a, b, this.x[7], 11);
            this.HH(ref b, c, d, a, this.x[15], 15);
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        private byte[] Padding(int nLength)
        {
            if (nLength > 0)
            {
                byte[] buffer = new byte[nLength];
                buffer[0] = 0x80;
                return buffer;
            }

            return null;
        }

        private uint ROL(uint x, byte n)
        {
            return ((x << n) | (x >> (0x20 - n)));
        }

    }
}