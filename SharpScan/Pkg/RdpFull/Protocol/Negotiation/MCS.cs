using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpRDPCheck
{
    internal class MCS
    {
        internal static readonly int DPUM = 8;
        internal static readonly int SDIN = 0x1a;
        internal static readonly int SDRQ = 0x19;
        internal static readonly int MCS_USERCHANNEL_BASE = 0x3e9; // 1001
        internal static readonly int MSC_GLOBAL_CHANNEL = 0x3eb; // 1003
        internal static List<int> serverSupportedChannels = new List<int>();

        internal static void sendConnectionRequest(byte[] loadBalanceToken, bool bAutoReconnect)
        {
            int num;
            Network.ConnectionStage = Network.eConnectionStage.Negotiating;

            if (Options.enableNLA)
            {
                // Client X.224 Connection Request PDU

                sendConnectNegotiation(
                    NegotiationProtocol.PROTOCOL_RDP |
                    NegotiationProtocol.PROTOCOL_SSL |
                    NegotiationProtocol.PROTOCOL_HYBRID,
                    loadBalanceToken);

                // Server X.224 Connection Confirm PDU
                num = receiveConnectNegotiation();


               if (((num & 1) != 0) || ((num & 2) != 0))
                    {
                        Network.ConnectionStage = Network.eConnectionStage.Securing;
                        Network.ConnectSSL();
                    }

                    if ((num & 2) != 0)
                    {
                        Network.ConnectionStage = Network.eConnectionStage.Authenticating;
                        CredSSP.Negotiate(Network.GetSSLPublicKey());
                    }
                
            }
            else
            {
                // Client X.224 Connection Request PDU
                sendConnectNegotiation(NegotiationProtocol.PROTOCOL_RDP, loadBalanceToken);

                // Server X.224 Connection Confirm PDU
                num = receiveConnectNegotiation();

                if (num != 0)
                {
                    throw new RDFatalException("Security negotiation failed!");
                }
            }
            if(Options.hash.Length>0)
                Console.WriteLine($"[+] {Options.Host} login successful:" + Options.Username + "  " + Options.hash);
            else
                Console.WriteLine($"[+] {Options.Host} Login failed:");

        }

        
        /// <summary>
        /// Client X.224 Connection Request PDU
        /// </summary>
        private static void sendConnectNegotiation(NegotiationProtocol NegotiationFlags, byte[] loadBalanceToken)
        {
            string domainAndUsername = Options.DomainAndUsername;

            if (domainAndUsername.Length > 9)
            {
                domainAndUsername = domainAndUsername.Substring(0, 9);
            }

            RdpPacket packet = new RdpPacket();
            packet.WriteByte(3);
            packet.WriteByte(0);
            long position = packet.Position;
            packet.WriteBigEndian16((short)0);
            packet.WriteByte(0);
            packet.WriteByte(0xe0);
            packet.WriteBigEndian16((short)0);
            packet.WriteBigEndian16((short)0);
            packet.WriteByte(0);

            if (loadBalanceToken != null)
            {
                packet.Write(loadBalanceToken, 0, loadBalanceToken.Length);
                packet.WriteString("\r\n", false);
            }
            else
            {
                packet.WriteString("Cookie: mstshash=" + domainAndUsername + "\r\n", true);
            }

            // RDP Negotiation Request
            packet.WriteByte(0x01);
            packet.WriteByte(0);
            packet.WriteLittleEndian16((short)8);
            packet.WriteLittleEndian32((int)NegotiationFlags); // Standard RDP Security, TLS 1.0, CredSSP

            long num2 = packet.Position;
            packet.Position = position;
            packet.WriteBigEndian16((short)num2);
            packet.WriteByte((byte)(num2 - 5L));

            IsoLayer.Write(packet);
        }

        /// <summary>
        /// Server X.224 Connection Confirm PDU
        /// </summary>
        private static int receiveConnectNegotiation()
        {
            RdpPacket packet = ISO.Receive();
            packet.Position += 7L;

            if (packet.Position >= packet.Length)
            {
                return 0;
            }

            switch (packet.ReadByte())
            {
                // TYPE_RDP_NEG_RSP
                case 0x02:
                    Options.serverNegotiateFlags = (NegotiationFlags)packet.ReadByte();
                    packet.ReadLittleEndian16();
                    return packet.ReadLittleEndian32();

                // TYPE_RDP_NEG_FAILURE
                case 0x03:
                    packet.ReadByte();
                    packet.ReadLittleEndian16();

                    switch ((NegotiationFailureCodes)packet.ReadLittleEndian32())
                    {
                        case NegotiationFailureCodes.SSL_REQUIRED_BY_SERVER:
                            throw new RDFatalException("The server requires that the client support Enhanced RDP Security with TLS 1.0");

                        case NegotiationFailureCodes.SSL_NOT_ALLOWED_BY_SERVER:
                            return 0x10000000;

                        case NegotiationFailureCodes.SSL_CERT_NOT_ON_SERVER:
                            throw new RDFatalException("The server does not possess a valid authentication certificate and cannot initialize the External Security Protocol Provider");

                        case NegotiationFailureCodes.INCONSISTENT_FLAGS:
                            throw new RDFatalException("The list of requested security protocols is not consistent with the current security protocol in effect.");

                        case NegotiationFailureCodes.HYBRID_REQUIRED_BY_SERVER:
                            throw new RDFatalException("The server requires that the client support Enhanced RDP Security with CredSSP");

                        case NegotiationFailureCodes.SSL_WITH_USER_AUTH_REQUIRED_BY_SERVER:
                            throw new RDFatalException("The server requires that the client support Enhanced RDP Security and certificate-based client authentication");
                    }

                    throw new RDFatalException("Unknown Negotiation failure!");
            }

            throw new RDFatalException("Negotiation failed, requested security level not supported by server.");
        }
        
        [Flags]
        private enum NegotiationProtocol
        {
            PROTOCOL_RDP = 0x00000000,
            PROTOCOL_SSL = 0x00000001,
            PROTOCOL_HYBRID = 0x00000002
        }

        [Flags]
        internal enum NegotiationFlags
        {
            EXTENDED_CLIENT_DATA_SUPPORTED = 0x01,
            DYNVC_GFX_PROTOCOL_SUPPORTED = 0x02,
            NEGRSP_FLAG_RESERVED = 0x04,
            RESTRICTED_ADMIN_MODE_SUPPORTED = 0x08
        }
    
        [Flags]
        private enum NegotiationFailureCodes
        {
            SSL_REQUIRED_BY_SERVER = 0x00000001,
            SSL_NOT_ALLOWED_BY_SERVER = 0x00000002,
            SSL_CERT_NOT_ON_SERVER = 0x00000003,
            INCONSISTENT_FLAGS = 0x00000004,
            HYBRID_REQUIRED_BY_SERVER = 0x00000005,
            SSL_WITH_USER_AUTH_REQUIRED_BY_SERVER = 0x00000006
        }
    }
}