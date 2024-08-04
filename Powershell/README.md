



```powershell

$base64String=""
# 打印出压缩后的Base64字符串（你可以将其复制并粘贴到你的Invoke-SharpScan脚本中）
$compressedBase64String
function Convert-ExeToCompressedBase64 {
    param (
        [string]$exePath
    )

    # 读取exe文件为字节数组
    $bytes = [System.IO.File]::ReadAllBytes($exePath)

    # 压缩字节数组
    $compressedStream = New-Object System.IO.MemoryStream
    $gzipStream = New-Object System.IO.Compression.GzipStream($compressedStream, [System.IO.Compression.CompressionMode]::Compress)
    $gzipStream.Write($bytes, 0, $bytes.Length)
    $gzipStream.Close()
    $compressedBytes = $compressedStream.ToArray()

    # 转换为Base64字符串
    $base64String = [Convert]::ToBase64String($compressedBytes)
    return $base64String
}

# 示例：将exe文件路径替换为实际路径


function Invoke-SharpScan {
    [CmdletBinding()]
    Param (
        
        [String]
        $Command = ""
    )

    # Base64 压缩的exe文件内容（请在这里填入转换后的Base64字符串）



    $a = New-Object IO.MemoryStream(, [Convert]::FromBase64String($base64String))
    $decompressed = New-Object IO.Compression.GzipStream($a, [IO.Compression.CompressionMode]::Decompress)
    $output = New-Object System.IO.MemoryStream
    $decompressed.CopyTo($output)
    [byte[]]$byteOutArray = $output.ToArray()

    $assembly = [System.Reflection.Assembly]::Load($byteOutArray)

    $source = @"
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

public class CustomTextWriter : TextWriter
{
    private TextWriter _originalOut;

    public CustomTextWriter(TextWriter originalOut)
    {
        _originalOut = originalOut;
    }

    public override Encoding Encoding
    {
        get { return _originalOut.Encoding; }
    }

    public override void Write(char value)
    {
        _originalOut.Write(value);
        Console.Out.Flush();
        WriteHost(value.ToString());
    }

    public override void WriteLine(string value)
    {
        _originalOut.WriteLine(value);
        Console.Out.Flush();
        WriteHost(value + Environment.NewLine);
    }

    private void WriteHost(string value)
    {
        using (PowerShell ps = PowerShell.Create())
        {
            ps.AddScript("Write-Host '" + value.Replace("'", "''") + "'");
            ps.Invoke();
        }
    }
}
"@

    Add-Type -TypeDefinition $source -Language CSharp

    $originalOut = [Console]::Out
    $customOut = New-Object CustomTextWriter $originalOut
    [Console]::SetOut($customOut)
    [SharpRDPCheck.Program]::Main($Command.Split(" "))
    [Console]::SetOut($originalOut)
}
$exePath = "SharpRDPCheck.exe"
$compressedBase64String = Convert-ExeToCompressedBase64 -exePath $exePath

# 打印出压缩后的Base64字符串（你可以将其复制并粘贴到你的Invoke-SharpScan脚本中）
$base64String=$compressedBase64String


```

