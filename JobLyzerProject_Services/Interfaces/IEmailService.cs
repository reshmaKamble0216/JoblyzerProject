using System.Threading.Tasks;

namespace JobLyzerProject_Services.Interfaces
{
    public interface IEmailService
    {
        Task SendRegistrationEmailAsync(string toEmail, string fullName, string role);
    }
}
