<#
.SYNOPSIS
    This script reverse the mouse wheel scrolling direction

.DESCRIPTION
    Open PowerShell in administrator mode.

    Then run script.

    0 - Default Mode, Windows behavior
    1 - Natural Mode, Mac behavior, reverse mode

    Restart your computer. Your settings will take effect after you restart.

.NOTES
    Author: Jason Go
    Source: https://answers.microsoft.com/en-us/windows/forum/all/reverse-mouse-wheel-scroll/657c4537-f346-4b8b-99f8-9e1f52cd94c2
#>
$mode = Read-host "How do you like your mouse scroll (0 or 1)?"; Get-PnpDevice -Class Mouse -PresentOnly -Status OK | ForEach-Object { "$($_.Name): $($_.DeviceID)"; Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Enum\$($_.DeviceID)\Device Parameters" -Name FlipFlopWheel -Value $mode; "+--- Value of FlipFlopWheel is set to " + (Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Enum\$($_.DeviceID)\Device Parameters").FlipFlopWheel + "`n" }
