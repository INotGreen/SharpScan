using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpScan
{
    internal class GetOsInfos
    {
        static int TIME_OUT = 1500;
        private static ConcurrentDictionary<string, string> dnsCache = new ConcurrentDictionary<string, string>();

        public  string GetOsVersion(string ip)
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    sock.ReceiveTimeout = TIME_OUT;
                    sock.SendTimeout = TIME_OUT;
                    sock.Connect(ip, 135);

                    byte[] buffer_v2 = new byte[] {
                        0x05, 0x00, 0x0b, 0x03, 0x10, 0x00, 0x00, 0x00, 0x78, 0x00, 0x28, 0x00, 0x03, 0x00, 0x00, 0x00,
                        0xb8, 0x10, 0xb8, 0x10, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                        0xa0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46,
                        0x00, 0x00, 0x00, 0x00, 0x04, 0x5d, 0x88, 0x8a, 0xeb, 0x1c, 0xc9, 0x11, 0x9f, 0xe8, 0x08, 0x00,
                        0x2b, 0x10, 0x48, 0x60, 0x02, 0x00, 0x00, 0x00, 0x0a, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x82, 0x08, 0xa2,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x06, 0x01, 0xb1, 0x1d, 0x00, 0x00, 0x00, 0x0f
                    };

                    sock.Send(buffer_v2);
                    byte[] packet2 = new byte[4096];
                    int received = sock.Receive(packet2);

                    var osVersionBytes = packet2.Skip(0xa0 - 54 + 10).Take(8).ToArray();
                    int majorVersion = osVersionBytes[0];
                    int minorVersion = osVersionBytes[1];
                    int buildNumber = BitConverter.ToInt16(osVersionBytes, 2);
                    string osVersion = $"Windows Version {majorVersion}.{minorVersion} Build {buildNumber}";

                    string detailedOsVersion = GetDetailedOsVersion(majorVersion, minorVersion, buildNumber);

                    return detailedOsVersion;

                }
                catch (Exception ex)
                {
                    return "null";
                }
            }
        }

        private  string GetDetailedOsVersion(int majorVersion, int minorVersion, int buildNumber)
        {
            if (majorVersion == 10)
            {
                if (buildNumber >= 22000) return "Windows 11";
                if (buildNumber >= 19041) return "Windows 10 Version 2004 or later";
                if (buildNumber >= 18362) return "Windows 10 Version 1903 or 1909";
                if (buildNumber >= 17763) return "Windows 10 Version 1809";
                if (buildNumber >= 17134) return "Windows 10 Version 1803";
                if (buildNumber >= 16299) return "Windows 10 Version 1709";
                if (buildNumber >= 15063) return "Windows 10 Version 1703";
                if (buildNumber >= 14393) return "Windows 10 Version 1607";
                if (buildNumber >= 10586) return "Windows 10 Version 1511";
                if (buildNumber >= 10240) return "Windows 10 Version 1507";
            }
            else if (majorVersion == 6 && minorVersion == 3)
            {
                if (buildNumber == 9600) return "Windows 8.1 or Windows Server 2012 R2";
            }
            else if (majorVersion == 6 && minorVersion == 2)
            {
                if (buildNumber == 9200) return "Windows 8 or Windows Server 2012";
            }
            else if (majorVersion == 6 && minorVersion == 1)
            {
                if (buildNumber == 7601) return "Windows 7 SP1 or Windows Server 2008 R2 SP1";
                if (buildNumber == 7600) return "Windows 7 or Windows Server 2008 R2";
            }
            else if (majorVersion == 6 && minorVersion == 0)
            {
                if (buildNumber == 6002) return "Windows Vista SP2 or Windows Server 2008 SP2";
                if (buildNumber == 6001) return "Windows Vista SP1 or Windows Server 2008 SP1";
                if (buildNumber == 6000) return "Windows Vista or Windows Server 2008";
            }

            return $"Unknown Windows Version {majorVersion}.{minorVersion} Build {buildNumber}";
        }

        [DllImport("Iphlpapi.dll")]
        static extern int SendARP(Int32 DestIP, Int32 SrcIP, ref Int64 MacAddr, ref Int32 PhyAddrLen);
        [DllImport("Ws2_32.dll")]
        static extern Int32 inet_addr(string ipaddr);

        public string getMacAddr(string RemoteIP)
        {
            StringBuilder macAddress = new StringBuilder();

            try
            {
                Int32 remote = inet_addr(RemoteIP);
                Int64 macInfo = new Int64();
                Int32 length = 6;
                SendARP(remote, 0, ref macInfo, ref length);
                string temp = Convert.ToString(macInfo, 16).PadLeft(12, '0').ToUpper();
                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        macAddress.Append(temp.Substring(x - 2, 2));
                    }
                    else
                    {
                        macAddress.Append(temp.Substring(x - 2, 2) + "-");
                    }
                    x -= 2;
                }
                if (macAddress.ToString() == "00-00-00-00-00-00")
                    return "                 "; //17

                return macAddress.ToString();
            }
            catch
            {
                return "00-00-00-00-00-00";
            }
        }

        public  string GetHostName(string IP)
        {
            try
            {
                if (dnsCache.TryGetValue(IP, out string cachedName))
                {
                    return cachedName;
                }

                string OSname = Dns.GetHostEntry(IP).HostName;
                if (OSname == IP)
                    return "NULL";

                dnsCache.TryAdd(IP, OSname);
                return OSname;
            }
            catch (Exception)
            {
                return "NULL";
            }
        }
    }
}
