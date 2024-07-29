using Microsoft.Win32;
using SharpRDPCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Text;

namespace SharpScan
{
    internal class RdpBroute
    {
        public RdpBroute(string IP, string User, string Password)
        {
            try
            {
                Options.Host = IP;
                Options.Port = 3389;
                Options.Username = User;
                Options.Password = Password;
                Network.Connect(Options.Host, 3389);
                MCS.sendConnectionRequest(null, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine("[!] " + exception.Message);
                Console.WriteLine("InnerException: " + exception.InnerException);
            }

        }
    }
}
