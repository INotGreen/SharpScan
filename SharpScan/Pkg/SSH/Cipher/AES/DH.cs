using Org.Mentalis.Security.Cryptography;

//For DiffieHellman usage

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

    public class DH : jsch.DH
    {
        //internal byte[] K;  // shared secret key unused
        internal byte[] K_array;

        private DiffieHellman dh;
        internal byte[] e_array;
        internal byte[] f; // your public key
        internal byte[] g;
        internal byte[] p;

        #region DH Members

        public void init()
        {
        }

        public byte[] getE()
        {
            if (e_array == null)
            {
                dh = new DiffieHellmanManaged(p, g, 0);
                e_array = dh.CreateKeyExchange();
            }
            return e_array;
        }

        public byte[] getK()
        {
            if (K_array == null)
            {
                K_array = dh.DecryptKeyExchange(f);
            }
            return K_array;
        }

        public void setP(byte[] p)
        {
            this.p = p;
        }

        public void setG(byte[] g)
        {
            this.g = g;
        }

        public void setF(byte[] f)
        {
            this.f = f;
        }

        #endregion

        //  void setP(BigInteger p){this.p=p;}
        //  void setG(BigInteger g){this.g=g;}
        //  void setF(BigInteger f){this.f=f;}
    }
}