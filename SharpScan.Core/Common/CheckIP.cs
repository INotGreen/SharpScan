using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpScan
{
    public class GetIP
    {
        public static List<string> IPList(string IPRange)
        {

            List<string> strings = new List<string>();
            List<IPAddress> ipList = GetIPRange(IPRange);

            // 输出IP地址列表
            foreach (var ip in ipList)
            {
                strings.Add(ip.ToString());
            }
            return strings;
        }

        public static List<IPAddress> GetIPRange(string cidr)
        {
            var ipList = new List<IPAddress>();

            // 解析CIDR
            string[] parts = cidr.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid CIDR format");
            }

            IPAddress baseAddress = IPAddress.Parse(parts[0]);
            int prefixLength = int.Parse(parts[1]);

            // 获取子网掩码
            uint mask = ~(uint.MaxValue >> prefixLength);

            // 获取起始IP地址的整数表示
            byte[] baseAddressBytes = baseAddress.GetAddressBytes();
            Array.Reverse(baseAddressBytes); // 以网络字节顺序排列
            uint baseAddressInt = BitConverter.ToUInt32(baseAddressBytes, 0);

            // 计算子网中的起始IP地址
            uint startAddressInt = baseAddressInt & mask;

            // 计算子网中的结束IP地址
            uint endAddressInt = startAddressInt | ~mask;

            // 生成IP地址列表
            for (uint addressInt = startAddressInt; addressInt <= endAddressInt; addressInt++)
            {
                byte[] addressBytes = BitConverter.GetBytes(addressInt);
                Array.Reverse(addressBytes); // 转换为主机字节顺序
                ipList.Add(new IPAddress(addressBytes));
            }

            return ipList;
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
            //JavaScriptSerializer jss = new JavaScriptSerializer();
            //try
            //{
            //    string jsonData = File.ReadAllText(path);
            //    MACDict = jss.Deserialize<Dictionary<string, string>>(jsonData);
            //}
            //catch
            //{
            //    return MACDict;
            //}
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
