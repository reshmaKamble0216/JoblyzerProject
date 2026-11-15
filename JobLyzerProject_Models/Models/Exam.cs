using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Exam
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal PassingPercentage { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
}
