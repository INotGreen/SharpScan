using Microsoft.Win32;
using SharpRDPCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;

namespace SharpScan
{
    internal class RdpBroute
    {
        public RdpBroute(string IP)
        {
            if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
            {
                try
                {
                    int Port = 3389;
                    if (!Helper.TestPort(IP, Port))
                    {
                        return;
                        // return $"{ip},445,Port unreachable";
                    }
                    Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is open");

                    if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
                    {
                        
                        Options.Host = IP;
                        Options.Port = Port;
                        Options.Username = Program.userName;
                        Options.Password = Program.passWord;
                        Network.Connect(Options.Host, Port);
                        MCS.sendConnectionRequest(null, true);
                    }
                    if (Program.userList != null && Program.passwordList != null)
                    {
                        foreach (var user in Program.userList)
                        {
                            foreach (var pass in Program.passwordList)
                            {
                                Options.Host = IP;
                                Options.Port = Port;
                                Options.Username = user;
                                Options.Password = pass;
                                Network.Connect(Options.Host, Port);
                                MCS.sendConnectionRequest(null, true);
                            }
                        }
                    }


                    else
                    {
                        if (Configuration.UserDictionary.TryGetValue("rdp", out List<string> rdpUsers))
                        {
                           // Console.WriteLine("RDP服务的用户名列表：");
                            foreach (var user in rdpUsers)
                            {
                                foreach(var pass in Configuration.Passwords)
                                {
                                    Options.Host = IP;
                                    Options.Port = Port;
                                    Options.Username = user;
                                    Options.Password = pass;
                                    Network.Connect(Options.Host, Port);
                                    MCS.sendConnectionRequest(null, true);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Username list for RDP service not found。");
                        }

                    }


                }
                catch (Exception exception)
                {
                    //Console.WriteLine("[!] " + exception.Message);
                    //Console.WriteLine("InnerException: " + exception.InnerException);
                }
            }


        }
    }
}
