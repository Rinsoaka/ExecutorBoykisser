[Setup]
AppName=Boykisser Executor
AppVersion=1.0.0
AppPublisher=Rinsoaka
DefaultDirName={pf}\BoykisserExecutor
DefaultGroupName=Boykisser Executor
OutputDir=.
OutputBaseFilename=BoykisserExecutor-Setup-1.0.0
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=admin
DisableProgramGroupPage=no
UninstallDisplayIcon={app}\BoykisserExecutor.exe

[Messages]
SetupAppTitle=Boykisser Executor 1.0.0
SetupWindowTitle=Install Boykisser Executor 1.0.0

[Files]
Source:"bin\Release\net8.0-windows\win-x64\publish\BoykisserExecutor.exe";DestDir:"{app}";Flags:ignoreversion
Source:"bin\Release\net8.0-windows\win-x64\publish\WebView2Loader.dll";DestDir:"{app}";Flags:ignoreversion
Source:"ExecutorDLL.dll";DestDir:"{app}";Flags:ignoreversion
Source:"ExecutorDLL\*";DestDir:"{app}\ExecutorDLL";Flags:ignoreversion recursesubdirs

[Icons]
Name:"{group}\Boykisser Executor";Filename:"{app}\BoykisserExecutor.exe";Comment:"Launch Boykisser Executor"
Name:"{group}\Uninstall Boykisser Executor";Filename:"{uninstallexe}"
Name:"{commondesktop}\Boykisser Executor";Filename:"{app}\BoykisserExecutor.exe";Comment:"Launch Boykisser Executor";Tasks:desktopicon

[Tasks]
Name:desktopicon;Description:"Create a desktop shortcut";GroupDescription:"Additional icons:";Flags:checkedbydefault

[Run]
Filename:"{app}\BoykisserExecutor.exe";Description:"Launch Boykisser Executor";Flags:postinstall nowait skipifsilent shellexec

[UninstallRun]
Filename:"{sys}\taskkill";Parameters:"/f /im BoykisserExecutor.exe";Flags:runhidden

[Code]
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  if Exec('cmd.exe', '/c "dotnet --list-runtimes 2>nul | findstr Microsoft.WindowsDesktop.App 8.0 >nul"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := ResultCode = 0
  else
    Result := False;
end;

function InitializeSetup(): Boolean;
begin
  if not IsDotNetInstalled() then
  begin
    if MsgBox('Boykisser Executor requires .NET 8 Desktop Runtime.'#13#13'Download and install it now?', mbConfirmation, MB_YESNO) = idYes then
      ShellExec('open', 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime', '', '', SW_SHOW, ewNoWait);
    Result := True;
  end
  else
    Result := True;
end;
