using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// DEPRECATED: This controller has been consolidated into OAuthController.
/// Use OAuthController for all OAuth operations instead.
/// This controller is kept for backward compatibility but will be removed in a future version.
/// </summary>
[Obsolete("This controller is deprecated. Use OAuthController instead.")]
[ApiController]
[Route("api/oauth-legacy")]
public class OAuthCallbackController : ControllerBase
{
    private readonly EmailOAuthService _oauthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public OAuthCallbackController(
        EmailOAuthService oauthService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _oauthService = oauthService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // NOTE: OAuth authorization initiation moved to OAuthController.Authorize()
    // This method was removed to fix ambiguous route conflict
    // Use: GET /api/oauth/authorize/{configId} in OAuthController instead

    /// <summary>
    /// Handle OAuth callback from Microsoft (LEGACY - Use OAuthController.Callback instead)
    /// Exchanges authorization code for tokens and stores them
    /// </summary>
    /// <param name="code">Authorization code from Microsoft</param>
    /// <param name="state">State parameter (email config ID)</param>
    /// <param name="error">Error message if authorization failed</param>
    /// <param name="error_description">Detailed error description</param>
    /// <returns>Redirect back to Angular app</returns>
    [HttpGet("callback-legacy")]
    public async Task<IActionResult> CallbackLegacy(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery] string? error_description)
    {
        try
        {
            // Check for authorization errors
            if (!string.IsNullOrEmpty(error))
            {
                var errorMsg = !string.IsNullOrEmpty(error_description) ? error_description : error;
                return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={Uri.EscapeDataString(errorMsg)}");
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={Uri.EscapeDataString("Missing authorization code or state")}");
            }

            // Parse state to get email config ID
            if (!Guid.TryParse(state, out var configId))
            {
                return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={Uri.EscapeDataString("Invalid state parameter")}");
            }

            // Exchange authorization code for tokens
            var tokenResult = await _oauthService.ExchangeCodeForTokensAsync(code);

            // Get email configuration from database
            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(configId);

            if (config == null)
            {
                return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={Uri.EscapeDataString("Email configuration not found")}");
            }

            // Update configuration with OAuth tokens
            config.AuthenticationType = EmailAuthenticationType.OAuth2;
            config.OAuthAccessToken = tokenResult.AccessToken;
            config.OAuthRefreshToken = tokenResult.RefreshToken;
            config.OAuthTokenExpiresAt = tokenResult.ExpiresOn;
            config.OAuthScopes = "https://outlook.office365.com/IMAP.AccessAsUser.All https://outlook.office365.com/SMTP.Send offline_access";
            config.UpdatedAt = DateTime.UtcNow;

            // Save changes to database
            _unitOfWork.Repository<EmailConfiguration>().Update(config);
            await _unitOfWork.SaveChangesAsync();

            // Redirect back to Angular with success
            return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=success&configId={configId}");
        }
        catch (Exception ex)
        {
            return Redirect($"http://localhost:4200/admin/email-ticketing-config?oauth=error&message={Uri.EscapeDataString(ex.Message)}");
        }
    }

    /// <summary>
    /// DEPRECATED: Use POST /api/oauth/refresh/{configId} in OAuthController instead.
    /// This endpoint is kept for backward compatibility only.
    /// </summary>
    /// <param name="configId">Email configuration ID</param>
    /// <returns>Redirect to new endpoint or error response</returns>
    [Obsolete("Use POST /api/oauth/refresh/{configId} in OAuthController instead")]
    [HttpPost("refresh/{configId}")]
    public async Task<IActionResult> RefreshToken(Guid configId)
    {
        try
        {
            // Get email configuration
            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(configId);

            if (config == null)
                return NotFound(new { message = "Email configuration not found" });

            if (config.AuthenticationType != EmailAuthenticationType.OAuth2)
                return BadRequest(new { message = "Configuration is not using OAuth2 authentication" });

            if (string.IsNullOrEmpty(config.ImapUsername))
                return BadRequest(new { message = "Email address not configured. Please complete email configuration." });

            // Refresh the access token using database refresh token (not MSAL cache)
            // This works with tokens obtained via browser OAuth flow
            var tokenResult = await _oauthService.RefreshAccessTokenAsync(config);

            // Update configuration with new tokens
            config.OAuthAccessToken = tokenResult.AccessToken;
            if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
            {
                config.OAuthRefreshToken = tokenResult.RefreshToken; // Update refresh token if new one provided
            }
            config.OAuthTokenExpiresAt = tokenResult.ExpiresOn;
            config.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<EmailConfiguration>().Update(config);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "Token refreshed successfully",
                expiresAt = tokenResult.ExpiresOn
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Token refresh failed: {ex.Message}" });
        }
    }
}
