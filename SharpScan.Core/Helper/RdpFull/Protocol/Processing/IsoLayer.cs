using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpRDPCheck
{
    internal class IsoLayer
    {
        internal static void Write(RdpPacket data)
        {
            data.Position = 0L;
            byte[] buffer = new byte[data.Length];
            data.Read(buffer, 0, (int)data.Length);

            Network.Send(buffer);
        }
        }
        
}