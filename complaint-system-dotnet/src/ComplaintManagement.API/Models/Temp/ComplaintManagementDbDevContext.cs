using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Models.Temp;

public partial class ComplaintManagementDbDevContext : DbContext
{
    public ComplaintManagementDbDevContext()
    {
    }

    public ComplaintManagementDbDevContext(DbContextOptions<ComplaintManagementDbDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ComplaintCategory> ComplaintCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplaintCategory>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_ComplaintCategories_Code").IsUnique();

            entity.HasIndex(e => e.DisplayOrder, "IX_ComplaintCategories_DisplayOrder");

            entity.HasIndex(e => e.ParentCategoryId, "IX_ComplaintCategories_ParentCategoryId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasForeignKey(d => d.ParentCategoryId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
