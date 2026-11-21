using JobLyzerProject_Models.Models;
using JobLyzerProject_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JobLyzerProject.Controllers
{
    public record LoginRequest(string Email, string Password);

    public record RegisterStudentDto(
        string FullName,
        string Email,
        string Password,
        string? Headline,
        int? GraduationYear,
        string? ResumeUrl
    );

    public record RegisterRecruiterRequest(
        string FullName,
        string Email,
        string Password,
        string CompanyName,
        string CompanyWebsite
    );

    public record RegisterAdminDto(
        string FullName,
        string Email,
        string Password
    );

    [ApiController]
    [Route("api/[controller]")]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly JobPlatformContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AccountController(
            JobPlatformContext context,
            IConfiguration config,
            IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password." });

                bool isPasswordValid = user.PasswordHash.StartsWith("$2")
                    ? BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)
                    : user.PasswordHash == dto.Password;

                if (!isPasswordValid)
                    return Unauthorized(new { message = "Invalid email or password." });

                var roleName = user.Role?.Name ?? "Student";
                var token = GenerateJwtToken(user, roleName);

                return Ok(new
                {
                    token,
                    userId = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    role = roleName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            if (studentRole == null)
            {
                studentRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Student"
                };
                _context.Roles.Add(studentRole);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = studentRole.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                Headline = dto.Headline,
                GraduationYear = dto.GraduationYear,
                ResumeUrl = dto.ResumeUrl
            };
            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            bool emailSent = false;
            try
            {
                await _emailService.SendRegistrationEmailAsync(user.Email, user.FullName, "Student");
                emailSent = true;
            }
            catch
            {
                emailSent = false;
            }

            var token = GenerateJwtToken(user, "Student");

            return Ok(new
            {
                message = "Student registered successfully.",
                emailSent,
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = "Student"
            });
        }

        [HttpPost("register-recruiter")]
        public async Task<IActionResult> RegisterRecruiter([FromBody] RegisterRecruiterRequest req)
        {
            if (await _context.Users.AnyAsync(u => u.Email == req.Email))
                return BadRequest(new { message = "Email is already registered." });

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
            if (role == null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Recruiter"
                };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                CompanyName = req.CompanyName,
                CompanyWebsite = req.CompanyWebsite
            });
            await _context.SaveChangesAsync();

            bool emailSent = false;
            try
            {
                await _emailService.SendRegistrationEmailAsync(user.Email, user.FullName, "Recruiter");
                emailSent = true;
            }
            catch
            {
                emailSent = false;
            }

            var token = GenerateJwtToken(user, "Recruiter");

            return Ok(new
            {
                message = "Recruiter registered successfully.",
                emailSent,
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = "Recruiter"
            });
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin"
                };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            bool emailSent = false;
            try
            {
                await _emailService.SendRegistrationEmailAsync(user.Email, user.FullName, "Admin");
                emailSent = true;
            }
            catch
            {
                emailSent = false;
            }

            var token = GenerateJwtToken(user, "Admin");

            return Ok(new
            {
                message = "Admin registered successfully.",
                emailSent,
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = "Admin"
            });
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var rawKey = _config["Jwt:Key"] ?? "SuperSecretJobLyzerKey2026_MustBe32Chars!";
            if (rawKey.Length < 32)
                rawKey = rawKey.PadRight(32, '_');

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "JobLyzerAPI",
                audience: _config["Jwt:Audience"] ?? "JobLyzerUI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
