using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Hangfire;

namespace PCShop_Backend.Service
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, IJwtTokenService jwtTokenService, IEmailService emailService, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<string> Login(LoginDto dto)
        {
            var existUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.username);
            if (existUser == null)
            {
                Log.Error("User with username: {Username} is not found.", dto.username);
                throw new NotFoundException($"User with username {dto.username} not found");
            }
            if(VerifyHashPassword(existUser, existUser.PasswordHash, dto.password) == PasswordVerificationResult.Success)
            {
                var token = _jwtTokenService.GenerateToken(existUser);
                Log.Information("User {Username} logged in successfully.", dto.username);
                return token;
            }
            else
            {
                Log.Error("Invalid password for user: {Username}.", dto.username);
                throw new InvalidCredentialsException("Invalid password");
            }
        }

        public PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
        }

        //Reset password section
        public async Task GenerateResetPasswordToken(string email)
        {
            var existEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (existEmail == null)
            {
                Log.Error("Email: {Email} not found for password reset.", email);
                throw new NotFoundException($"Email {email} not found");
            }

            var existingReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email == email);

            // Check for existing valid token
            if (existingReset != null)
            {
                if(existingReset.ExpireDate > DateTime.UtcNow)
                {
                    Log.Error("A valid reset token already exists for email: {Email}", email);
                    throw new InvalidTokenException("A valid reset token already exists. Please check your email.");
                }
                else if(existingReset.ExpireDate <= DateTime.UtcNow)
                {
                    // Remove expired token
                    _context.PasswordResets.Remove(existingReset);
                    await _context.SaveChangesAsync();
                }
            }


            // generate a reset token
            var token = Guid.NewGuid().ToString();
            await _context.PasswordResets.AddAsync(new PasswordReset
            {
                Email = email,
                Token = token,
                ExpireDate = DateTime.UtcNow.AddMinutes(30) // Token valid for 30 minutes
            });

            await _context.SaveChangesAsync();

            _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(email, "Password Reset", $"Your password reset token is: {token}"));
        }


        public async Task ResetPassword(ResetPasswordRequestDto dto)
        {
            // Find the password reset token
            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Token == dto.Token);

            if (passwordReset == null)
            {
                Log.Error("Invalid password reset token.");
                throw new InvalidTokenException("Invalid or expired reset token.");
            }

            // Check if token is expired
            if (passwordReset.ExpireDate < DateTime.UtcNow)
            {
                Log.Error("Password reset token has expired for email: {Email}", passwordReset.Email);
                _context.PasswordResets.Remove(passwordReset);
                await _context.SaveChangesAsync();
                throw new InvalidTokenException("Reset token has expired. Please request a new password reset.");
            }

            // Find the user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == passwordReset.Email);

            if (user == null)
            {
                Log.Error("User not found for email: {Email}", passwordReset.Email);
                throw new NotFoundException("User not found.");
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
