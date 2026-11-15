using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class JobApplication
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid StudentId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime AppliedAt { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
