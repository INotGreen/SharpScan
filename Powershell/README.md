

用这段powershell代码将C#程序转成Powershell脚本

```powershell
$temp = @'
function Invoke-SharpScan {
    [CmdletBinding()]
    Param (
        [String]
        $Command = ""
    )

    # Base64 压缩的exe文件内容（请在这里填入转换后的Base64字符串）
    $base64String = "ReplaceBase64"

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
    #[Console]::SetOut($customOut)
    [SharpScan.Program]::Main($Command.Split(" "))
   # [Console]::SetOut($originalOut)
}

#Invoke-SharpScan -Command "-h 192.168.244.1/24"

'@



$compressedBase64String
function Convert-ExeToCompressedBase64 {
    param (
        [string]$exePath
    )
    $bytes = [System.IO.File]::ReadAllBytes($exePath)
    $compressedStream = New-Object System.IO.MemoryStream
    $gzipStream = New-Object System.IO.Compression.GzipStream($compressedStream, [System.IO.Compression.CompressionMode]::Compress)
    $gzipStream.Write($bytes, 0, $bytes.Length)
    $gzipStream.Close()
    $compressedBytes = $compressedStream.ToArray()
    $base64String = [Convert]::ToBase64String($compressedBytes)
    return $base64String
}

$base64String = ""
$exePath = "SharpScan.exe"
$compressedBase64String = Convert-ExeToCompressedBase64 -exePath $exePath
$base64String = $compressedBase64String

$stub1 = $temp.Replace("ReplaceBase64", $base64String) >Invoke-SharpScan.ps1

```



