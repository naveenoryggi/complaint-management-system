# Update-Config.ps1
# Updates appsettings.json with the provided connection string and port configuration

param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigPath,

    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,

    [Parameter(Mandatory=$false)]
    [string]$WebPort = "80",

    [Parameter(Mandatory=$false)]
    [string]$ApiPort = "5000",

    [Parameter(Mandatory=$false)]
    [string]$Hostname = "localhost",

    [Parameter(Mandatory=$false)]
    [switch]$IsIIS = $false
)

try {
    Write-Host "Updating configuration at: $ConfigPath"
    Write-Host "Hostname: $Hostname, Web Port: $WebPort, API Port: $ApiPort"

    $json = Get-Content $ConfigPath -Raw | ConvertFrom-Json

    # Update connection string
    $json.ConnectionStrings.DefaultConnection = $ConnectionString

    # Update Kestrel URL to use configured API port (bind to all interfaces)
    if (-not $json.PSObject.Properties['Kestrel']) {
        $json | Add-Member -MemberType NoteProperty -Name 'Kestrel' -Value @{}
    }
    $json.Kestrel = @{
        Endpoints = @{
            Http = @{
                Url = "http://*:$ApiPort"
            }
        }
    }

    # Build API URL based on hostname
    if ($WebPort -eq "80") {
        $webUrl = "http://$Hostname"
    } else {
        $webUrl = "http://${Hostname}:$WebPort"
    }

    if ($ApiPort -eq "80") {
        $apiUrl = "http://$Hostname"
    } else {
        $apiUrl = "http://${Hostname}:$ApiPort"
    }

    # Update CORS allowed hosts (any port is allowed for these hosts)
    if ($json.PSObject.Properties['Cors']) {
        $allowedHosts = @(
            "localhost",
            "127.0.0.1"
        )
        # Add configured hostname if different from localhost
        if ($Hostname -ne "localhost" -and $Hostname -ne "127.0.0.1") {
            $allowedHosts += $Hostname
        }

        # Ensure Cors object has AllowedHosts property
        if (-not $json.Cors.PSObject.Properties['AllowedHosts']) {
            $json.Cors | Add-Member -MemberType NoteProperty -Name 'AllowedHosts' -Value @()
        }
        # Remove old AllowedOrigins if exists
        if ($json.Cors.PSObject.Properties['AllowedOrigins']) {
            $json.Cors.PSObject.Properties.Remove('AllowedOrigins')
        }
        $json.Cors.AllowedHosts = $allowedHosts | Select-Object -Unique
    }

    # Save updated configuration
    $json | ConvertTo-Json -Depth 32 | Set-Content $ConfigPath -Encoding UTF8

    Write-Host "Configuration updated successfully"

    # Also update the Angular environment if it exists
    $wwwPath = Split-Path -Parent (Split-Path -Parent $ConfigPath)
    $wwwPath = Join-Path $wwwPath "WWW"

    # Create a config.json file for the frontend to read API URL
    $frontendConfigPath = Join-Path $wwwPath "assets\config.json"
    $frontendConfigDir = Split-Path -Parent $frontendConfigPath

    if (-not (Test-Path $frontendConfigDir)) {
        New-Item -ItemType Directory -Path $frontendConfigDir -Force | Out-Null
    }

    # For IIS: API is at /api virtual path, and API routes also use /api prefix = /api/api
    # For Windows Service: API serves frontend at same origin, use relative /api
    if ($IsIIS) {
        # IIS mode: Frontend and API are on same port via IIS
        # API is at /api virtual path, internal routes also have /api prefix
        $frontendApiUrl = "/api/api"
        Write-Host "IIS Mode: Using relative API URL /api/api" -ForegroundColor Cyan
    } else {
        # Windows Service mode: API serves frontend at same origin
        # Use relative URL to avoid CORS issues
        $frontendApiUrl = "/api"
        Write-Host "Service Mode: Using relative API URL /api" -ForegroundColor Cyan
    }

    $frontendConfig = @{
        apiUrl = $frontendApiUrl
        baseUrl = $webUrl
        hostname = $Hostname
        webPort = $WebPort
        apiPort = $ApiPort
        isIIS = $IsIIS.IsPresent
    }
    $frontendConfig | ConvertTo-Json | Set-Content $frontendConfigPath -Encoding UTF8
    Write-Host "Frontend config created at: $frontendConfigPath"
    Write-Host "API URL: $frontendApiUrl"
    Write-Host "Web URL: $webUrl"

    exit 0
} catch {
    Write-Host "Error: $($_.Exception.Message)"
    exit 1
}
