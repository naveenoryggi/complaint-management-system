# Test Cleanup Functionality
# This script tests the sync history and cleanup endpoints

$baseUrl = "http://localhost:5058"
$tenantId = "18910DFB-1F39-46F8-979C-D8B845A5388D"

# Login first to get JWT token
Write-Host "=== Authenticating ===" -ForegroundColor Cyan
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    Write-Host "✅ Authentication successful" -ForegroundColor Green
} catch {
    Write-Host "❌ Login failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Testing Sync History Endpoint ===" -ForegroundColor Cyan
Write-Host "GET $baseUrl/api/OryggiSync/history/$tenantId?count=10" -ForegroundColor Yellow

try {
    $historyResponse = Invoke-RestMethod -Uri "$baseUrl/api/OryggiSync/history/$tenantId?count=10" -Method Get -Headers $headers

    Write-Host "`nSuccess: $($historyResponse.success)" -ForegroundColor Green
    Write-Host "Total Syncs: $($historyResponse.data.Count)" -ForegroundColor Green

    if ($historyResponse.data.Count -gt 0) {
        Write-Host "`n=== Sync Records ===" -ForegroundColor Cyan
        $historyResponse.data | ForEach-Object -Begin { $i = 1 } -Process {
            Write-Host "`n[$i] Sync Log ID (GUID being sent to frontend): $($_.syncLogId)" -ForegroundColor Magenta
            Write-Host "    Type: $($_.syncType)"
            Write-Host "    Status: $($_.status)"
            Write-Host "    Started: $($_.startedAt)"
            Write-Host "    Duration: $($_.duration) seconds"

            if ($_.status -eq "IN_PROGRESS") {
                Write-Host "    >>> THIS SYNC CAN BE CLEANED UP <<<" -ForegroundColor Yellow

                # Store the first IN_PROGRESS sync ID for testing
                if (-not $script:testSyncId) {
                    $script:testSyncId = $_.syncLogId
                }
            }
            $i++
        }

        # If we found an IN_PROGRESS sync, test cleanup
        if ($script:testSyncId) {
            Write-Host "`n=== Testing Cleanup Endpoint ===" -ForegroundColor Cyan
            Write-Host "Testing cleanup for sync: $script:testSyncId" -ForegroundColor Yellow
            Write-Host "POST $baseUrl/api/OryggiSync/diagnostics/cleanup-sync/$script:testSyncId" -ForegroundColor Yellow
            Write-Host "Body: { reason: 'Testing cleanup functionality' }" -ForegroundColor Yellow

            $cleanupBody = @{
                reason = "Testing cleanup functionality"
            } | ConvertTo-Json

            try {
                $cleanupResponse = Invoke-RestMethod -Uri "$baseUrl/api/OryggiSync/diagnostics/cleanup-sync/$script:testSyncId" -Method Post -Body $cleanupBody -ContentType "application/json" -Headers $headers

                Write-Host "`n✅ Cleanup Response:" -ForegroundColor Green
                Write-Host "   Success: $($cleanupResponse.success)"
                Write-Host "   Message: $($cleanupResponse.message)"

                # Verify the sync was marked as FAILED
                Write-Host "`n=== Verifying Cleanup Result ===" -ForegroundColor Cyan
                Start-Sleep -Seconds 1

                $verifyResponse = Invoke-RestMethod -Uri "$baseUrl/api/OryggiSync/history/$tenantId?count=10" -Method Get -Headers $headers
                $updatedSync = $verifyResponse.data | Where-Object { $_.syncLogId -eq $script:testSyncId }

                if ($updatedSync) {
                    Write-Host "   Sync Status After Cleanup: $($updatedSync.status)" -ForegroundColor $(if ($updatedSync.status -eq "FAILED") { "Green" } else { "Red" })
                    if ($updatedSync.status -eq "FAILED") {
                        Write-Host "   ✅ CLEANUP SUCCESSFUL - Status changed to FAILED" -ForegroundColor Green
                    } else {
                        Write-Host "   ❌ CLEANUP FAILED - Status still $($updatedSync.status)" -ForegroundColor Red
                    }
                }
            } catch {
                Write-Host "`n❌ Cleanup Request Failed:" -ForegroundColor Red
                Write-Host "   Error: $_"
                Write-Host "   StatusCode: $($_.Exception.Response.StatusCode.value__)"
            }
        } else {
            Write-Host "`n⚠️ No IN_PROGRESS syncs found to test cleanup" -ForegroundColor Yellow
            Write-Host "All syncs are either COMPLETED or FAILED" -ForegroundColor Yellow
        }
    } else {
        Write-Host "`n⚠️ No sync records found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "`n❌ Request Failed:" -ForegroundColor Red
    Write-Host "   Error: $_"
    if ($_.Exception.Response) {
        Write-Host "   StatusCode: $($_.Exception.Response.StatusCode.value__)"
    }
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
