using Microsoft.VisualBasic.ApplicationServices;
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
                    using (var client = new TcpClient())
                    {
                        var result = client.BeginConnect(IP, Port, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1.5));
                        if (success)
                        {
                            
                            
                            Console.WriteLine($"[*] {IP}:{Port}{Helper.GetServiceByPort(Port)} is open");
                            
                            Options.Host = IP;
                            Options.Port = Port;
                            Options.Username = Program.userName;
                            Options.Password = Program.passWord;
                            Network.Connect(Options.Host, Port);
                            MCS.sendConnectionRequest(null, true);
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
