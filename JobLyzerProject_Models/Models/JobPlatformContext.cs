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

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobApplication> JobApplications { get; set; }

    public virtual DbSet<JobSkill> JobSkills { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RecruiterProfile> RecruiterProfiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SkillTag> SkillTags { get; set; }

    public virtual DbSet<StudentExam> StudentExams { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<StudentSkillMetric> StudentSkillMetrics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=89.106.69.104;Database=JobLyzerDB;User Id=jobuser;Password=YSAAS#2026;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("jobuser");

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exams__3214EC07F6911E49");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DurationMinutes).HasDefaultValue(60);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PassingPercentage)
                .HasDefaultValue(60.00m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Jobs__3214EC07D3D763CC");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.PostedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Recruiter).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.RecruiterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Jobs__RecruiterI__75A278F5");
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobAppli__3214EC071E03BC85");

            entity.HasIndex(e => e.JobId, "IX_JobApplications_JobId");

            entity.HasIndex(e => new { e.JobId, e.StudentId }, "UQ_Job_Student").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Applied");

            entity.HasOne(d => d.Job).WithMany(p => p.JobApplications)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JobApplic__JobId__01142BA1");

            entity.HasOne(d => d.Student).WithMany(p => p.JobApplications)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobApplic__Stude__02084FDA");
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillTagId }).HasName("PK__JobSkill__6A2CAD3C2139FC1B");

            entity.Property(e => e.MinimumProficiencyRequired)
                .HasDefaultValue(60.00m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JobSkills__JobId__7A672E12");

            entity.HasOne(d => d.SkillTag).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.SkillTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobSkills__Skill__7B5B524B");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Options__3214EC07CAC8E1F9");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.OptionText).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Options__Questio__6A30C649");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07698794FA");

            entity.HasIndex(e => e.ExamId, "IX_Questions_ExamId");

            entity.HasIndex(e => e.SkillTagId, "IX_Questions_SkillTagId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Points).HasDefaultValue(1);

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__Questions__ExamI__6477ECF3");

            entity.HasOne(d => d.SkillTag).WithMany(p => p.Questions)
                .HasForeignKey(d => d.SkillTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Questions__Skill__656C112C");
        });

        modelBuilder.Entity<RecruiterProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Recruite__1788CC4CFF19C28B");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.CompanyWebsite).HasMaxLength(250);

            entity.HasOne(d => d.User).WithOne(p => p.RecruiterProfile)
                .HasForeignKey<RecruiterProfile>(d => d.UserId)
                .HasConstraintName("FK__Recruiter__UserI__5629CD9C");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07333AA7AB");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6B7051F3F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SkillTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SkillTag__3214EC07E8D8C785");

            entity.HasIndex(e => e.Name, "UQ__SkillTag__737584F65C776ACD").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<StudentExam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentE__3214EC07CFE67539");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ScorePercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("InProgress");

            entity.HasOne(d => d.Exam).WithMany(p => p.StudentExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentEx__ExamI__6FE99F9F");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentExams)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentEx__Stude__6EF57B66");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__StudentP__1788CC4CFB983CC8");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Headline).HasMaxLength(200);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .HasConstraintName("FK__StudentPr__UserI__534D60F1");
        });

        modelBuilder.Entity<StudentSkillMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentS__3214EC07D6553C97");

            entity.HasIndex(e => e.StudentId, "IX_StudentSkillMetrics_StudentId");

            entity.HasIndex(e => new { e.StudentId, e.SkillTagId }, "UQ_Student_Skill").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ProficiencyScore).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.SkillTag).WithMany(p => p.StudentSkillMetrics)
                .HasForeignKey(d => d.SkillTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentSk__Skill__09A971A2");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSkillMetrics)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__StudentSk__Stude__08B54D69");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07708C0977");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534CFFC0227").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(150);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4F7CD00D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
