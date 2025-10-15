; EK24_Installer.iss
#define SoftwareName "EK24"
#define RevitVersion "2024"
#define SoftwareVersion "14.0.0"
#define BackupOutputDir "T:\50_DESIGN DATA\Software"

[Setup]
AppName={#SoftwareName}
AppVersion={#SoftwareVersion}
DefaultDirName={userappdata}\Autodesk\Revit\Addins\{#RevitVersion}
DisableDirPage=yes
OutputDir=.
OutputBaseFilename={#SoftwareName}_Installer_{#SoftwareVersion}
Compression=lzma
SolidCompression=yes
Uninstallable=no
CreateUninstallRegKey=no
DisableProgramGroupPage=yes
SetupIconFile=icons\icon.ico
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