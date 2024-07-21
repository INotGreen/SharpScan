using System;
using System.IO;
using System.Security.Cryptography;

namespace Tamir.SharpSsh.jsch.jce
{
    /* -*-mode:java; c-basic-offset:2; -*- */
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

    public class SignatureDSA : jsch.SignatureDSA
    {
        //java.security.Signature signature;
        //  KeyFactory keyFactory;
        private DSAParameters DSAKeyInfo;
        private CryptoStream cs;
        private SHA1CryptoServiceProvider sha1;

        #region SignatureDSA Members

        public void init()
        {
            sha1 = new SHA1CryptoServiceProvider();
            cs = new CryptoStream(Stream.Null, sha1, CryptoStreamMode.Write);
        }

        public void setPubKey(byte[] y, byte[] p, byte[] q, byte[] g)
        {
            DSAKeyInfo.Y = Util.stripLeadingZeros(y);
            DSAKeyInfo.P = Util.stripLeadingZeros(p);
            DSAKeyInfo.Q = Util.stripLeadingZeros(q);
            DSAKeyInfo.G = Util.stripLeadingZeros(g);
        }

        public void setPrvKey(byte[] x, byte[] p, byte[] q, byte[] g)
        {
            DSAKeyInfo.X = Util.stripLeadingZeros(x);
            DSAKeyInfo.P = Util.stripLeadingZeros(p);
            DSAKeyInfo.Q = Util.stripLeadingZeros(q);
            DSAKeyInfo.G = Util.stripLeadingZeros(g);
        }

        //This method will probably won't work, we need to get rid of the ASN.1 format (Tamir)
        public byte[] sign()
        {
            //byte[] sig=signature.sign();   
            cs.Close();
            var DSA = new DSACryptoServiceProvider();
            DSA.ImportParameters(DSAKeyInfo);
            var DSAFormatter = new DSASignatureFormatter(DSA);
            DSAFormatter.SetHashAlgorithm("SHA1");

            byte[] sig = DSAFormatter.CreateSignature(sha1);
            return sig;
        }

        public void update(byte[] foo)
        {
            //signature.update(foo);
            cs.Write(foo, 0, foo.Length);
        }

        public bool verify(byte[] sig)
        {
            cs.Close();
            var DSA = new DSACryptoServiceProvider();
            DSA.ImportParameters(DSAKeyInfo);
            var DSADeformatter = new DSASignatureDeformatter(DSA);
            DSADeformatter.SetHashAlgorithm("SHA1");

            long i = 0;
            long j = 0;
            byte[] tmp;

            //This makes sure sig is always 40 bytes?
            if (sig[0] == 0 && sig[1] == 0 && sig[2] == 0)
            {
                long i1 = (sig[i++] << 24) & 0xff000000;
                long i2 = (sig[i++] << 16) & 0x00ff0000;
                long i3 = (sig[i++] << 8) & 0x0000ff00;
                long i4 = (sig[i++]) & 0x000000ff;
                j = i1 | i2 | i3 | i4;

                i += j;

                i1 = (sig[i++] << 24) & 0xff000000;
                i2 = (sig[i++] << 16) & 0x00ff0000;
                i3 = (sig[i++] << 8) & 0x0000ff00;
                i4 = (sig[i++]) & 0x000000ff;
                j = i1 | i2 | i3 | i4;

                tmp = new byte[j];
                Array.Copy(sig, i, tmp, 0, j);
                sig = tmp;
            }
            bool res = DSADeformatter.VerifySignature(sha1, sig);
            return res;
        }

        #endregion
    }
}