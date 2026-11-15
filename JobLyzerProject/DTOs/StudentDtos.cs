
using System.ComponentModel.DataAnnotations;

namespace JobLyzerProject.DTOs
{
    public class StudentRegisterRequestDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Headline { get; set; }
        public string? ResumeUrl { get; set; }
        public int? GraduationYear { get; set; }
    }


    public class StudentResumeDto
    {
        public string? Headline { get; set; }
        public string? ResumeUrl { get; set; }
        public int? GraduationYear { get; set; }
    }

    public class StudentEducationDto
    {
        [Required]
        public string Institution { get; set; } = string.Empty;

        [Required]
        public string Degree { get; set; } = string.Empty;

        [Required]
        public int GraduationYear { get; set; }
    }

    public class StudentExperienceDto
    {
        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string RoleTitle { get; set; } = string.Empty;

        [Required]
        public string DurationText { get; set; } = string.Empty;
    }

    public class StudentSkillDto
    {
        [Required]
        public string SkillName { get; set; } = string.Empty;

        [Range(1, 100)]
        public decimal ProficiencyScore { get; set; }
    }
}
