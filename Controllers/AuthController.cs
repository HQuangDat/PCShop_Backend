using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Service;

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
        public async Task<IActionResult> Login([FromForm] LoginDto dto)
        {
            try
            {
                await _authService.Login(dto);
                return Ok("Login successful");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            return Ok("Logged out successfully");
        }
    }
}
