using System;
using System.Collections.Generic;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        // 示例输入，可以更改为其他CIDR地址，例如 "10.0.0.0/8" 或 "172.16.0.0/16"
        
        List<IPAddress> ipList = GetIPRange(args[0]);

        // 输出IP地址列表
        foreach (var ip in ipList)
        {
            Console.WriteLine(ip);
        }
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
}
