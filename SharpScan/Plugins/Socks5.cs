using System;
using System.Net;
using System.Threading;
using Socks5.Core.Plugin;
using Socks5.Core.Socks;
using Socks5.Core.SocksServer;
namespace SharpScan
{
    public class Socks5
    {
        public void Run(int Port,string userName,string passWord)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
            {
                var socks5Server = new Socks5Server(IPAddress.Any, Port);
                socks5Server.Start();
                PluginLoader.ChangePluginStatus(true, typeof(Auth));
                Console.WriteLine($"[+] Start socks5 port:{Port} , User:{Program.userName}  Password:{Program.passWord}");
                while (true) { }
                //while (true)
                //{
                //    Console.Write("Total Clients: \t{0}\nTotal Recvd: \t{1:0.00##}MB\nTotal Sent: \t{2:0.00##}MB\n", socks5Server.Stats.TotalClients, ((socks5Server.Stats.NetworkReceived / 1024f) / 1024f), ((socks5Server.Stats.NetworkSent / 1024f) / 1024f));
                //    Console.Write("Receiving/sec: \t{0}\nSending/sec: \t{1}", socks5Server.Stats.ReceivedBytesPerSecond(), socks5Server.Stats.SentBytesPerSecond());
                //    Thread.Sleep(3000);
                //    Console.Clear();
                //}
            }
            else
            {
                Console.WriteLine("[!] Please enter your account password");
            }
            
        }
    }

    class Auth : LoginHandler
    {
        public override bool OnStart() => true;

        private bool _enabled = false;
        public override bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public override LoginStatus HandleLogin(User user)
        {
            if (user != null && user.Username == Program.userName && user.Password == Program.passWord)
            {
                return LoginStatus.Correct;
            }
            else
            {
                return LoginStatus.Denied;
            }
        }
    }
}

