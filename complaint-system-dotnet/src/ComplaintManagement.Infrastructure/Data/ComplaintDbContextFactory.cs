using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ComplaintManagement.Infrastructure.Data;

/// <summary>
/// Design-time factory for ComplaintDbContext
/// Used by EF Core tools for migrations
/// </summary>
public class ComplaintDbContextFactory : IDesignTimeDbContextFactory<ComplaintDbContext>
{
    public ComplaintDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ComplaintDbContext>();

        // Use the connection string from appsettings.json
        optionsBuilder.UseSqlServer(
            "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=ComplaintManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=60;Command Timeout=600",
            b => b.MigrationsAssembly("ComplaintManagement.Infrastructure"));

        return new ComplaintDbContext(optionsBuilder.Options);
    }
}
