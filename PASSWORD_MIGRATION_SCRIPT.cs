using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ComplaintManagement.Migration;

/// <summary>
/// One-time migration script to encrypt all existing plaintext passwords in the database
///
/// USAGE:
/// 1. Create a new Console Application project
/// 2. Add references to ComplaintManagement.Infrastructure and ComplaintManagement.Application
/// 3. Copy this file into the project
/// 4. Configure appsettings.json with database connection string
/// 5. Run: dotnet run
///
/// SAFETY:
/// - Idempotent: Can be run multiple times safely (detects already-encrypted passwords)
/// - Backup: Always backup database before running
/// - Rollback: Keep database backup for rollback if needed
///
/// WHAT IT ENCRYPTS:
/// - EmailServerSettings: Password, OAuthClientSecret
/// - EmailConfiguration: ImapPassword, SmtpPassword, OAuthClientSecret, SmtpSeparatePassword, SmtpSeparateOAuthClientSecret
/// </summary>
public class PasswordMigrationScript
{
    private readonly ComplaintDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PasswordMigrationScript> _logger;

    public PasswordMigrationScript(
        ComplaintDbContext context,
        IEncryptionService encryptionService,
        ILogger<PasswordMigrationScript> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("Starting Password Migration");
        _logger.LogInformation("========================================");

        var stats = new MigrationStatistics();

        try
        {
            // Step 1: Migrate EmailServerSettings
            _logger.LogInformation("Step 1: Migrating EmailServerSettings passwords...");
            await MigrateEmailServerSettingsAsync(stats);

            // Step 2: Migrate EmailConfiguration
            _logger.LogInformation("Step 2: Migrating EmailConfiguration passwords...");
            await MigrateEmailConfigurationAsync(stats);

            // Step 3: Save all changes
            _logger.LogInformation("Step 3: Saving changes to database...");
            await _context.SaveChangesAsync();

            // Step 4: Print summary
            PrintMigrationSummary(stats);

            _logger.LogInformation("========================================");
            _logger.LogInformation("Migration Completed Successfully!");
            _logger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL ERROR during password migration!");
            _logger.LogError("Database changes NOT saved. Please review error and retry.");
            throw;
        }
    }

    private async Task MigrateEmailServerSettingsAsync(MigrationStatistics stats)
    {
        var serverSettings = await _context.EmailServerSettings
            .Where(s => !s.IsDeleted)
            .ToListAsync();

        stats.EmailServerSettings.Total = serverSettings.Count;

        foreach (var setting in serverSettings)
        {
            _logger.LogInformation("Processing EmailServerSetting: {Id} - {Name}", setting.Id, setting.Name);

            // Migrate Password
            if (!string.IsNullOrEmpty(setting.Password))
            {
                if (IsAlreadyEncrypted(setting.Password))
                {
                    _logger.LogInformation("  Password: Already encrypted (skipping)");
                    stats.EmailServerSettings.AlreadyEncrypted++;
                }
                else
                {
                    setting.Password = _encryptionService.EncryptPassword(setting.Password);
                    _logger.LogInformation("  Password: Encrypted successfully");
                    stats.EmailServerSettings.NewlyEncrypted++;
                }
            }
            else
            {
                _logger.LogInformation("  Password: NULL (skipping)");
                stats.EmailServerSettings.Null++;
            }

            // Migrate OAuth Client Secret
            if (!string.IsNullOrEmpty(setting.OAuthClientSecret))
            {
                if (IsAlreadyEncrypted(setting.OAuthClientSecret))
                {
                    _logger.LogInformation("  OAuthClientSecret: Already encrypted (skipping)");
                    stats.EmailServerSettings.OAuthSecretsAlreadyEncrypted++;
                }
                else
                {
                    setting.OAuthClientSecret = _encryptionService.EncryptPassword(setting.OAuthClientSecret);
                    _logger.LogInformation("  OAuthClientSecret: Encrypted successfully");
                    stats.EmailServerSettings.OAuthSecretsNewlyEncrypted++;
                }
            }
            else
            {
                _logger.LogInformation("  OAuthClientSecret: NULL (skipping)");
                stats.EmailServerSettings.OAuthSecretsNull++;
            }
        }

        _logger.LogInformation("EmailServerSettings migration complete: {Total} records processed", stats.EmailServerSettings.Total);
    }

