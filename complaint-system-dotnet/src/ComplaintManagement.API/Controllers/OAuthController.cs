using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for handling OAuth 2.0 authorization flows (Office 365, Gmail, etc.)
/// </summary>
[ApiController]
[Route("api/oauth")]
public class OAuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ComplaintDbContext _context;
    private readonly ILogger<OAuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEncryptionService _encryptionService;

    public OAuthController(
        IUnitOfWork unitOfWork,
        ComplaintDbContext context,
        ILogger<OAuthController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Initiates OAuth 2.0 authorization flow
    /// Redirects user to Microsoft/Google login page
    /// </summary>
    /// <remarks>
    /// This endpoint does not require authentication to allow browser redirects.
    /// Security is maintained through:
    /// - State parameter prevents CSRF attacks
    /// - Only users with access to config ID can initiate OAuth
    /// - Microsoft OAuth provides additional security layer
    /// </remarks>
    [HttpGet("authorize/{configId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authorize(Guid configId)
    {
        try
        {
            // Get configuration (no company verification needed - state parameter provides security)
            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(configId);
            if (config == null)
            {
                _logger.LogWarning("OAuth authorization attempted for non-existent config {ConfigId}", configId);
                return NotFound("Email configuration not found");
            }

            // Validate OAuth configuration
            if (config.AuthenticationType != EmailAuthenticationType.OAuth2)
            {
                _logger.LogWarning("OAuth authorization attempted for non-OAuth config {ConfigId}", configId);
                return BadRequest("Configuration is not set up for OAuth 2.0");
            }

            if (string.IsNullOrWhiteSpace(config.OAuthClientId))
            {
                _logger.LogWarning("OAuth authorization attempted without Client ID for config {ConfigId}", configId);
                return BadRequest("OAuth Client ID is required");
            }

            // Generate secure state parameter (prevents CSRF)
            var state = GenerateSecureState(configId);

            // Build authorization URL based on provider
            var authUrl = BuildAuthorizationUrl(config, state);

            _logger.LogInformation("Redirecting user to OAuth provider for config {ConfigId}", configId);

            // Redirect user to OAuth provider
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating OAuth authorization for config {ConfigId}", configId);
            return StatusCode(500, "An error occurred while initiating OAuth authorization");
        }
    }

    /// <summary>
    /// OAuth 2.0 callback endpoint
    /// Receives authorization code from Microsoft/Google and exchanges it for tokens
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous] // Callback comes from OAuth provider, not authenticated user
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
    {
        try
        {
            // Handle OAuth error responses
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("OAuth callback received error: {Error}", error);
                return Redirect($"{GetFrontendUrl()}/admin/communication-settings?oauth=error&message={Uri.EscapeDataString(error)}");
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                _logger.LogWarning("OAuth callback missing code or state parameter");
                return BadRequest("Invalid OAuth callback - missing parameters");
            }

            // Validate and extract configId from state
            var configId = ValidateAndExtractState(state);
            if (configId == Guid.Empty)
            {
                _logger.LogWarning("OAuth callback with invalid state parameter");
                return BadRequest("Invalid state parameter");
            }

            // Get configuration
            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(configId);
            if (config == null)
            {
                _logger.LogWarning("OAuth callback for non-existent config {ConfigId}", configId);
                return BadRequest("Email configuration not found");
            }

            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeCodeForTokens(config, code);

            if (!tokenResponse.IsSuccess)
            {
                _logger.LogError("Token exchange failed for config {ConfigId}: {Error}", configId, tokenResponse.Error);
                return Redirect($"{GetFrontendUrl()}/admin/communication-settings?oauth=error&message={Uri.EscapeDataString(tokenResponse.Error)}");
            }

            // Update configuration with tokens
            config.OAuthAccessToken = tokenResponse.AccessToken;
            config.OAuthRefreshToken = tokenResponse.RefreshToken;
            config.OAuthTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            config.IsEnabled = true; // Auto-enable after successful OAuth
            config.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<EmailConfiguration>().Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("OAuth tokens successfully stored for config {ConfigId}, expires at {ExpiresAt}",
                configId, config.OAuthTokenExpiresAt);

            // Redirect back to frontend with success
            return Redirect($"{GetFrontendUrl()}/admin/communication-settings?oauth=success&configId={configId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback");
            return Redirect($"{GetFrontendUrl()}/admin/communication-settings?oauth=error&message={Uri.EscapeDataString("An unexpected error occurred")}");
        }
    }

    /// <summary>
    /// Manually refresh OAuth 2.0 tokens (also called automatically by background service)
    /// </summary>
    [HttpPost("refresh/{configId}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<TokenRefreshResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(Guid configId)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // Get configuration
            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(configId);
            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to refresh token for config from different company");
                return Forbid();
            }

            // Validate OAuth configuration
            if (config.AuthenticationType != EmailAuthenticationType.OAuth2)
            {
                return BadRequest(Result.Failure("Configuration is not set up for OAuth 2.0"));
            }

            if (string.IsNullOrWhiteSpace(config.OAuthRefreshToken))
            {
                return BadRequest(Result.Failure("No refresh token available. Please re-authorize."));
            }

            // Refresh tokens
            var tokenResponse = await RefreshAccessToken(config);

            if (!tokenResponse.IsSuccess)
            {
                _logger.LogError("Token refresh failed for config {ConfigId}: {Error}", configId, tokenResponse.Error);
                return Ok(Result<TokenRefreshResult>.Failure(tokenResponse.Error));
            }

            // Update configuration with new tokens
            config.OAuthAccessToken = tokenResponse.AccessToken;
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                config.OAuthRefreshToken = tokenResponse.RefreshToken;
            }
            config.OAuthTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            config.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<EmailConfiguration>().Update(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("OAuth tokens successfully refreshed for config {ConfigId}, expires at {ExpiresAt}",
                configId, config.OAuthTokenExpiresAt);

            var result = new TokenRefreshResult
            {
                ExpiresAt = config.OAuthTokenExpiresAt.Value,
                ExpiresIn = tokenResponse.ExpiresIn
            };

            return Ok(Result<TokenRefreshResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing OAuth token for config {ConfigId}", configId);
            return StatusCode(500, Result.Failure("An error occurred while refreshing the OAuth token"));
        }
    }

    #region Email Server Settings OAuth Endpoints

    /// <summary>
    /// Initiates OAuth 2.0 authorization flow for Email Server Settings (SMTP)
    /// Redirects user to Microsoft/Google login page
    /// </summary>
    [HttpGet("authorize-smtp/{settingsId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AuthorizeSmtp(Guid settingsId)
    {
        try
        {
            // Get email server settings
            var settings = await _context.EmailServerSettings.FindAsync(settingsId);
            if (settings == null || settings.IsDeleted)
            {
                _logger.LogWarning("OAuth authorization attempted for non-existent SMTP settings {SettingsId}", settingsId);
                return NotFound("Email server settings not found");
            }

            // Validate OAuth configuration
            if (settings.AuthenticationType != EmailAuthenticationType.OAuth2)
            {
                _logger.LogWarning("OAuth authorization attempted for non-OAuth SMTP settings {SettingsId}", settingsId);
                return BadRequest("Email server settings not configured for OAuth 2.0");
            }

            if (string.IsNullOrWhiteSpace(settings.OAuthClientId))
            {
                _logger.LogWarning("OAuth authorization attempted without Client ID for SMTP settings {SettingsId}", settingsId);
                return BadRequest("OAuth Client ID is required");
            }

            if (string.IsNullOrWhiteSpace(settings.Username))
            {
                _logger.LogWarning("OAuth authorization attempted without email address for SMTP settings {SettingsId}", settingsId);
                return BadRequest("Email address (Username) is required for OAuth authorization");
            }

            // Generate secure state parameter (prevents CSRF) with type=smtp indicator
            var state = GenerateSecureStateForSmtp(settingsId);

            // Build authorization URL based on provider
            var authUrl = BuildAuthorizationUrlForSmtp(settings, state);

            _logger.LogInformation("Redirecting user to OAuth provider for SMTP settings {SettingsId}", settingsId);

            // Redirect user to OAuth provider
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating OAuth authorization for SMTP settings {SettingsId}", settingsId);
            return StatusCode(500, "An error occurred while initiating OAuth authorization");
        }
    }

    /// <summary>
    /// OAuth 2.0 callback endpoint for Email Server Settings
    /// Receives authorization code from Microsoft/Google and exchanges it for tokens
    /// </summary>
    [HttpGet("callback-smtp")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CallbackSmtp([FromQuery] string code, [FromQuery] string state, [FromQuery] string? error)
    {
        try
        {
            // Handle OAuth error responses
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("OAuth callback received error for SMTP: {Error}", error);
                return Redirect($"{GetFrontendUrl()}/admin/email-settings?oauth=error&message={Uri.EscapeDataString(error)}");
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                _logger.LogWarning("OAuth callback missing code or state parameter for SMTP");
                return BadRequest("Invalid OAuth callback - missing parameters");
            }

            // Validate and extract settingsId from state
            var settingsId = ValidateAndExtractStateForSmtp(state);
            if (settingsId == Guid.Empty)
            {
                _logger.LogWarning("OAuth callback with invalid state parameter for SMTP");
                return BadRequest("Invalid state parameter");
            }

            // Get email server settings
            var settings = await _context.EmailServerSettings.FindAsync(settingsId);
            if (settings == null || settings.IsDeleted)
            {
                _logger.LogWarning("OAuth callback for non-existent SMTP settings {SettingsId}", settingsId);
                return BadRequest("Email server settings not found");
            }

            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeCodeForTokensSmtp(settings, code);

            if (!tokenResponse.IsSuccess)
            {
                _logger.LogError("Token exchange failed for SMTP settings {SettingsId}: {Error}", settingsId, tokenResponse.Error);
                return Redirect($"{GetFrontendUrl()}/admin/email-settings?oauth=error&message={Uri.EscapeDataString(tokenResponse.Error)}");
            }

            // Update settings with tokens
            settings.OAuthAccessToken = tokenResponse.AccessToken;
            settings.OAuthRefreshToken = tokenResponse.RefreshToken;
            settings.OAuthTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            settings.IsActive = true; // Auto-enable after successful OAuth
            settings.UpdatedAt = DateTime.UtcNow;

            _context.EmailServerSettings.Update(settings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("OAuth tokens successfully stored for SMTP settings {SettingsId}, expires at {ExpiresAt}",
                settingsId, settings.OAuthTokenExpiresAt);

            // Redirect back to frontend with success
            return Redirect($"{GetFrontendUrl()}/admin/email-settings?oauth=success&settingsId={settingsId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback for SMTP");
            return Redirect($"{GetFrontendUrl()}/admin/email-settings?oauth=error&message={Uri.EscapeDataString("An unexpected error occurred")}");
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Builds OAuth authorization URL based on email provider
    /// </summary>
    private string BuildAuthorizationUrl(EmailConfiguration config, string state)
    {
        var callbackUrl = GetCallbackUrl();
        var clientId = config.OAuthClientId;

        // Determine provider based on IMAP host
        var isOffice365 = config.ImapHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.ImapHost?.Contains("imap.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        if (isOffice365)
        {
            // Microsoft/Office 365 OAuth
            var tenantId = config.OAuthTenantId ?? "common";
            var scope = Uri.EscapeDataString("https://outlook.office365.com/IMAP.AccessAsUser.All https://outlook.office365.com/SMTP.Send offline_access");

            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"response_mode=query&" +
                   $"scope={scope}&" +
                   $"state={state}";
        }
        else if (isGmail)
        {
            // Google/Gmail OAuth
            var scope = Uri.EscapeDataString("https://mail.google.com/");

            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"scope={scope}&" +
                   $"access_type=offline&" +
                   $"prompt=consent&" +
                   $"state={state}";
        }
        else
        {
            // Fallback to Office 365 format
            _logger.LogWarning("Unknown OAuth provider for host {Host}, using Office 365 format", config.ImapHost);
            var tenantId = config.OAuthTenantId ?? "common";
            var scope = Uri.EscapeDataString("https://outlook.office365.com/IMAP.AccessAsUser.All https://outlook.office365.com/SMTP.Send offline_access");

            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"response_mode=query&" +
                   $"scope={scope}&" +
                   $"state={state}";
        }
    }

    /// <summary>
    /// Exchanges authorization code for access/refresh tokens
    /// </summary>
    private async Task<TokenResponse> ExchangeCodeForTokens(EmailConfiguration config, string code)
    {
        var callbackUrl = GetCallbackUrl();
        var isOffice365 = config.ImapHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.ImapHost?.Contains("imap.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // SECURITY: Decrypt OAuth client secret before sending to token endpoint
            var decryptedSecret = !string.IsNullOrEmpty(config.OAuthClientSecret)
                ? _encryptionService.DecryptPassword(config.OAuthClientSecret)
                : config.OAuthClientSecret;

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.OAuthClientId!,
                ["client_secret"] = decryptedSecret!,
                ["code"] = code,
                ["redirect_uri"] = callbackUrl,
                ["grant_type"] = "authorization_code"
            };

            string tokenEndpoint;
            if (isOffice365)
            {
                var tenantId = config.OAuthTenantId ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }
            else if (isGmail)
            {
                tokenEndpoint = "https://oauth2.googleapis.com/token";
            }
            else
            {
                // Fallback to Office 365
                var tenantId = config.OAuthTenantId ?? "common";
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
                _logger.LogError("Token exchange failed: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return new TokenResponse
                {
                    IsSuccess = false,
                    Error = $"Token exchange failed: {response.StatusCode}"
                };
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return new TokenResponse
            {
                IsSuccess = true,
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            return new TokenResponse
            {
                IsSuccess = false,
                Error = "An error occurred during token exchange"
            };
        }
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    private async Task<TokenResponse> RefreshAccessToken(EmailConfiguration config)
    {
        var isOffice365 = config.ImapHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.ImapHost?.Contains("imap.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // SECURITY: Decrypt OAuth client secret before sending to token endpoint
            var decryptedSecret = !string.IsNullOrEmpty(config.OAuthClientSecret)
                ? _encryptionService.DecryptPassword(config.OAuthClientSecret)
                : config.OAuthClientSecret;

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.OAuthClientId!,
                ["client_secret"] = decryptedSecret!,
                ["refresh_token"] = config.OAuthRefreshToken!,
                ["grant_type"] = "refresh_token"
            };

            string tokenEndpoint;
            if (isOffice365)
            {
                var tenantId = config.OAuthTenantId ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }
            else if (isGmail)
            {
                tokenEndpoint = "https://oauth2.googleapis.com/token";
            }
            else
            {
                var tenantId = config.OAuthTenantId ?? "common";
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
                _logger.LogError("Token refresh failed: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return new TokenResponse
                {
                    IsSuccess = false,
                    Error = $"Token refresh failed: {response.StatusCode}"
                };
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return new TokenResponse
            {
                IsSuccess = true,
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return new TokenResponse
            {
                IsSuccess = false,
                Error = "An error occurred during token refresh"
            };
        }
    }

    /// <summary>
    /// Generates secure state parameter with encrypted configId
    /// </summary>
    private string GenerateSecureState(Guid configId)
    {
        // Create state: configId|timestamp|nonce
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var state = $"{configId}|{timestamp}|{nonce}";

        // Base64 encode for URL safety
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(state));
    }

    /// <summary>
    /// Validates state parameter and extracts configId
    /// </summary>
    private Guid ValidateAndExtractState(string state)
    {
        try
        {
            // Decode state
            var decodedState = Encoding.UTF8.GetString(Convert.FromBase64String(state));
            var parts = decodedState.Split('|');

            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid state format");
                return Guid.Empty;
            }

            // Extract configId
            if (!Guid.TryParse(parts[0], out Guid configId))
            {
                _logger.LogWarning("Invalid configId in state");
                return Guid.Empty;
            }

            // Validate timestamp (reject if older than 10 minutes)
            if (long.TryParse(parts[1], out long timestamp))
            {
                var stateAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                if (stateAge > 600) // 10 minutes
                {
                    _logger.LogWarning("State parameter expired (age: {Age} seconds)", stateAge);
                    return Guid.Empty;
                }
            }

            return configId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating state parameter");
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Gets OAuth callback URL from configuration
    /// </summary>
    private string GetCallbackUrl()
    {
        var baseUrl = _configuration["OAuth:CallbackBaseUrl"] ?? "http://localhost:5000";
        return $"{baseUrl}/api/oauth/callback";
    }

    /// <summary>
    /// Gets frontend URL for redirects
    /// </summary>
    private string GetFrontendUrl()
    {
        return _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
    }

    /// <summary>
    /// Builds OAuth authorization URL for SMTP email server settings
    /// </summary>
    private string BuildAuthorizationUrlForSmtp(EmailServerSettings settings, string state)
    {
        var callbackUrl = GetCallbackUrlForSmtp();
        var clientId = settings.OAuthClientId;

        // Determine provider based on SMTP host
        var isOffice365 = settings.Host?.Contains("smtp.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = settings.Host?.Contains("smtp.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        if (isOffice365)
        {
            // Microsoft/Office 365 OAuth - SMTP only scopes
            var tenantId = settings.OAuthTenantId ?? "common";
            var scope = Uri.EscapeDataString("https://outlook.office365.com/SMTP.Send offline_access");

            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"response_mode=query&" +
                   $"scope={scope}&" +
                   $"state={state}&" +
                   $"login_hint={Uri.EscapeDataString(settings.Username ?? "")}";
        }
        else if (isGmail)
        {
            // Google/Gmail OAuth
            var scope = Uri.EscapeDataString("https://mail.google.com/");

            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"scope={scope}&" +
                   $"access_type=offline&" +
                   $"prompt=consent&" +
                   $"state={state}&" +
                   $"login_hint={Uri.EscapeDataString(settings.Username ?? "")}";
        }
        else
        {
            // Fallback to Office 365 format
            _logger.LogWarning("Unknown OAuth provider for SMTP host {Host}, using Office 365 format", settings.Host);
            var tenantId = settings.OAuthTenantId ?? "common";
            var scope = Uri.EscapeDataString("https://outlook.office365.com/SMTP.Send offline_access");

            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                   $"response_mode=query&" +
                   $"scope={scope}&" +
                   $"state={state}&" +
                   $"login_hint={Uri.EscapeDataString(settings.Username ?? "")}";
        }
    }

    /// <summary>
    /// Exchanges authorization code for access/refresh tokens for SMTP
    /// </summary>
    private async Task<TokenResponse> ExchangeCodeForTokensSmtp(EmailServerSettings settings, string code)
    {
        var callbackUrl = GetCallbackUrlForSmtp();
        var isOffice365 = settings.Host?.Contains("smtp.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = settings.Host?.Contains("smtp.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // SECURITY: Decrypt OAuth client secret before sending to token endpoint
            var decryptedSecret = !string.IsNullOrEmpty(settings.OAuthClientSecret)
                ? _encryptionService.DecryptPassword(settings.OAuthClientSecret)
                : settings.OAuthClientSecret;

            _logger.LogInformation("Exchanging code for SMTP tokens with decrypted secret for settings {SettingsId}", settings.Id);

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = settings.OAuthClientId!,
                ["client_secret"] = decryptedSecret!,
                ["code"] = code,
                ["redirect_uri"] = callbackUrl,
                ["grant_type"] = "authorization_code"
            };

            string tokenEndpoint;
            if (isOffice365)
            {
                var tenantId = settings.OAuthTenantId ?? "common";
                tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            }
            else if (isGmail)
            {
                tokenEndpoint = "https://oauth2.googleapis.com/token";
            }
            else
            {
                var tenantId = settings.OAuthTenantId ?? "common";
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
                _logger.LogError("SMTP token exchange failed: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return new TokenResponse
                {
                    IsSuccess = false,
                    Error = $"Token exchange failed: {response.StatusCode}"
                };
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return new TokenResponse
            {
                IsSuccess = true,
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for SMTP tokens");
            return new TokenResponse
            {
                IsSuccess = false,
                Error = "An error occurred during token exchange"
            };
        }
    }

    /// <summary>
    /// Generates secure state parameter for SMTP with encrypted settingsId
    /// </summary>
    private string GenerateSecureStateForSmtp(Guid settingsId)
    {
        // Create state: smtp|settingsId|timestamp|nonce
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var state = $"smtp|{settingsId}|{timestamp}|{nonce}";

        // Base64 encode for URL safety
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(state));
    }

    /// <summary>
    /// Validates state parameter for SMTP and extracts settingsId
    /// </summary>
    private Guid ValidateAndExtractStateForSmtp(string state)
    {
        try
        {
            // Decode state
            var decodedState = Encoding.UTF8.GetString(Convert.FromBase64String(state));
            var parts = decodedState.Split('|');

            if (parts.Length != 4 || parts[0] != "smtp")
            {
                _logger.LogWarning("Invalid SMTP state format");
                return Guid.Empty;
            }

            // Extract settingsId
            if (!Guid.TryParse(parts[1], out Guid settingsId))
            {
                _logger.LogWarning("Invalid settingsId in SMTP state");
                return Guid.Empty;
            }

            // Validate timestamp (reject if older than 10 minutes)
            if (long.TryParse(parts[2], out long timestamp))
            {
                var stateAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                if (stateAge > 600) // 10 minutes
                {
                    _logger.LogWarning("SMTP state parameter expired (age: {Age} seconds)", stateAge);
                    return Guid.Empty;
                }
            }

            return settingsId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SMTP state parameter");
            return Guid.Empty;
        }
    }

    /// <summary>
    /// Gets OAuth callback URL for SMTP from configuration
    /// </summary>
    private string GetCallbackUrlForSmtp()
    {
        var baseUrl = _configuration["OAuth:CallbackBaseUrl"] ?? "http://localhost:5000";
        return $"{baseUrl}/api/oauth/callback-smtp";
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// Response from token exchange/refresh
/// </summary>
public class TokenResponse
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Result from token refresh endpoint
/// </summary>
public class TokenRefreshResult
{
    public DateTime ExpiresAt { get; set; }
    public int ExpiresIn { get; set; }
}

#endregion
