using AutoMapper;
using Imperium.Core;
using Imperium.Core.Enums;
using Imperium.Core.Models;
using Imperium.Data.Repositories;
using Imperium.Service.DTOs.Auth;
using Imperium.Service.Services.Email;
using Imperium.Service.Services.GoogleAuth;
using Imperium.Service.Services.Registration;
using Imperium.Service.Services.Validation;
using Imperium.Service.Services.WhatsApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVerificationRepository _verificationRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IPhoneValidationService _phoneValidationService;
        private readonly IRegistrationSessionService _registrationSessionService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IVerificationRepository verificationRepository,
            IPasswordResetRepository passwordResetRepository,
            IMapper mapper,
            IConfiguration configuration,
            IEmailService emailService,
            IWhatsAppService whatsAppService,
            IGoogleAuthService googleAuthService,
            IPhoneValidationService phoneValidationService,
            IRegistrationSessionService registrationSessionService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _verificationRepository = verificationRepository;
            _passwordResetRepository = passwordResetRepository;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _googleAuthService = googleAuthService;
            _phoneValidationService = phoneValidationService;
            _registrationSessionService = registrationSessionService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
                throw new InvalidOperationException("Email already exists");

            if (!string.IsNullOrEmpty(registerDto.Phone) && await _userRepository.PhoneExistsAsync(registerDto.Phone))
                throw new InvalidOperationException("Phone already exists");

            var user = _mapper.Map<User>(registerDto);
            user.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var userId = await _userRepository.AddAsync(user);
            user.Id = userId;

            var response = _mapper.Map<AuthResponseDto>(user);
            response.Token = GenerateJwtToken(user);
            response.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return response;
        }

        public async Task<OtpResponseDto> RegisterWithEmailAsync(EmailRegisterDto registerDto)
        {
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
                throw new InvalidOperationException("Email already exists");

            // Store registration data in session
            await _registrationSessionService.StoreRegistrationDataAsync(
                registerDto.Email,
                registerDto.FullName,
                VerificationType.Email.ToString());

            var otpCode = GenerateOtpCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(Constants.OTP_EXPIRY_MINUTES);

            var verification = new Core.Models.Verification
            {
                UserId = Guid.Empty, // Temporary user, will be updated after password set
                Type = VerificationType.Email.ToString(),
                Contact = registerDto.Email,
                Code = otpCode,
                ExpiresAt = expiresAt,
                AttemptCount = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _verificationRepository.AddAsync(verification);

            var emailSent = await _emailService.SendVerificationCodeAsync(registerDto.Email, otpCode);
            if (!emailSent)
                throw new InvalidOperationException("Failed to send verification email");

            return new OtpResponseDto
            {
                Success = true,
                Message = "OTP sent to your email",
                RemainingAttempts = Constants.OTP_COUNT - 1
            };
        }

        public async Task<OtpResponseDto> RegisterWithPhoneAsync(PhoneRegisterDto registerDto)
        {
            var formattedPhone = _phoneValidationService.FormatPhoneNumber(registerDto.PhoneNumber);

            if (!_phoneValidationService.IsValidKazakhstanPhoneNumber(formattedPhone))
                throw new InvalidOperationException("Invalid Kazakhstan phone number format");

            if (await _userRepository.PhoneExistsAsync(formattedPhone))
                throw new InvalidOperationException("Phone number already exists");

            // Store registration data in session
            await _registrationSessionService.StoreRegistrationDataAsync(
                formattedPhone,
                registerDto.FullName,
                VerificationType.Phone.ToString());

            var otpCode = GenerateOtpCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(Constants.OTP_EXPIRY_MINUTES);

            var verification = new Core.Models.Verification
            {
                UserId = Guid.Empty,
                Type = VerificationType.Phone.ToString(),
                Contact = formattedPhone,
                Code = otpCode,
                ExpiresAt = expiresAt,
                AttemptCount = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _verificationRepository.AddAsync(verification);

            var smsSent = await _whatsAppService.SendVerificationCodeAsync(formattedPhone, otpCode);
            if (!smsSent)
                throw new InvalidOperationException("Failed to send verification SMS");

            return new OtpResponseDto
            {
                Success = true,
                Message = "OTP sent to your WhatsApp",
                RemainingAttempts = Constants.OTP_COUNT - 1
            };
        }

        public async Task<AuthResponseDto> VerifyRegistrationOtpAsync(VerifyOtpDto verifyDto)
        {
            var verification = await GetActiveVerificationAsync(verifyDto.Contact, verifyDto.OtpCode);

            if (verification == null)
                throw new InvalidOperationException("Invalid or expired OTP code");

            // Get registration data from session
            var registrationData = await _registrationSessionService.GetRegistrationDataAsync(verifyDto.Contact);
            if (registrationData == null)
                throw new InvalidOperationException("Registration session expired. Please start registration again.");

            // Create new user
            var user = new User
            {
                FullName = registrationData.FullName,
                Email = IsEmailFormat(verifyDto.Contact) ? verifyDto.Contact : string.Empty,
                Phone = _phoneValidationService.IsPhoneNumberFormat(verifyDto.Contact) ? verifyDto.Contact : null,
                Password = BCrypt.Net.BCrypt.HashPassword(verifyDto.Password),
                IsEmailVerified = IsEmailFormat(verifyDto.Contact),
                IsPhoneVerified = _phoneValidationService.IsPhoneNumberFormat(verifyDto.Contact),
                Role = UserRole.Client
            };

            var userId = await _userRepository.AddAsync(user);
            user.Id = userId;

            // Mark verification as used
            verification.IsVerified = true;
            verification.UserId = userId;
            await _verificationRepository.UpdateAsync(verification);

            // Clear registration session
            await _registrationSessionService.ClearRegistrationDataAsync(verifyDto.Contact);

            var response = _mapper.Map<AuthResponseDto>(user);
            response.Token = GenerateJwtToken(user);
            response.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return response;
        }

        public async Task<OtpResponseDto> LoginWithOtpAsync(LoginWithOtpDto loginDto)
        {
            User? user = null;

            if (IsEmailFormat(loginDto.Contact))
            {
                user = await _userRepository.GetByEmailAsync(loginDto.Contact);
            }
            else if (_phoneValidationService.IsPhoneNumberFormat(loginDto.Contact))
            {
                var formattedPhone = _phoneValidationService.FormatPhoneNumber(loginDto.Contact);
                user = await _userRepository.GetByPhoneAsync(formattedPhone);
            }

            if (user == null)
                throw new InvalidOperationException("User not found");

            var otpCode = GenerateOtpCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(Constants.OTP_EXPIRY_MINUTES);

            var verification = new Core.Models.Verification
            {
                UserId = user.Id,
                Type = IsEmailFormat(loginDto.Contact) ? VerificationType.Email.ToString() : VerificationType.Phone.ToString(),
                Contact = loginDto.Contact,
                Code = otpCode,
                ExpiresAt = expiresAt,
                AttemptCount = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _verificationRepository.AddAsync(verification);

            bool sent = false;
            if (IsEmailFormat(loginDto.Contact))
            {
                sent = await _emailService.SendVerificationCodeAsync(loginDto.Contact, otpCode);
            }
            else
            {
                sent = await _whatsAppService.SendVerificationCodeAsync(loginDto.Contact, otpCode);
            }

            if (!sent)
                throw new InvalidOperationException("Failed to send OTP");

            return new OtpResponseDto
            {
                Success = true,
                Message = "OTP sent for login verification",
                RemainingAttempts = Constants.OTP_COUNT - 1
            };
        }

        public async Task<AuthResponseDto> VerifyLoginOtpAsync(VerifyLoginOtpDto verifyDto)
        {
            var verification = await GetActiveVerificationAsync(verifyDto.Contact, verifyDto.OtpCode);

            if (verification == null || verification.UserId == Guid.Empty)
                throw new InvalidOperationException("Invalid or expired OTP code");

            var user = await _userRepository.GetByIdAsync(verification.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Mark verification as used
            verification.IsVerified = true;
            await _verificationRepository.UpdateAsync(verification);

            var response = _mapper.Map<AuthResponseDto>(user);
            response.Token = GenerateJwtToken(user);
            response.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return response;
        }

        public async Task<AuthResponseDto> GoogleAuthAsync(GoogleAuthDto googleAuthDto)
        {
            var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(googleAuthDto.IdToken);
            if (googleUser == null)
                throw new UnauthorizedAccessException("Invalid Google token");

            var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email);

            if (existingUser != null)
            {
                // Update user info if needed
                existingUser.IsEmailVerified = true;
                await _userRepository.UpdateAsync(existingUser);

                var response = _mapper.Map<AuthResponseDto>(existingUser);
                response.Token = GenerateJwtToken(existingUser);
                response.ExpiresAt = DateTime.UtcNow.AddDays(7);
                return response;
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    FullName = googleUser.Name,
                    Email = googleUser.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                    IsEmailVerified = true,
                    Role = UserRole.Client
                };

                var userId = await _userRepository.AddAsync(newUser);
                newUser.Id = userId;

                var response = _mapper.Map<AuthResponseDto>(newUser);
                response.Token = GenerateJwtToken(newUser);
                response.ExpiresAt = DateTime.UtcNow.AddDays(7);
                return response;
            }
        }

        public async Task<OtpResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userRepository.GetByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var resetToken = GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.AddHours(Constants.PASSWORD_RESET_EXPIRY_HOURS);

            var passwordReset = new PasswordReset
            {
                UserId = user.Id,
                Email = user.Email,
                ResetToken = resetToken,
                ExpiresAt = expiresAt
            };

            await _passwordResetRepository.AddAsync(passwordReset);

            var resetLink = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={resetToken}";
            var emailSent = await _emailService.SendPasswordResetAsync(user.Email, resetLink);

            if (!emailSent)
                throw new InvalidOperationException("Failed to send password reset email");

            return new OtpResponseDto
            {
                Success = true,
                Message = "Password reset link sent to your email"
            };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var passwordReset = await _passwordResetRepository.GetActiveByTokenAsync(resetPasswordDto.ResetToken);
            if (passwordReset == null)
                throw new InvalidOperationException("Invalid or expired reset token");

            var user = await _userRepository.GetByIdAsync(passwordReset.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            await _userRepository.UpdateAsync(user);

            await _passwordResetRepository.MarkAsUsedAsync(passwordReset.Id);

            var response = _mapper.Map<AuthResponseDto>(user);
            response.Token = GenerateJwtToken(user);
            response.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return response;
        }

        public async Task<OtpResponseDto> ResendOtpAsync(string contact)
        {
            var recentVerifications = await _verificationRepository.GetByContactAndTypeAsync(
                contact,
                IsEmailFormat(contact) ? VerificationType.Email.ToString() : VerificationType.Phone.ToString());

            var lastVerification = recentVerifications
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();

            if (lastVerification != null && lastVerification.CreatedAt > DateTime.UtcNow.AddMinutes(-Constants.OTP_RESEND_COOLDOWN_MINUTES))
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = $"Please wait {Constants.OTP_RESEND_COOLDOWN_MINUTES} minute(s) before requesting another OTP",
                    NextRetryAt = lastVerification.CreatedAt.AddMinutes(Constants.OTP_RESEND_COOLDOWN_MINUTES)
                };
            }

            if (lastVerification != null && lastVerification.AttemptCount >= Constants.OTP_COUNT)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Maximum OTP attempts exceeded. Please try again later.",
                    RemainingAttempts = 0
                };
            }

            var otpCode = GenerateOtpCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(Constants.OTP_EXPIRY_MINUTES);

            var verification = new Core.Models.Verification
            {
                UserId = lastVerification?.UserId ?? Guid.Empty,
                Type = IsEmailFormat(contact) ? VerificationType.Email.ToString() : VerificationType.Phone.ToString(),
                Contact = contact,
                Code = otpCode,
                ExpiresAt = expiresAt,
                AttemptCount = (lastVerification?.AttemptCount ?? 0) + 1,
                CreatedAt = DateTime.UtcNow
            };

            await _verificationRepository.AddAsync(verification);

            bool sent = false;
            if (IsEmailFormat(contact))
            {
                sent = await _emailService.SendVerificationCodeAsync(contact, otpCode);
            }
            else
            {
                sent = await _whatsAppService.SendVerificationCodeAsync(contact, otpCode);
            }

            if (!sent)
                throw new InvalidOperationException("Failed to resend OTP");

            return new OtpResponseDto
            {
                Success = true,
                Message = "OTP resent successfully",
                RemainingAttempts = Constants.OTP_COUNT - verification.AttemptCount
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            var response = _mapper.Map<AuthResponseDto>(user);
            response.Token = GenerateJwtToken(user);
            response.ExpiresAt = DateTime.UtcNow.AddDays(7);

            return response;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _userRepository.PhoneExistsAsync(phone);
        }

        private async Task<Core.Models.Verification?> GetActiveVerificationAsync(string contact, string code)
        {
            var verifications = await _verificationRepository.GetByContactAndTypeAsync(
                contact,
                IsEmailFormat(contact) ? VerificationType.Email.ToString() : VerificationType.Phone.ToString());

            return verifications
                .Where(v => v.Code == code && v.ExpiresAt > DateTime.UtcNow && !v.IsVerified)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();
        }

        private static string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static bool IsEmailFormat(string input)
        {
            return input.Contains("@") && input.Contains(".");
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}