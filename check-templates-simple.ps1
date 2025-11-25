$token = Get-Content .working-token -Raw -ErrorAction SilentlyContinue
$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "Checking templates..."

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/communication-templates" -Headers $headers

    Write-Host "Total templates: $($response.data.Count)"

    if ($response.data.Count -gt 0) {
        $response.data | Select-Object name, code | Format-Table

        $autoAck = $response.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" }
        if ($autoAck) {
            Write-Host "AUTO_ACK template ID: $($autoAck.id)"
            $autoAck.id | Out-File -FilePath ".template-id.txt" -NoNewline
        }
    } else {
        Write-Host "No templates found - need to insert templates"
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
