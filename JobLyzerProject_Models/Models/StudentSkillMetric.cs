using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class StudentSkillMetric
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid SkillTagId { get; set; }

    public int TotalQuestionsAttempted { get; set; }

    public int TotalCorrectAnswers { get; set; }

    public decimal ProficiencyScore { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual SkillTag SkillTag { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
