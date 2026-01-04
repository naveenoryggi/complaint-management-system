; Complaint Management System - Inno Setup Script
; This creates a professional Windows installer with GUI
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "Complaint Management System"
#define MyAppVersion "1.4.1"
#define MyAppPublisher "Oryggi Technology"
#define MyAppURL "https://www.yourcompany.com/"
#define MyAppExeName "ComplaintManagement.API.exe"

[Setup]
AppId={{B8E9F3A1-2C4D-4E6F-9A8B-1D3C5E7F9B0A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\ComplaintManagement
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=installer-output
OutputBaseFilename=ComplaintManagementSetup-v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.DatabaseConfigTitle=Database Configuration
english.DatabaseConfigSubTitle=Configure SQL Server connection

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "installservice"; Description: "Install API as Windows Service (runs in background)"; GroupDescription: "Select Installation Type:"; Flags: exclusive unchecked
Name: "configureiss"; Description: "Configure IIS Website (for production use)"; GroupDescription: "Select Installation Type:"; Flags: exclusive unchecked

[Files]
; Backend API
Source: "complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish\*"; DestDir: "{app}\API"; Flags: ignoreversion recursesubdirs createallsubdirs
; Frontend
Source: "complaint-system-angular\dist\complaint-system-angular\browser\*"; DestDir: "{app}\WWW"; Flags: ignoreversion recursesubdirs createallsubdirs
; Scripts
Source: "Scripts\Configure-IIS.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Create-Database.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Update-Config.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Install-Prerequisites.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Install-Prerequisites.ps1"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall
Source: "Scripts\Verify-Installation.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\migration_script.sql"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "Scripts\Run-DatabaseMigration.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion

[Dirs]
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Uploads"; Permissions: users-modify

[Code]
var
  DatabasePage: TWizardPage;
  PortConfigPage: TWizardPage;
  PrerequisitesPage: TWizardPage;
  SqlServerCombo: TNewComboBox;
  DatabaseNameEdit: TEdit;
  CreateNewDatabaseCheck: TCheckBox;
  UseWindowsAuthCheck: TCheckBox;
  SqlUsernameEdit: TEdit;
  SqlPasswordEdit: TPasswordEdit;
  TestConnectionButton: TButton;
  StatusLabel: TLabel;
  ExampleLabel: TLabel;
  PrereqStatusLabel: TLabel;
  DotNetStatus: TLabel;
  IISStatus: TLabel;
  UrlRewriteStatus: TLabel;
  PrerequisitesInstalled: Boolean;
  WebPortEdit: TEdit;
  ApiPortEdit: TEdit;
  WebPortStatusLabel: TLabel;
  ApiPortStatusLabel: TLabel;
  CheckPortsButton: TButton;
  HostnameEdit: TEdit;
  HostnameStatusLabel: TLabel;

procedure UpdateAuthFields(Sender: TObject); forward;
procedure TestConnection(Sender: TObject); forward;
procedure InstallPrerequisites(Sender: TObject); forward;
procedure CheckPortAvailability(Sender: TObject); forward;

function GetWebPort(Param: String): String;
begin
  Result := WebPortEdit.Text;
end;

function GetApiPort(Param: String): String;
begin
  Result := ApiPortEdit.Text;
end;

function GetHostname(Param: String): String;
begin
  Result := HostnameEdit.Text;
end;

function GetServerIP: String;
var
  ResultCode: Integer;
  TmpFile: String;
  IPAddress: AnsiString;
begin
  Result := '';
  TmpFile := ExpandConstant('{tmp}\serverip.txt');

  if Exec('cmd.exe', '/c powershell -Command "(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike ''Loopback*'' -and $_.IPAddress -notlike ''169.*'' } | Select-Object -First 1).IPAddress" > "' + TmpFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if LoadStringFromFile(TmpFile, IPAddress) then
    begin
      Result := Trim(String(IPAddress));
    end;
    DeleteFile(TmpFile);
  end;

  if Result = '' then
    Result := 'localhost';
end;

function IsPortInUse(Port: Integer): Boolean;
var
  ResultCode: Integer;
  PortStr: String;
begin
  Result := False;
  PortStr := IntToStr(Port);
  if Exec('cmd.exe', '/c netstat -an | findstr "LISTENING" | findstr ":' + PortStr + ' "', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := (ResultCode = 0);
end;

function IsValidPort(PortStr: String): Boolean;
var
  Port: Integer;
begin
  Result := False;
  try
    Port := StrToIntDef(PortStr, 0);
    if (Port >= 1) and (Port <= 65535) then
      Result := True;
  except
    Result := False;
  end;
end;

procedure UpdatePortStatus(PortEdit: TEdit; StatusLabel: TLabel; PortName: String);
var
  Port: Integer;
  PortStr: String;
begin
  PortStr := PortEdit.Text;

  if not IsValidPort(PortStr) then
  begin
    StatusLabel.Caption := PortName + ': Invalid port number (1-65535)';
    StatusLabel.Font.Color := clRed;
    Exit;
  end;

  Port := StrToInt(PortStr);

  if IsPortInUse(Port) then
  begin
    StatusLabel.Caption := PortName + ' ' + PortStr + ': IN USE - Choose another port!';
    StatusLabel.Font.Color := clRed;
  end
  else
  begin
    StatusLabel.Caption := PortName + ' ' + PortStr + ': Available';
    StatusLabel.Font.Color := clGreen;
  end;
end;

procedure CheckPortAvailability(Sender: TObject);
begin
  UpdatePortStatus(WebPortEdit, WebPortStatusLabel, 'Web Port');
  UpdatePortStatus(ApiPortEdit, ApiPortStatusLabel, 'API Port');
end;

function CheckDotNet8: Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  if Exec('cmd.exe', '/c dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 8."', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := (ResultCode = 0);
end;

function CheckIIS: Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  if Exec('cmd.exe', '/c sc query W3SVC | findstr "RUNNING"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    Result := (ResultCode = 0);
  if not Result then
    if Exec('cmd.exe', '/c sc query W3SVC | findstr "STOPPED"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      Result := (ResultCode = 0);
end;

function CheckUrlRewrite: Boolean;
begin
  Result := FileExists(ExpandConstant('{pf}\IIS\URL Rewrite\rewrite.dll')) or
            FileExists(ExpandConstant('{pf32}\IIS\URL Rewrite\rewrite.dll')) or
            RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\IIS Extensions\URL Rewrite');
end;

function GetComputerNameString: String;
var
  ComputerName: String;
begin
  { Try registry location 1 }
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName', 'ComputerName', ComputerName) then
  begin
    if ComputerName <> '' then
    begin
      Result := ComputerName;
      Exit;
    end;
  end;
  { Try registry location 2 }
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName', 'ComputerName', ComputerName) then
  begin
    if ComputerName <> '' then
    begin
      Result := ComputerName;
      Exit;
    end;
  end;
  { Try Inno Setup constant }
  ComputerName := ExpandConstant('{computername}');
  if ComputerName <> '' then
  begin
    Result := ComputerName;
    Exit;
  end;
  { Try environment variable via GetEnv }
  ComputerName := GetEnv('COMPUTERNAME');
  if ComputerName <> '' then
  begin
    Result := ComputerName;
    Exit;
  end;
  { Last resort - return localhost }
  Result := 'localhost';
end;

procedure GetSqlServerInstances(ComboBox: TNewComboBox);
var
  Names: TArrayOfString;
  I: Integer;
  InstanceName: String;
  MachineName: String;
  ResultCode: Integer;
  TmpFile: String;
  FileContent: AnsiString;
begin
  { Clear existing items }
  ComboBox.Items.Clear;

  { Get computer name }
  MachineName := GetComputerNameString;

  { Add computer name with common SQL instances first }
  if (MachineName <> '') and (MachineName <> 'localhost') then
  begin
    ComboBox.Items.Add(MachineName + '\SQLEXPRESS');
    ComboBox.Items.Add(MachineName);
  end;

  { Check registry for SQL Server instances }
  if RegGetSubkeyNames(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      InstanceName := Names[I];
      if InstanceName = 'MSSQLSERVER' then
        { Default instance - just use machine name }
        ComboBox.Items.Add(MachineName)
      else
        { Named instance - use MACHINE\INSTANCE format }
        ComboBox.Items.Add(MachineName + '\' + InstanceName);
    end;
  end;

  { Also check 32-bit registry for 32-bit SQL installs }
  if RegGetSubkeyNames(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Microsoft\Microsoft SQL Server\Instance Names\SQL', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      InstanceName := Names[I];
      if InstanceName = 'MSSQLSERVER' then
      begin
        if ComboBox.Items.IndexOf(MachineName) < 0 then
          ComboBox.Items.Add(MachineName);
      end
      else
      begin
        if ComboBox.Items.IndexOf(MachineName + '\' + InstanceName) < 0 then
          ComboBox.Items.Add(MachineName + '\' + InstanceName);
      end;
    end;
  end;

  { Add localhost variations as fallback }
  if ComboBox.Items.IndexOf('localhost\SQLEXPRESS') < 0 then
    ComboBox.Items.Add('localhost\SQLEXPRESS');
  if ComboBox.Items.IndexOf('localhost') < 0 then
    ComboBox.Items.Add('localhost');
  if ComboBox.Items.IndexOf('.\SQLEXPRESS') < 0 then
    ComboBox.Items.Add('.\SQLEXPRESS');

  { Select first item if available }
  if ComboBox.Items.Count > 0 then
    ComboBox.ItemIndex := 0;
end;

procedure DoUpdatePrerequisitesStatus;
begin
  if CheckDotNet8 then
  begin
    DotNetStatus.Caption := '.NET 8 Runtime: INSTALLED';
    DotNetStatus.Font.Color := clGreen;
  end
  else
  begin
    DotNetStatus.Caption := '.NET 8 Runtime: NOT INSTALLED';
    DotNetStatus.Font.Color := clRed;
  end;

  if CheckIIS then
  begin
    IISStatus.Caption := 'IIS Web Server: INSTALLED';
    IISStatus.Font.Color := clGreen;
  end
  else
  begin
    IISStatus.Caption := 'IIS Web Server: NOT INSTALLED';
    IISStatus.Font.Color := clRed;
  end;

  if CheckUrlRewrite then
  begin
    UrlRewriteStatus.Caption := 'URL Rewrite Module: INSTALLED';
    UrlRewriteStatus.Font.Color := clGreen;
  end
  else
  begin
    UrlRewriteStatus.Caption := 'URL Rewrite Module: NOT INSTALLED (will be installed)';
    UrlRewriteStatus.Font.Color := $000080FF; // Orange
  end;

  PrerequisitesInstalled := CheckDotNet8 and CheckIIS;
end;

procedure RefreshPrerequisites(Sender: TObject);
begin
  DoUpdatePrerequisitesStatus;
end;

procedure InstallPrerequisites(Sender: TObject);
var
  ResultCode: Integer;
  ScriptPath: string;
  DefaultInstallPath: string;
begin
  PrereqStatusLabel.Caption := 'Installing prerequisites... This may take several minutes.';
  PrereqStatusLabel.Font.Color := clBlue;

  ScriptPath := ExpandConstant('{tmp}\Install-Prerequisites.ps1');
  DefaultInstallPath := 'C:\Program Files\ComplaintManagement';

  { Extract and run the prerequisites script with default path }
  ExtractTemporaryFile('Install-Prerequisites.ps1');

  { Show PowerShell window so user can see progress }
  if Exec('powershell.exe', '-ExecutionPolicy Bypass -File "' + ScriptPath + '" -InstallPath "' + DefaultInstallPath + '"', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
    begin
      PrereqStatusLabel.Caption := 'Prerequisites installed successfully! Click Refresh to verify.';
      PrereqStatusLabel.Font.Color := clGreen;
    end
    else
    begin
      PrereqStatusLabel.Caption := 'Some prerequisites may not have installed. Check manually if needed.';
      PrereqStatusLabel.Font.Color := $000080FF;
    end;
  end
  else
  begin
    PrereqStatusLabel.Caption := 'Failed to run prerequisites script.';
    PrereqStatusLabel.Font.Color := clRed;
  end;

  DoUpdatePrerequisitesStatus;
end;

function GetSqlServer(Param: String): String;
begin
  Result := SqlServerCombo.Text;
end;

function GetDatabaseName(Param: String): String;
begin
  Result := DatabaseNameEdit.Text;
end;

function GetSqlUsername(Param: String): String;
begin
  Result := SqlUsernameEdit.Text;
end;

function GetSqlPassword(Param: String): String;
begin
  Result := SqlPasswordEdit.Text;
end;

function UseWindowsAuth: Boolean;
begin
  Result := UseWindowsAuthCheck.Checked;
end;

function ShouldCreateDatabase: Boolean;
begin
  Result := CreateNewDatabaseCheck.Checked;
end;

function GetWindowsAuthParam(Param: String): String;
begin
  if UseWindowsAuthCheck.Checked then
    Result := '-WindowsAuth'
  else
    Result := '';
end;

function GetConnectionString(Param: String): String;
begin
  if UseWindowsAuthCheck.Checked then
    Result := 'Server=' + SqlServerCombo.Text + ';Database=' + DatabaseNameEdit.Text + ';Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=60;Command Timeout=600'
  else
    Result := 'Server=' + SqlServerCombo.Text + ';Database=' + DatabaseNameEdit.Text + ';User Id=' + SqlUsernameEdit.Text + ';Password=' + SqlPasswordEdit.Text + ';TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=60;Command Timeout=600';
end;

procedure InitializeWizard;
var
  Label1, Label2, Label3, Label4, Label5: TLabel;
  PrereqLabel: TLabel;
  InstallPrereqButton: TButton;
  RefreshButton: TButton;
  TopPos: Integer;
begin
  { Create prerequisites check page }
  PrerequisitesPage := CreateCustomPage(wpWelcome,
    'System Prerequisites',
    'The installer will check and install required components automatically.');

  PrereqLabel := TLabel.Create(PrerequisitesPage);
  PrereqLabel.Parent := PrerequisitesPage.Surface;
  PrereqLabel.Caption := 'The following components are required for the Complaint Management System:';
  PrereqLabel.Left := 0;
  PrereqLabel.Top := 0;
  PrereqLabel.Width := 450;
  PrereqLabel.Font.Style := [fsBold];

  DotNetStatus := TLabel.Create(PrerequisitesPage);
  DotNetStatus.Parent := PrerequisitesPage.Surface;
  DotNetStatus.Caption := '.NET 8 Runtime: Checking...';
  DotNetStatus.Left := 20;
  DotNetStatus.Top := 30;
  DotNetStatus.Width := 400;

  IISStatus := TLabel.Create(PrerequisitesPage);
  IISStatus.Parent := PrerequisitesPage.Surface;
  IISStatus.Caption := 'IIS Web Server: Checking...';
  IISStatus.Left := 20;
  IISStatus.Top := 50;
  IISStatus.Width := 400;

  UrlRewriteStatus := TLabel.Create(PrerequisitesPage);
  UrlRewriteStatus.Parent := PrerequisitesPage.Surface;
  UrlRewriteStatus.Caption := 'URL Rewrite Module: Checking...';
  UrlRewriteStatus.Left := 20;
  UrlRewriteStatus.Top := 70;
  UrlRewriteStatus.Width := 400;

  InstallPrereqButton := TButton.Create(PrerequisitesPage);
  InstallPrereqButton.Parent := PrerequisitesPage.Surface;
  InstallPrereqButton.Caption := 'Install All Prerequisites';
  InstallPrereqButton.Left := 0;
  InstallPrereqButton.Top := 110;
  InstallPrereqButton.Width := 180;
  InstallPrereqButton.Height := 30;
  InstallPrereqButton.OnClick := @InstallPrerequisites;

  RefreshButton := TButton.Create(PrerequisitesPage);
  RefreshButton.Parent := PrerequisitesPage.Surface;
  RefreshButton.Caption := 'Refresh Status';
  RefreshButton.Left := 190;
  RefreshButton.Top := 110;
  RefreshButton.Width := 120;
  RefreshButton.Height := 30;
  RefreshButton.OnClick := @RefreshPrerequisites;

  PrereqStatusLabel := TLabel.Create(PrerequisitesPage);
  PrereqStatusLabel.Parent := PrerequisitesPage.Surface;
  PrereqStatusLabel.Caption := '';
  PrereqStatusLabel.Left := 0;
  PrereqStatusLabel.Top := 150;
  PrereqStatusLabel.Width := 450;
  PrereqStatusLabel.WordWrap := True;
  PrereqStatusLabel.Height := 60;

  { Check prerequisites on page load }
  DoUpdatePrerequisitesStatus;

  { Create custom database configuration page }
  DatabasePage := CreateCustomPage(PrerequisitesPage.ID,
    'Database Configuration',
    'Configure your SQL Server connection settings for the application database.');

  TopPos := 0;

  { SQL Server Name - Dropdown with detected instances }
  Label1 := TLabel.Create(DatabasePage);
  Label1.Parent := DatabasePage.Surface;
  Label1.Caption := 'SQL Server Instance (select from detected or type manually):';
  Label1.Left := 0;
  Label1.Top := TopPos;
  Label1.Font.Style := [fsBold];

  TopPos := TopPos + 20;

  SqlServerCombo := TNewComboBox.Create(DatabasePage);
  SqlServerCombo.Parent := DatabasePage.Surface;
  SqlServerCombo.Left := 0;
  SqlServerCombo.Top := TopPos;
  SqlServerCombo.Width := 350;
  SqlServerCombo.Style := csDropDown;  { Allows typing custom values }

  { Detect and populate SQL Server instances }
  GetSqlServerInstances(SqlServerCombo);

  TopPos := TopPos + 28;

  ExampleLabel := TLabel.Create(DatabasePage);
  ExampleLabel.Parent := DatabasePage.Surface;
  ExampleLabel.Caption := 'Detected instances shown above. You can also type: localhost, .\SQLEXPRESS, SERVERNAME\INSTANCE';
  ExampleLabel.Left := 0;
  ExampleLabel.Top := TopPos;
  ExampleLabel.Font.Color := clGray;
  ExampleLabel.Font.Size := 7;

  TopPos := TopPos + 25;

  { Database Name }
  Label2 := TLabel.Create(DatabasePage);
  Label2.Parent := DatabasePage.Surface;
  Label2.Caption := 'Database Name:';
  Label2.Left := 0;
  Label2.Top := TopPos;
  Label2.Font.Style := [fsBold];

  TopPos := TopPos + 20;

  DatabaseNameEdit := TEdit.Create(DatabasePage);
  DatabaseNameEdit.Parent := DatabasePage.Surface;
  DatabaseNameEdit.Left := 0;
  DatabaseNameEdit.Top := TopPos;
  DatabaseNameEdit.Width := 350;
  DatabaseNameEdit.Text := 'ComplaintManagementDB';

  TopPos := TopPos + 30;

  { Create New Database Option }
  CreateNewDatabaseCheck := TCheckBox.Create(DatabasePage);
  CreateNewDatabaseCheck.Parent := DatabasePage.Surface;
  CreateNewDatabaseCheck.Caption := 'Create new database automatically';
  CreateNewDatabaseCheck.Left := 0;
  CreateNewDatabaseCheck.Top := TopPos;
  CreateNewDatabaseCheck.Width := 400;
  CreateNewDatabaseCheck.Checked := True;

  TopPos := TopPos + 30;

  { Authentication Section }
  Label3 := TLabel.Create(DatabasePage);
  Label3.Parent := DatabasePage.Surface;
  Label3.Caption := 'Authentication Method:';
  Label3.Left := 0;
  Label3.Top := TopPos;
  Label3.Font.Style := [fsBold];

  TopPos := TopPos + 20;

  UseWindowsAuthCheck := TCheckBox.Create(DatabasePage);
  UseWindowsAuthCheck.Parent := DatabasePage.Surface;
  UseWindowsAuthCheck.Caption := 'Use Windows Authentication (Recommended for local development)';
  UseWindowsAuthCheck.Left := 0;
  UseWindowsAuthCheck.Top := TopPos;
  UseWindowsAuthCheck.Width := 450;
  UseWindowsAuthCheck.Checked := False;
  UseWindowsAuthCheck.OnClick := @UpdateAuthFields;

  TopPos := TopPos + 30;

  { SQL Username }
  Label4 := TLabel.Create(DatabasePage);
  Label4.Parent := DatabasePage.Surface;
  Label4.Caption := 'SQL Username:';
  Label4.Left := 0;
  Label4.Top := TopPos;

  TopPos := TopPos + 18;

  SqlUsernameEdit := TEdit.Create(DatabasePage);
  SqlUsernameEdit.Parent := DatabasePage.Surface;
  SqlUsernameEdit.Left := 0;
  SqlUsernameEdit.Top := TopPos;
  SqlUsernameEdit.Width := 200;
  SqlUsernameEdit.Text := 'sa';
  SqlUsernameEdit.Enabled := True;

  TopPos := TopPos + 28;

  { SQL Password }
  Label5 := TLabel.Create(DatabasePage);
  Label5.Parent := DatabasePage.Surface;
  Label5.Caption := 'SQL Password:';
  Label5.Left := 0;
  Label5.Top := TopPos;

  TopPos := TopPos + 18;

  SqlPasswordEdit := TPasswordEdit.Create(DatabasePage);
  SqlPasswordEdit.Parent := DatabasePage.Surface;
  SqlPasswordEdit.Left := 0;
  SqlPasswordEdit.Top := TopPos;
  SqlPasswordEdit.Width := 200;
  SqlPasswordEdit.Text := '';
  SqlPasswordEdit.Enabled := True;

  TopPos := TopPos + 35;

  { Test Connection Button }
  TestConnectionButton := TButton.Create(DatabasePage);
  TestConnectionButton.Parent := DatabasePage.Surface;
  TestConnectionButton.Caption := 'Test Connection';
  TestConnectionButton.Left := 0;
  TestConnectionButton.Top := TopPos;
  TestConnectionButton.Width := 130;
  TestConnectionButton.Height := 28;
  TestConnectionButton.OnClick := @TestConnection;

  { Status Label }
  StatusLabel := TLabel.Create(DatabasePage);
  StatusLabel.Parent := DatabasePage.Surface;
  StatusLabel.Caption := '';
  StatusLabel.Left := 140;
  StatusLabel.Top := TopPos + 5;
  StatusLabel.Width := 300;

  { Create Port Configuration page }
  PortConfigPage := CreateCustomPage(DatabasePage.ID,
    'Network Configuration',
    'Configure the hostname/IP and ports for the application.');

  TopPos := 0;

  { Hostname/IP Configuration }
  Label1 := TLabel.Create(PortConfigPage);
  Label1.Parent := PortConfigPage.Surface;
  Label1.Caption := 'Server Hostname or IP Address (for access from other computers):';
  Label1.Left := 0;
  Label1.Top := TopPos;
  Label1.Font.Style := [fsBold];

  TopPos := TopPos + 20;

  HostnameEdit := TEdit.Create(PortConfigPage);
  HostnameEdit.Parent := PortConfigPage.Surface;
  HostnameEdit.Left := 0;
  HostnameEdit.Top := TopPos;
  HostnameEdit.Width := 250;
  HostnameEdit.Text := GetServerIP;

  HostnameStatusLabel := TLabel.Create(PortConfigPage);
  HostnameStatusLabel.Parent := PortConfigPage.Surface;
  HostnameStatusLabel.Caption := '(Auto-detected server IP)';
  HostnameStatusLabel.Left := 260;
  HostnameStatusLabel.Top := TopPos + 3;
  HostnameStatusLabel.Font.Color := clGray;

  TopPos := TopPos + 35;

  { Port Configuration Info }
  Label2 := TLabel.Create(PortConfigPage);
  Label2.Parent := PortConfigPage.Surface;
  Label2.Caption := 'Configure the network ports:';
  Label2.Left := 0;
  Label2.Top := TopPos;
  Label2.Font.Style := [fsBold];

  TopPos := TopPos + 25;

  { Web Port }
  Label2 := TLabel.Create(PortConfigPage);
  Label2.Parent := PortConfigPage.Surface;
  Label2.Caption := 'Web Frontend Port (for browser access):';
  Label2.Left := 0;
  Label2.Top := TopPos;

  TopPos := TopPos + 20;

  WebPortEdit := TEdit.Create(PortConfigPage);
  WebPortEdit.Parent := PortConfigPage.Surface;
  WebPortEdit.Left := 0;
  WebPortEdit.Top := TopPos;
  WebPortEdit.Width := 100;
  WebPortEdit.Text := '80';

  WebPortStatusLabel := TLabel.Create(PortConfigPage);
  WebPortStatusLabel.Parent := PortConfigPage.Surface;
  WebPortStatusLabel.Caption := '';
  WebPortStatusLabel.Left := 120;
  WebPortStatusLabel.Top := TopPos + 3;
  WebPortStatusLabel.Width := 350;

  TopPos := TopPos + 35;

  { API Port }
  Label3 := TLabel.Create(PortConfigPage);
  Label3.Parent := PortConfigPage.Surface;
  Label3.Caption := 'API Backend Port (for application services):';
  Label3.Left := 0;
  Label3.Top := TopPos;

  TopPos := TopPos + 20;

  ApiPortEdit := TEdit.Create(PortConfigPage);
  ApiPortEdit.Parent := PortConfigPage.Surface;
  ApiPortEdit.Left := 0;
  ApiPortEdit.Top := TopPos;
  ApiPortEdit.Width := 100;
  ApiPortEdit.Text := '5000';

  ApiPortStatusLabel := TLabel.Create(PortConfigPage);
  ApiPortStatusLabel.Parent := PortConfigPage.Surface;
  ApiPortStatusLabel.Caption := '';
  ApiPortStatusLabel.Left := 120;
  ApiPortStatusLabel.Top := TopPos + 3;
  ApiPortStatusLabel.Width := 350;

  TopPos := TopPos + 40;

  { Check Ports Button }
  CheckPortsButton := TButton.Create(PortConfigPage);
  CheckPortsButton.Parent := PortConfigPage.Surface;
  CheckPortsButton.Caption := 'Check Port Availability';
  CheckPortsButton.Left := 0;
  CheckPortsButton.Top := TopPos;
  CheckPortsButton.Width := 160;
  CheckPortsButton.Height := 28;
  CheckPortsButton.OnClick := @CheckPortAvailability;

  TopPos := TopPos + 45;

  { Port Info Label }
  Label4 := TLabel.Create(PortConfigPage);
  Label4.Parent := PortConfigPage.Surface;
  Label4.Caption := 'Common ports: 80 (HTTP default), 8080, 8000, 3000, 11020';
  Label4.Left := 0;
  Label4.Top := TopPos;
  Label4.Font.Color := clGray;

  TopPos := TopPos + 18;

  Label5 := TLabel.Create(PortConfigPage);
  Label5.Parent := PortConfigPage.Surface;
  Label5.Caption := 'Note: Port 80 requires IIS. For Windows Service, use any available port.';
  Label5.Left := 0;
  Label5.Top := TopPos;
  Label5.Font.Color := clGray;

  { Check ports on page load }
  CheckPortAvailability(nil);
end;

procedure UpdateAuthFields(Sender: TObject);
begin
  SqlUsernameEdit.Enabled := not UseWindowsAuthCheck.Checked;
  SqlPasswordEdit.Enabled := not UseWindowsAuthCheck.Checked;
  if UseWindowsAuthCheck.Checked then
  begin
    SqlUsernameEdit.Text := '';
    SqlPasswordEdit.Text := '';
  end
  else
  begin
    SqlUsernameEdit.Text := 'sa';
  end;
end;

procedure TestConnection(Sender: TObject);
var
  ConnectionString: string;
  ResultCode: Integer;
  TestScript: string;
  TempFile: string;
begin
  StatusLabel.Caption := 'Testing connection...';
  StatusLabel.Font.Color := clBlue;

  { Build connection string for master database }
  if UseWindowsAuthCheck.Checked then
    ConnectionString := 'Server=' + SqlServerCombo.Text + ';Database=master;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=10;'
  else
    ConnectionString := 'Server=' + SqlServerCombo.Text + ';Database=master;User Id=' + SqlUsernameEdit.Text + ';Password=' + SqlPasswordEdit.Text + ';TrustServerCertificate=True;Connection Timeout=10;';

  TempFile := ExpandConstant('{tmp}\test-db-connection.ps1');

  TestScript :=
    '$ErrorActionPreference = "Stop"' + #13#10 +
    'try {' + #13#10 +
    '  Add-Type -AssemblyName "System.Data"' + #13#10 +
    '  $conn = New-Object System.Data.SqlClient.SqlConnection' + #13#10 +
    '  $conn.ConnectionString = "' + ConnectionString + '"' + #13#10 +
    '  $conn.Open()' + #13#10 +
    '  $conn.Close()' + #13#10 +
    '  exit 0' + #13#10 +
    '} catch {' + #13#10 +
    '  Write-Host $_.Exception.Message' + #13#10 +
    '  exit 1' + #13#10 +
    '}';

  SaveStringToFile(TempFile, TestScript, False);

  if Exec('powershell.exe', '-ExecutionPolicy Bypass -WindowStyle Hidden -File "' + TempFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
    begin
      StatusLabel.Caption := 'Connection successful!';
      StatusLabel.Font.Color := clGreen;
      MsgBox('SQL Server connection successful!' + #13#10 + #13#10 +
             'Server: ' + SqlServerCombo.Text + #13#10 +
             'Database to create: ' + DatabaseNameEdit.Text, mbInformation, MB_OK);
    end
    else
    begin
      StatusLabel.Caption := 'Connection failed!';
      StatusLabel.Font.Color := clRed;
      MsgBox('SQL Server connection failed!' + #13#10 + #13#10 +
             'Please verify:' + #13#10 +
             '- SQL Server name is correct (e.g., localhost\SQLEXPRESS)' + #13#10 +
             '- SQL Server service is running' + #13#10 +
             '- Username and password are correct' + #13#10 +
             '- SQL Server allows remote connections', mbError, MB_OK);
    end;
  end
  else
  begin
    StatusLabel.Caption := 'Test failed!';
    StatusLabel.Font.Color := clRed;
    MsgBox('Failed to execute connection test. Please ensure PowerShell is available.', mbError, MB_OK);
  end;

  DeleteFile(TempFile);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  WebPort, ApiPort: Integer;
begin
  Result := True;

  { Validate database page }
  if CurPageID = DatabasePage.ID then
  begin
    if Trim(SqlServerCombo.Text) = '' then
    begin
      MsgBox('Please enter a SQL Server name.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if Trim(DatabaseNameEdit.Text) = '' then
    begin
      MsgBox('Please enter a database name.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    if (not UseWindowsAuthCheck.Checked) and (Trim(SqlUsernameEdit.Text) = '') then
    begin
      MsgBox('Please enter a SQL username or select Windows Authentication.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;

  { Validate port configuration page }
  if CurPageID = PortConfigPage.ID then
  begin
    { Validate Web Port }
    if not IsValidPort(WebPortEdit.Text) then
    begin
      MsgBox('Invalid Web Port. Please enter a number between 1 and 65535.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    { Validate API Port }
    if not IsValidPort(ApiPortEdit.Text) then
    begin
      MsgBox('Invalid API Port. Please enter a number between 1 and 65535.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    WebPort := StrToInt(WebPortEdit.Text);
    ApiPort := StrToInt(ApiPortEdit.Text);

    { Check if ports are the same }
    if WebPort = ApiPort then
    begin
      MsgBox('Web Port and API Port cannot be the same. Please use different ports.', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    { Check if Web Port is in use }
    if IsPortInUse(WebPort) then
    begin
      if MsgBox('Web Port ' + IntToStr(WebPort) + ' is currently in use!' + #13#10 + #13#10 +
                'This may cause conflicts. Do you want to continue anyway?', mbConfirmation, MB_YESNO) = IDNO then
      begin
        Result := False;
        Exit;
      end;
    end;

    { Check if API Port is in use }
    if IsPortInUse(ApiPort) then
    begin
      if MsgBox('API Port ' + IntToStr(ApiPort) + ' is currently in use!' + #13#10 + #13#10 +
                'This may cause conflicts. Do you want to continue anyway?', mbConfirmation, MB_YESNO) = IDNO then
      begin
        Result := False;
        Exit;
      end;
    end;
  end;
end;

[Run]
; NOTE: Prerequisites are installed via the button on Prerequisites page - not here

; Update appsettings.json with connection string, hostname and ports
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Update-Config.ps1"" -ConfigPath ""{app}\API\appsettings.json"" -ConnectionString ""{code:GetConnectionString}"" -WebPort ""{code:GetWebPort}"" -ApiPort ""{code:GetApiPort}"" -Hostname ""{code:GetHostname}"""; Flags: runhidden; StatusMsg: "Updating database and network configuration..."

; Create database (if selected) - SHOW PROGRESS
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Create-Database.ps1"" -Server ""{code:GetSqlServer}"" -Database ""{code:GetDatabaseName}"" -Username ""{code:GetSqlUsername}"" -Password ""{code:GetSqlPassword}"" {code:GetWindowsAuthParam}"; StatusMsg: "Creating database (watch PowerShell window for progress)..."; Check: ShouldCreateDatabase

; Run migration script to create all database tables - SHOW PROGRESS
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Run-DatabaseMigration.ps1"" -Server ""{code:GetSqlServer}"" -Database ""{code:GetDatabaseName}"" -Username ""{code:GetSqlUsername}"" -Password ""{code:GetSqlPassword}"" {code:GetWindowsAuthParam} -ScriptPath ""{app}\Scripts\migration_script.sql"""; StatusMsg: "Running database migration (creating tables)..."; Check: ShouldCreateDatabase

; Run the API to seed default users - SHOW PROGRESS
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Write-Host 'Starting API to seed default users...' -ForegroundColor Cyan; Write-Host 'Seeding admin users and default data (30 seconds)...' -ForegroundColor Yellow; $p = Start-Process -FilePath '{app}\API\ComplaintManagement.API.exe' -WorkingDirectory '{app}\API' -PassThru; Start-Sleep -Seconds 30; Write-Host 'Stopping API...' -ForegroundColor Yellow; Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue; Write-Host 'Database seeding complete!' -ForegroundColor Green; Start-Sleep -Seconds 2"""; StatusMsg: "Seeding default users and data (30 seconds)..."; Check: ShouldCreateDatabase

; Install Windows Service (if selected)
Filename: "sc.exe"; Parameters: "create ComplaintManagementAPI binPath= ""{app}\API\ComplaintManagement.API.exe"" start= auto DisplayName= ""Complaint Management API"""; Flags: runhidden; StatusMsg: "Installing Windows Service..."; Tasks: installservice

; Set service description
Filename: "sc.exe"; Parameters: "description ComplaintManagementAPI ""Complaint Management System API Service"""; Flags: runhidden; StatusMsg: "Configuring Windows Service..."; Tasks: installservice

; Start Windows Service
Filename: "sc.exe"; Parameters: "start ComplaintManagementAPI"; Flags: runhidden; StatusMsg: "Starting Windows Service..."; Tasks: installservice

; Configure IIS with custom port (if selected)
Filename: "powershell.exe"; Parameters: "-NonInteractive -ExecutionPolicy Bypass -File ""{app}\Scripts\Configure-IIS.ps1"" -InstallPath ""{app}"" -WebPort {code:GetWebPort} -ApiPort {code:GetApiPort}"; Flags: runhidden; StatusMsg: "Configuring IIS Website on port {code:GetWebPort}..."; Tasks: configureiss

; Create LaunchApp.bat for shortcuts - IIS uses WebPort, Service uses ApiPort
Filename: "powershell.exe"; Parameters: "-NonInteractive -ExecutionPolicy Bypass -Command ""$port = '{code:GetWebPort}'; if ($port -eq '80') {{ $url = 'http://localhost' }} else {{ $url = 'http://localhost:' + $port }}; '@echo off`r`nstart ' + $url | Out-File -FilePath '{app}\LaunchApp.bat' -Encoding ASCII"""; Flags: runhidden; StatusMsg: "Creating application shortcuts..."; Tasks: configureiss
Filename: "powershell.exe"; Parameters: "-NonInteractive -ExecutionPolicy Bypass -Command ""$port = '{code:GetApiPort}'; if ($port -eq '80') {{ $url = 'http://localhost' }} else {{ $url = 'http://localhost:' + $port }}; '@echo off`r`nstart ' + $url | Out-File -FilePath '{app}\LaunchApp.bat' -Encoding ASCII"""; Flags: runhidden; StatusMsg: "Creating application shortcuts..."; Tasks: installservice

; Wait a moment for services to fully start
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Write-Host 'Waiting for services to start...' -ForegroundColor Yellow; Start-Sleep -Seconds 5"""; StatusMsg: "Waiting for services to start..."; Tasks: installservice
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Write-Host 'Waiting for IIS to start...' -ForegroundColor Yellow; Start-Sleep -Seconds 3"""; StatusMsg: "Waiting for IIS to start..."; Tasks: configureiss

; Verify installation and show status to user - SHOW PROGRESS
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Verify-Installation.ps1"" -Hostname ""{code:GetHostname}"" -ApiPort {code:GetApiPort} -WebPort {code:GetWebPort}"; Description: "Verify Installation Status"; Flags: postinstall; Tasks: installservice
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Verify-Installation.ps1"" -Hostname ""{code:GetHostname}"" -ApiPort {code:GetApiPort} -WebPort {code:GetWebPort} -IsIIS"; Description: "Verify Installation Status"; Flags: postinstall; Tasks: configureiss

; Open browser after installation (using localhost to avoid CORS issues)
; For Windows Service - uses API port since that's where the service runs
Filename: "cmd.exe"; Parameters: "/c start http://localhost:{code:GetApiPort}"; Description: "Launch Complaint Management System"; Flags: postinstall skipifsilent nowait shellexec; Tasks: installservice
; For IIS - uses Web port since IIS serves the frontend
Filename: "cmd.exe"; Parameters: "/c start http://localhost:{code:GetWebPort}"; Description: "Launch Complaint Management System"; Flags: postinstall skipifsilent nowait shellexec; Tasks: configureiss

[UninstallRun]
; Stop and remove Windows Service
Filename: "sc.exe"; Parameters: "stop ComplaintManagementAPI"; Flags: runhidden; RunOnceId: "StopService"
Filename: "sc.exe"; Parameters: "delete ComplaintManagementAPI"; Flags: runhidden; RunOnceId: "DeleteService"

; Remove IIS website
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Import-Module WebAdministration -ErrorAction SilentlyContinue; Remove-Website -Name 'ComplaintManagement' -ErrorAction SilentlyContinue; Remove-WebAppPool -Name 'ComplaintManagementAppPool' -ErrorAction SilentlyContinue; Remove-WebAppPool -Name 'ComplaintManagementAPIPool' -ErrorAction SilentlyContinue"""; Flags: runhidden; RunOnceId: "RemoveIIS"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\LaunchApp.bat"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\LaunchApp.bat"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "SqlServer"; ValueData: "{code:GetSqlServer}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Database"; ValueData: "{code:GetDatabaseName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "WebPort"; ValueData: "{code:GetWebPort}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "ApiPort"; ValueData: "{code:GetApiPort}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Hostname"; ValueData: "{code:GetHostname}"; Flags: uninsdeletekey
