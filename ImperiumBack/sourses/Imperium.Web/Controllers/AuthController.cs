using Imperium.Service.DTOs.Auth;
using Imperium.Service.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        #region Traditional Registration/Login

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in traditional registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in traditional login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region OTP-based Registration

        [HttpPost("register/email")]
        public async Task<ActionResult<OtpResponseDto>> RegisterWithEmail([FromBody] EmailRegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterWithEmailAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in email registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("register/phone")]
        public async Task<ActionResult<OtpResponseDto>> RegisterWithPhone([FromBody] PhoneRegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterWithPhoneAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in phone registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("register/verify-otp")]
        public async Task<ActionResult<AuthResponseDto>> VerifyRegistrationOtp([FromBody] VerifyOtpDto verifyDto)
        {
            try
            {
                var result = await _authService.VerifyRegistrationOtpAsync(verifyDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OTP verification for registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region OTP-based Login

        [HttpPost("login/otp")]
        public async Task<ActionResult<OtpResponseDto>> LoginWithOtp([FromBody] LoginWithOtpDto loginDto)
        {
            try
            {
                var result = await _authService.LoginWithOtpAsync(loginDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OTP login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("login/verify-otp")]
        public async Task<ActionResult<AuthResponseDto>> VerifyLoginOtp([FromBody] VerifyLoginOtpDto verifyDto)
        {
            try
            {
                var result = await _authService.VerifyLoginOtpAsync(verifyDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login OTP verification");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Google OAuth

        [HttpPost("google")]
        public async Task<ActionResult<AuthResponseDto>> GoogleAuth([FromBody] GoogleAuthDto googleAuthDto)
        {
            try
            {
                var result = await _authService.GoogleAuthAsync(googleAuthDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google authentication");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Password Reset

        [HttpPost("forgot-password")]
        public async Task<ActionResult<OtpResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in password reset");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region OTP Management

        [HttpPost("resend-otp")]
        public async Task<ActionResult<OtpResponseDto>> ResendOtp([FromBody] string contact)
        {
            try
            {
                var result = await _authService.ResendOtpAsync(contact);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in resending OTP");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Utility Endpoints

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            try
            {
                var exists = await _authService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("check-phone/{phone}")]
        public async Task<ActionResult<bool>> CheckPhone(string phone)
        {
            try
            {
                var exists = await _authService.PhoneExistsAsync(phone);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking phone availability");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("validate-phone/{phone}")]
        public async Task<ActionResult<bool>> ValidatePhone(string phone)
        {
            try
            {
                // You would inject IPhoneValidationService here
                // For now, basic validation
                var isValid = phone.StartsWith("+7") && phone.Length >= 12;
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating phone number");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region User Info

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // You can fetch full user details here if needed
                return Ok(new
                {
                    id = userId,
                    email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                    name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion
    }
}