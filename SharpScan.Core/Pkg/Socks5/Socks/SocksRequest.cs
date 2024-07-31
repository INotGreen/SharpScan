using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Socks5.Core.Socks
{
    public class SocksRequest
    {
        public SocksRequest(StreamTypes type, AddressType addrtype, string address, int port)
        {
            Type = addrtype;
            StreamType = type;
            Address = address;
            Port = port;
            Error = SocksError.Granted;
            var p = Ip; //get Error on the stack.
        }

        public AddressType Type { get; set; }
        public StreamTypes StreamType { get; private set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public SocksError Error { get; set; }

        public IPAddress Ip
        {
            get
            {
                if (Type == AddressType.Ip)
                {
                    try
                    {
                        return IPAddress.Parse(Address);
                    }
                    catch
                    {
                        Error = SocksError.NotSupported;
                        return null;
                    }
                }

                if (Type != AddressType.Domain)
                {
                    return null;
                }

                try
                {
                    foreach (var p in Dns.GetHostAddresses(Address))
                        if (p.AddressFamily == AddressFamily.InterNetwork)
                            return p;
                    return null;
                }
                catch
                {
                    Error = SocksError.HostUnreachable;
                    return null;
                }
            }
        }

        public byte[] GetData(bool networkToHostOrder)
        {
            byte[] data;
            var port = 0;
            port = networkToHostOrder ? IPAddress.NetworkToHostOrder(Port) : IPAddress.HostToNetworkOrder((short)Port);

            switch (Type)
            {
                case AddressType.Ip:
                    {
                        data = new byte[10];
                        var content = Ip?.ToString().Split('.');
                        for (var i = 4; i < content?.Length + 4; i++)
                        {
                            data[i] = Convert.ToByte(Convert.ToInt32(content?[i - 4]));
                        }
                        Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, 8, 2);
                        break;
                    }
                case AddressType.Domain:
                    data = new byte[Address.Length + 7];
                    data[4] = Convert.ToByte(Address.Length);
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(Address), 0, data, 5, Address.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, data.Length - 2, 2);
                    break;
                default:
                    throw new NotImplementedException();
            }

            data[0] = 0x05;
            data[1] = (byte)Error;
            data[2] = 0x00;
            data[3] = (byte)Type;
            return data;
        }
    }
}

