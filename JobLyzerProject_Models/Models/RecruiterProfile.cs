using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class RecruiterProfile
{
    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public bool IsPrimaryContact { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
