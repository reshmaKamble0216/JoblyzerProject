using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Experience
{
    public int Id { get; set; }

    public int StudentUserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual StudentProfile StudentUser { get; set; } = null!;
}
