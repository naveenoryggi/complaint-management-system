using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.Configuration;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Background service that automatically refreshes OAuth tokens before they expire
/// Runs every hour and checks for tokens expiring within the configured warning period
/// </summary>
public class OAuthTokenRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OAuthTokenRefreshBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private TimeSpan _refreshInterval; // Now mutable - can be updated from database
    private int _tokenExpiryWarningDays; // Now mutable - can be updated from database

    public OAuthTokenRefreshBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OAuthTokenRefreshBackgroundService> logger,
        IConfiguration _configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        this._configuration = _configuration;

        // Get fallback configuration values from appsettings.json
        // These are used if database configuration is not available
        var intervalMinutes = int.TryParse(_configuration["OAuth:TokenRefreshIntervalMinutes"], out var interval) ? interval : 60;
        _refreshInterval = TimeSpan.FromMinutes(intervalMinutes);
        _tokenExpiryWarningDays = int.TryParse(_configuration["OAuth:TokenExpiryWarningDays"], out var days) ? days : 7;

        _logger.LogInformation(
            "OAuth Token Refresh Background Service initialized. Fallback Interval: {Interval} minutes, Warning Days: {Days}",
            intervalMinutes, _tokenExpiryWarningDays);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OAuth Token Refresh Background Service is starting");

        // Wait 2 minutes before first execution (let application fully start)
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshExpiringTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OAuth token refresh cycle");
            }

            // Wait for next interval
            await Task.Delay(_refreshInterval, stoppingToken);
        }

        _logger.LogInformation("OAuth Token Refresh Background Service is stopping");
    }

    /// <summary>
    /// Load configuration from database (refresh interval and expiry warning)
    /// Falls back to appsettings.json if database config not available
    /// </summary>
    private async Task LoadConfigurationFromDatabaseAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            // Get all system configurations (one per company)
            var systemConfigs = await unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable()
                .ToListAsync(cancellationToken);

            if (systemConfigs.Any())
            {
                // Use the minimum refresh interval across all companies to ensure all tokens are refreshed
                var minIntervalMinutes = systemConfigs.Min(c => c.OAuthTokenRefreshIntervalMinutes);
                var maxWarningDays = systemConfigs.Max(c => c.OAuthTokenExpiryWarningDays);

                _refreshInterval = TimeSpan.FromMinutes(minIntervalMinutes);
                _tokenExpiryWarningDays = maxWarningDays;

                _logger.LogInformation(
                    "Loaded configuration from database: Interval = {Interval} minutes, Warning = {Warning} days",
                    minIntervalMinutes, maxWarningDays);
            }
            else
            {
                _logger.LogWarning("No system configuration found in database, using fallback values from appsettings.json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration from database, using fallback values");
        }
    }

    /// <summary>
    /// Finds and refreshes all OAuth tokens that are expiring soon
    /// </summary>
    private async Task RefreshExpiringTokensAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting OAuth token refresh cycle");

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Reload configuration from database at the start of each cycle
            // This ensures changes made in the UI take effect without restarting the service
            await LoadConfigurationFromDatabaseAsync(unitOfWork, cancellationToken);

            // Find all OAuth configurations that are enabled
            var allConfigs = await unitOfWork.Repository<EmailConfiguration>()
                .FindAsync(e => e.AuthenticationType == EmailAuthenticationType.OAuth2 && e.IsEnabled, cancellationToken);

            // Check each config to see if it needs refreshing (using async method)
            var configsToRefresh = new List<EmailConfiguration>();
            foreach (var config in allConfigs)
            {
                if (await ShouldRefreshTokenAsync(config, unitOfWork, cancellationToken))
                {
                    configsToRefresh.Add(config);
                }
            }

            if (!configsToRefresh.Any())
            {
                _logger.LogInformation("No OAuth tokens need refreshing at this time");
                return;
            }

            _logger.LogInformation("Found {Count} OAuth tokens that need refreshing", configsToRefresh.Count);

            int successCount = 0;
            int failureCount = 0;

            foreach (var config in configsToRefresh)
            {
                try
                {
                    _logger.LogInformation(
                        "Refreshing token for config {ConfigId} (Company: {CompanyId}, Email: {Email}, Expires: {ExpiresAt})",
                        config.Id, config.CompanyId, config.FromEmail, config.OAuthTokenExpiresAt);

                    var refreshResult = await RefreshTokenAsync(config, cancellationToken);

                    if (refreshResult.IsSuccess)
                    {
                        // Update configuration with new tokens
                        config.OAuthAccessToken = refreshResult.AccessToken;
                        if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                        {
                            config.OAuthRefreshToken = refreshResult.RefreshToken;
                        }
                        config.OAuthTokenExpiresAt = DateTime.UtcNow.AddSeconds(refreshResult.ExpiresIn);
                        config.UpdatedAt = DateTime.UtcNow;

                        unitOfWork.Repository<EmailConfiguration>().Update(config);
                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        successCount++;
                        _logger.LogInformation(
                            "Successfully refreshed token for config {ConfigId}. New expiry: {ExpiresAt}",
                            config.Id, config.OAuthTokenExpiresAt);
                    }
                    else
                    {
                        failureCount++;
                        _logger.LogError(
                            "Failed to refresh token for config {ConfigId}: {Error}",
                            config.Id, refreshResult.Error);

                        // Optionally disable configuration if refresh fails repeatedly
                        // For now, just log the error
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex,
                        "Exception while refreshing token for config {ConfigId}",
                        config.Id);
                }

                // Small delay between refreshes to avoid rate limiting
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            _logger.LogInformation(
                "OAuth token refresh cycle completed. Success: {Success}, Failures: {Failures}",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth token refresh cycle");
        }
    }

    /// <summary>
    /// Gets the effective OAuth token refresh interval for a specific email configuration.
    /// Uses precedence: Individual account setting > System default > appsettings.json fallback
    /// </summary>
    private async Task<int> GetEffectiveRefreshIntervalMinutesAsync(
        EmailConfiguration config,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        // 1. Check per-account setting first (highest priority)
        if (config.OAuthTokenRefreshIntervalMinutes.HasValue)
        {
            _logger.LogDebug(
                "Using per-account OAuth refresh interval: {Minutes} min for config {ConfigId} (Email: {Email})",
                config.OAuthTokenRefreshIntervalMinutes.Value, config.Id, config.FromEmail);
            return config.OAuthTokenRefreshIntervalMinutes.Value;
        }

        // 2. Load system default for this company (second priority)
        var systemConfig = await unitOfWork.Repository<SystemConfiguration>()
            .GetQueryable()
            .FirstOrDefaultAsync(sc => sc.CompanyId == config.CompanyId, cancellationToken);

        if (systemConfig != null)
        {
            _logger.LogDebug(
                "Using system default OAuth refresh interval: {Minutes} min for config {ConfigId} (Email: {Email})",
                systemConfig.OAuthTokenRefreshIntervalMinutes, config.Id, config.FromEmail);
            return systemConfig.OAuthTokenRefreshIntervalMinutes;
        }

        // 3. Fall back to appsettings.json (lowest priority)
        var fallbackMinutes = (int)_refreshInterval.TotalMinutes;
        _logger.LogDebug(
            "Using fallback OAuth refresh interval from appsettings: {Minutes} min for config {ConfigId} (Email: {Email})",
            fallbackMinutes, config.Id, config.FromEmail);
        return fallbackMinutes;
    }

    /// <summary>
    /// Determines if a token should be refreshed based on expiry time and configured refresh interval.
    /// Tokens are refreshed when they will expire within the configured refresh interval period.
    /// This ensures proactive refresh before expiration.
    /// </summary>
    private async Task<bool> ShouldRefreshTokenAsync(
        EmailConfiguration config,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        // No refresh token available
        if (string.IsNullOrEmpty(config.OAuthRefreshToken))
        {
            _logger.LogWarning(
                "Config {ConfigId} has no refresh token. User needs to re-authorize.",
                config.Id);
            return false;
        }

        // No expiry date set
        if (!config.OAuthTokenExpiresAt.HasValue)
        {
            _logger.LogWarning(
                "Config {ConfigId} has no token expiry date set. Attempting refresh.",
                config.Id);
            return true; // Try to refresh anyway
        }

        // Get the effective refresh interval for this account (with fallback chain)
        var effectiveRefreshIntervalMinutes = await GetEffectiveRefreshIntervalMinutesAsync(
            config, unitOfWork, cancellationToken);

        // Calculate time until expiry
        var timeUntilExpiry = config.OAuthTokenExpiresAt.Value - DateTime.UtcNow;

        // Already expired
        if (timeUntilExpiry.TotalSeconds <= 0)
        {
            _logger.LogWarning(
                "Config {ConfigId} token has expired. Attempting refresh.",
                config.Id);
            return true;
        }

        // Refresh if token will expire within the refresh interval period
        // This ensures tokens are refreshed proactively before they expire
        // Example: If refresh interval is 30 min and token expires in 25 min, refresh now
        if (timeUntilExpiry.TotalMinutes <= effectiveRefreshIntervalMinutes)
        {
            _logger.LogInformation(
                "Config {ConfigId} (Email: {Email}) token expires in {Minutes:F1} minutes (refresh threshold: {Threshold} min). Refreshing.",
                config.Id, config.FromEmail, timeUntilExpiry.TotalMinutes, effectiveRefreshIntervalMinutes);
            return true;
        }

        // Token is still valid for a while
        _logger.LogDebug(
            "Config {ConfigId} (Email: {Email}) token is still valid for {Minutes:F1} minutes (refresh threshold: {Threshold} min). No refresh needed.",
            config.Id, config.FromEmail, timeUntilExpiry.TotalMinutes, effectiveRefreshIntervalMinutes);
        return false;
    }

    /// <summary>
    /// Refreshes an OAuth access token using the refresh token
    /// </summary>
    private async Task<TokenRefreshResult> RefreshTokenAsync(
        EmailConfiguration config,
        CancellationToken cancellationToken)
    {
        var isOffice365 = config.ImapHost?.Contains("outlook.office365.com", StringComparison.OrdinalIgnoreCase) ?? false;
        var isGmail = config.ImapHost?.Contains("imap.gmail.com", StringComparison.OrdinalIgnoreCase) ?? false;

        try
        {
            using var httpClient = new HttpClient();

            var requestBody = new Dictionary<string, string>
            {
                ["client_id"] = config.OAuthClientId!,
                ["client_secret"] = config.OAuthClientSecret!,
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

            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new TokenRefreshResult
                {
                    IsSuccess = false,
                    Error = $"Token refresh failed: {response.StatusCode} - {responseContent}"
                };
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return new TokenRefreshResult
            {
                IsSuccess = true,
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32()
            };
        }
        catch (Exception ex)
        {
            return new TokenRefreshResult
            {
                IsSuccess = false,
                Error = $"Exception during token refresh: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Result from token refresh operation
/// </summary>
public class TokenRefreshResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? Error { get; set; }
}
