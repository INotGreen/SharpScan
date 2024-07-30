using System.Net;

namespace Tamir.SharpSsh.Sharp.net
{
    /// <summary>
    /// Summary description for InetAddress.
    /// </summary>
    public class InetAddress
    {
        internal IPAddress addr;

        public InetAddress(string addr)
        {
            this.addr = IPAddress.Parse(addr);
        }

        public InetAddress(IPAddress addr)
        {
            this.addr = addr;
        }

        public bool isAnyLocalAddress()
        {
            return IPAddress.IsLoopback(addr);
        }

        public bool equals(InetAddress addr)
        {
            return addr.ToString().Equals(addr.ToString());
        }

        public bool equals(string addr)
        {
            return addr.Equals(addr);
        }

        public override string ToString()
        {
            return addr.ToString();
        }

        public override bool Equals(object obj)
        {
            return equals(obj.ToString());
        }

        public string getHostAddress()
        {
            return ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static InetAddress getByName(string name)
        {
#if NET_1_1
			return new InetAddress( Dns.GetHostByName(name).AddressList[0] );
#else
            return new InetAddress(Dns.GetHostEntry(name).AddressList[0]);
#endif
        }
    }
}