using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class RecruiterProfile
{
    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? CompanyWebsite { get; set; }

    public virtual User User { get; set; } = null!;
}
