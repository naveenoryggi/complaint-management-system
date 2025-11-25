# Reset admin password using AES encryption matching the API
# Key: "ComplaintManagement12345678" (padded to 32 bytes)
# IV: "ComplaintMgmt_IV" (exactly 16 bytes)

$connectionString = "Server=LAPTOP-NF9BTG7Q\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60"

# AES Encryption function matching the C# implementation
function Encrypt-Password {
    param([string]$plaintext)

    $key = "ComplaintManagement12345678"
    $iv = "ComplaintMgmt_IV"

    # Create 32-byte key and 16-byte IV
    $keyBytes = [System.Text.Encoding]::UTF8.GetBytes($key)
    $ivBytes = [System.Text.Encoding]::UTF8.GetBytes($iv)

    $keyArray = New-Object byte[] 32
    $ivArray = New-Object byte[] 16

    [Array]::Copy($keyBytes, $keyArray, [Math]::Min($keyBytes.Length, 32))
    [Array]::Copy($ivBytes, $ivArray, [Math]::Min($ivBytes.Length, 16))

    # Create AES encryptor
    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Key = $keyArray
    $aes.IV = $ivArray
    $aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7

    $encryptor = $aes.CreateEncryptor()
    $plaintextBytes = [System.Text.Encoding]::UTF8.GetBytes($plaintext)
    $ciphertextBytes = $encryptor.TransformFinalBlock($plaintextBytes, 0, $plaintextBytes.Length)

    $aes.Dispose()

    return [Convert]::ToBase64String($ciphertextBytes)
}

try {
    Write-Host "Encrypting password 'Admin@123'..." -ForegroundColor Yellow
    $encryptedPassword = Encrypt-Password "Admin@123"
    Write-Host "Encrypted password: $encryptedPassword" -ForegroundColor Cyan

    Write-Host "`nConnecting to database..." -ForegroundColor Yellow
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected successfully!" -ForegroundColor Green

    # Update admin user password
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
UPDATE Users
SET PasswordHash = @PasswordHash,
    IsDeleted = 0,
    DeletedAt = NULL,
    IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE Email = 'admin@complaintmanagement.com'

SELECT @@ROWCOUNT as RowsAffected
"@
    $cmd.Parameters.AddWithValue("@PasswordHash", $encryptedPassword) | Out-Null

    $reader = $cmd.ExecuteReader()
    $rowsAffected = 0
    if ($reader.Read()) {
        $rowsAffected = $reader["RowsAffected"]
    }
    $reader.Close()

    if ($rowsAffected -gt 0) {
        Write-Host "`nSUCCESS! Admin password reset successfully!" -ForegroundColor Green
        Write-Host "Email: admin@complaintmanagement.com" -ForegroundColor Cyan
        Write-Host "Password: Admin@123" -ForegroundColor Cyan
        Write-Host "Rows updated: $rowsAffected" -ForegroundColor Green
    } else {
        Write-Host "`nWARNING: No admin user found to update" -ForegroundColor Yellow
    }

    $connection.Close()
}
catch {
    Write-Host "`nERROR: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
    exit 1
}
