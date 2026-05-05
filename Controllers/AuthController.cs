using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Interfaces;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.Login(dto);
            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            await _authService.GenerateResetPasswordToken(email);
            return Ok("Password reset token sent to your email.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _authService.ResetPassword(dto);
            return Ok("Password reset successful.");
        }
    }
}
