; EK24_Installer.iss

#define SoftwareName "EK24"
#define RevitVersion "2024"
#define SoftwareVersion "1.0.0"

[Setup]
AppName={#SoftwareName}
AppVersion={#SoftwareVersion}
DefaultDirName={userappdata}\Autodesk\Revit\Addins\{#RevitVersion}
DisableDirPage=yes
OutputDir=.
OutputBaseFilename={#SoftwareName}_Installer
Compression=lzma
SolidCompression=yes
Uninstallable=no
CreateUninstallRegKey=no
DisableProgramGroupPage=yes
SetupIconFile=icon.ico
; SilentInstall=Yes
WizardStyle=modern

[Files]
Source: "dist\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Run]
Filename: "powershell.exe"; \
  Parameters: "-ExecutionPolicy Bypass -Command ""Get-ChildItem '{app}' -Recurse | Unblock-File"""; \
  Flags: runhidden

[Code]
procedure DeleteOldFiles();
var
  AddinsPath, AddinFile, PluginFolder: string;
begin
  AddinsPath := ExpandConstant('{userappdata}\Autodesk\Revit\Addins\{#RevitVersion}');
  AddinFile := AddinsPath + '\ek24.addin';
  PluginFolder := AddinsPath + '\ek24';

  if FileExists(AddinFile) then
    DeleteFile(AddinFile);

  if DirExists(PluginFolder) then
    DelTree(PluginFolder, True, True, True);
end;

function InitializeSetup(): Boolean;
begin
  DeleteOldFiles();
  Result := True;
end;

