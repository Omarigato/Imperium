using System.Threading.Tasks;
using Imperium.Service.DTOs.Auth;

namespace Imperium.Service.Services.Auth
{
    public interface IAuthService
    {
        // Traditional registration/login
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        // OTP-based registration
        Task<OtpResponseDto> RegisterWithEmailAsync(EmailRegisterDto registerDto);
        Task<OtpResponseDto> RegisterWithPhoneAsync(PhoneRegisterDto registerDto);
        Task<AuthResponseDto> VerifyRegistrationOtpAsync(VerifyOtpDto verifyDto);

        // OTP-based login
        Task<OtpResponseDto> LoginWithOtpAsync(LoginWithOtpDto loginDto);
        Task<AuthResponseDto> VerifyLoginOtpAsync(VerifyLoginOtpDto verifyDto);

        // Google OAuth
        Task<AuthResponseDto> GoogleAuthAsync(GoogleAuthDto googleAuthDto);

        // Password reset
        Task<OtpResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        // Resend OTP
        Task<OtpResponseDto> ResendOtpAsync(string contact);

        // Utility methods
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
    }
}