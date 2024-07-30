using System;
using System.Diagnostics;

namespace Tamir.SharpSsh.jsch
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


    public class DHG1 : KeyExchange
    {
        internal const int SSH_MSG_KEXDH_INIT = 30;
        internal const int SSH_MSG_KEXDH_REPLY = 31;

        internal const int RSA = 0;
        internal const int DSS = 1;
        internal static byte[] g = new byte[] {2};

        internal static byte[] p = new[]
                                       {
                                           (byte) 0x00,
                                           (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF,
                                           (byte) 0xFF, (byte) 0xFF,
                                           (byte) 0xC9, (byte) 0x0F, (byte) 0xDA, (byte) 0xA2, (byte) 0x21, (byte) 0x68,
                                           (byte) 0xC2, (byte) 0x34,
                                           (byte) 0xC4, (byte) 0xC6, (byte) 0x62, (byte) 0x8B, (byte) 0x80, (byte) 0xDC,
                                           (byte) 0x1C, (byte) 0xD1,
                                           (byte) 0x29, (byte) 0x02, (byte) 0x4E, (byte) 0x08, (byte) 0x8A, (byte) 0x67,
                                           (byte) 0xCC, (byte) 0x74,
                                           (byte) 0x02, (byte) 0x0B, (byte) 0xBE, (byte) 0xA6, (byte) 0x3B, (byte) 0x13,
                                           (byte) 0x9B, (byte) 0x22,
                                           (byte) 0x51, (byte) 0x4A, (byte) 0x08, (byte) 0x79, (byte) 0x8E, (byte) 0x34,
                                           (byte) 0x04, (byte) 0xDD,
                                           (byte) 0xEF, (byte) 0x95, (byte) 0x19, (byte) 0xB3, (byte) 0xCD, (byte) 0x3A,
                                           (byte) 0x43, (byte) 0x1B,
                                           (byte) 0x30, (byte) 0x2B, (byte) 0x0A, (byte) 0x6D, (byte) 0xF2, (byte) 0x5F,
                                           (byte) 0x14, (byte) 0x37,
                                           (byte) 0x4F, (byte) 0xE1, (byte) 0x35, (byte) 0x6D, (byte) 0x6D, (byte) 0x51,
                                           (byte) 0xC2, (byte) 0x45,
                                           (byte) 0xE4, (byte) 0x85, (byte) 0xB5, (byte) 0x76, (byte) 0x62, (byte) 0x5E,
                                           (byte) 0x7E, (byte) 0xC6,
                                           (byte) 0xF4, (byte) 0x4C, (byte) 0x42, (byte) 0xE9, (byte) 0xA6, (byte) 0x37,
                                           (byte) 0xED, (byte) 0x6B,
                                           (byte) 0x0B, (byte) 0xFF, (byte) 0x5C, (byte) 0xB6, (byte) 0xF4, (byte) 0x06,
                                           (byte) 0xB7, (byte) 0xED,
                                           (byte) 0xEE, (byte) 0x38, (byte) 0x6B, (byte) 0xFB, (byte) 0x5A, (byte) 0x89,
                                           (byte) 0x9F, (byte) 0xA5,
                                           (byte) 0xAE, (byte) 0x9F, (byte) 0x24, (byte) 0x11, (byte) 0x7C, (byte) 0x4B,
                                           (byte) 0x1F, (byte) 0xE6,
                                           (byte) 0x49, (byte) 0x28, (byte) 0x66, (byte) 0x51, (byte) 0xEC, (byte) 0xE6,
                                           (byte) 0x53, (byte) 0x81,
                                           (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF,
                                           (byte) 0xFF, (byte) 0xFF
                                       };

        internal byte[] I_C;
        internal byte[] I_S;
        internal byte[] V_C;
        internal byte[] V_S;

        //  byte[] K_S;

        private Buffer buf;
        internal DH dh;
        internal byte[] e;
        private Packet packet;
        private int state;
        private int type;

        public override void init(Session session,
                                  byte[] V_S, byte[] V_C, byte[] I_S, byte[] I_C)
        {
            this.session = session;
            this.V_S = V_S;
            this.V_C = V_C;
            this.I_S = I_S;
            this.I_C = I_C;

            //    sha=new SHA1();
            //    sha.init();
            try
            {
                Type t = Type.GetType(session.getConfig("sha-1"));
                sha = (HASH) (Activator.CreateInstance(t));
                sha.init();
            }
            catch (Exception ee)
            {
                Trace.WriteLine(ee, "SharpSSH");
            }

            buf = new Buffer();
            packet = new Packet(buf);

            try
            {
                Type t = Type.GetType(session.getConfig("dh"));
                dh = (DH) (Activator.CreateInstance(t));
                dh.init();
            }
            catch (Exception ee)
            {
                throw ee;
            }

            dh.setP(p);
            dh.setG(g);

            // The client responds with:
            // byte  SSH_MSG_KEXDH_INIT(30)
            // mpint e <- g^x mod p
            //         x is a random number (1 < x < (p-1)/2)

            e = dh.getE();

            packet.reset();
            buf.putByte(SSH_MSG_KEXDH_INIT);
            buf.putMPInt(e);
            session.write(packet);

            state = SSH_MSG_KEXDH_REPLY;
        }

        public override bool next(Buffer _buf)
        {
            int i, j;
            bool result = false;
            switch (state)
            {
                case SSH_MSG_KEXDH_REPLY:
                    // The server responds with:
                    // byte      SSH_MSG_KEXDH_REPLY(31)
                    // string    server public host key and certificates (K_S)
                    // mpint     f
                    // string    signature of H
                    j = _buf.getInt();
                    j = _buf.getByte();
                    j = _buf.getByte();
                    if (j != 31)
                    {
                        Trace.WriteLine("type: must be 31 " + j, "SharpSSH");
                        result = false;
                        break;
                    }

                    K_S = _buf.getString();
                    // K_S is server_key_blob, which includes ....
                    // string ssh-dss
                    // impint p of dsa
                    // impint q of dsa
                    // impint g of dsa
                    // impint pub_key of dsa
                    //System.out.print("K_S: "); //dump(K_S, 0, K_S.length);
                    byte[] f = _buf.getMPInt();
                    byte[] sig_of_H = _buf.getString();
                    /*
			  for(int ii=0; ii<sig_of_H.length;ii++){
				System.out.print(Integer.toHexString(sig_of_H[ii]&0xff));
				System.out.print(": ");
			  }
			  Console.WriteLine("");
					*/

                    dh.setF(f);
                    K = dh.getK();

                    //The hash H is computed as the HASH hash of the concatenation of the
                    //following:
                    // string    V_C, the client's version string (CR and NL excluded)
                    // string    V_S, the server's version string (CR and NL excluded)
                    // string    I_C, the payload of the client's SSH_MSG_KEXINIT
                    // string    I_S, the payload of the server's SSH_MSG_KEXINIT
                    // string    K_S, the host key
                    // mpint     e, exchange value sent by the client
                    // mpint     f, exchange value sent by the server
                    // mpint     K, the shared secret
                    // This value is called the exchange hash, and it is used to authenti-
                    // cate the key exchange.
                    buf.reset();
                    buf.putString(V_C);
                    buf.putString(V_S);
                    buf.putString(I_C);
                    buf.putString(I_S);
                    buf.putString(K_S);
                    buf.putMPInt(e);
                    buf.putMPInt(f);
                    buf.putMPInt(K);
                    var foo = new byte[buf.getLength()];
                    buf.getByte(foo);
                    sha.update(foo, 0, foo.Length);
                    H = sha.digest();
                    //System.out.print("H -> "); //dump(H, 0, H.length);

                    i = 0;
                    j = 0;
                    j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                        ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                    String alg = Util.getString(K_S, i, j);
                    i += j;

                    result = false;

                    if (alg.Equals("ssh-rsa"))
                    {
                        byte[] tmp;
                        byte[] ee;
                        byte[] n;

                        type = RSA;

                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        ee = tmp;
                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        n = tmp;

                        //	SignatureRSA sig=new SignatureRSA();
                        //	sig.init();

                        SignatureRSA sig = null;
                        try
                        {
                            Type t = Type.GetType(session.getConfig("signature.rsa"));
                            sig = (SignatureRSA) (Activator.CreateInstance(t));
                            sig.init();
                        }
                        catch (Exception eee)
                        {
                            Console.WriteLine(eee);
                        }

                        sig.setPubKey(ee, n);
                        sig.update(H);
                        result = sig.verify(sig_of_H);
                        //MainClass.dump(ee, n, sig_of_H, H);
                    }
                    else if (alg.Equals("ssh-dss"))
                    {
                        byte[] q = null;
                        byte[] tmp;
                        byte[] p;
                        byte[] g;

                        type = DSS;

                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        p = tmp;
                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        q = tmp;
                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        g = tmp;
                        j = (int) ((K_S[i++] << 24) & 0xff000000) | ((K_S[i++] << 16) & 0x00ff0000) |
                            ((K_S[i++] << 8) & 0x0000ff00) | ((K_S[i++]) & 0x000000ff);
                        tmp = new byte[j];
                        Array.Copy(K_S, i, tmp, 0, j);
                        i += j;
                        f = tmp;
                        //	SignatureDSA sig=new SignatureDSA();
                        //	sig.init();
                        SignatureDSA sig = null;
                        try
                        {
                            Type t = Type.GetType(session.getConfig("signature.dss"));
                            sig = (SignatureDSA) (Activator.CreateInstance(t));
                            sig.init();
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee);
                        }
                        sig.setPubKey(f, p, q, g);
                        sig.update(H);
                        result = sig.verify(sig_of_H);
                    }
                    else
                    {
                        Console.WriteLine("unknow alg");
                    }
                    state = STATE_END;
                    break;
            }
            return result;
        }

        public override String getKeyType()
        {
            if (type == DSS) return "DSA";
            return "RSA";
        }

        public override int getState()
        {
            return state;
        }
    }
}