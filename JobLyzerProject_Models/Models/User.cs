using System;
using System.Collections.Generic;

namespace JobLyzerProject_Models.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int UserType { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual RecruiterProfile? RecruiterProfile { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual StudentProfile? StudentProfile { get; set; }
}
