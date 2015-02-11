@echo off

rem Script path.
rem echo %~dp0
rem Script file name and extension.
rem echo %~nx0

reg add "HKCU\Software\Microsoft\Command Processor" /v Autorun /t REG_SZ /d "%~dp0notepadAlias.cmd" /f
