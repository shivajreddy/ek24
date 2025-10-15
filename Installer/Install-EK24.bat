@echo off
REM EK24 Installer Launcher

setlocal

set "version=14.0.0"
set "powershell_path=%~dp0src\Install-EK24.ps1"
set "dist_path=%~dp0src\dist"


REM Check if PowerShell script exists
if not exist "%powershell_path%" (
    echo ERROR: Install-EK24.ps1 not found in src folder!
    echo Expected location: %powershell_path%
    echo.
    pause
    exit /b 1
)

REM Check if dist folder exists
if not exist "%dist_path%" (
    echo ERROR: dist folder not found in src folder!
    echo Expected location: %dist_path%
    echo.
    pause
    exit /b 1
)

REM Run PowerShell script with execution policy bypass

powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%powershell_path%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Installation failed with error code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

exit /b 0
