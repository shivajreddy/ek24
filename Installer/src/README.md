
# WORKFLOW

- The Visual Studio Post Build script, will copy the .addin file and 
  ek24 folder(with all its contents) into the 'dst' folder just like 
  it pastes into revit's addin folder
- Now the 'dist' folder has the lastest plugin
- Open the 'EK24_Installer' Inno script file, and hit Compile
- Share the 'EK24_Installer.exe' file to the users


open the 'Developer Command Prompt' inside Visual Studio

step1: paste following, make sure to replace the password with my pin
signtool sign ^
/f "C:\Users\sreddy\dev\ek24\Installer\src\smplsystems.pfx" ^
/p "my_password_here" ^
/tr http://timestamp.digicert.com ^
/td sha256 ^
/fd sha256 ^
"C:\Users\sreddy\dev\ek24\Installer\src\dist\ek24\ek24.dll"

step2: paste this also in the 'Developer Command Prompt'
signtool verify /pa /v "C:\Users\sreddy\dev\ek24\Installer\src\dist\ek24\ek24.dll"


