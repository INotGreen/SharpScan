using System.Net.Sockets;

namespace Tamir.SharpSsh.Sharp.net
{
    /// <summary>
    /// Summary description for ServerSocket.
    /// </summary>
    public class ServerSocket : TcpListener
    {
        public ServerSocket(int port, int arg, InetAddress addr) : base(addr.addr, port)
        {
            Start();
        }

        public Socket accept()
        {
            return new Socket(AcceptSocket());
        }

        public void close()
        {
            Stop();
        }
    }
}