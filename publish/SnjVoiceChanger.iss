#define AppName "Snj Voice Changer"
#define AppVersion "1.1"
#define AppPublisher "SNJ7SNJ Development"
#define AppExeName "SnjVoiceChanger.exe"
#define PublishDir "app"

[Setup]
AppId={{9B661D72-42CE-44A5-A5B2-19C50A0F1D7E}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\SNJ7SNJ\SnjVoiceChanger
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=SnjVoiceChanger_v1.1
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=..\SnjVoiceChanger\Assets\app.ico
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\common\VBCABLE_Driver_Pack45\VBCABLE_Setup_x64.exe"; Description: "Install VB-CABLE virtual audio driver"; Flags: postinstall shellexec skipifsilent
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: postinstall nowait skipifsilent
