rem Crash with rightmost CTRL key, and press the SCROLL LOCK key twice.
rem https://msdn.microsoft.com/en-us/library/windows/hardware/ff545499%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
rem https://msdn.microsoft.com/en-us/library/windows/hardware/ff542953%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
reg add HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\i8042prt\Parameters /v CrashOnCtrlScroll /t REG_DWORD  /d 0x01
reg add HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\kbdhid\Parameters /v CrashOnCtrlScroll /t REG_DWORD /d 0x01
