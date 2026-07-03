; Terminal Image Transfer Manager - Inno Setup Script
#define MyAppName "Terminal Image Transfer Manager"
#define MyAppVersion "1.2.2"
#define MyAppPublisher "小蔡有点料"
#define MyAppExeName "ImageManager.exe"
#define MyAppId "E8C2F5A1-3B7D-4F9E-A6D4-1C8E5B2F7A39"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-
DefaultDirName={autopf}\ImageManager
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=ImageManager_Setup_{#MyAppVersion}
SetupIconFile=ImageManager\Resources\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
DisableDirPage=no
AllowNoIcons=yes
LicenseFile=installer\license.txt
CloseApplications=yes
CloseApplicationsFilter=ImageManager.exe
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "ImageManager\Resources\app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "{userappdata}\ImageManager"
Type: filesandordirs; Name: "{localappdata}\ImageManager"

[Code]
function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  Exec('taskkill.exe', '/f /im ImageManager.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := True;
end;
