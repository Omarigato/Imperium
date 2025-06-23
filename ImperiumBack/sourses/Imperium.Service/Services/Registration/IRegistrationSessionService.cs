using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Registration
{
    public interface IRegistrationSessionService
    {
        Task StoreRegistrationDataAsync(string contact, string fullName, string type);
        Task<RegistrationData?> GetRegistrationDataAsync(string contact);
        Task ClearRegistrationDataAsync(string contact);
    }

    public class RegistrationData
    {
        public string FullName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Email or Phone
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
