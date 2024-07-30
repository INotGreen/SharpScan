using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace SharpScan
{
    public class GetIP
    {
        public static List<string> IPList(string IPRange)
        {
            List<string> strings = new List<string>();
            List<string> ipList = GetIPRange(IPRange);

            // 输出IP地址列表
            foreach (var ip in ipList)
            {
                strings.Add(ip);
            }
            return strings;
        }

        public static List<string> GetIPRange(string ipRange)
        {
            var ipList = new List<string>();

            if (!IPAddress.TryParse(ipRange.Split('/')[0], out IPAddress baseAddress))
            {
                throw new ArgumentException("Invalid IP address format");
            }

            if (!ipRange.Contains("/"))
            {
                // 如果没有CIDR前缀，直接返回单个IP
                ipList.Add(baseAddress.ToString());
                return ipList;
            }

            int prefixLength = GetPrefixLength(baseAddress);

            string[] parts = ipRange.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out prefixLength))
            {
                throw new ArgumentException("Invalid CIDR format");
            }

            uint mask = ~(uint.MaxValue >> prefixLength);
            byte[] baseAddressBytes = baseAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(baseAddressBytes); // 以网络字节顺序排列
            }
            uint baseAddressInt = BitConverter.ToUInt32(baseAddressBytes, 0);

            uint startAddressInt = baseAddressInt & mask;
            uint endAddressInt = startAddressInt | ~mask;

            for (uint addressInt = startAddressInt; addressInt <= endAddressInt; addressInt++)
            {
                byte[] addressBytes = BitConverter.GetBytes(addressInt);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(addressBytes); // 转换为主机字节顺序
                }
                ipList.Add(new IPAddress(addressBytes).ToString());
            }

            return ipList;
        }
        public static int GetPrefixLength(IPAddress ipAddress)
        {
            byte[] addressBytes = ipAddress.GetAddressBytes();
            if (addressBytes[0] >= 1 && addressBytes[0] <= 126)
            {
                return 8;  // A类地址
            }
            else if (addressBytes[0] >= 128 && addressBytes[0] <= 191)
            {
                return 16; // B类地址
            }
            else if (addressBytes[0] >= 192 && addressBytes[0] <= 223)
            {
                return 24; // C类地址
            }
            else
            {
                throw new ArgumentException("IP address is not in the A, B, or C class ranges");
            }
        }
        public static Dictionary<string, string> GetMACDict()
        {
            Dictionary<string, string> MACDict = new Dictionary<string, string>()
                {
                    {"000c29","Vmware"},
                    {"005056","Vmware"},
                    {"000569","Vmware"},
                    {"001c14","Vmware"},
                    {"0242ac","Docker"},
                    {"0003ff","HyperV" },
                    {"000D3a","HyperV" },
                    {"00125a","HyperV" },
                    {"00155d","HyperV" },
                    {"0017fa","HyperV" },
                    {"001dd8","HyperV" },
                    {"002248","HyperV" },
                    {"0025ae","HyperV" },
                    {"0050f2","HyperV" },
                    {"444553","HyperV" },
                    {"7Ced8d","HyperV" },
                    {"0010e0","VirtualBox" },
                    {"00144f","VirtualBox" },
                    {"0020f2","VirtualBox" },
                    {"002128","VirtualBox" },
                    {"0021f6","VirtualBox" },
                    {"080027","VirtualBox" },
                    {"001c42","ParallelsVM" },
                    {"00163e","XensourceVM" },
                    {"080020","VirtualBox" },
                    {"0050c2","IEEE ReGi VM" },
                };
            // https://www.wireshark.org/assets/js/manuf.json
            string path = "manuf.json";
            if (!File.Exists(path))
            {
                return MACDict;
            }
            //SharpScriptSerializer jss = new SharpScriptSerializer();
            try
            {
                string jsonData = File.ReadAllText(path);
                //MACDict = jss.Deserialize<Dictionary<string, string>>(jsonData);
            }
            catch
            {
                return MACDict;
            }
            return MACDict;
        }
        //检测输入的ip是否是一个有效的 IPv4 地址。
        public static bool regCheckIP(string IP)
        {
            Regex chkIP = new Regex(@"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$");
            if (chkIP.IsMatch(IP))
                return true;
            else
                return false;
        }
        //检查输入的字符串是否是一个部分的 IPv4 地址（即前三段）
        public static bool regIPRegion(string IPRegion)
        {
            Regex chkIP = new Regex(@"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$");
            if (chkIP.IsMatch(IPRegion))
                return true;
            else
                return false;
        }
        public static string getIPAddress(string hostName)
        {
            System.Net.IPAddress addr;
            addr = new System.Net.IPAddress(Dns.GetHostByName(hostName).AddressList[0].Address);
            return addr.ToString();
        }
    }
}