    private async Task MigrateEmailConfigurationAsync(MigrationStatistics stats)
    {
        var emailConfigs = await _context.Set<EmailConfiguration>().ToListAsync();

        stats.EmailConfiguration.Total = emailConfigs.Count;

        foreach (var config in emailConfigs)
        {
            _logger.LogInformation("Processing EmailConfiguration: {Id} - {Email}", config.Id, config.FromEmail);

            // Migrate IMAP Password
            if (!string.IsNullOrEmpty(config.ImapPassword))
            {
                if (IsAlreadyEncrypted(config.ImapPassword))
                {
                    _logger.LogInformation("  ImapPassword: Already encrypted (skipping)");
                    stats.EmailConfiguration.ImapPasswordsAlreadyEncrypted++;
                }
                else
                {
                    config.ImapPassword = _encryptionService.EncryptPassword(config.ImapPassword);
                    _logger.LogInformation("  ImapPassword: Encrypted successfully");
                    stats.EmailConfiguration.ImapPasswordsNewlyEncrypted++;
                }
            }
            else
            {
                _logger.LogInformation("  ImapPassword: NULL (skipping)");
                stats.EmailConfiguration.ImapPasswordsNull++;
            }

            // Migrate SMTP Password
            if (!string.IsNullOrEmpty(config.SmtpPassword))
            {
                if (IsAlreadyEncrypted(config.SmtpPassword))
                {
                    _logger.LogInformation("  SmtpPassword: Already encrypted (skipping)");
                    stats.EmailConfiguration.SmtpPasswordsAlreadyEncrypted++;
                }
                else
                {
                    config.SmtpPassword = _encryptionService.EncryptPassword(config.SmtpPassword);
                    _logger.LogInformation("  SmtpPassword: Encrypted successfully");
                    stats.EmailConfiguration.SmtpPasswordsNewlyEncrypted++;
                }
            }
            else
            {
                _logger.LogInformation("  SmtpPassword: NULL (skipping)");
                stats.EmailConfiguration.SmtpPasswordsNull++;
            }

            // Migrate OAuth Client Secret
            if (!string.IsNullOrEmpty(config.OAuthClientSecret))
            {
                if (IsAlreadyEncrypted(config.OAuthClientSecret))
                {
                    _logger.LogInformation("  OAuthClientSecret: Already encrypted (skipping)");
                    stats.EmailConfiguration.OAuthSecretsAlreadyEncrypted++;
                }
                else
                {
                    config.OAuthClientSecret = _encryptionService.EncryptPassword(config.OAuthClientSecret);
                    _logger.LogInformation("  OAuthClientSecret: Encrypted successfully");
                    stats.EmailConfiguration.OAuthSecretsNewlyEncrypted++;
                }
            }
            else
            {
                _logger.LogInformation("  OAuthClientSecret: NULL (skipping)");
                stats.EmailConfiguration.OAuthSecretsNull++;
            }

            // Migrate Separate SMTP Password (if using separate account)
            if (config.UseSeparateSmtpAccount)
            {
                if (!string.IsNullOrEmpty(config.SmtpSeparatePassword))
                {
                    if (IsAlreadyEncrypted(config.SmtpSeparatePassword))
                    {
                        _logger.LogInformation("  SmtpSeparatePassword: Already encrypted (skipping)");
                        stats.EmailConfiguration.SeparateSmtpPasswordsAlreadyEncrypted++;
                    }
                    else
                    {
                        config.SmtpSeparatePassword = _encryptionService.EncryptPassword(config.SmtpSeparatePassword);
                        _logger.LogInformation("  SmtpSeparatePassword: Encrypted successfully");
                        stats.EmailConfiguration.SeparateSmtpPasswordsNewlyEncrypted++;
                    }
                }
                else
                {
                    _logger.LogInformation("  SmtpSeparatePassword: NULL (skipping)");
                    stats.EmailConfiguration.SeparateSmtpPasswordsNull++;
                }

                if (!string.IsNullOrEmpty(config.SmtpSeparateOAuthClientSecret))
                {
                    if (IsAlreadyEncrypted(config.SmtpSeparateOAuthClientSecret))
                    {
                        _logger.LogInformation("  SmtpSeparateOAuthClientSecret: Already encrypted (skipping)");
                        stats.EmailConfiguration.SeparateOAuthSecretsAlreadyEncrypted++;
                    }
                    else
                    {
                        config.SmtpSeparateOAuthClientSecret = _encryptionService.EncryptPassword(config.SmtpSeparateOAuthClientSecret);
                        _logger.LogInformation("  SmtpSeparateOAuthClientSecret: Encrypted successfully");
                        stats.EmailConfiguration.SeparateOAuthSecretsNewlyEncrypted++;
                    }
                }
                else
                {
                    _logger.LogInformation("  SmtpSeparateOAuthClientSecret: NULL (skipping)");
                    stats.EmailConfiguration.SeparateOAuthSecretsNull++;
                }
            }
        }

        _logger.LogInformation("EmailConfiguration migration complete: {Total} records processed", stats.EmailConfiguration.Total);
    }

