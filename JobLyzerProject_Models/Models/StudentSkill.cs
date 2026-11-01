using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class StudentSkill
{
    public int Id { get; set; }

    public int StudentUserId { get; set; }

    public string SkillName { get; set; } = null!;

    public int ProficiencyPercentage { get; set; }

    public virtual StudentProfile StudentUser { get; set; } = null!;
}
