using System.Net;

namespace Socks5.Core.Socks
{
    public class User
    {
        public User(byte authTypeVersion, string un, string pw, IPEndPoint ip)
        {
            AuthTypeVersion = authTypeVersion;
            Username = un;
            Password = pw;
            Ip = ip;
        }

        public byte AuthTypeVersion { get; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public IPEndPoint Ip { get; private set; }
    }
}

