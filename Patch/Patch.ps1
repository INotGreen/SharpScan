
$filePath = "SharpScan.exe"
$outputFilePath = "SharpScan_Patched.exe"
$findBytes = [System.Text.Encoding]::ASCII.GetBytes("v2.0.50727")
$replaceBytes = [System.Text.Encoding]::ASCII.GetBytes("v4.0.30319")
$content = [System.IO.File]::ReadAllBytes($filePath)
for ($i = 0; $i -le $content.Length - $findBytes.Length; $i++) {
    $match = $true
    for ($j = 0; $j -lt $findBytes.Length; $j++) {
        if ($content[$i + $j] -ne $findBytes[$j]) {
            $match = $false
            break
        }
    }
    if ($match) {
        [Array]::Copy($replaceBytes, 0, $content, $i, $replaceBytes.Length)
    }
}

[System.IO.File]::WriteAllBytes($outputFilePath, $content)

Write-Host "new exe:$outputFilePath"
