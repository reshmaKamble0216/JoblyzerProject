using System.ComponentModel.DataAnnotations;

namespace JobLyzerProject.ViewModels
{
    // 1. View Model for Login Flow
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    // 2. View Model for Student Signup Flow
 
        public class RegisterStudentViewModel
        {
            [Required(ErrorMessage = "Email address is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }

            // --- RESUME BUILDER INITIALIZATION ---
            [Required(ErrorMessage = "Please specify initial resume storage path location or label")]
            [Display(Name = "Initial Resume Headline / File Reference")]
            public string ResumeFilePath { get; set; }

            // --- EDUCATION HISTORY CAPTURE ---
            [Required(ErrorMessage = "Highest level institution name is required")]
            public string HighestInstitution { get; set; }

            [Required(ErrorMessage = "Degree or specialization name is required")]
            public string DegreeSpecialization { get; set; }

            // --- WORK EXPERIENCE CAPTURE ---
            [Required(ErrorMessage = "Latest corporate company title name is required (or 'Fresher')")]
            public string LatestCompany { get; set; }

            [Required(ErrorMessage = "Latest structural role title name is required (or 'Student')")]
            public string LatestRole { get; set; }

            // --- INTERACTIVE SKILL DASHBOARD MATRIX SEEDING ---
            [Required(ErrorMessage = "Core technical skill classification name is mandatory")]
            public string CoreTechnicalSkill { get; set; }

            [Required(ErrorMessage = "Skill proficiency metric level rating is required")]
            [Range(1, 100, ErrorMessage = "Proficiency level metric must be rated between 1 and 100%")]
            public int CoreSkillProficiency { get; set; }
        }
    

    // 3. View Model for Recruiter Signup Flow
    public class RegisterRecruiterViewModel
    {
        [Required(ErrorMessage = "Corporate Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid corporate email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Company Website URL is required")]
        [Url(ErrorMessage = "Invalid URL format. Include http:// or https://")]
        public string CompanyWebsite { get; set; }

        [Required(ErrorMessage = "Please specify team size capacity")]
        [Range(1, 100, ErrorMessage = "Team allocation must be between 1 and 100")]
        public int TeamSizeAllocation { get; set; }
    }

    // 4. View Model for Admin Signup Flow
    public class RegisterAdminViewModel
    {
        [Required(ErrorMessage = "Admin Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "System Security Key is mandatory")]
        public string AdminSecurityToken { get; set; }
    }
}
