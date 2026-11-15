using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Job
{
    public Guid Id { get; set; }

    public Guid RecruiterId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Location { get; set; }

    public bool IsActive { get; set; }

    public DateTime PostedAt { get; set; }

    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public virtual User Recruiter { get; set; } = null!;
}
