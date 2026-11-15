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
    public record AdminRegisterDto(
        string FullName,
        string Email,
        string Password,
        string AdminSecretCode
    );

    public record AdminLoginDto(
        string Email,
        string Password
    );

    public record ChangeUserRoleDto(
        Guid UserId,
        string NewRoleName
    );

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly JobPlatformContext _context;
        private readonly IConfiguration _config;

        public AdminController(JobPlatformContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 1. POST: api/Admin/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterDto dto)
        {
            var expectedSecret = _config["Admin:SecretCode"] ?? "ADMIN2026";
            if (dto.AdminSecretCode != expectedSecret)
                return Unauthorized(new { message = "Invalid Admin Secret Code." });

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
                PasswordHash = dto.Password,
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Admin registered successfully.",
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = "Admin"
            });
        }

        // 2. POST: api/Admin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.PasswordHash == dto.Password);

            if (user == null || user.Role?.Name != "Admin")
                return Unauthorized(new { message = "Invalid admin credentials." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = "Admin"
            });
        }

        // 3. GET: api/Admin/dashboard-stats
        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users.CountAsync(u => u.Role.Name == "Student");
            var totalRecruiters = await _context.Users.CountAsync(u => u.Role.Name == "Recruiter");
            var totalAdmins = await _context.Users.CountAsync(u => u.Role.Name == "Admin");

            return Ok(new
            {
                totalUsers,
                totalStudents,
                totalRecruiters,
                totalAdmins
            });
        }

        // 4. GET: api/Admin/users
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.RecruiterProfile)
                .Include(u => u.StudentProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.Name.ToLower() == role.ToLower());

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.Name,
                    u.CreatedAt,
                    CompanyName = u.RecruiterProfile != null ? u.RecruiterProfile.CompanyName : null,
                    Headline = u.StudentProfile != null ? u.StudentProfile.Headline : null
                })
                .ToListAsync();

            return Ok(users);
        }

        // 5. PUT: api/Admin/users/role
        [Authorize(Roles = "Admin")]
        [HttpPut("users/role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var targetRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == dto.NewRoleName.ToLower());

            if (targetRole == null)
            {
                targetRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = dto.NewRoleName
                };
                _context.Roles.Add(targetRole);
                await _context.SaveChangesAsync();
            }

            user.RoleId = targetRole.Id;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User role updated to {targetRole.Name}." });
        }

        // 6. DELETE: api/Admin/users/{userId}
        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var currentAdminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentAdminIdStr, out Guid currentAdminId) && currentAdminId == userId)
                return BadRequest(new { message = "You cannot delete your own admin account." });

            var user = await _context.Users
                .Include(u => u.RecruiterProfile)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.RecruiterProfile != null)
                _context.RecruiterProfiles.Remove(user.RecruiterProfile);

            if (user.StudentProfile != null)
                _context.StudentProfiles.Remove(user.StudentProfile);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User removed successfully." });
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
                new Claim(ClaimTypes.Role, "Admin")
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
