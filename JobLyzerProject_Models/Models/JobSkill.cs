using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class JobSkill
{
    public Guid JobId { get; set; }

    public Guid SkillTagId { get; set; }

    public decimal MinimumProficiencyRequired { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual SkillTag SkillTag { get; set; } = null!;
}
