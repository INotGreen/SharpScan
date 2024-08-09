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
        public static void Run(int Port,string userName,string passWord)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
            {
                var socks5Server = new Socks5Server(IPAddress.Parse("0.0.0.0"), Port);
                socks5Server.Start();
                PluginLoader.ChangePluginStatus(true, typeof(Auth));
                Console.WriteLine($"[+] Start socks5 port:{Port} , User:{Program.userName}  Password:{Program.passWord}");
                while (true) {  }
            }
            else
            {
                Console.WriteLine("[!] Please enter your: Account or password");
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

