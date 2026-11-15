using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class SkillTag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<StudentSkillMetric> StudentSkillMetrics { get; set; } = new List<StudentSkillMetric>();
}
