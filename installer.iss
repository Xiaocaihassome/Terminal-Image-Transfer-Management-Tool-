; 终端图片中转管理工具 - Inno Setup 安装脚本
#define MyAppName "终端图片中转管理工具"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "小蔡有点料"
#define MyAppExeName "ImageManager.exe"

[Setup]
AppId={{B8E3F7A2-4C6D-4E9A-A1F3-2D5B8C7E9F01}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/Xiaocaihassome/ImageManager
DefaultDirName={autopf}\ImageManager
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=ImageManager_Setup_{#MyAppVersion}
SetupIconFile=ImageManager\Resources\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
DisableDirPage=no
AllowNoIcons=yes
LicenseFile=installer\license.txt
CloseApplications=yes
CloseApplicationsFilter=ImageManager.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "ImageManager\Resources\app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 删除应用安装目录
Type: filesandordirs; Name: "{app}"
; 删除用户配置和日志
Type: filesandordirs; Name: "{localappdata}\ImageManager"
Type: filesandordirs; Name: "{userappdata}\ImageManager"
; 删除临时文件
Type: filesandordirs; Name: "{tmp}\ImageManager"
