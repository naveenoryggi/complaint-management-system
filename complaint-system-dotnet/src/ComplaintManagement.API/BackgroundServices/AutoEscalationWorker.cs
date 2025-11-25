using ComplaintManagement.Application.Interfaces.Services;

namespace ComplaintManagement.API.BackgroundServices;

/// <summary>
/// Background worker that periodically checks and processes auto-escalations
/// </summary>
public class AutoEscalationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoEscalationWorker> _logger;
    private readonly IConfiguration _configuration;

    public AutoEscalationWorker(
        IServiceProvider serviceProvider,
        ILogger<AutoEscalationWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto-Escalation Worker starting at {Time}", DateTimeOffset.UtcNow);

        // Get interval from configuration (default: 15 minutes)
        var intervalMinutes = _configuration.GetValue<int>("AutoEscalation:IntervalMinutes", 15);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        // Optional: Delay start to allow application to fully initialize
        var startupDelaySeconds = _configuration.GetValue<int>("AutoEscalation:StartupDelaySeconds", 30);
        if (startupDelaySeconds > 0)
        {
            _logger.LogInformation("Delaying auto-escalation worker start by {Seconds} seconds", startupDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(startupDelaySeconds), stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Auto-Escalation Worker running at {Time}", DateTimeOffset.UtcNow);

                // Create a new scope for each iteration to get fresh instances
                using (var scope = _serviceProvider.CreateScope())
                {
                    var autoEscalationService = scope.ServiceProvider
                        .GetRequiredService<IAutoEscalationService>();

                    await autoEscalationService.ProcessAutoEscalationsAsync(stoppingToken);
                }

                _logger.LogInformation(
                    "Auto-Escalation Worker completed. Next run in {Minutes} minutes",
                    intervalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Auto-Escalation Worker");
            }

            // Wait for the specified interval before next execution
            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("Auto-Escalation Worker stopping at {Time}", DateTimeOffset.UtcNow);
    }
}
