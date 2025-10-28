using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace PCShop_Backend.Service
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<string> Login(LoginDto dto)
        {
            var existUser = await _context.Users
                .Include(u => u.Role)
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

                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddDays(1);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                Log.Information("User {Username} logged in successfully.", dto.username);
                return tokenHandler.WriteToken(token);
            }
            else
            {
                Log.Error("Invalid password for user: {Username}.", dto.username);
                throw new Exception("Invalid password");
            }
        }

        public PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
        }

        //Reset password section

        public async Task ResetPassword(ResetPasswordRequestDto dto)
        {
            // Find the password reset token
            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Token == dto.Token);

            if (passwordReset == null)
            {
                Log.Error("Invalid password reset token.");
                throw new Exception("Invalid or expired reset token.");
            }

            // Check if token is expired
            if (passwordReset.ExpireDate < DateTime.UtcNow)
            {
                Log.Error("Password reset token has expired for email: {Email}", passwordReset.Email);
                _context.PasswordResets.Remove(passwordReset);
                await _context.SaveChangesAsync();
                throw new Exception("Reset token has expired. Please request a new password reset.");
            }

            // Find the user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == passwordReset.Email);

            if (user == null)
            {
                Log.Error("User not found for email: {Email}", passwordReset.Email);
                throw new Exception("User not found.");
            }

            // Hash the new password and update user
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            _context.Users.Update(user);

            // Remove the used token
            _context.PasswordResets.Remove(passwordReset);

            await _context.SaveChangesAsync();

            Log.Information("Password reset successfully for user: {Email}", passwordReset.Email);
        }
    }
}
