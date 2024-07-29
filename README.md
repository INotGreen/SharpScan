





## 1.特点

- C#开发的内网资产扫描器，用于内网横向移动和域内的信息收集
- 参考了Ladon，Fscan、Kscan等扫描器的原理
- 为了兼容更古老的系统，所以采用.NET Framework3.5 和.NET Core6.0开发
- 使用异步和高并发、扫描速度快并且可控、内存自动回收
- 用inline-assembly或者execute-assembly进行内存加载，实现无文件落地扫描

- 体积较小(目前500kb)、传输快、一键自动化扫描+信息收集，一条龙服务

- 尽最大可能地OPSEC，


## 2. 主要功能

- 存活探测(Icmp、Arp)
- 端口扫描(Tcp)
- 支持NetBios(默认137端口)、SMB(默认445端口)和WMI(默认135端口)服务快速探测
- 主机信息探测、目标网卡探测
- 高危漏洞扫描：ms17010、CVE-2020-0796(SMBGhost)、ZeroLogon（CVE-2020-1472）
- Webtitle探测，指纹识别常见CMS、OA框架等
- 各类服务弱口令爆破、账号密码枚举(SSH、SMB、RDP)，ssh命令执行
- 探测当前主机.net版本、操作系统版本信息、杀毒软件/内网设备（AV/EDR/XDR）查询等
- 导出本地RDP登录日志(rdp端口、mstsc缓存、cmdkey缓存、登录成功、失败日志)
- 判断是否在域内、定位域控IP、信息收集域控的FQDN、域管理员组、域企业管理员组、LDAP查询等
- 导出扫描结果

## 3.正在完成(TODO)

- 数据库密码爆破(mysql、mssql、redis、psql、oracle等)
- UDP端口扫描
- redis写公钥或写计划任务
- weblogic、st2、shiro的POC扫描检测

## 4.兼容性：

- Windows ：支持win7-win11，windows server2008-2022

- Linux：支持 glibc 2.17以上 的系统

- MacOS： arm x64_x86，intel_x64_86(macOS 10.15以上)

## 5.使用

```powershell
Delay:10   MaxConcurrency:600
Usage: SharpScan [OPTIONS]

Options:
  -i, --icmp                 Perform icmp scan
  -a, --arp                  Perform arp scan
  -U, --udp                  Perform udp scan
  -t, --Target=VALUE         Target segment to scan
  -p, --ports=VALUE          Ports to scan (e.g. "0-1024" or "80,443,8080")
  -d, --delay=VALUE          Scan delay(ms),Defalt:1000
  -m, --maxconcurrency=VALUE Maximum number of concurrent scans,Defalt:600
  -u, --username=VALUE       Username for authentication
      --pw, --password=VALUE Password for authentication
  -h, --help                 Show this usage and help
  -o, --output=VALUE         Output file to save console output

Example:
  SharpScan.exe -t 192.168.1.1/24
  SharpScan.exe -t 192.168.1.107 -p 100-1024
```



默认扫描C段，使用所有模块

```powershell
SharpScan.exe -s 192.168.1.1/24  (扫描C段)
SharpScan.exe -s 192.168.1.1/16  (扫描B段)
```

[demo](https://private-user-images.githubusercontent.com/89376703/352985272-6c4d2f2d-b21e-43b3-ad8b-578cd6163f05.mp4)



扫描指定IP(默认使用TCP)，端口范围80-1024，0延时，最大并发600，用时3秒

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

使用UDP协议扫描端口，端口范围100-10000，10ms延时，最大并发600，用时21秒

```
SharpScan.exe -t 192.168.244.141 -U -p 100-10000
```

```powershell
Delay:10   MaxConcurrency:600
[+] TLS 1.2 registry keys for current user have been set successfully.
[+] 192.168.244.141:135 (msrpc) is open
[+] 192.168.244.141:139 (netbios) is open
[+] 192.168.244.141:389 (ldap) is open
[+] 192.168.244.141:445 (smb) is open
[+] 192.168.244.141:464 (kpasswd) is open
[+] 192.168.244.141:636 (ldaps) is open
[+] 192.168.244.141:593 is open
[+] 192.168.244.141:3268 is open
[+] 192.168.244.141:3269 is open
[+] 192.168.244.141:9389 is open

[+] Scanning completed in 21.07 seconds
```

