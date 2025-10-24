using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Models;
using Serilog;
using System.Security.Claims;

namespace PCShop_Backend.Service
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task ChangePassword(int id, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task Login(LoginDto dto)
        {
            var existUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.username);
            if (existUser == null)
            {
                Log.Error("User with username: {Username} is not found.", dto.username);
                throw new Exception("User not found");
            }
            if(VerifyHashPassword(existUser, existUser.PasswordHash, dto.password) == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, existUser.Username),
                        new Claim(ClaimTypes.Role, existUser.Role.RoleName),
                        new Claim(ClaimTypes.NameIdentifier, existUser.UserId.ToString())
                    };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                Log.Information("User {Username} logged in successfully.", dto.username);
            }
            else
            {
                Log.Error("Invalid password for user: {Username}.", dto.username);
                throw new Exception("Invalid password");
            }
        }

        public async Task Logout()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Log.Information("User logged out successfully.");
        }

        public PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
        }
    }
}
