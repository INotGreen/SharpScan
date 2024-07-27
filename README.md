





## 1.特点

- C#开发的内网资产扫描器，用于横向渗透的信息收集
- 参考了Ladon，Fscan、Kscan等扫描器的原理
- 为了兼容更古老的系统，所以使用.NET Framework3.5 和.NET Core6.0编译
- 使用异步和高并发、扫描速度快并且可控
- 用inline-assembly和execute-assembly进行内存加载，考虑绕过AMSI和ETW

- 体积相对较小(目前400kb)，传输快，使用方便


## 2. 主要功能

1.信息搜集:

- 存活探测(ICMP、ARP)
- 端口扫描(TCP)

2.爆破功能:

- 各类服务爆破(ssh、smb)

3.系统信息、漏洞扫描:

- netbios探测、域控识别(TODO)
- 获取目标网卡信息
- 高危漏洞扫描(ms17010)

4.Web探测功能:

- webtitle探测
- web指纹识别(常见cms、oa框架等)

6.其他功能:

- 文件保存

## 3.正在完成(TODO)

- RDP 弱口令

- 数据库密码爆破(mysql、mssql、redis、psql、oracle等)

- UDP端口扫描

  

## 4.兼容性：

Windows ：支持win7-win11，windows server2008-2022

Linux：支持 glibc 2.17以上 的系统

MacOS： arm x64_x86，intel_x64_86





## 5.使用

```powershell
C:\>SharpScan.exe

  ______   __                                       ______
 /      \ /  |                                     /      \
/$$$$$$  |$$ |____    ______    ______    ______  /$$$$$$  |  _______   ______   _______
$$ \__$$/ $$      \  /      \  /      \  /      \ $$ \__$$/  /       | /      \ /       \
$$      \ $$$$$$$  | $$$$$$  |/$$$$$$  |/$$$$$$  |$$      \ /$$$$$$$/  $$$$$$  |$$$$$$$  |
 $$$$$$  |$$ |  $$ | /    $$ |$$ |  $$/ $$ |  $$ | $$$$$$  |$$ |       /    $$ |$$ |  $$ |
/  \__$$ |$$ |  $$ |/$$$$$$$ |$$ |      $$ |__$$ |/  \__$$ |$$ \_____ /$$$$$$$ |$$ |  $$ |
$$    $$/ $$ |  $$ |$$    $$ |$$ |      $$    $$/ $$    $$/ $$       |$$    $$ |$$ |  $$ |
 $$$$$$/  $$/   $$/  $$$$$$$/ $$/       $$$$$$$/   $$$$$$/   $$$$$$$/  $$$$$$$/ $$/   $$/
                                        $$ |
                                        $$ |
                                        $$/

Delay:0   MaxConcurrency:600
Target segment must be specified using -s or --segment.
Usage: SharpScan [OPTIONS]
Perform network scans using different protocols.

Options:
  -i, --icmp                 Perform ICMP scan
  -a, --arp                  Perform ARP scan
  -s, --segment=VALUE        Target segment to scan
  -p, --ports=VALUE          Ports to scan (e.g. "0-1024" or "80,443,8080")
  -d, --delay=VALUE          Scan Delay
  -m, --maxconcurrency=VALUE Maximum number of concurrent scans,Defalt:600
  -h, --help                 Show this usage and help
  -o, --output=VALUE         Output file to save console output
```



默认扫描C段，使用所有模块

```powershell
SharpScan.exe -s 192.168.1.1/24
```

```powershell
Delay:0   MaxConcurrency:600

C_Segment: 192.168.244.8/24.
===================================================================
IP                           HostName                     OsVersion
192.168.244.1(ICMP)          LAPTOP-476JT8H0              Windows 11
192.168.244.169(ICMP)        DESKTOP-PESL5DR.local        Windows 11
192.168.244.142(ICMP)        NULL                         null
192.168.244.154(ICMP)        WIN-TNU2SVQRBP9              Windows 8.1 or Windows Server 2012 R2
192.168.244.171(ICMP)        owa                          Windows 7 SP1 or Windows Server 2008 R2 SP1
192.168.244.164(ICMP)        NULL                         null
===================================================================
[+] onlinePC: 6
===================================================================
192.168.244.1:445 (smb) is open
192.168.244.1:139 (netbios) is open
192.168.244.169:135 (findnet) is open
192.168.244.169:3389 (rdp) is open
192.168.244.154:445 (smb) is open
192.168.244.169:139 (netbios) is open
192.168.244.171:135 (findnet) is open
192.168.244.171:139 (netbios) is open
192.168.244.171:445 (smb) is open
192.168.244.164:22 (ssh) is open
192.168.244.169:445 (smb) is open
192.168.244.154:139 (netbios) is open
192.168.244.154:135 (findnet) is open
192.168.244.1:135 (findnet) is open

[+] Port Scanning completed in 1.92 seconds

192.168.244.171:80 (web) is open
192.168.244.171:88 (web) is open

[+] WebPort Scanning completed in 5.62 seconds

===================================================================
[+] alive ports len is: 16
===================================================================
[+] (MS17-010) Host: 192.168.244.171 have MS17-010!  User:owa   OS:Windows 7 SP1 or Windows Server 2008 R2 SP1
[!] (WebTitle) http://192.168.244.171:88 Request error: 基础连接已经关闭: 接收时发生错误。
[+] SMB logon Success: liukaifeng01:Lang123456789
[+] SMB logon Success: liukaifeng01:Lang123456789
[+] SMB logon Success: liukaifeng01:Lang123456789
[+] (WebTitle) http://192.168.244.171:80 HTTP Status Code: 200 (OK)
[+] (WebTitle) URL: http://192.168.244.171:80   Title: is: IIS7
[+] SMB logon Success: liukaifeng01:123456789
```

扫描指定IP，端口范围80-1024，0延时，最大并发600

```postgresql
SharpScan.exe -s 192.168.244.169 -p 80-1024 -d 0 -m 600
```

```powershell

Delay:0   MaxConcurrency:600
[+] 192.168.244.169:135 (findnet) is open
[+] 192.168.244.169:139 (netbios) is open
[+] 192.168.244.169:445 (smb) is open

[+] Scanning completed in 3.53 seconds
```

