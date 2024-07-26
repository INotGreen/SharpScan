





## 1.介绍

- C#开发的内网资产扫描器，用于横向渗透的信息收集
- 参考了Ladon，Fscan等优秀的扫描器
- 为了兼容更古老的系统，所以采用.NET Framework3.5 和.NET Core6.0开发

## 2.特点

- 并发高、扫描速度快

- 用inline-assembly和execute-assembly进行内存加载

- 体积相对较小，传输方便


## 2. 主要功能

1.信息搜集:

- 存活探测(ICMP)
- 端口扫描(TCP)

2.爆破功能:

- 各类服务爆破(ssh、smb、rdp等)弱口令
- 数据库密码爆破(mysql、mssql、redis、psql、oracle等)

3.系统信息、漏洞扫描:

- netbios探测、域控识别
- 获取目标网卡信息
- 高危漏洞扫描(ms17010等)

4.Web探测功能:

- webtitle探测
- web指纹识别(常见cms、oa框架等)

6.其他功能:

- 文件保存

## 3.兼容性：

Windows ：支持win7-win11，windows server2008-2022

Linux：支持 glibc 2.17以上 的系统

MacOS： arm x64_x86，intel_x64_86

4.使用

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



