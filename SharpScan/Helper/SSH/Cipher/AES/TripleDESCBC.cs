using System;
using System.Security.Cryptography;

namespace Tamir.SharpSsh.jsch.jce
{
    /* -*-mode:Sharp; c-basic-offset:2; -*- */
    /*
	Copyright (c) 2002,2003,2004 ymnk, JCraft,Inc. All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:

	  1. Redistributions of source code must retain the above copyright notice,
		 this list of conditions and the following disclaimer.

	  2. Redistributions in binary form must reproduce the above copyright 
		 notice, this list of conditions and the following disclaimer in 
		 the documentation and/or other materials provided with the distribution.

	  3. The names of the authors may not be used to endorse or promote products
		 derived from this software without specific prior written permission.

	THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
	INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
	FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
	INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
	INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
	LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
	OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
	LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
	NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
	EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	*/

    public class TripleDESCBC : Cipher
    {
        private const int ivsize = 8;
        private const int bsize = 24;
        private ICryptoTransform cipher;
        private TripleDES triDes;
        //private Sharpx.crypto.Cipher cipher;    
        public override int getIVSize()
        {
            return ivsize;
        }

        public override int getBlockSize()
        {
            return bsize;
        }

        public override void init(int mode, byte[] key, byte[] iv)
        {
            triDes = new TripleDESCryptoServiceProvider();
            triDes.Mode = CipherMode.CBC;
            triDes.Padding = PaddingMode.None;
            //String pad="NoPadding";      
            //if(padding) pad="PKCS5Padding";
            byte[] tmp;
            if (iv.Length > ivsize)
            {
                tmp = new byte[ivsize];
                Array.Copy(iv, 0, tmp, 0, tmp.Length);
                iv = tmp;
            }
            if (key.Length > bsize)
            {
                tmp = new byte[bsize];
                Array.Copy(key, 0, tmp, 0, tmp.Length);
                key = tmp;
            }

            try
            {
                //      cipher=Sharpx.crypto.Cipher.getInstance("DESede/CBC/"+pad);
                /*
					  // The following code does not work on IBM's JDK 1.4.1
					  SecretKeySpec skeySpec = new SecretKeySpec(key, "DESede");
					  cipher.init((mode==ENCRYPT_MODE?
						   Sharpx.crypto.Cipher.ENCRYPT_MODE:
						   Sharpx.crypto.Cipher.DECRYPT_MODE),
						  skeySpec, new IvParameterSpec(iv));
				*/
                //      DESedeKeySpec keyspec=new DESedeKeySpec(key);
                //      SecretKeyFactory keyfactory=SecretKeyFactory.getInstance("DESede");
                //      SecretKey _key=keyfactory.generateSecret(keyspec);
                //      cipher.init((mode==ENCRYPT_MODE?
                //		   Sharpx.crypto.Cipher.ENCRYPT_MODE:
                //		   Sharpx.crypto.Cipher.DECRYPT_MODE),
                //		  _key, new IvParameterSpec(iv));
                cipher = (mode == ENCRYPT_MODE
                              ? triDes.CreateEncryptor(key, iv)
                              : triDes.CreateDecryptor(key, iv));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                cipher = null;
            }
        }

        public override void update(byte[] foo, int s1, int len, byte[] bar, int s2)
        {
            // cipher.update(foo, s1, len, bar, s2);
            cipher.TransformBlock(foo, s1, len, bar, s2);
        }

        public override string ToString()
        {
            return "3des-cbc";
        }
    }
}