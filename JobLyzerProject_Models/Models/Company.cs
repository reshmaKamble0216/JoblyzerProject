using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Website { get; set; }

    public bool IsVerified { get; set; }

    public virtual ICollection<RecruiterProfile> RecruiterProfiles { get; set; } = new List<RecruiterProfile>();
}
