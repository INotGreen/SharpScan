using System;
using System.IO;
using System.Text;

namespace SharpRDPCheck
{
    internal static class MD5
    {
  
        public static byte[] ComputeHash(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
            }
            ABCDStruct aBCDValue = new ABCDStruct
            {
                A = 0x67452301,
                B = 0xefcdab89,
                C = 0x98badcfe,
                D = 0x10325476
            };
            int ibStart = 0;
            while (ibStart <= (input.Length - 0x40))
            {
                GetHashBlock(input, ref aBCDValue, ibStart);
                ibStart += 0x40;
            }
            return GetHashFinalBlock(input, ibStart, input.Length - ibStart, aBCDValue, input.Length * 8L);
        }

        public static byte[] ComputeHash(string input, Encoding encoding)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding", "Unable to calculate hash over a string without a default encoding. Consider using the GetHash(string) overload to use UTF8 Encoding");
            }
            return ComputeHash(encoding.GetBytes(input));
        }


        public static string ComputeHashString(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
            }
            return BitConverter.ToString(ComputeHash(input)).Replace("-", "");
        }

        private static uint[] Converter(byte[] input, int ibStart)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "Unable convert null array to array of uInts");
            }
            uint[] numArray = new uint[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                numArray[i] = input[ibStart + (i * 4)];
                numArray[i] += (uint)(input[(ibStart + (i * 4)) + 1] << 8);
                numArray[i] += (uint)(input[(ibStart + (i * 4)) + 2] << 0x10);
                numArray[i] += (uint)(input[(ibStart + (i * 4)) + 3] << 0x18);
            }
            return numArray;
        }

        internal static void GetHashBlock(byte[] input, ref ABCDStruct ABCDValue, int ibStart)
        {
            uint[] numArray = Converter(input, ibStart);
            uint a = ABCDValue.A;
            uint b = ABCDValue.B;
            uint c = ABCDValue.C;
            uint d = ABCDValue.D;
            a = r1(a, b, c, d, numArray[0], 7, 0xd76aa478);
            d = r1(d, a, b, c, numArray[1], 12, 0xe8c7b756);
            c = r1(c, d, a, b, numArray[2], 0x11, 0x242070db);
            b = r1(b, c, d, a, numArray[3], 0x16, 0xc1bdceee);
            a = r1(a, b, c, d, numArray[4], 7, 0xf57c0faf);
            d = r1(d, a, b, c, numArray[5], 12, 0x4787c62a);
            c = r1(c, d, a, b, numArray[6], 0x11, 0xa8304613);
            b = r1(b, c, d, a, numArray[7], 0x16, 0xfd469501);
            a = r1(a, b, c, d, numArray[8], 7, 0x698098d8);
            d = r1(d, a, b, c, numArray[9], 12, 0x8b44f7af);
            c = r1(c, d, a, b, numArray[10], 0x11, 0xffff5bb1);
            b = r1(b, c, d, a, numArray[11], 0x16, 0x895cd7be);
            a = r1(a, b, c, d, numArray[12], 7, 0x6b901122);
            d = r1(d, a, b, c, numArray[13], 12, 0xfd987193);
            c = r1(c, d, a, b, numArray[14], 0x11, 0xa679438e);
            b = r1(b, c, d, a, numArray[15], 0x16, 0x49b40821);
            a = r2(a, b, c, d, numArray[1], 5, 0xf61e2562);
            d = r2(d, a, b, c, numArray[6], 9, 0xc040b340);
            c = r2(c, d, a, b, numArray[11], 14, 0x265e5a51);
            b = r2(b, c, d, a, numArray[0], 20, 0xe9b6c7aa);
            a = r2(a, b, c, d, numArray[5], 5, 0xd62f105d);
            d = r2(d, a, b, c, numArray[10], 9, 0x2441453);
            c = r2(c, d, a, b, numArray[15], 14, 0xd8a1e681);
            b = r2(b, c, d, a, numArray[4], 20, 0xe7d3fbc8);
            a = r2(a, b, c, d, numArray[9], 5, 0x21e1cde6);
            d = r2(d, a, b, c, numArray[14], 9, 0xc33707d6);
            c = r2(c, d, a, b, numArray[3], 14, 0xf4d50d87);
            b = r2(b, c, d, a, numArray[8], 20, 0x455a14ed);
            a = r2(a, b, c, d, numArray[13], 5, 0xa9e3e905);
            d = r2(d, a, b, c, numArray[2], 9, 0xfcefa3f8);
            c = r2(c, d, a, b, numArray[7], 14, 0x676f02d9);
            b = r2(b, c, d, a, numArray[12], 20, 0x8d2a4c8a);
            a = r3(a, b, c, d, numArray[5], 4, 0xfffa3942);
            d = r3(d, a, b, c, numArray[8], 11, 0x8771f681);
            c = r3(c, d, a, b, numArray[11], 0x10, 0x6d9d6122);
            b = r3(b, c, d, a, numArray[14], 0x17, 0xfde5380c);
            a = r3(a, b, c, d, numArray[1], 4, 0xa4beea44);
            d = r3(d, a, b, c, numArray[4], 11, 0x4bdecfa9);
            c = r3(c, d, a, b, numArray[7], 0x10, 0xf6bb4b60);
            b = r3(b, c, d, a, numArray[10], 0x17, 0xbebfbc70);
            a = r3(a, b, c, d, numArray[13], 4, 0x289b7ec6);
            d = r3(d, a, b, c, numArray[0], 11, 0xeaa127fa);
            c = r3(c, d, a, b, numArray[3], 0x10, 0xd4ef3085);
            b = r3(b, c, d, a, numArray[6], 0x17, 0x4881d05);
            a = r3(a, b, c, d, numArray[9], 4, 0xd9d4d039);
            d = r3(d, a, b, c, numArray[12], 11, 0xe6db99e5);
            c = r3(c, d, a, b, numArray[15], 0x10, 0x1fa27cf8);
            b = r3(b, c, d, a, numArray[2], 0x17, 0xc4ac5665);
            a = r4(a, b, c, d, numArray[0], 6, 0xf4292244);
            d = r4(d, a, b, c, numArray[7], 10, 0x432aff97);
            c = r4(c, d, a, b, numArray[14], 15, 0xab9423a7);
            b = r4(b, c, d, a, numArray[5], 0x15, 0xfc93a039);
            a = r4(a, b, c, d, numArray[12], 6, 0x655b59c3);
            d = r4(d, a, b, c, numArray[3], 10, 0x8f0ccc92);
            c = r4(c, d, a, b, numArray[10], 15, 0xffeff47d);
            b = r4(b, c, d, a, numArray[1], 0x15, 0x85845dd1);
            a = r4(a, b, c, d, numArray[8], 6, 0x6fa87e4f);
            d = r4(d, a, b, c, numArray[15], 10, 0xfe2ce6e0);
            c = r4(c, d, a, b, numArray[6], 15, 0xa3014314);
            b = r4(b, c, d, a, numArray[13], 0x15, 0x4e0811a1);
            a = r4(a, b, c, d, numArray[4], 6, 0xf7537e82);
            d = r4(d, a, b, c, numArray[11], 10, 0xbd3af235);
            c = r4(c, d, a, b, numArray[2], 15, 0x2ad7d2bb);
            b = r4(b, c, d, a, numArray[9], 0x15, 0xeb86d391);
            ABCDValue.A = a + ABCDValue.A;
            ABCDValue.B = b + ABCDValue.B;
            ABCDValue.C = c + ABCDValue.C;
            ABCDValue.D = d + ABCDValue.D;
        }

        internal static byte[] GetHashFinalBlock(byte[] input, int ibStart, int cbSize, ABCDStruct ABCD, long len)
        {
            byte[] destinationArray = new byte[0x40];
            byte[] bytes = BitConverter.GetBytes(len);
            Array.Copy(input, ibStart, destinationArray, 0, cbSize);
            destinationArray[cbSize] = 0x80;
            if (cbSize <= 0x38)
            {
                Array.Copy(bytes, 0, destinationArray, 0x38, 8);
                GetHashBlock(destinationArray, ref ABCD, 0);
            }
            else
            {
                GetHashBlock(destinationArray, ref ABCD, 0);
                destinationArray = new byte[0x40];
                Array.Copy(bytes, 0, destinationArray, 0x38, 8);
                GetHashBlock(destinationArray, ref ABCD, 0);
            }
            byte[] buffer3 = new byte[0x10];
            Array.Copy(BitConverter.GetBytes(ABCD.A), 0, buffer3, 0, 4);
            Array.Copy(BitConverter.GetBytes(ABCD.B), 0, buffer3, 4, 4);
            Array.Copy(BitConverter.GetBytes(ABCD.C), 0, buffer3, 8, 4);
            Array.Copy(BitConverter.GetBytes(ABCD.D), 0, buffer3, 12, 4);
            return buffer3;
        }

        private static uint LSR(uint i, int s)
        {
            return ((i << s) | (i >> (0x20 - s)));
        }

        private static uint r1(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            return (b + LSR(((a + ((b & c) | ((b ^ uint.MaxValue) & d))) + x) + t, s));
        }

        private static uint r2(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            return (b + LSR(((a + ((b & d) | (c & (d ^ uint.MaxValue)))) + x) + t, s));
        }

        private static uint r3(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            return (b + LSR(((a + ((b ^ c) ^ d)) + x) + t, s));
        }

        private static uint r4(uint a, uint b, uint c, uint d, uint x, int s, uint t)
        {
            return (b + LSR(((a + (c ^ (b | (d ^ uint.MaxValue)))) + x) + t, s));
        }

    }
}