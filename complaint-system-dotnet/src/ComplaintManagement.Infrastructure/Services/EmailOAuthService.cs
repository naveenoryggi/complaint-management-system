using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for handling Office 365 OAuth 2.0 authentication
/// Manages token acquisition, refresh, and authorization URLs
/// </summary>
public class EmailOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEncryptionService _encryptionService;
    private IConfidentialClientApplication? _app;

    public EmailOAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IEncryptionService encryptionService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Initialize the confidential client application
    /// This is done lazily to avoid errors if Azure AD is not configured
    /// </summary>
    private IConfidentialClientApplication GetConfidentialClient()
    {
        if (_app != null)
            return _app;

        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var tenantId = _configuration["AzureAd:TenantId"];
        var redirectUri = _configuration["AzureAd:RedirectUri"] ?? "http://localhost:5000/api/oauth/callback";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException(
                "Azure AD configuration is missing. Please configure AzureAd:ClientId, AzureAd:ClientSecret, and AzureAd:TenantId in appsettings.json");
        }

        _app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .WithRedirectUri(redirectUri)
            .Build();

        return _app;
    }

    /// <summary>
    /// Get a new access token using silent authentication with cached tokens
    /// MSAL handles refresh token logic internally
    /// </summary>
    /// <param name="userEmail">The email address of the user</param>
    /// <returns>The new access token</returns>
    [Obsolete("Use RefreshAccessTokenAsync(EmailConfiguration) instead. This method relies on MSAL cache which may not contain tokens from browser OAuth flow.")]
    public async Task<OAuthTokenResult> RefreshAccessTokenAsync(string userEmail)
    {
        try
        {
            var client = GetConfidentialClient();

            var scopes = new[] {
                "https://outlook.office365.com/IMAP.AccessAsUser.All",
                "https://outlook.office365.com/SMTP.Send",
                "offline_access"
            };

            // Get the account from cache
            var accounts = await client.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a => a.Username.Equals(userEmail, StringComparison.OrdinalIgnoreCase));

            if (account == null)
            {
                throw new InvalidOperationException(
                    $"No cached account found for {userEmail}. User needs to re-authorize the application.");
            }

            // Try to acquire token silently (MSAL will use refresh token internally if needed)
            var result = await client.AcquireTokenSilent(scopes, account)
                .ExecuteAsync();

            return new OAuthTokenResult
            {
                AccessToken = result.AccessToken,
                RefreshToken = string.Empty, // Refresh tokens are managed by MSAL internally
                ExpiresOn = result.ExpiresOn.UtcDateTime
            };
        }
        catch (MsalUiRequiredException)
        {
            // Token expired and cannot be refreshed silently - user needs to re-authorize
            throw new InvalidOperationException(
                "Token has expired and requires user interaction. Please re-authorize the email configuration.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to refresh access token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Refresh access token using refresh token from database (HTTP-based, not MSAL cache)
    /// This method is used by the email polling service to refresh tokens obtained via browser OAuth flow
    /// </summary>
    /// <param name="config">Email configuration with refresh token</param>
    /// <returns>New OAuth token result</returns>
    public async Task<OAuthTokenResult> RefreshAccessTokenAsync(EmailConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.OAuthRefreshToken))
        {
            throw new InvalidOperationException("No refresh token available. Please re-authorize the email configuration.");
        }

        var isOffice365 = config.ImapHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.ImapHost?.Contains("imap.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // SECURITY: Decrypt OAuth client secret before use
            var clientSecret = config.OAuthClientSecret ?? _configuration["AzureAd:ClientSecret"];
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("OAuth Client Secret not configured");
            }

            string decryptedClientSecret;
            try
            {
                // Try to decrypt - if it fails, it might be in plaintext (migration scenario)
                decryptedClientSecret = _encryptionService.DecryptPassword(clientSecret);
            }
            catch
            {
                // If decryption fails, assume it's plaintext (for backward compatibility during migration)
                decryptedClientSecret = clientSecret;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.OAuthClientId ?? _configuration["AzureAd:ClientId"] ?? throw new InvalidOperationException("OAuth Client ID not configured"),
                ["client_secret"] = decryptedClientSecret,
                ["refresh_token"] = config.OAuthRefreshToken,
                ["grant_type"] = "refresh_token"
            };

            string tokenEndpoint;
            if (isOffice365)
            {
                var tenantId = config.OAuthTenantId ?? _configuration["AzureAd:TenantId"] ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }
            else if (isGmail)
            {
                tokenEndpoint = "https://oauth2.googleapis.com/token";
            }
            else
            {
                // Fallback to Office 365
                var tenantId = config.OAuthTenantId ?? _configuration["AzureAd:TenantId"] ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token refresh failed: {response.StatusCode} - {responseContent}");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var accessToken = tokenData.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token not found in response");

            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

            // Microsoft may return a new refresh token, or keep the old one
            var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) && rt.ValueKind != JsonValueKind.Null
                ? rt.GetString()
                : config.OAuthRefreshToken; // Keep existing refresh token if not provided

            return new OAuthTokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken ?? config.OAuthRefreshToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(expiresIn)
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error during token refresh: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse token response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to refresh access token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Refresh SMTP-specific access token using separate SMTP OAuth credentials
    /// Used when UseSeparateSmtpAccount = true and SmtpAuthenticationType = OAuth2
    /// </summary>
    /// <param name="config">Email configuration with separate SMTP OAuth refresh token</param>
    /// <returns>New OAuth token result for SMTP</returns>
    public async Task<OAuthTokenResult> RefreshSmtpAccessTokenAsync(EmailConfiguration config)
    {
        if (!config.UseSeparateSmtpAccount)
        {
            throw new InvalidOperationException("This method should only be called when UseSeparateSmtpAccount is true");
        }

        if (string.IsNullOrWhiteSpace(config.SmtpSeparateOAuthRefreshToken))
        {
            throw new InvalidOperationException("No SMTP refresh token available. Please re-authorize the SMTP email configuration.");
        }

        var isOffice365 = config.SmtpHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.SmtpHost?.Contains("smtp.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // SECURITY: Decrypt separate SMTP OAuth client secret before use
            var clientSecret = config.SmtpSeparateOAuthClientSecret ?? _configuration["AzureAd:ClientSecret"];
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("SMTP OAuth Client Secret not configured");
            }

            string decryptedClientSecret;
            try
            {
                // Try to decrypt - if it fails, it might be in plaintext (migration scenario)
                decryptedClientSecret = _encryptionService.DecryptPassword(clientSecret);
            }
            catch
            {
                // If decryption fails, assume it's plaintext (for backward compatibility during migration)
                decryptedClientSecret = clientSecret;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.SmtpSeparateOAuthClientId ?? _configuration["AzureAd:ClientId"] ?? throw new InvalidOperationException("SMTP OAuth Client ID not configured"),
                ["client_secret"] = decryptedClientSecret,
                ["refresh_token"] = config.SmtpSeparateOAuthRefreshToken,
                ["grant_type"] = "refresh_token"
            };

            string tokenEndpoint;
            if (isOffice365)
            {
                var tenantId = config.SmtpSeparateOAuthTenantId ?? _configuration["AzureAd:TenantId"] ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }
            else if (isGmail)
            {
                tokenEndpoint = "https://oauth2.googleapis.com/token";
            }
            else
            {
                // Fallback to Office 365
                var tenantId = config.SmtpSeparateOAuthTenantId ?? _configuration["AzureAd:TenantId"] ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"SMTP token refresh failed: {response.StatusCode} - {responseContent}");
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var accessToken = tokenData.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("SMTP access token not found in response");

            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

            // Microsoft may return a new refresh token, or keep the old one
            var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) && rt.ValueKind != JsonValueKind.Null
                ? rt.GetString()
                : config.SmtpSeparateOAuthRefreshToken; // Keep existing refresh token if not provided

            return new OAuthTokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken ?? config.SmtpSeparateOAuthRefreshToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(expiresIn)
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error during SMTP token refresh: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse SMTP token response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to refresh SMTP access token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generate the authorization URL for initiating OAuth flow
    /// </summary>
    /// <param name="state">State parameter (typically the email config ID)</param>
    /// <returns>The authorization URL to redirect the user to</returns>
    public string GetAuthorizationUrl(string state)
    {
        var scopes = new[] {
            "https://outlook.office365.com/IMAP.AccessAsUser.All",
            "https://outlook.office365.com/SMTP.Send",
            "offline_access"
        };

        var client = GetConfidentialClient();

        // Build authorization URL with state parameter
        var authUrl = client.GetAuthorizationRequestUrl(scopes)
            .WithExtraQueryParameters($"state={state}")
            .ExecuteAsync()
            .GetAwaiter()
            .GetResult();

        return authUrl.ToString();
    }

    /// <summary>
    /// Exchange an authorization code for access tokens
    /// MSAL handles refresh tokens internally - they're not exposed in confidential client flow
    /// </summary>
    /// <param name="code">The authorization code from the callback</param>
    /// <returns>Token result with access token and expiration</returns>
    public async Task<OAuthTokenResult> ExchangeCodeForTokensAsync(string code)
    {
        try
        {
            var client = GetConfidentialClient();

            var scopes = new[] {
                "https://outlook.office365.com/IMAP.AccessAsUser.All",
                "https://outlook.office365.com/SMTP.Send",
                "offline_access"
            };

            // Exchange authorization code for access token
            var result = await client.AcquireTokenByAuthorizationCode(scopes, code)
                .ExecuteAsync();

            return new OAuthTokenResult
            {
                AccessToken = result.AccessToken,
                RefreshToken = string.Empty, // Refresh tokens are managed by MSAL internally
                ExpiresOn = result.ExpiresOn.UtcDateTime
            };
        }
        catch (MsalServiceException ex)
        {
            throw new InvalidOperationException(
                $"Failed to exchange authorization code: {ex.Message}. " +
                $"Error Code: {ex.ErrorCode}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to exchange authorization code: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if a token needs to be refreshed (within 5 minutes of expiration)
    /// </summary>
    public bool TokenNeedsRefresh(DateTime? expiresAt)
    {
        if (!expiresAt.HasValue)
            return true;

        return expiresAt.Value <= DateTime.UtcNow.AddMinutes(5);
    }
}

/// <summary>
/// Result of OAuth token operations
/// </summary>
public class OAuthTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
}
