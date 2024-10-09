@echo off
setlocal
set "reg_path=HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\HID"

echo Setting natural scrolling for all connected mice...

REM Use reg query to find all connected HID devices
for /f "tokens=*" %%A in ('reg query "%reg_path%" /s /f "FlipFlopWheel" 2^>nul') do (
    REM Check if FlipFlopWheel exists
    for /f "tokens=2*" %%B in ('reg query "%%A" /v FlipFlopWheel 2^>nul') do (
        REM Read current value
        set "current_value=%%C"
        REM Set to MacBook-like natural scrolling: FlipFlopWheel = 1
        if not "%%C"=="0x1" (
            echo Setting FlipFlopWheel to 1 for %%A
            reg add "%%A" /v FlipFlopWheel /t REG_DWORD /d 1 /f
        )
    )
)

echo Natural scrolling has been enabled for all connected mice.
pause
