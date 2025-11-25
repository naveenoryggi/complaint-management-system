using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Background service that periodically polls email accounts for new messages
/// and processes them into tickets/complaints
/// </summary>
public class EmailPollingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailPollingBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute for configs to poll

    public EmailPollingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmailPollingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Polling Background Service started at {Time}", DateTime.UtcNow);

        // Wait a bit before starting to allow application to fully initialize
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAllEmailConfigurationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in email polling background service");
            }

            // Wait before next check cycle
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Email Polling Background Service stopped at {Time}", DateTime.UtcNow);
    }

    /// <summary>
    /// Polls all enabled email configurations that are due for polling
    /// </summary>
    private async Task PollAllEmailConfigurationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ComplaintDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailTicketingService>();

        try
        {
            // Get all enabled email configurations
            var configs = await dbContext.EmailConfigurations
                .Where(c => c.IsEnabled && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!configs.Any())
            {
                _logger.LogDebug("No enabled email configurations found for polling");
                return;
            }

            _logger.LogDebug("Found {Count} enabled email configurations", configs.Count);

            foreach (var config in configs)
            {
                try
                {
                    // Check if this configuration is due for polling
                    if (ShouldPollConfiguration(config))
                    {
                        await PollEmailConfigurationAsync(config, emailService, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error polling email configuration {ConfigId} for company {CompanyId}",
                        config.Id, config.CompanyId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email configurations for polling");
        }
    }

    /// <summary>
    /// Determines if a configuration should be polled based on last poll time and interval
    /// </summary>
    private bool ShouldPollConfiguration(EmailConfiguration config)
    {
        if (!config.LastPolledAt.HasValue)
        {
            // Never polled before - poll now
            return true;
        }

        var timeSinceLastPoll = DateTime.UtcNow - config.LastPolledAt.Value;

        // Use seconds if configured, otherwise fall back to minutes
        var pollingInterval = config.PollingIntervalSeconds.HasValue
            ? TimeSpan.FromSeconds(config.PollingIntervalSeconds.Value)
            : TimeSpan.FromMinutes(config.PollingIntervalMinutes);

        return timeSinceLastPoll >= pollingInterval;
    }

    /// <summary>
    /// Polls a specific email configuration and processes new emails
    /// </summary>
    private async Task PollEmailConfigurationAsync(
        EmailConfiguration config,
        IEmailTicketingService emailService,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Starting email poll for configuration {ConfigId} (Company: {CompanyId}, Interval: {Interval}min)",
            config.Id, config.CompanyId, config.PollingIntervalMinutes);

        try
        {
            var result = await emailService.FetchAndProcessEmailsAsync(config.Id, cancellationToken);

            if (result.IsSuccess)
            {
                var processingTime = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "Email poll completed for configuration {ConfigId}. " +
                    "Fetched: {Fetched}, New Tickets: {NewTickets}, Updated: {Updated}, " +
                    "Ignored: {Ignored}, Failed: {Failed}, Duration: {Duration}ms",
                    config.Id,
                    result.Data.TotalEmailsFetched,
                    result.Data.NewTicketsCreated,
                    result.Data.ExistingTicketsUpdated,
                    result.Data.EmailsIgnored,
                    result.Data.EmailsFailed,
                    processingTime.TotalMilliseconds);

                // Log warnings if any
                if (result.Data.Warnings.Any())
                {
                    foreach (var warning in result.Data.Warnings)
                    {
                        _logger.LogWarning("Email polling warning for config {ConfigId}: {Warning}",
                            config.Id, warning);
                    }
                }

                // Log errors if any
                if (result.Data.Errors.Any())
                {
                    foreach (var error in result.Data.Errors)
                    {
                        _logger.LogError("Email polling error for config {ConfigId}: {Error}",
                            config.Id, error);
                    }
                }
            }
            else
            {
                _logger.LogError(
                    "Email poll failed for configuration {ConfigId}: {Error}",
                    config.Id, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception occurred while polling email configuration {ConfigId}",
                config.Id);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Polling Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
