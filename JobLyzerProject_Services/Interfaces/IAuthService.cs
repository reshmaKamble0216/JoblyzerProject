using System.Threading.Tasks;
using JobLyzerProject_Models.Models;

namespace JobLyzerProject_Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> ValidateUserAsync(string email, string password);
        Task<bool> RegisterStudentWithProfileAsync(
            string email,
            string password,
            string resumePath,
            string institution,
            string degree,
            string company,
            string role,
            string skillName,
            int proficiency);
        Task<bool> RegisterRecruiterAsync(string email, string password, string companyName, string companyWebsite, int teamSizeAllocation);
        Task<bool> RegisterAdminAsync(string email, string password);
    }
}
