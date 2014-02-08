
function PrintHresult($constantName, $value, $message)
{
    $message = $message -replace """", """"""
    #Write-Output "$constantName, $value, ""$message"""
    Write-Output "    printf(""$constantName, %#08X\r\n"", $value);"
}

function ParseWinerrorLine($line)
{
    if ($line -match "^//\s+(.+)$")
    {
        $script:messageText = $matches[1]
    }
    elseif ($line -match "#define\s+([A-Z0-9_]+)\s+([0-9]+)L")
    {
        PrintHresult $matches[1] $matches[2] $script:messageText
        $script:messageText = $null
    }
    elseif ($line -match "#define\s+([A-Z0-9_]+)\s+([0-9]+)$")
    {
        PrintHresult $matches[1] $matches[2] $script:messageText
        $script:messageText = $null
    }
    elseif ($line -match "#define\s+([A-Z0-9_]+)\s+([x0-9A-F]+)$")
    {
        PrintHresult $matches[1] $matches[2] $script:messageText
        $script:messageText = $null
    }
    elseif ($line -match "#define\s+([A-Z0-9_]+)\s+([A-Z0-9_]+)$")
    {
        PrintHresult $matches[1] $matches[2] $script:messageText
        $script:messageText = $null
    }
    elseif ($line -match "#define\s+([A-Z0-9_]+)\s+_HRESULT_TYPEDEF_\(([A-Z0-9_]+)\)$")
    {
        #Write-Host "$($matches[1]), $($matches[2]), $script:messageText" -BackgroundColor DarkGreen
        PrintHresult $matches[1] $matches[2] $script:messageText
        $script:messageText = $null
    }
    elseif ($line -match "#define\s+")
    {
        # This defines are skipped.
        Write-Host "$line" -ForegroundColor red
    }
}

function ParseWinerrorFile($fileName)
{
    $reader = [System.IO.File]::OpenText($fileName)
    try
    {
        for(;;)
        {
            $line = $reader.ReadLine()
            if ($line -eq $null)
            {
                break
            }
            
            # process the line
            ParseWinerrorLine $line
        }
    }
    finally {
        $reader.Close()
    }
}

$messageText = $null
$fileName = 'C:\Program Files (x86)\Windows Kits\8.1\Include\shared\winerror.h';
ParseWinerrorFile($fileName)
