; Complaint Management System - Inno Setup Script
; This creates a professional Windows installer with GUI
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "Complaint Management System"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Oryggi Technology"
#define MyAppURL "https://www.yourcompany.com/"
#define MyAppExeName "ComplaintManagement.API.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
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
;LicenseFile=LICENSE.txt
;InfoBeforeFile=README.txt
OutputDir=installer-output
OutputBaseFilename=ComplaintManagementSetup-v{#MyAppVersion}
;SetupIconFile=app-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.DatabaseConfigTitle=Database Configuration
english.DatabaseConfigSubTitle=Configure SQL Server connection
english.PrerequisitesTitle=Prerequisites Check
english.PrerequisitesSubTitle=Checking required components

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "installservice"; Description: "Install API as Windows Service"; GroupDescription: "Services:"; Flags: unchecked
Name: "configureiss"; Description: "Configure IIS Website"; GroupDescription: "Web Server:"; Flags: unchecked

[Files]
; Backend API
Source: "complaint-system-dotnet\src\ComplaintManagement.API\bin\Release\net8.0\publish\*"; DestDir: "{app}\API"; Flags: ignoreversion recursesubdirs createallsubdirs
; Frontend
Source: "complaint-system-angular\dist\complaint-system-angular\browser\*"; DestDir: "{app}\WWW"; Flags: ignoreversion recursesubdirs createallsubdirs
; Documentation
Source: "INSTALLATION_GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
;Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
; Scripts
;Source: "Install-Service.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
;Source: "Uninstall-Service.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion

[Dirs]
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Uploads"; Permissions: users-modify

[Code]
var
  DatabaseConfigPage: TInputQueryWizardPage;
  SqlServerEdit: TEdit;
  DatabaseNameEdit: TEdit;
  CreateNewDatabaseCheck: TCheckBox;
  UseWindowsAuthCheck: TCheckBox;
  SqlUsernameEdit: TEdit;
  SqlPasswordEdit: TPasswordEdit;
  TestConnectionButton: TButton;

procedure WindowsAuthCheckClick(Sender: TObject); forward;
procedure TestConnectionButtonClick(Sender: TObject); forward;
procedure CreateDatabaseCheckClick(Sender: TObject); forward;

procedure InitializeWizard;
var
  Page: TWizardPage;
  Label1, Label2, Label3, Label4, Label5: TLabel;
  Panel: TPanel;
begin
  { Create custom page for database configuration }
  Page := CreateCustomPage(wpSelectDir, 'Database Configuration',
    'Configure your SQL Server connection settings');

  { SQL Server Name }
  Label1 := TLabel.Create(Page);
  Label1.Parent := Page.Surface;
  Label1.Caption := 'SQL Server Name / IP Address:';
  Label1.Left := 0;
  Label1.Top := 0;
  Label1.Width := 300;

  SqlServerEdit := TEdit.Create(Page);
  SqlServerEdit.Parent := Page.Surface;
  SqlServerEdit.Left := 0;
  SqlServerEdit.Top := Label1.Top + Label1.Height + 4;
  SqlServerEdit.Width := 300;
  SqlServerEdit.Text := 'localhost';

  { Database Name }
  Label2 := TLabel.Create(Page);
  Label2.Parent := Page.Surface;
  Label2.Caption := 'Database Name:';
  Label2.Left := 0;
  Label2.Top := SqlServerEdit.Top + SqlServerEdit.Height + 16;
  Label2.Width := 300;

  DatabaseNameEdit := TEdit.Create(Page);
  DatabaseNameEdit.Parent := Page.Surface;
  DatabaseNameEdit.Left := 0;
  DatabaseNameEdit.Top := Label2.Top + Label2.Height + 4;
  DatabaseNameEdit.Width := 300;
  DatabaseNameEdit.Text := 'ComplaintManagementDB';

  { Create New Database Option }
  CreateNewDatabaseCheck := TCheckBox.Create(Page);
  CreateNewDatabaseCheck.Parent := Page.Surface;
  CreateNewDatabaseCheck.Caption := 'Create new database (uncheck if database already exists)';
  CreateNewDatabaseCheck.Left := 0;
  CreateNewDatabaseCheck.Top := DatabaseNameEdit.Top + DatabaseNameEdit.Height + 12;
  CreateNewDatabaseCheck.Width := 400;
  CreateNewDatabaseCheck.Checked := True;
  CreateNewDatabaseCheck.OnClick := @CreateDatabaseCheckClick;

  { Authentication Type }
  Label3 := TLabel.Create(Page);
  Label3.Parent := Page.Surface;
  Label3.Caption := 'Authentication:';
  Label3.Left := 0;
  Label3.Top := CreateNewDatabaseCheck.Top + CreateNewDatabaseCheck.Height + 16;
  Label3.Width := 300;

  UseWindowsAuthCheck := TCheckBox.Create(Page);
  UseWindowsAuthCheck.Parent := Page.Surface;
  UseWindowsAuthCheck.Caption := 'Use Windows Authentication (Recommended)';
  UseWindowsAuthCheck.Left := 0;
  UseWindowsAuthCheck.Top := Label3.Top + Label3.Height + 4;
  UseWindowsAuthCheck.Width := 350;
  UseWindowsAuthCheck.Checked := True;

  { SQL Username }
  Label4 := TLabel.Create(Page);
  Label4.Parent := Page.Surface;
  Label4.Caption := 'SQL Username (if not using Windows Auth):';
  Label4.Left := 0;
  Label4.Top := UseWindowsAuthCheck.Top + UseWindowsAuthCheck.Height + 16;
  Label4.Width := 300;

  SqlUsernameEdit := TEdit.Create(Page);
  SqlUsernameEdit.Parent := Page.Surface;
  SqlUsernameEdit.Left := 0;
  SqlUsernameEdit.Top := Label4.Top + Label4.Height + 4;
  SqlUsernameEdit.Width := 300;
  SqlUsernameEdit.Enabled := False;

  { SQL Password }
  Label5 := TLabel.Create(Page);
  Label5.Parent := Page.Surface;
  Label5.Caption := 'SQL Password:';
  Label5.Left := 0;
  Label5.Top := SqlUsernameEdit.Top + SqlUsernameEdit.Height + 8;
  Label5.Width := 300;

  SqlPasswordEdit := TPasswordEdit.Create(Page);
  SqlPasswordEdit.Parent := Page.Surface;
  SqlPasswordEdit.Left := 0;
  SqlPasswordEdit.Top := Label5.Top + Label5.Height + 4;
  SqlPasswordEdit.Width := 300;
  SqlPasswordEdit.Enabled := False;

  { Test Connection Button }
  TestConnectionButton := TButton.Create(Page);
  TestConnectionButton.Parent := Page.Surface;
  TestConnectionButton.Caption := 'Test Connection';
  TestConnectionButton.Left := 0;
  TestConnectionButton.Top := SqlPasswordEdit.Top + SqlPasswordEdit.Height + 20;
  TestConnectionButton.Width := 150;
  TestConnectionButton.Height := 30;
  TestConnectionButton.OnClick := @TestConnectionButtonClick;

  { Enable/Disable username and password based on auth type }
  UseWindowsAuthCheck.OnClick := @WindowsAuthCheckClick;
end;

procedure WindowsAuthCheckClick(Sender: TObject);
begin
  SqlUsernameEdit.Enabled := not UseWindowsAuthCheck.Checked;
  SqlPasswordEdit.Enabled := not UseWindowsAuthCheck.Checked;
end;

procedure CreateDatabaseCheckClick(Sender: TObject);
begin
  { This handler can be used for future logic if needed }
  { Currently just a placeholder for the checkbox event }
end;

procedure TestConnectionButtonClick(Sender: TObject);
var
  ConnectionString: string;
  ResultCode: Integer;
  TestScript: string;
  TestDatabase: string;
  SuccessMessage: string;
begin
  { Determine which database to test against }
  if CreateNewDatabaseCheck.Checked then
  begin
    TestDatabase := 'master';
    SuccessMessage := 'SQL Server connection successful! Database will be created during installation.';
  end
  else
  begin
    TestDatabase := DatabaseNameEdit.Text;
    SuccessMessage := 'Database connection successful! Existing database found.';
  end;

  { Build connection string }
  if UseWindowsAuthCheck.Checked then
    ConnectionString := 'Server=' + SqlServerEdit.Text + ';Database=' + TestDatabase + ';Integrated Security=True;TrustServerCertificate=True;'
  else
    ConnectionString := 'Server=' + SqlServerEdit.Text + ';Database=' + TestDatabase + ';User Id=' + SqlUsernameEdit.Text + ';Password=' + SqlPasswordEdit.Text + ';TrustServerCertificate=True;';

  { Create test script }
  TestScript :=
    'try {' + #13#10 +
    '  $conn = New-Object System.Data.SqlClient.SqlConnection' + #13#10 +
    '  $conn.ConnectionString = "' + ConnectionString + '"' + #13#10 +
    '  $conn.Open()' + #13#10 +
    '  $conn.Close()' + #13#10 +
    '  Write-Host "SUCCESS"' + #13#10 +
    '  exit 0' + #13#10 +
    '} catch {' + #13#10 +
    '  Write-Host "ERROR: $_"' + #13#10 +
    '  exit 1' + #13#10 +
    '}';

  SaveStringToFile(ExpandConstant('{tmp}\test-connection.ps1'), TestScript, False);

  { Run test }
  if Exec('powershell.exe', '-ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\test-connection.ps1') + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
      MsgBox(SuccessMessage, mbInformation, MB_OK)
    else
    begin
      if CreateNewDatabaseCheck.Checked then
        MsgBox('SQL Server connection failed. Please check your server name and credentials.', mbError, MB_OK)
      else
        MsgBox('Database connection failed. The database may not exist. Try checking "Create new database" option.', mbError, MB_OK);
    end;
  end
  else
    MsgBox('Failed to test connection. Please ensure PowerShell is available.', mbError, MB_OK);
end;

function GetConnectionString(Param: String): String;
begin
  if UseWindowsAuthCheck.Checked then
    Result := 'Server=' + SqlServerEdit.Text + ';Database=' + DatabaseNameEdit.Text + ';Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True'
  else
    Result := 'Server=' + SqlServerEdit.Text + ';Database=' + DatabaseNameEdit.Text + ';User Id=' + SqlUsernameEdit.Text + ';Password=' + SqlPasswordEdit.Text + ';TrustServerCertificate=True;MultipleActiveResultSets=True';
end;

function ShouldCreateDatabase: Boolean;
begin
  Result := CreateNewDatabaseCheck.Checked;
end;

[Run]
; Update appsettings.json with connection string
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""$json = Get-Content '{app}\API\appsettings.json' | ConvertFrom-Json; $json.ConnectionStrings.DefaultConnection = '{code:GetConnectionString}'; $json | ConvertTo-Json -Depth 10 | Set-Content '{app}\API\appsettings.json'"""; Flags: runhidden; StatusMsg: "Updating database configuration..."

; Create database and run migrations (only if "Create new database" is checked)
Filename: "dotnet.exe"; WorkingDir: "{app}\API"; Parameters: "ComplaintManagement.API.dll"; Flags: runhidden waituntilterminated; StatusMsg: "Creating database and running migrations..."; Check: ShouldCreateDatabase

; Run migrations only (if connecting to existing database)
Filename: "dotnet.exe"; WorkingDir: "{app}\API"; Parameters: "ComplaintManagement.API.dll"; Flags: runhidden waituntilterminated; StatusMsg: "Running database migrations..."; Check: not ShouldCreateDatabase

; Install Windows Service (if selected)
Filename: "sc.exe"; Parameters: "create ComplaintManagementAPI binPath= ""{app}\API\ComplaintManagement.API.exe"" start= auto"; Flags: runhidden; StatusMsg: "Installing Windows Service..."; Tasks: installservice

; Start Windows Service
Filename: "sc.exe"; Parameters: "start ComplaintManagementAPI"; Flags: runhidden; StatusMsg: "Starting Windows Service..."; Tasks: installservice

; Configure IIS (if selected)
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Scripts\Configure-IIS.ps1"""; Flags: runhidden; StatusMsg: "Configuring IIS..."; Tasks: configureiss

; Open browser after installation
Filename: "http://localhost"; Description: "Launch Complaint Management System"; Flags: shellexec postinstall skipifsilent

[UninstallRun]
; Stop and remove Windows Service
Filename: "sc.exe"; Parameters: "stop ComplaintManagementAPI"; Flags: runhidden; RunOnceId: "StopService"
Filename: "sc.exe"; Parameters: "delete ComplaintManagementAPI"; Flags: runhidden; RunOnceId: "DeleteService"

; Remove IIS website
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Remove-Website -Name 'ComplaintManagement' -ErrorAction SilentlyContinue; Remove-WebAppPool -Name 'ComplaintManagement' -ErrorAction SilentlyContinue"""; Flags: runhidden; RunOnceId: "RemoveIIS"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "http://localhost"; IconFilename: "{app}\app-icon.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "http://localhost"; IconFilename: "{app}\app-icon.ico"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
