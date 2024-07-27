using System;
using System.Collections.Generic;
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
