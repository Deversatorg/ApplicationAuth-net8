using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.Shared
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
