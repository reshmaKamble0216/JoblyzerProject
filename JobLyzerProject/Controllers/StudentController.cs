using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using JobLyzerProject_Models.Models;

namespace JobLyzerProject.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly JobPlatformContext _context;

        public StudentController(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login", "Account");

            var profile = await _context.StudentProfiles
                .Include(s => s.Educations)
                .Include(s => s.Experiences)
                .Include(s => s.StudentSkills)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileData(string type, string field1, string field2, int? metricValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login", "Account");

            if (type == "Education")
            {
                _context.Educations.Add(new Education { StudentUserId = userId, Institution = field1, Degree = field2, StartDate = System.DateTime.UtcNow });
            }
            else if (type == "Experience")
            {
                _context.Experiences.Add(new Experience { StudentUserId = userId, CompanyName = field1, Role = field2, StartDate = System.DateTime.UtcNow });
            }
            else if (type == "Skill" && metricValue.HasValue)
            {
                _context.StudentSkills.Add(new StudentSkill { StudentUserId = userId, SkillName = field1, ProficiencyPercentage = metricValue.Value });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
