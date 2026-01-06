; MozaHotkey Inno Setup Script
; Creates a Windows installer with .NET 8 dependency checking

#define MyAppName "MozaHotkey"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "MozaHotkey"
#define MyAppURL "https://github.com/d-b-c-e/mozahotkey"
#define MyAppExeName "MozaHotkey.App.exe"

[Setup]
AppId={{8F4E9A71-3B2C-4D5E-9F1A-2B3C4D5E6F7A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=..\releases
OutputBaseFilename=MozaHotkey-Setup-v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Start MozaHotkey when Windows starts"; GroupDescription: "Startup:"; Flags: unchecked

[Files]
Source: "..\src\MozaHotkey.App\bin\Release\net8.0-windows\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "MozaHotkey"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
const
  DOTNET_RUNTIME_VERSION = '8.0.0';
  DOTNET_DOWNLOAD_URL = 'https://dotnet.microsoft.com/download/dotnet/8.0';

function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
  Output: AnsiString;
  TempFile: String;
begin
  Result := False;
  TempFile := ExpandConstant('{tmp}\dotnet_check.txt');

  // Try to run dotnet --list-runtimes and check for Microsoft.WindowsDesktop.App 8.x
  if Exec('cmd.exe', '/c dotnet --list-runtimes > "' + TempFile + '" 2>&1', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
    begin
      if LoadStringFromFile(TempFile, Output) then
      begin
        // Check for .NET 8 Desktop Runtime
        if Pos('Microsoft.WindowsDesktop.App 8.', Output) > 0 then
          Result := True;
      end;
    end;
  end;

  DeleteFile(TempFile);
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;

  if not IsDotNetInstalled() then
  begin
    if MsgBox(
      'MozaHotkey requires the .NET 8.0 Desktop Runtime, which is not installed on your system.' + #13#10 + #13#10 +
      'Would you like to open the download page now?' + #13#10 + #13#10 +
      'After installing .NET 8.0, run this setup again.',
      mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', DOTNET_DOWNLOAD_URL, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
    Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  SettingsDir: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Ask if user wants to remove settings
    SettingsDir := ExpandConstant('{localappdata}\MozaHotkey');
    if DirExists(SettingsDir) then
    begin
      if MsgBox('Do you want to remove MozaHotkey settings and preferences?', mbConfirmation, MB_YESNO) = IDYES then
      begin
        DelTree(SettingsDir, True, True, True);
      end;
    end;
  end;
end;