    /// <summary>
    /// Detects if a password is already encrypted by attempting to decrypt it
    /// </summary>
    private bool IsAlreadyEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            // Try to decrypt - if successful, it's already encrypted
            _encryptionService.DecryptPassword(value);
            return true;
        }
        catch
        {
            // Decryption failed - it's plaintext
            return false;
        }
    }

    private void PrintMigrationSummary(MigrationStatistics stats)
    {
        _logger.LogInformation("");
        _logger.LogInformation("========================================");
        _logger.LogInformation("Migration Summary");
        _logger.LogInformation("========================================");

        _logger.LogInformation("");
        _logger.LogInformation("EmailServerSettings:");
        _logger.LogInformation("  Total Records: {Total}", stats.EmailServerSettings.Total);
        _logger.LogInformation("  Passwords:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailServerSettings.NewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailServerSettings.AlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailServerSettings.Null);
        _logger.LogInformation("  OAuth Client Secrets:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailServerSettings.OAuthSecretsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailServerSettings.OAuthSecretsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailServerSettings.OAuthSecretsNull);

        _logger.LogInformation("");
        _logger.LogInformation("EmailConfiguration:");
        _logger.LogInformation("  Total Records: {Total}", stats.EmailConfiguration.Total);
        _logger.LogInformation("  IMAP Passwords:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailConfiguration.ImapPasswordsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailConfiguration.ImapPasswordsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailConfiguration.ImapPasswordsNull);
        _logger.LogInformation("  SMTP Passwords:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailConfiguration.SmtpPasswordsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailConfiguration.SmtpPasswordsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailConfiguration.SmtpPasswordsNull);
        _logger.LogInformation("  OAuth Client Secrets:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailConfiguration.OAuthSecretsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailConfiguration.OAuthSecretsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailConfiguration.OAuthSecretsNull);
        _logger.LogInformation("  Separate SMTP Passwords:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailConfiguration.SeparateSmtpPasswordsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailConfiguration.SeparateSmtpPasswordsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailConfiguration.SeparateSmtpPasswordsNull);
        _logger.LogInformation("  Separate OAuth Secrets:");
        _logger.LogInformation("    - Newly Encrypted: {Count}", stats.EmailConfiguration.SeparateOAuthSecretsNewlyEncrypted);
        _logger.LogInformation("    - Already Encrypted: {Count}", stats.EmailConfiguration.SeparateOAuthSecretsAlreadyEncrypted);
        _logger.LogInformation("    - NULL: {Count}", stats.EmailConfiguration.SeparateOAuthSecretsNull);

        _logger.LogInformation("");
        _logger.LogInformation("Total Passwords Newly Encrypted: {Count}",
            stats.EmailServerSettings.NewlyEncrypted +
            stats.EmailServerSettings.OAuthSecretsNewlyEncrypted +
            stats.EmailConfiguration.ImapPasswordsNewlyEncrypted +
            stats.EmailConfiguration.SmtpPasswordsNewlyEncrypted +
            stats.EmailConfiguration.OAuthSecretsNewlyEncrypted +
            stats.EmailConfiguration.SeparateSmtpPasswordsNewlyEncrypted +
            stats.EmailConfiguration.SeparateOAuthSecretsNewlyEncrypted);
    }
}

/// <summary>
/// Statistics for migration tracking
/// </summary>
public class MigrationStatistics
{
    public EmailServerSettingsStats EmailServerSettings { get; set; } = new();
    public EmailConfigurationStats EmailConfiguration { get; set; } = new();
}

public class EmailServerSettingsStats
{
    public int Total { get; set; }
    public int NewlyEncrypted { get; set; }
    public int AlreadyEncrypted { get; set; }
    public int Null { get; set; }
    public int OAuthSecretsNewlyEncrypted { get; set; }
    public int OAuthSecretsAlreadyEncrypted { get; set; }
    public int OAuthSecretsNull { get; set; }
}

public class EmailConfigurationStats
{
    public int Total { get; set; }
    public int ImapPasswordsNewlyEncrypted { get; set; }
    public int ImapPasswordsAlreadyEncrypted { get; set; }
    public int ImapPasswordsNull { get; set; }
    public int SmtpPasswordsNewlyEncrypted { get; set; }
    public int SmtpPasswordsAlreadyEncrypted { get; set; }
    public int SmtpPasswordsNull { get; set; }
    public int OAuthSecretsNewlyEncrypted { get; set; }
    public int OAuthSecretsAlreadyEncrypted { get; set; }
    public int OAuthSecretsNull { get; set; }
    public int SeparateSmtpPasswordsNewlyEncrypted { get; set; }
    public int SeparateSmtpPasswordsAlreadyEncrypted { get; set; }
    public int SeparateSmtpPasswordsNull { get; set; }
    public int SeparateOAuthSecretsNewlyEncrypted { get; set; }
    public int SeparateOAuthSecretsAlreadyEncrypted { get; set; }
    public int SeparateOAuthSecretsNull { get; set; }
}

/// <summary>
/// Console application entry point for migration
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Configure services
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add DbContext
        services.AddDbContext<ComplaintDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Infrastructure services (includes IEncryptionService)
        services.AddScoped<ComplaintManagement.Infrastructure.Services.AesEncryptionService>();
        services.AddScoped<IEncryptionService, ComplaintManagement.Infrastructure.Services.AesEncryptionService>();

        // Add migration script
        services.AddScoped<PasswordMigrationScript>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Execute migration
        using (var scope = serviceProvider.CreateScope())
        {
            var migration = scope.ServiceProvider.GetRequiredService<PasswordMigrationScript>();

            Console.WriteLine("========================================");
            Console.WriteLine("PASSWORD MIGRATION TOOL");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("⚠️  WARNING: This will encrypt all plaintext passwords in the database.");
            Console.WriteLine("⚠️  Ensure you have a database backup before proceeding!");
            Console.WriteLine();
            Console.Write("Continue? (yes/no): ");

            var response = Console.ReadLine();
            if (response?.ToLower() != "yes")
            {
                Console.WriteLine("Migration cancelled by user.");
                return;
            }

            Console.WriteLine();
            await migration.ExecuteAsync();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
