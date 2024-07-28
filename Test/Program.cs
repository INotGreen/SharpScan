using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace CmsFingerprint
{
    class Program
    {
        static string targetIp;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CmsFingerprint <target IP>");
                return;
            }

            targetIp = args[0].TrimEnd('/');

            var cmsFingerprints = LoadCmsFingerprints("cms.json");

            ScanCMS(targetIp, cmsFingerprints);
        }

        static Dictionary<string, List<Fingerprint>> LoadCmsFingerprints(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<Dictionary<string, List<Fingerprint>>>(json);
        }

        static string FetchContent(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 10000; // 10 seconds
                request.KeepAlive = false; // Disable persistent connections

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Received HTTP {response.StatusCode} - {response.StatusDescription} for {url} using GET");
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)ex.Response)
                    {
                        Console.WriteLine($"Error: {errorResponse.StatusCode} - {errorResponse.StatusDescription} for {url} using GET");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {ex.Message} for {url} using GET");
                }
                return null;
            }
        }

        static void ScanCMS(string targetIp, Dictionary<string, List<Fingerprint>> fingerprints)
        {
            foreach (var cms in fingerprints)
            {
                foreach (var fingerprint in cms.Value)
                {
                    // 尝试http和https协议
                    string[] protocols = { "http://", "https://" };
                    bool identified = false;

                    foreach (var protocol in protocols)
                    {
                        string url = $"{protocol}{targetIp}{fingerprint.Path}";
                        string content = FetchContent(url);

                        if (content == null)
                        {
                            continue;
                        }

                        bool match = false;
                        if (fingerprint.Option == "md5")
                        {
                            match = VerifyMd5(content, fingerprint.Content);
                        }
                        else if (fingerprint.Option == "keyword")
                        {
                            match = content.Contains(fingerprint.Content);
                        }
                        else if (fingerprint.Option == "regex")
                        {
                            match = Regex.IsMatch(content, fingerprint.Content);
                        }

                        if (match)
                        {
                            Console.WriteLine($"CMS identified: {cms.Key} using {protocol}");
                            identified = true;
                            break;
                        }
                    }

                    if (identified)
                    {
                        break;
                    }
                }
                
            }

            Console.WriteLine("CMS could not be identified.");
        }

        static bool VerifyMd5(string content, string expectedMd5)
        {
            using (var md5 = MD5.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = md5.ComputeHash(contentBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return hash == expectedMd5;
            }
        }

        class Fingerprint
        {
            public string Path { get; set; }
            public string Option { get; set; }
            public string Content { get; set; }
        }
    }
}
