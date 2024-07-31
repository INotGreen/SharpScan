using System;

namespace SharpRDPCheck
{
    internal class RC4
    {
        public static readonly int DECRYPT = 2;
        public static readonly int ENCRYPT = 1;
        private int[] sBox = new int[0x100];
        public static readonly int UNINITIALIZED = 0;
        private int state;
        private int x;
        private int y;

        public byte[] crypt(byte[] data)
        {
            byte[] outData = new byte[data.Length];
            this.engineUpdate(data, 0, data.Length, outData, 0);
            return outData;
        }

        public byte[] crypt(byte[] data, int position, int length)
        {
            byte[] outData = new byte[length];
            this.engineUpdate(data, position, length, outData, 0);
            return outData;
        }

        public void engineInitDecrypt(byte[] key)
        {
            this.makeKey(key);
            this.state = ENCRYPT;
        }

        public void engineInitEncrypt(byte[] key)
        {
            this.makeKey(key);
            this.state = ENCRYPT;
        }

        protected int engineUpdate(byte[] inData, int inOffset, int inLen, byte[] outData, int outOffset)
        {
            if (inLen < 0)
            {
                throw new RDFatalException("inLen < 0");

            }
            this.getState();
            int eNCRYPT = ENCRYPT;
            if ((inData == outData) && (((outOffset >= inOffset) && (outOffset < (inOffset + inLen))) || ((inOffset >= outOffset) && (inOffset < (outOffset + inLen)))))
            {
                byte[] destinationArray = new byte[inLen];
                Array.Copy(inData, inOffset, destinationArray, 0, inLen);
                inData = destinationArray;
                inOffset = 0;
            }
            this.rc4(inData, inOffset, inLen, outData, outOffset);
            return inLen;
        }

        public int getState()
        {
            return this.state;
        }

        private void makeKey(byte[] userkey)
        {
            if (userkey == null)
            {
                throw new RDFatalException("Null key for RC4");
            }

            int length = userkey.Length;

            if (length == 0)
            {
                throw new RDFatalException("Zero key length in RC4");
            }

            this.x = this.y = 0;

            for (int i = 0; i < 0x100; i++)
            {
                this.sBox[i] = i;
            }

            int index = 0;
            int num4 = 0;

            for (int j = 0; j < 0x100; j++)
            {
                num4 = (((userkey[index] & 0xff) + this.sBox[j]) + num4) & 0xff;
                int num6 = this.sBox[j];
                this.sBox[j] = this.sBox[num4];
                this.sBox[num4] = num6;
                index = (index + 1) % length;
            }
        }

        private void rc4(byte[] inData, int inOffset, int inLen, byte[] outData, int outOffset)
        {
            for (int i = 0; i < inLen; i++)
            {
                this.x = (this.x + 1) & 0xff;
                this.y = (this.sBox[this.x] + this.y) & 0xff;
                int num2 = this.sBox[this.x];
                this.sBox[this.x] = this.sBox[this.y];
                this.sBox[this.y] = num2;
                int index = (this.sBox[this.x] + this.sBox[this.y]) & 0xff;
                outData[outOffset++] = (byte) (inData[inOffset++] ^ this.sBox[index]);
            }
        }

    }
}