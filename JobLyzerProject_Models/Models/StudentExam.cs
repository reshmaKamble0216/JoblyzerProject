using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class StudentExam
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ExamId { get; set; }

    public string Status { get; set; } = null!;

    public decimal? ScorePercentage { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
