@echo off
rem Double-click helper: runs build.ps1 without changing the PowerShell execution policy
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
pause
