using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;

namespace SharpScan.Plugins
{
    internal class WmiExec
    {
        public static ManagementScope scope;

        public static void Run(string host)
        {
            string funcName = Program.funcName;
            string command = Program.command;
            string localFile = Program.localFile;
            string remoteFile = Program.remoteFile;
           // int Port = 137;
            //if (!Helper.TestPort(host, Port))
            //{
            //    return;
            //}
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(funcName))
            {
                return;
            }

            try
            {
                Console.WriteLine($"[+] Connecting to host: {host}");
                ConnectionOptions options = new ConnectionOptions();
                int delay = 5000;
                scope = new ManagementScope($"\\\\{host}\\root\\cimv2", options)
                {
                    Options = { Impersonation = ImpersonationLevel.Impersonate, EnablePrivileges = true }
                };
                scope.Connect();
                Console.WriteLine("[+] Connection established.");

                if (funcName == "cmd")
                {
                    if (string.IsNullOrEmpty(command))
                    {
                        Console.WriteLine("Command cannot be empty for 'cmd' function.");
                        return;
                    }

                    Console.WriteLine($"[+] Executing command: {command}");
                    string powershellCommand = "powershell -enc " + Base64Encode(command);
                    string code = "$a=(" + powershellCommand + ");$b=[Convert]::ToBase64String([System.Text.UnicodeEncoding]::Unicode.GetBytes($a));$reg = Get-WmiObject -List -Namespace root\\default | Where-Object {$_.Name -eq \"StdRegProv\"};$reg.SetStringValue(2147483650,\"\",\"txt\",$b)";

                    ExecCmd("powershell -enc " + Base64Encode(code));
                    Console.WriteLine("[+] Exec done!");
                    Thread.Sleep(delay);

                    Console.WriteLine("[+] Retrieving command output from registry.");
                    var registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);
                    var inParams = registry.GetMethodParameters("GetStringValue");
                    inParams["sSubKeyName"] = "";
                    inParams["sValueName"] = "txt";
                    var outParams = registry.InvokeMethod("GetStringValue", inParams, null);

                    Console.WriteLine("[+] Output -> \n\n" + Base64Decode(outParams["sValue"].ToString()));
                }
                else if (funcName == "upload")
                {
                    if (string.IsNullOrEmpty(localFile) || string.IsNullOrEmpty(remoteFile))
                    {
                        Console.WriteLine("Local file and remote file paths cannot be empty for 'upload' function.");
                        return;
                    }

                   // Console.WriteLine($"[+] Reading local file: {localFile}");
                    byte[] fileBytes = File.ReadAllBytes(localFile);
                    string base64File = Convert.ToBase64String(fileBytes);

                    //Console.WriteLine("[+] Writing file data to registry.");
                    var registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);
                    var inParams = registry.GetMethodParameters("SetStringValue");
                    inParams["hDefKey"] = 2147483650; // HKEY_LOCAL_MACHINE
                    inParams["sSubKeyName"] = "";
                    inParams["sValueName"] = "upload";
                    inParams["sValue"] = base64File;
                    registry.InvokeMethod("SetStringValue", inParams, null);

                    string pscode = $"$wmi = [wmiclass]\"Root\\default:stdRegProv\";$data=($wmi.GetStringValue(2147483650,\"\",\"upload\")).sValue;$byteArray = [Convert]::FromBase64String($data);[io.file]::WriteAllBytes(\"{remoteFile}\",$byteArray);";
                    string powershellCommand = "powershell -enc " + Base64Encode(pscode);
                    Thread.Sleep(delay);
                    ExecCmd(powershellCommand);
                    Console.WriteLine("[+] Upload file done!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static int ExecCmd(string cmd)
        {
           // Console.WriteLine($"[+] Running command: {cmd}");
            using (var managementClass = new ManagementClass(scope, new ManagementPath("Win32_Process"), new ObjectGetOptions()))
            {
                var inputParams = managementClass.GetMethodParameters("Create");
                inputParams["CommandLine"] = cmd;

                var outParams = managementClass.InvokeMethod("Create", inputParams, null);
                int returnValue = Convert.ToInt32(outParams["returnValue"]);
                Console.WriteLine($"[+] Command executed with return value: {returnValue}");
                return returnValue;
            }
        }

        public static string Base64Encode(string content)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string content)
        {
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.Unicode.GetString(bytes);
        }
    }
}
