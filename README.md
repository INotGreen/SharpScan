





## 1.介绍

C#开发的内网资产扫描器，为了兼容更古老的系统，所以采用.NET Framework3.5 和.NET Core6.0开发



## 2. 主要功能

1.信息搜集:

- 存活探测(icmp)
- 端口扫描

2.爆破功能:

- 各类服务爆破(ssh、smb、rdp等)
- 数据库密码爆破(mysql、mssql、redis、psql、oracle等)

3.系统信息、漏洞扫描:

- netbios探测、域控识别
- 获取目标网卡信息
- 高危漏洞扫描(ms17010等)

4.Web探测功能:

- webtitle探测
- web指纹识别(常见cms、oa框架等)
- web漏洞扫描(weblogic、st2等,支持xray的poc)

5.漏洞验证:

- ssh命令执行，爆破弱口令

6.其他功能:

- 文件保存

## 3.兼容性：

Windows ：支持win7-win11，windows server2008-2022

Linux：支持 glibc 2.17以上 

MacOS： arm x64_x86



