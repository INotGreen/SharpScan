using System;
using System.Threading;

namespace SharpRDPCheck
{
    internal class Network
    {        
        private static eConnectionStage m_ConnectionStage = eConnectionStage.None;
        private static PacketLogger m_Logger = null;
        internal static NetworkSocket m_OpenSocket = null;
        internal delegate void ConnectionStageChangedHandler();
        internal static event ConnectionStageChangedHandler ConnectionStageChanged;

        internal static void Connect(string host, int port)
        {
            ConnectionStage = eConnectionStage.Connecting;
            try
            {
                NetworkSocket socket = new NetworkSocket(host.Replace(".", ""));
                socket.Connect(host, port);
                m_OpenSocket = socket;
            }
            catch { }
        }

        internal static void ConnectSSL()
        {
            m_OpenSocket.ConnectSSL();
        } 

        public static byte[] GetSSLPublicKey()
        {
            return m_OpenSocket.GetSSLPublicKey();
        }

        public static int Receive(byte[] buffer)
        {
            return Receive(buffer, buffer.Length);
        }

        public static int Receive(byte[] buffer, int size)
        {
            return m_OpenSocket.Receive(buffer, size);
        }

        public static int Send(byte[] buffer)
        {
            return m_OpenSocket.Send(buffer);
        }


        internal static eConnectionStage ConnectionStage
        {
            get
            {
                return m_ConnectionStage;
            }
            set
            {
                eConnectionStage connectionStage = m_ConnectionStage;
                m_ConnectionStage = value;
                if ((connectionStage != value) && (ConnectionStageChanged != null))
                {
                    ConnectionStageChanged();
                }
            }
        }

        public static PacketLogger Logger
        {
            get
            {
                return m_Logger;
            }
            set
            {
                m_Logger = value;
            }
        }

        public static NetworkSocket OpenSocket
        {
            get
            {
                return m_OpenSocket;
            }
        }


        public enum eConnectionStage
        {
            None,
            Connecting,
            ConnectingToGateway,
            ConnectingToHost,
            Negotiating,
            Securing,
            Authenticating,
            Establishing,
            Login,
            Reconnecting,
            SecureAndLogin
        }

    }
}