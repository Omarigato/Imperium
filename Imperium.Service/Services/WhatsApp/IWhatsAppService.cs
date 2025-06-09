using System.Threading.Tasks;

namespace Imperium.Service.Services.WhatsApp
{
    public interface IWhatsAppService
    {
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
        Task<bool> SendOrderNotificationAsync(string phoneNumber, string orderNumber, decimal totalAmount);
        Task<bool> NotifyAdminNewOrderAsync(string orderNumber, string customerName);
    }
}
