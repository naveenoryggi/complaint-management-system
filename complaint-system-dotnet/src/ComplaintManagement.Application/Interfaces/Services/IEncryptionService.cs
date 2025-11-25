namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for password encryption and decryption
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Decrypt AES 256-bit encrypted password from Oryggi
    /// </summary>
    /// <param name="encryptedPassword">AES encrypted password</param>
    /// <returns>Plain text password</returns>
    string DecryptPassword(string encryptedPassword);

    /// <summary>
    /// Encrypt plaintext password using AES 256-bit encryption
    /// </summary>
    /// <param name="plaintext">Plain text password</param>
    /// <returns>AES encrypted password</returns>
    string EncryptPassword(string plaintext);

    /// <summary>
    /// Verify plaintext password against encrypted password
    /// </summary>
    /// <param name="plaintext">Plain text password to verify</param>
    /// <param name="encryptedPassword">Stored encrypted password</param>
    /// <returns>True if passwords match</returns>
    bool VerifyPassword(string plaintext, string encryptedPassword);
}
