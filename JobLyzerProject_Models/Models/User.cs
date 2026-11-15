using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public Guid RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual RecruiterProfile? RecruiterProfile { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();

    public virtual StudentProfile? StudentProfile { get; set; }

    public virtual ICollection<StudentSkillMetric> StudentSkillMetrics { get; set; } = new List<StudentSkillMetric>();
}
