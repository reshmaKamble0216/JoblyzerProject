using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class Education
{
    public int Id { get; set; }

    public int StudentUserId { get; set; }

    public string Institution { get; set; } = null!;

    public string Degree { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual StudentProfile StudentUser { get; set; } = null!;
}
