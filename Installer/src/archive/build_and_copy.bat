@echo off
echo Building installer...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "EK24_Installer.iss"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful! Copying to backup location...
    if not exist "T:\50_DESIGN DATA\Software" mkdir "T:\50_DESIGN DATA\Software"
    copy "EK24_Installer_14.0.0.exe" "T:\50_DESIGN DATA\Software\" /Y
    echo.
    echo Done! Installer created in current directory and copied to T:\50_DESIGN DATA\Software
) else (
    echo.
    echo Build failed!
)
pause
