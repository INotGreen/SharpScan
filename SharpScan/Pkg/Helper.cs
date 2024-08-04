using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SharpScan
{
    public class Helper
    {
        public static string GetServiceByPort(int port)
        {
            var service = Configuration.PortList.FirstOrDefault(p => p.Value == port).Key;
            if (service != null)
            {
                return $" ({service})";
            }
            return "";


        }

       public static bool TestPort(string remoteHost, int remotePort)
        {
            int timeout = 3000; // 3 seconds
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult result = client.BeginConnect(remoteHost, remotePort, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeout, false);
                    if (!success)
                    {
                        //client.Close();
                        return false;
                    }
                    client.EndConnect(result);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
