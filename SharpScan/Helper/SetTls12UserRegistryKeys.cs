using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Text;

namespace SharpScan
{
    internal class SetTls12UserRegistryKeys
    {

        public SetTls12UserRegistryKeys()
        {
            bool isTls12Enabled = false;
            try
            {
                // 检查 .NET Framework 的注册表键
                using (RegistryKey netFrameworkKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework", true))
                {
                    if (netFrameworkKey != null)
                    {
                        object schUseStrongCrypto = netFrameworkKey.GetValue("SchUseStrongCrypto");
                        object systemDefaultTlsVersions = netFrameworkKey.GetValue("SystemDefaultTlsVersions");

                        if (schUseStrongCrypto != null && systemDefaultTlsVersions != null)
                        {
                            isTls12Enabled = (int)schUseStrongCrypto == 1 && (int)systemDefaultTlsVersions == 1;
                        }
                        else
                        {
                            netFrameworkKey.SetValue("SchUseStrongCrypto", 1, RegistryValueKind.DWord);
                            netFrameworkKey.SetValue("SystemDefaultTlsVersions", 1, RegistryValueKind.DWord);
                            isTls12Enabled = true;
                           // Console.WriteLine("[+] TLS 1.2 registry keys for current user have been set successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Error checking or setting TLS 1.2 registry keys: " + ex.Message);
            }

            if (!isTls12Enabled)
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

                    //Console.WriteLine("[+] TLS 1.2 registry keys for current user have been set successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[!] Failed to set TLS 1.2 registry keys for current user: " + ex.Message);
                }
            }

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
        }
    }
}
