using JobLyzerProject_Models.Models;
using JobLyzerProject_Services.Interfaces;
using JobLyzerProject_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JoblyzerProject_Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JobPlatformContext _context;

        public AuthService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<(bool Success, string Message)> RegisterStudentAsync(
            string fullName,
            string email,
            string password,
            string? headline,
            string? resumeUrl,
            int? graduationYear)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Email is already registered.");

            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            if (studentRole == null)
                return (false, "Student role not found in database.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = studentRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.StudentProfiles.Add(new StudentProfile
            {
                UserId = user.Id,
                Headline = headline,
                ResumeUrl = resumeUrl,
                GraduationYear = graduationYear
            });

            await _context.SaveChangesAsync();
            return (true, "Student registered successfully.");
        }

        public async Task<(bool Success, string Message)> RegisterRecruiterAsync(
            string fullName,
            string email,
            string password,
            string companyName,
            string? companyWebsite)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Corporate email is already registered.");

            var recruiterRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
            if (recruiterRole == null)
                return (false, "Recruiter role not found in database.");

            bool isVerified = false;
            if (!string.IsNullOrWhiteSpace(companyWebsite) && email.Contains('@'))
            {
                string emailDomain = email.Split('@').Last().ToLower();
                string webDomain = companyWebsite
                    .Replace("https://", "")
                    .Replace("http://", "")
                    .Replace("www.", "")
                    .Split('/')
                    .First()
                    .ToLower();

                isVerified = emailDomain == webDomain
                    && emailDomain != "gmail.com"
                    && emailDomain != "yahoo.com"
                    && emailDomain != "outlook.com";
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = recruiterRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                CompanyName = companyName,
                CompanyWebsite = companyWebsite,
               
            });

            await _context.SaveChangesAsync();
            return (true, "Recruiter registered successfully.");
        }

        public async Task<(bool Success, string Message)> RegisterAdminAsync(
            string fullName,
            string email,
            string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Admin email is already registered.");

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
                return (false, "Admin role not found in database.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (true, "Admin registered successfully.");
        }

        public Task<bool> RegisterStudentWithProfileAsync(string email, string password, string resumePath, string institution, string degree, string company, string role, string skillName, int proficiency)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterRecruiterAsync(string email, string password, string companyName, string companyWebsite, int teamSizeAllocation)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterAdminAsync(string email, string password)
        {
            throw new NotImplementedException();
        }
    }
}
