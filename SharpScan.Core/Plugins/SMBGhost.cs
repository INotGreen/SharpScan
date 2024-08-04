using System;
using System.Net.Sockets;
using System.Text;

namespace SharpScan
{
    internal class SmbGhost
    {
        const string pkt = "\x00" + // session
            "\x00\x00\xc0" + // length

            "\xfeSMB@\x00" + // protocol

            // [MS-SMB2]: SMB2 NEGOTIATE Request
            "\x00\x00" +
            "\x00\x00" +
            "\x00\x00" +
            "\x00\x00" +
            "\x1f\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +

            // [MS-SMB2]: SMB2 NEGOTIATE_CONTEXT
            "$\x00" +
            "\x08\x00" +
            "\x01\x00" +
            "\x00\x00" +
            "\x7f\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "x\x00" +
            "\x00\x00" +
            "\x02\x00" +
            "\x00\x00" +
            "\x02\x02" +
            "\x10\x02" +
            "\x22\x02" +
            "$\x02" +
            "\x00\x03" +
            "\x02\x03" +
            "\x10\x03" +
            "\x11\x03" +
            "\x00\x00\x00\x00" +

            // [MS-SMB2]: SMB2_PREAUTH_INTEGRITY_CAPABILITIES
            "\x01\x00" +
            "&\x00" +
            "\x00\x00\x00\x00" +
            "\x01\x00" +
            "\x20\x00" +
            "\x01\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00\x00\x00" +
            "\x00\x00" +

            // [MS-SMB2]: SMB2_COMPRESSION_CAPABILITIES
            "\x03\x00" +
            "\x0e\x00" +
            "\x00\x00\x00\x00" +
            "\x01\x00" + // CompressionAlgorithmCount
            "\x00\x00" +
            "\x01\x00\x00\x00" +
            "\x01\x00" + // LZNT1
            "\x00\x00" +
            "\x00\x00\x00\x00";

        public  void Run(string IP)
        {
            try
            {
                SmbGhostScan(IP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");
            }
        }

        private static void SmbGhostScan(string IP)
        {
            int port = 445;
            int timeout = 5000; // Timeout in milliseconds
            string address = $"{IP}:{port}";
            byte[] packet = Encoding.ASCII.GetBytes(pkt);

            using (TcpClient client = new TcpClient())
            {
                try
                {
                    IAsyncResult result = client.BeginConnect(IP, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

                    if (!success)
                    {
                        throw new TimeoutException("Connection timed out.");
                    }

                    NetworkStream stream = client.GetStream();
                    stream.Write(packet, 0, packet.Length);
                    stream.ReadTimeout = timeout;

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0 && Encoding.ASCII.GetString(buffer).Contains("Public") &&
                        buffer.Length >= 76 && buffer[72] == 0x11 && buffer[73] == 0x03 &&
                        buffer[74] == 0x02 && buffer[75] == 0x00)
                    {
                        string resultMsg = $"[+] {IP} CVE-2020-0796 SmbGhost Vulnerable";
                        LogSuccess(resultMsg);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[!] Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
            }
        }

        private static void LogSuccess(string message)
        {
            // Implement your logging mechanism here
            Console.WriteLine(message);
        }
    }
}
