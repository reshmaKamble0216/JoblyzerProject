using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class StudentProfile
{
    public Guid UserId { get; set; }

    public string? Headline { get; set; }

    public string? ResumeUrl { get; set; }

    public int? GraduationYear { get; set; }

    public virtual User User { get; set; } = null!;
}
