using Socks5.Core.Socks;
using System;

namespace Socks5.Core.TCP
{
    public class SocksClientEventArgs : EventArgs
    {
        public SocksClientEventArgs(SocksClient client)
        {
            Client = client;
        }

        public SocksClient Client { get; private set; }
    }
}

