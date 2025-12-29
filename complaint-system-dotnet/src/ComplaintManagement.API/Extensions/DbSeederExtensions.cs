using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplaintManagement.API.Extensions;

public static class DbSeederExtensions
{
    public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ComplaintDbContext>();
            var logger = services.GetRequiredService<ILogger<DbSeeder>>();
            var encryptionService = services.GetRequiredService<IEncryptionService>();

            // FIRST: Check if database tables already exist (created by installer SQL script)
            // This MUST happen before any EF Core migration operations to avoid duplicate column errors
            var tableCount = await GetTableCountAsync(context);
            logger.LogInformation("Database table count: {TableCount}", tableCount);

            if (tableCount >= 50)
            {
                // Database schema was created by the installer's SQL migration script
                // Skip EF Core migration entirely - just run seeding
                logger.LogInformation("Database schema already exists ({TableCount} tables). Skipping EF Core migrations.", tableCount);
            }
            else if (tableCount > 0 && tableCount < 50)
            {
                // Partial schema - something went wrong
                logger.LogWarning("Database has partial schema ({TableCount} tables). Expected 50+. May need manual intervention.", tableCount);

                // Try to apply remaining migrations
                try
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Attempting to apply {Count} pending migrations...", pendingMigrations.Count());
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Migrations applied successfully.");
                    }
                }
                catch (Exception migEx)
                {
                    logger.LogError(migEx, "Failed to apply migrations. Database may require manual setup.");
                    throw;
                }
            }
            else
            {
                // Empty database - apply migrations normally
                logger.LogInformation("Empty database detected. Applying EF Core migrations...");
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
                catch (Exception migEx)
                {
                    logger.LogError(migEx, "Failed to apply migrations to empty database.");
                    throw;
                }
            }

            // Run seeder to create default data (roles, permissions, admin user, etc.)
            logger.LogInformation("Running database seeder...");
            var seeder = new DbSeeder(context, logger, encryptionService);
            await seeder.SeedAsync();
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DbSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }

        return app;
    }

    private static async Task<int> GetTableCountAsync(ComplaintDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            var result = await command.ExecuteScalarAsync();

            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch
        {
            return 0;
        }
    }
}
