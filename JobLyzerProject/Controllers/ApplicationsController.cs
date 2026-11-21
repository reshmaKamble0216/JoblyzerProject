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
    public class ApplicationsController : ControllerBase
    {
        private readonly JobPlatformContext _context;

        private static readonly string[] AllowedStatuses =
        {
            "Applied",
            "Shortlisted",
            "Exam Scheduled",
            "Rejected"
        };

        public ApplicationsController(JobPlatformContext context)
        {
            _context = context;
        }

        // 1. POST: api/Applications/apply/{jobId}
        [Authorize(Roles = "Student")]
        [HttpPost("apply/{jobId:guid}")]
        public async Task<IActionResult> ApplyToJob(Guid jobId)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid studentId))
                return Unauthorized();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);

            if (job == null)
                return NotFound(new { message = "Job not found or is no longer active." });

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.StudentId == studentId);

            if (alreadyApplied)
                return BadRequest(new { message = "You have already applied for this job." });

            var application = new JobApplication
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                StudentId = studentId,
                Status = "Applied",
                AppliedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Application submitted successfully.",
                applicationId = application.Id,
                status = application.Status
            });
        }

        // 2. GET: api/Applications/my-applications
        [Authorize(Roles = "Student")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid studentId))
                return Unauthorized();

            var applications = await _context.JobApplications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Recruiter)
                        .ThenInclude(u => u.RecruiterProfile)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    applicationId = a.Id,
                    jobId = a.JobId,
                    jobTitle = a.Job.Title,
                    location = a.Job.Location,
                    companyName = a.Job.Recruiter.RecruiterProfile != null
                        ? a.Job.Recruiter.RecruiterProfile.CompanyName
                        : "Company",
                    status = a.Status,
                    appliedAt = a.AppliedAt
                })
                .ToListAsync();

            return Ok(applications);
        }

        // 3. GET: api/Applications/job/{jobId}/pipeline
        [Authorize(Roles = "Recruiter")]
        [HttpGet("job/{jobId:guid}/pipeline")]
        public async Task<IActionResult> GetJobApplicantPipeline(Guid jobId)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound(new { message = "Job not found or access denied." });

            var applicants = await _context.JobApplications
                .Include(a => a.Student)
                    .ThenInclude(s => s.StudentProfile)
                .Include(a => a.Student)
                    .ThenInclude(s => s.StudentExams)
                .Include(a => a.Student)
                    .ThenInclude(s => s.StudentSkillMetrics)
                        .ThenInclude(m => m.SkillTag)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    applicationId = a.Id,
                    studentId = a.StudentId,
                    fullName = a.Student.FullName,
                    email = a.Student.Email,
                    headline = a.Student.StudentProfile != null ? a.Student.StudentProfile.Headline : null,
                    graduationYear = a.Student.StudentProfile != null ? a.Student.StudentProfile.GraduationYear : null,
                    resumeUrl = a.Student.StudentProfile != null ? a.Student.StudentProfile.ResumeUrl : null,
                    bestExamScore = a.Student.StudentExams
                        .Where(e => e.ScorePercentage.HasValue)
                        .Max(e => (decimal?)e.ScorePercentage),
                    skills = a.Student.StudentSkillMetrics.Select(m => new
                    {
                        skillName = m.SkillTag.Name,
                        proficiencyScore = m.ProficiencyScore
                    }),
                    status = a.Status,
                    appliedAt = a.AppliedAt
                })
                .ToListAsync();

            return Ok(new
            {
                jobId = job.Id,
                jobTitle = job.Title,
                totalApplicants = applicants.Count,
                counts = new
                {
                    applied = applicants.Count(x => x.status == "Applied"),
                    shortlisted = applicants.Count(x => x.status == "Shortlisted"),
                    examScheduled = applicants.Count(x => x.status == "Exam Scheduled"),
                    rejected = applicants.Count(x => x.status == "Rejected")
                },
                applicants
            });
        }

        // 4. PATCH: api/Applications/{applicationId}/status
        [Authorize(Roles = "Recruiter")]
        [HttpPatch("{applicationId:guid}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(Guid applicationId, [FromBody] UpdateApplicationStatusDto dto)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid recruiterId))
                return Unauthorized();

            var matchedStatus = AllowedStatuses
                .FirstOrDefault(s => s.Equals(dto.Status?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (matchedStatus == null)
            {
                return BadRequest(new
                {
                    message = "Invalid status. Allowed values: Applied, Shortlisted, Exam Scheduled, Rejected."
                });
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Job.RecruiterId == recruiterId);

            if (application == null)
                return NotFound(new { message = "Application not found or access denied." });

            application.Status = matchedStatus;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Candidate status updated to {matchedStatus}.",
                applicationId = application.Id,
                status = application.Status
            });
        }
    }
}
