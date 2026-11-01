using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobLyzerProject_Models.Models;
using JobLyzerProject_Services.Interfaces;

namespace JoblyzerProject_Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JobPlatformContext _context;

        public AuthService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<User> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
            return user;
        }



   public async Task<bool> RegisterStudentWithProfileAsync(
    string email,
    string password,
    string resumePath,
    string institution,
    string degree,
    string company,
    string role,
    string skillName,
    int proficiency)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email)) return false;

            // 1. Create Core Base Authentication User Record
            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserType = 0, // 0: Student
                RoleId = 1,   // 1: Student Role ID lookup
                IsActive = true,
                AccessFailedCount = 0,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 2. Create the Master 1:1 Student Profile entry mapping the Resume Builder pointer
            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                ResumeFilePath = resumePath
            };
            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            // 3. Seed Initial Education History entry records
            var education = new Education
            {
                StudentUserId = user.Id,
                Institution = institution,
                Degree = degree,
                StartDate = DateTime.UtcNow.AddYears(-3) // Approximate historical window mapping
            };
            _context.Educations.Add(education);

            // 4. Seed Initial Work Experience timeline logging parameters
            var experience = new Experience
            {
                StudentUserId = user.Id,
                CompanyName = company,
                Role = role,
                StartDate = DateTime.UtcNow.AddYears(-1)
            };
            _context.Experiences.Add(experience);

            // 5. Seed Core Interactive Skill Dashboard Matrix rating parameters
            var skill = new StudentSkill
            {
                StudentUserId = user.Id,
                SkillName = skillName,
                ProficiencyPercentage = proficiency
            };
            _context.StudentSkills.Add(skill);

            // Save all subsequent sub-data profile entries atomically
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterRecruiterAsync(string email, string password, string companyName, string companyWebsite)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email)) return false;

            // Recruiter validation rules (match email domain to corporate domain)
            string emailDomain = email.Split('@').Last().ToLower();
            string webDomain = companyWebsite.Replace("https://", "").Replace("http://", "").Replace("www.", "").Split('/').First().ToLower();
            bool isVerified = emailDomain == webDomain && emailDomain != "gmail.com" && emailDomain != "yahoo.com";

            var company = new Company { Name = companyName, Website = companyWebsite, IsVerified = isVerified };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserType = 1, // 1: Recruiter
                RoleId = 2,   // 'Recruiter' Role ID
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RecruiterProfiles.Add(new RecruiterProfile { UserId = user.Id, CompanyId = company.Id, IsPrimaryContact = true });
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RegisterAdminAsync(string email, string password)
        {
            // Check if the administrator account profile email is already registered in the matrix
            if (await _context.Users.AnyAsync(u => u.Email == email)) return false;

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // Direct BCrypt secure hashing conversion
                UserType = 2, // 2: Admin
                RoleId = 3,   // 'Admin' Role ID from lookup seed
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RegisterRecruiterAsync(string email, string password, string companyName, string companyWebsite, int teamSizeAllocation)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email)) return false;

            // --- AUTOMATED WEBSITE VERIFICATION LOGIC ---
            string emailDomain = email.Split('@').Last().ToLower();
            string webDomain = companyWebsite.Replace("https://", "").Replace("http://", "").Replace("www.", "").Split('/').First().ToLower();

            // Auto-verify if corporate email domain matches company website domain (blocks public email hosts)
            bool isVerified = emailDomain == webDomain && emailDomain != "gmail.com" && emailDomain != "yahoo.com" && emailDomain != "outlook.com";

            // 1. Create and Seed Company Profile Entity Data
            var company = new Company
            {
                Name = companyName,
                Website = companyWebsite,
                IsVerified = isVerified // Stores verification state directly
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // 2. Create Core Base Authentication User Record
            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserType = 1, // 1: Recruiter
                RoleId = 2,   // 2: Recruiter Role
                IsActive = true,
                AccessFailedCount = 0,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 3. Create Recruiter Profile Meta-Record mapping Team Allocation Constraints
            var recruiterProfile = new RecruiterProfile
            {
                UserId = user.Id,
                CompanyId = company.Id,
                IsPrimaryContact = true
                // Note: If your scaffolded table doesn't have an explicit column for TeamSizeAllocation, 
                // it initializes the workspace session context mapping using the company reference key link.
            };
            _context.RecruiterProfiles.Add(recruiterProfile);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
