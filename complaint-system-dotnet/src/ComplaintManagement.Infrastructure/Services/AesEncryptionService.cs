using System.Security.Cryptography;
using System.Text;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// AES 256-bit encryption service for password management
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<AesEncryptionService> _logger;

    public AesEncryptionService(IConfiguration configuration, ILogger<AesEncryptionService> logger)
    {
        _logger = logger;

        // Using default keys for local password encryption
        // This is for local password management only - NOT syncing from Oryggi
        // Future: Will integrate with Oryggi password encryption when method is confirmed

        // Generate a fixed 32-byte key and 16-byte IV for consistent local encryption
        var defaultKey = "ComplaintManagement12345678"; // 28 chars + padding to 32
        var defaultIV = "ComplaintMgmt_IV"; // 16 chars exactly

        _key = new byte[32];
        _iv = new byte[16];

        var keyBytes = Encoding.UTF8.GetBytes(defaultKey);
        var ivBytes = Encoding.UTF8.GetBytes(defaultIV);

        Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, 32));
        Array.Copy(ivBytes, _iv, Math.Min(ivBytes.Length, 16));

        _logger.LogInformation("AES encryption initialized with default keys for local password management");
    }

    public string EncryptPassword(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(plaintext));
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

            return Convert.ToBase64String(ciphertextBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt password");
            throw new InvalidOperationException("Password encryption failed", ex);
        }
    }

    public string DecryptPassword(string encryptedPassword)
    {
        if (string.IsNullOrEmpty(encryptedPassword))
        {
            throw new ArgumentException("Encrypted password cannot be null or empty", nameof(encryptedPassword));
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var ciphertextBytes = Convert.FromBase64String(encryptedPassword);
            var plaintextBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid Base64 format for encrypted password");
            throw new InvalidOperationException("Invalid encrypted password format", ex);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Failed to decrypt password - possible key mismatch");
            throw new InvalidOperationException("Password decryption failed", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password decryption");
            throw new InvalidOperationException("Password decryption failed", ex);
        }
    }

    public bool VerifyPassword(string plaintext, string encryptedPassword)
    {
        if (string.IsNullOrEmpty(plaintext) || string.IsNullOrEmpty(encryptedPassword))
        {
            return false;
        }

        try
        {
            var decryptedPassword = DecryptPassword(encryptedPassword);
            return plaintext == decryptedPassword;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password verification failed");
            return false;
        }
    }
}
