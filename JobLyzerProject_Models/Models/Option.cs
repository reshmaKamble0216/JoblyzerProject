using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Option
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; } = null!;
}
