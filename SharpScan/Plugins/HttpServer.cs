﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class SimpleTcpHttpServer
{
    private readonly TcpListener _listener;
    private readonly string _baseFolder;

    public SimpleTcpHttpServer(string ipAddress, int port, string baseFolder)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _baseFolder = baseFolder;
        _listener.Start();
        Console.WriteLine($"Listening on {ipAddress}:{port}...");
    }

    public void Run()
    {
        while (true)
        {
            var client = _listener.AcceptTcpClient();
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            var requestLine = reader.ReadLine();
            if (requestLine != null)
            {
                var tokens = requestLine.Split(' ');
                if (tokens.Length >= 2 && tokens[0] == "GET")
                {
                    var url = tokens[1].TrimStart('/');
                    var filePath = Path.Combine(_baseFolder, url);

                    if (Directory.Exists(filePath))
                    {
                        ServeDirectoryListing(filePath, writer);
                    }
                    else if (File.Exists(filePath))
                    {
                        ServeFile(filePath, writer, stream);
                    }
                    else
                    {
                        writer.WriteLine("HTTP/1.1 404 Not Found");
                        writer.WriteLine("Connection: close");
                        writer.WriteLine();
                    }
                }
                else
                {
                    writer.WriteLine("HTTP/1.1 400 Bad Request");
                    writer.WriteLine("Connection: close");
                    writer.WriteLine();
                }
            }
        }
        client.Close();
    }

    private void ServeDirectoryListing(string path, StreamWriter writer)
    {
        var files = Directory.GetFiles(path);
        var directories = Directory.GetDirectories(path);

        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head><title>Directory listing</title></head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h2>Directory listing for {path}</h2>");
        sb.AppendLine("<ul>");
        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);
            sb.AppendLine($"<li><a href=\"{Uri.EscapeDataString(dirName)}/\">{dirName}/</a></li>");
        }
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            sb.AppendLine($"<li><a href=\"{Uri.EscapeDataString(fileName)}\">{fileName}</a></li>");
        }
        sb.AppendLine("</ul>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        var response = sb.ToString();
        writer.WriteLine("HTTP/1.1 200 OK");
        writer.WriteLine("Content-Type: text/html; charset=UTF-8");
        writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(response)}");
        writer.WriteLine("Connection: close");
        writer.WriteLine();
        writer.Write(response);
    }

    private void ServeFile(string path, StreamWriter writer, NetworkStream stream)
    {
        try
        {
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Content-Type: application/octet-stream");
            writer.WriteLine($"Content-Length: {new FileInfo(path).Length}");
            writer.WriteLine("Connection: close");
            writer.WriteLine();
            writer.Flush(); // 确保头信息被发送

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
        }
    }
}
