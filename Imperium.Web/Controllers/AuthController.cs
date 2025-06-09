using Microsoft.AspNetCore.Mvc;
using Imperium.Service.DTOs.Auth;
using Imperium.Service.Services.Auth;

namespace Imperium.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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
                return BadRequest(ex.Message);
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
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            var exists = await _authService.EmailExistsAsync(email);
            return Ok(exists);
        }

        [HttpGet("check-phone/{phone}")]
        public async Task<ActionResult<bool>> CheckPhone(string phone)
        {
            var exists = await _authService.PhoneExistsAsync(phone);
            return Ok(exists);
        }
    }
}