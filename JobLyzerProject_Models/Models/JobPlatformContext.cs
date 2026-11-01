using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JobLyzerProject_Models.Models;

public partial class JobPlatformContext : DbContext
{
    public JobPlatformContext()
    {
    }

    public JobPlatformContext(DbContextOptions<JobPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Experience> Experiences { get; set; }

    public virtual DbSet<RecruiterProfile> RecruiterProfiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<StudentSkill> StudentSkills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-E408G7J\\SQLEXPRESS;Database=JobPlatformDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.Website).HasMaxLength(512);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.Property(e => e.Degree).HasMaxLength(256);
            entity.Property(e => e.Institution).HasMaxLength(256);

            entity.HasOne(d => d.StudentUser).WithMany(p => p.Educations).HasForeignKey(d => d.StudentUserId);
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.Property(e => e.CompanyName).HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(256);

            entity.HasOne(d => d.StudentUser).WithMany(p => p.Experiences).HasForeignKey(d => d.StudentUserId);
        });

        modelBuilder.Entity<RecruiterProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.Company).WithMany(p => p.RecruiterProfiles).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.User).WithOne(p => p.RecruiterProfile).HasForeignKey<RecruiterProfile>(d => d.UserId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6CA62CA1F").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile).HasForeignKey<StudentProfile>(d => d.UserId);
        });

        modelBuilder.Entity<StudentSkill>(entity =>
        {
            entity.Property(e => e.SkillName).HasMaxLength(100);

            entity.HasOne(d => d.StudentUser).WithMany(p => p.StudentSkills).HasForeignKey(d => d.StudentUserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F1952FEE").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
