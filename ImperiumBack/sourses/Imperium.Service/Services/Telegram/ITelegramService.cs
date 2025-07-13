using System.Threading.Tasks;

namespace Imperium.Service.Services.Telegram
{
    public interface ITelegramService
    {
        Task<bool> SendNewOrderNotificationAsync(string orderNumber, string clientName, string clientPhone, decimal totalAmount, string orderUrl);
        Task<bool> SendOrderStatusUpdateAsync(string orderNumber, string newStatus, string managerName);
        Task<bool> SendTestMessageAsync(string message);
    }
}
