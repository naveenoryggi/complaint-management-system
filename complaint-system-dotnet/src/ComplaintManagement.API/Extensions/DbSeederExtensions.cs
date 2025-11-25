using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

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

            // Apply pending migrations
            logger.LogInformation("Applying pending migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            // Run seeder
            var seeder = new DbSeeder(context, logger, encryptionService);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DbSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }

        return app;
    }
}
