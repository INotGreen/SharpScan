using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SharpScan
{
    public class WebTitle
    {
        public static void Run(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(IgnoreCertificateValidation);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = true; // 允许自动重定向
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine($"[+] (WebTitle) {url} HTTP Status Code: {(int)response.StatusCode} ({response.StatusCode})");

                        // 获取响应流编码
                        Encoding encoding = GetEncodingFromResponse(response);

                        using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                        {
                            string responseBody = reader.ReadToEnd();
                            string title = ExtractTitle(responseBody);

                            if (title != null)
                            {
                                Console.WriteLine($"[+] (WebTitle) URL: {url} Title: {title}");
                            }
                            else
                            {
                                Console.WriteLine($"[!] (WebTitle) No title found for the webpage at {url}");
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var errorResponse = (HttpWebResponse)e.Response;
                    Console.WriteLine($"[!] (WebTitle) {url} HTTP Status Code: {(int)errorResponse.StatusCode} ({errorResponse.StatusCode})");
                }
                else
                {
                    Console.WriteLine($"[!] (WebTitle) {url} Request error: {e.Message}");
                }
            }
        }

        private static Encoding GetEncodingFromResponse(HttpWebResponse response)
        {
            string charset = response.CharacterSet;
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    return Encoding.GetEncoding(charset);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"[!] (WebTitle) Unknown encoding: {charset}. Defaulting to UTF-8.");
                }
            }
            return Encoding.UTF8;
        }

        public static string BuildUrl(string ip, string port)
        {
            if (int.TryParse(port, out int portNumber))
            {
                if (portNumber == 443)
                {
                    return $"https://{ip}:{port}";
                }
                else
                {
                    return $"http://{ip}:{port}";
                }
            }
            return $"http://{ip}:{port}";
        }

        static bool IgnoreCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // 忽略证书验证
        }

        static string ExtractTitle(string html)
        {
            Match match = Regex.Match(html, @"<title>\s*(.+?)\s*</title>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
    }
}
