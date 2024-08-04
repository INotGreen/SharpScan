function Invoke-SharpScan {
    [CmdletBinding()]
    Param (
        [String]
        $Command = ""
    )

    # Base64 压缩的exe文件内容（请在这里填入转换后的Base64字符串）
    $base64String = ""

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
    [SharpScan.Program]::Main($Command.Split(" "))
    [Console]::SetOut($originalOut)
}



