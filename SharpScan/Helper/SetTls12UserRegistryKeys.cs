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
            const string registryPath = @"SOFTWARE\Microsoft\.NETFramework";
            const string schUseStrongCryptoValue = "SchUseStrongCrypto";
            const string systemDefaultTlsVersionsValue = "SystemDefaultTlsVersions";
            const int enabledValue = 1;

            try
            {
                using (RegistryKey netFrameworkKey = Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (netFrameworkKey != null)
                    {
                        object schUseStrongCrypto = netFrameworkKey.GetValue(schUseStrongCryptoValue);
                        object systemDefaultTlsVersions = netFrameworkKey.GetValue(systemDefaultTlsVersionsValue);

                        if (schUseStrongCrypto != null && systemDefaultTlsVersions != null)
                        {
                            isTls12Enabled = (int)schUseStrongCrypto == enabledValue && (int)systemDefaultTlsVersions == enabledValue;
                        }

                        if (!isTls12Enabled)
                        {
                            netFrameworkKey.SetValue(schUseStrongCryptoValue, enabledValue, RegistryValueKind.DWord);
                            netFrameworkKey.SetValue(systemDefaultTlsVersionsValue, enabledValue, RegistryValueKind.DWord);
                            isTls12Enabled = true;
                            Console.WriteLine("[+] TLS 1.2 registry keys for current user have been set successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Error checking or setting TLS 1.2 registry keys: " + ex.Message);
            }

            if (isTls12Enabled)
            {
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
}
