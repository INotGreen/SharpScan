using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace SharpRDPCheck
{
    internal class Options
    {
        internal static int SocketTimeout = 3000;  
        internal static string ClientName = "Windows7"; // Client Name
        internal static string Domain = ""; // Domain
        internal static string DomainAndUsername = ""; // Domain and Username
        internal static string Host = ""; // Host
        internal static string hostname = "";
        internal static string Username = ""; // Username
        internal static string Password = ""; // Password
        internal static string hash = ""; // NTLM hash
        internal static int Port = 3389; // Port
        internal static bool enableNLA = true; // Enable NLA
        internal static MCS.NegotiationFlags serverNegotiateFlags;
    }
}