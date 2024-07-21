using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpScan
{
    internal class CheckIP
    {

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
