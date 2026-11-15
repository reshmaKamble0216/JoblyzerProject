using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Question
{
    public Guid Id { get; set; }

    public Guid ExamId { get; set; }

    public Guid SkillTagId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int Points { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual SkillTag SkillTag { get; set; } = null!;
}
