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


        static void Run(string IP, string User, string Password)
        {
            SetTls12UserRegistryKeys();

            // 使用反射设置 TLS 1.2
            const SslProtocols Tls12 = (SslProtocols)3072;
            Type t = typeof(ServicePointManager);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = t.GetField("s_SecurityProtocolType", flags);
            if (field != null)
            {
                field.SetValue(null, Tls12);
            }
            else
            {
                PropertyInfo property = t.GetProperty("SecurityProtocol", flags);
                if (property != null)
                {
                    property.SetValue(null, Tls12, null);
                }
            }

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


        static void SetTls12UserRegistryKeys()
        {
            try
            {
                // 设置当前用户的 .NET Framework 的注册表键
                using (RegistryKey netFrameworkKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework", true))
                {
                    if (netFrameworkKey != null)
                    {
                        netFrameworkKey.SetValue("SchUseStrongCrypto", 1, RegistryValueKind.DWord);
                        netFrameworkKey.SetValue("SystemDefaultTlsVersions", 1, RegistryValueKind.DWord);
                    }
                }

                //Console.WriteLine("TLS 1.2 registry keys for current user have been set successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set TLS 1.2 registry keys for current user: " + ex.Message);
            }
        }
    }
}
