namespace Socks5.Core.Socks
{
    public enum AuthTypes
    {
        Login = 0x02,
        SocksCompress = 0x88,
        SocksEncrypt = 0x90,
        SocksBoth = 0xFE,
        Unsupported = 0xFF,
        None = 0x00
    }
}

