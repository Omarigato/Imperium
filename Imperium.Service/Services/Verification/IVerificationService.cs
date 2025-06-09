using Imperium.Core.Enums;
using System;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Verification
{
    public interface IVerificationService
    {
        Task<string> SendVerificationCodeAsync(Guid userId, string contact, VerificationType type);
        Task<bool> VerifyCodeAsync(Guid userId, string contact, string code, VerificationType type);
        Task<string> ResendVerificationCodeAsync(Guid userId, string contact, VerificationType type);
        Task<bool> IsContactVerifiedAsync(Guid userId, VerificationType type);
    }
}
