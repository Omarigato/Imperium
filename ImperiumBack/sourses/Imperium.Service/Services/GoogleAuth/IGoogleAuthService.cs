using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.GoogleAuth
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }
}
