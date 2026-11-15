using JobLyzerProject_Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JobLyzerProject.Controllers
{
    public record RecruiterRegisterDto(
        string FullName,
        string Email,
        string Password,
        string CompanyName,
        string? CompanyWebsite
    );

    public record RecruiterLoginDto(
        string Email,
        string Password
    );

    public record CompanyUpdateDto(
        string CompanyName,
        string? CompanyWebsite
    );

    public record TeamMemberAddDto(
        string FullName,
        string Email,
        string TemporaryPassword
    );

    [ApiController]
    [Route("api/[controller]")]
    public class RecruiterController : ControllerBase
    {
        private readonly JobPlatformContext _context;
        private readonly IConfiguration _config;

        public RecruiterController(JobPlatformContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 1. POST: api/Recruiter/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RecruiterRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var recruiterRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
            if (recruiterRole == null)
            {
                recruiterRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Recruiter"
                };
                _context.Roles.Add(recruiterRole);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = dto.Password,
                RoleId = recruiterRole.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new RecruiterProfile
            {
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                CompanyWebsite = dto.CompanyWebsite
            };
            _context.RecruiterProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Recruiter registered successfully.",
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                companyName = profile.CompanyName,
                role = "Recruiter"
            });
        }

        // 2. POST: api/Recruiter/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RecruiterLoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.RecruiterProfile)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.PasswordHash == dto.Password);

            if (user == null || user.Role?.Name != "Recruiter")
                return Unauthorized(new { message = "Invalid recruiter credentials." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                companyName = user.RecruiterProfile?.CompanyName,
                companyWebsite = user.RecruiterProfile?.CompanyWebsite,
                role = "Recruiter"
            });
        }

        // 3. GET: api/Recruiter/profile
        [Authorize(Roles = "Recruiter")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var profile = await _context.RecruiterProfiles
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Recruiter profile not found." });

            var teamMembers = await _context.RecruiterProfiles
                .Include(r => r.User)
                .Where(r => r.CompanyName.ToLower() == profile.CompanyName.ToLower())
                .Select(r => new
                {
                    r.UserId,
                    r.User.FullName,
                    r.User.Email,
                    r.User.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                userId = profile.UserId,
                fullName = profile.User.FullName,
                email = profile.User.Email,
                companyName = profile.CompanyName,
                companyWebsite = profile.CompanyWebsite,
                teamMembers
            });
        }

        // 4. PUT: api/Recruiter/company
        [Authorize(Roles = "Recruiter")]
        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyUpdateDto dto)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var profile = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (profile == null)
                return NotFound(new { message = "Recruiter profile not found." });

            profile.CompanyName = dto.CompanyName;
            profile.CompanyWebsite = dto.CompanyWebsite;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Company profile updated successfully.",
                companyName = profile.CompanyName,
                companyWebsite = profile.CompanyWebsite
            });
        }

        // 5. POST: api/Recruiter/verify-website
        [Authorize(Roles = "Recruiter")]
        [HttpPost("verify-website")]
        public async Task<IActionResult> VerifyWebsite()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (!Guid.TryParse(userIdStr, out Guid userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            var profile = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (profile == null || string.IsNullOrWhiteSpace(profile.CompanyWebsite))
                return BadRequest(new { verified = false, message = "Company website is not set." });

            var emailDomain = email.Split('@').Last().ToLower();
            var rawUrl = profile.CompanyWebsite.StartsWith("http")
                ? profile.CompanyWebsite
                : $"https://{profile.CompanyWebsite}";

            if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var parsedUri))
                return BadRequest(new { verified = false, message = "Invalid website URL format." });

            var websiteHost = parsedUri.Host.Replace("www.", "").ToLower();

            if (emailDomain == websiteHost)
                return Ok(new { verified = true, message = "Company website verified via corporate email domain!" });

            return BadRequest(new
            {
                verified = false,
                message = $"Email domain (@{emailDomain}) does not match company website ({websiteHost})."
            });
        }

        // 6. POST: api/Recruiter/team
        [Authorize(Roles = "Recruiter")]
        [HttpPost("team")]
        public async Task<IActionResult> AddTeamMember([FromBody] TeamMemberAddDto dto)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var currentRecruiter = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (currentRecruiter == null)
                return NotFound(new { message = "Recruiter profile not found." });

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "A user with this email already exists." });

            var recruiterRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Recruiter");
            if (recruiterRole == null)
                return BadRequest(new { message = "Recruiter role not configured." });

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = dto.TemporaryPassword,
                RoleId = recruiterRole.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _context.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = newUser.Id,
                CompanyName = currentRecruiter.CompanyName,
                CompanyWebsite = currentRecruiter.CompanyWebsite
            });
            await _context.SaveChangesAsync();

             return Ok(new
                            {
                                message = $"{dto.FullName} added to {currentRecruiter.CompanyName} team.",
                                memberUserId = newUser.Id
                            });
        }

        // 7. DELETE: api/Recruiter/team/{memberUserId}
        [Authorize(Roles = "Recruiter")]
        [HttpDelete("team/{memberUserId:guid}")]
        public async Task<IActionResult> RemoveTeamMember(Guid memberUserId)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid currentUserId))
                return Unauthorized();

            if (currentUserId == memberUserId)
                return BadRequest(new { message = "You cannot remove yourself from the team." });

            var currentRecruiter = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(r => r.UserId == currentUserId);

            if (currentRecruiter == null)
                return NotFound(new { message = "Recruiter profile not found." });

            var targetMember = await _context.RecruiterProfiles
                .FirstOrDefaultAsync(r => r.UserId == memberUserId && r.CompanyName == currentRecruiter.CompanyName);

            if (targetMember == null)
                return NotFound(new { message = "Team member not found in your company." });

            _context.RecruiterProfiles.Remove(targetMember);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Team member removed successfully." });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "JobLyzerSuperSecretJwtKey_Minimum32Chars!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Recruiter")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "JobLyzerAPI",
                audience: _config["Jwt:Audience"] ?? "JobLyzerAngular",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
