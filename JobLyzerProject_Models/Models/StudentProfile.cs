using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class StudentProfile
{
    public int UserId { get; set; }

    public string? ResumeFilePath { get; set; }

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();

    public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();

    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();

    public virtual User User { get; set; } = null!;
}
