using ComplaintManagement.Domain.Entities.Oryggi;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Data;

public class OryggiDbContext : DbContext
{
    public OryggiDbContext(DbContextOptions<OryggiDbContext> options) : base(options)
    {
    }

    public DbSet<CompanyMaster> CompanyMaster { get; set; }
    public DbSet<BranchMaster> BranchMaster { get; set; }
    public DbSet<DeptMaster> DeptMaster { get; set; }
    public DbSet<SectionMaster> SectionMaster { get; set; }
    public DbSet<DesignationMaster> DesignationMaster { get; set; }
    public DbSet<EmployeeMaster> EmployeeMaster { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CompanyMaster configuration
        modelBuilder.Entity<CompanyMaster>(entity =>
        {
            entity.ToTable("CompanyMaster");
            entity.HasKey(e => e.Ccode);

            entity.HasMany(e => e.Branches)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.Ccode)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // BranchMaster configuration
        modelBuilder.Entity<BranchMaster>(entity =>
        {
            entity.ToTable("BranchMaster");
            entity.HasKey(e => e.BranchCode);

            entity.HasMany(e => e.Departments)
                .WithOne(e => e.Branch)
                .HasForeignKey(e => e.BranchCode)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // DeptMaster configuration
        modelBuilder.Entity<DeptMaster>(entity =>
        {
            entity.ToTable("DeptMaster");
            entity.HasKey(e => e.Dcode);

            entity.HasMany(e => e.Sections)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.Dcode)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // SectionMaster configuration
        modelBuilder.Entity<SectionMaster>(entity =>
        {
            entity.ToTable("SectionMaster");
            entity.HasKey(e => e.SecCode);

            entity.HasMany(e => e.Employees)
                .WithOne(e => e.Section)
                .HasForeignKey(e => e.SecCode)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // DesignationMaster configuration
        modelBuilder.Entity<DesignationMaster>(entity =>
        {
            entity.ToTable("DesignationMaster");
            entity.HasKey(e => e.DesigCode);

            entity.HasMany(e => e.Employees)
                .WithOne(e => e.Designation)
                .HasForeignKey(e => e.DesigCode)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // EmployeeMaster configuration
        modelBuilder.Entity<EmployeeMaster>(entity =>
        {
            entity.ToTable("EmployeeMaster");
            entity.HasKey(e => e.Ecode);

            // Self-referencing relationship for reporting hierarchy
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ReportingHeadEcode)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
