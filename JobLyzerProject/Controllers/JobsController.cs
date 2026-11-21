using JobLyzerProject.DTOs;
using JobLyzerProject_Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JobLyzerProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly JobPlatformContext _context;

        public JobsController(JobPlatformContext context)
        {
            _context = context;
        }

        // 1. POST: api/Jobs (Recruiter posts a new job)
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var job = new Job
            {
                Id = Guid.NewGuid(),
                RecruiterId = recruiterId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                IsActive = true,
                PostedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Attach skill tags if provided
            if (dto.RequiredSkills != null && dto.RequiredSkills.Any())
            {
                foreach (var rawSkill in dto.RequiredSkills.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var skillName = rawSkill.Trim();
                    var tag = await _context.SkillTags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == skillName.ToLower());

                    if (tag == null)
                    {
                        tag = new SkillTag { Id = Guid.NewGuid(), Name = skillName };
                        _context.SkillTags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    _context.JobSkills.Add(new JobSkill
                    {
                        JobId = job.Id,
                        SkillTagId = tag.Id
                    });

                   ;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Job posted successfully.",
                jobId = job.Id,
                title = job.Title
            });
        }

        // 2. PUT: api/Jobs/{id} (Recruiter edits a job)
        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateJob(Guid id, [FromBody] UpdateJobDto dto)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound(new { message = "Job not found or access denied." });

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Location = dto.Location;
            job.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Job updated successfully." });
        }

        // 3. PATCH: api/Jobs/{id}/archive (Recruiter archives or reactivates a job)
        [Authorize(Roles = "Recruiter")]
        [HttpPatch("{id:guid}/archive")]
        public async Task<IActionResult> ToggleArchiveJob(Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound(new { message = "Job not found or access denied." });

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = job.IsActive ? "Job is now Active." : "Job has been Archived.",
                isActive = job.IsActive
            });
        }

        // 4. GET: api/Jobs/my-jobs (Recruiter views their active & archived jobs)
        [Authorize(Roles = "Recruiter")]
        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs([FromQuery] bool? isActive = null)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var query = _context.Jobs
                .Include(j => j.JobApplications)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.SkillTag)
                .Where(j => j.RecruiterId == recruiterId);

            if (isActive.HasValue)
                query = query.Where(j => j.IsActive == isActive.Value);

            var jobs = await query
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.IsActive,
                    j.PostedAt,
                    skills = j.JobSkills.Select(js => js.SkillTag.Name).ToList(),
                    totalApplicants = j.JobApplications.Count
                })
                .ToListAsync();

            return Ok(jobs);
        }

        // 5. GET: api/Jobs/search (Students search & filter active jobs)
        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] string? keyword = null,
            [FromQuery] string? location = null,
            [FromQuery] string? skill = null,
            [FromQuery] decimal? minStudentScore = null)
        {
            var query = _context.Jobs
                .Include(j => j.Recruiter)
                    .ThenInclude(u => u.RecruiterProfile)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.SkillTag)
                .Where(j => j.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(k) || j.Description.ToLower().Contains(k));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim().ToLower();
                query = query.Where(j => j.Location != null && j.Location.ToLower().Contains(loc));
            }

            if (!string.IsNullOrWhiteSpace(skill))
            {
                var sk = skill.Trim().ToLower();
                query = query.Where(j => j.JobSkills.Any(js => js.SkillTag.Name.ToLower().Contains(sk)));
            }

            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid studentId);

            decimal studentBestScore = 0;
            if (studentId != Guid.Empty)
            {
                studentBestScore = await _context.StudentExams
                    .Where(e => e.StudentId == studentId && e.ScorePercentage.HasValue)
                    .MaxAsync(e => (decimal?)e.ScorePercentage) ?? 0;
            }

            if (minStudentScore.HasValue && studentBestScore < minStudentScore.Value)
            {
                return Ok(Array.Empty<object>());
            }

            var results = await query
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.PostedAt,
                    skills = j.JobSkills.Select(js => js.SkillTag.Name).ToList(),
                    companyName = j.Recruiter.RecruiterProfile != null
                        ? j.Recruiter.RecruiterProfile.CompanyName
                        : "Company",
                    companyWebsite = j.Recruiter.RecruiterProfile != null
                        ? j.Recruiter.RecruiterProfile.CompanyWebsite
                        : null,
                    hasAlreadyApplied = studentId != Guid.Empty
                        && j.JobApplications.Any(a => a.StudentId == studentId)
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
