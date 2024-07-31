using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
