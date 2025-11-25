# Automated Database Fix via Command Line
# Connects directly to SQL Server and applies fixes

param(
    [string]$ServerInstance = "LAPTOP-NF9BTG7Q\SQLEXPRESS",
    [string]$Database = "ComplaintManagementDB"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Automated Database Fix" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Test SQL Server connection
Write-Host "[1/3] Testing SQL Server connection..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT @@VERSION"
    $result = sqlcmd -S $ServerInstance -d $Database -Q $testQuery -h -1 -W
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  V SQL Server connected successfully" -ForegroundColor Green
    } else {
        throw "SQL Server connection failed"
    }
} catch {
    Write-Host "  X Failed to connect to SQL Server" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure SQL Server is running" -ForegroundColor White
    Write-Host "2. Verify server name: $ServerInstance" -ForegroundColor White
    Write-Host "3. Check database name: $Database" -ForegroundColor White
    exit 1
}

# Step 2: Fix AuthenticationType
Write-Host "[2/3] Fixing AuthenticationType..." -ForegroundColor Yellow

try {
    # Run UPDATE command
    $updateQuery = "UPDATE EmailConfigurations SET AuthenticationType = 1 WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';"
    sqlcmd -S $ServerInstance -d $Database -Q $updateQuery -b | Out-Null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  V AuthenticationType updated successfully" -ForegroundColor Green
    } else {
        Write-Host "  X Update command failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "  X Update failed: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Verify the fix
Write-Host "[3/3] Verifying the fix..." -ForegroundColor Yellow

$verifyQuery = @"
SET NOCOUNT ON;
SELECT
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 0 THEN 'Basic Auth'
        WHEN 1 THEN 'OAuth 2.0'
        ELSE 'INVALID'
    END as AuthType,
    CASE WHEN OAuthClientId IS NOT NULL AND OAuthClientId != '' THEN 'Yes' ELSE 'No' END as HasClientID,
    CASE WHEN OAuthAccessToken IS NOT NULL AND OAuthAccessToken != '' THEN 'Yes' ELSE 'No' END as HasToken,
    IsEnabled
FROM EmailConfigurations
WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';
"@

try {
    $verifyResult = sqlcmd -S $ServerInstance -d $Database -Q $verifyQuery -s "," -W

    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "Configuration Status" -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    $verifyResult | ForEach-Object { Write-Host $_ -ForegroundColor White }
    Write-Host ""

    # Check if OAuth is set
    if ($verifyResult -match "OAuth 2.0") {
        Write-Host "V Database fix successful!" -ForegroundColor Green
        Write-Host "V AuthenticationType is now set to OAuth 2.0" -ForegroundColor Green

        # Check OAuth credentials
        if ($verifyResult -match ",Yes,") {
            Write-Host "V OAuth credentials are configured" -ForegroundColor Green
            if ($verifyResult -match ",Yes,Yes,") {
                Write-Host "V OAuth token is present (already authorized)" -ForegroundColor Green
                Write-Host ""
                Write-Host "Status: FULLY CONFIGURED" -ForegroundColor Green
            } else {
                Write-Host "! OAuth token not present (needs authorization)" -ForegroundColor Yellow
                Write-Host ""
                Write-Host "Next Step: Click 'Authorize Now' in the UI" -ForegroundColor Cyan
            }
        } else {
            Write-Host "! OAuth credentials not configured" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Next Steps:" -ForegroundColor Cyan
            Write-Host "1. Follow Azure AD setup guide" -ForegroundColor White
            Write-Host "2. Enter Client ID, Tenant ID, Secret in UI" -ForegroundColor White
        }
    } else {
        Write-Host "X AuthenticationType is not OAuth 2.0" -ForegroundColor Red
    }

} catch {
    Write-Host "  X Verification failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Database Fix Complete" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database fixes applied successfully!" -ForegroundColor Green
Write-Host "Proceed to UI verification..." -ForegroundColor Cyan
