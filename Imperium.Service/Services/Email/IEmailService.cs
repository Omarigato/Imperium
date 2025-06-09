using System.Threading.Tasks;

namespace Imperium.Service.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendVerificationCodeAsync(string email, string code);
        Task<bool> SendOrderConfirmationAsync(string email, string orderNumber);
        Task<bool> SendPasswordResetAsync(string email, string resetLink);
    }
}