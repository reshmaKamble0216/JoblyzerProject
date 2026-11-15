using JobLyzerProject_Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobLyzerProject.Controllers
{
    public record StudentSkillDto(string SkillName, decimal ProficiencyScore);
    public record UpdateProfileDto(string? Headline, int? GraduationYear);

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly JobPlatformContext _context;

        public StudentController(JobPlatformContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var profile = await _context.StudentProfiles
                .Where(s => s.UserId == userId)
                .Select(s => new { s.UserId, s.Headline, s.GraduationYear, s.ResumeUrl })
                .FirstOrDefaultAsync();

            var skills = await _context.StudentSkillMetrics
                .Include(m => m.SkillTag)
                .Where(m => m.StudentId == userId)
                .Select(m => new
                {
                    m.Id,
                    SkillName = m.SkillTag.Name,
                    m.ProficiencyScore,
                    m.TotalQuestionsAttempted,
                    m.TotalCorrectAnswers
                })
                .ToListAsync();

            return Ok(new { profile, skills });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (profile == null) return NotFound();

            profile.Headline = dto.Headline;
            profile.GraduationYear = dto.GraduationYear;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });
        }

        [HttpPost("skills")]
        public async Task<IActionResult> AddOrUpdateSkill([FromBody] StudentSkillDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var normalized = dto.SkillName.Trim();
            var tag = await _context.SkillTags.FirstOrDefaultAsync(t => t.Name.ToLower() == normalized.ToLower());

            if (tag == null)
            {
                tag = new SkillTag { Id = Guid.NewGuid(), Name = normalized };
                _context.SkillTags.Add(tag);
                await _context.SaveChangesAsync();
            }

            var metric = await _context.StudentSkillMetrics
                .FirstOrDefaultAsync(m => m.StudentId == userId && m.SkillTagId == tag.Id);

            if (metric != null)
            {
                metric.ProficiencyScore = dto.ProficiencyScore;
                metric.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _context.StudentSkillMetrics.Add(new StudentSkillMetric
                {
                    Id = Guid.NewGuid(),
                    StudentId = userId,
                    SkillTagId = tag.Id,
                    ProficiencyScore = dto.ProficiencyScore,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Skill saved." });
        }

        [HttpPost("upload-resume")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (profile != null)
            {
                profile.ResumeUrl = $"/resumes/{fileName}";
                await _context.SaveChangesAsync();
            }

            return Ok(new { resumeUrl = $"/resumes/{fileName}" });
        }
    }
}
