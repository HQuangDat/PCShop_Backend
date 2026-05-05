using Hangfire;
using Microsoft.AspNetCore.Identity;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;
using Serilog;

namespace PCShop_Backend.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthService(IAuthRepository authRepository, IPasswordHasher<User> passwordHasher, IJwtTokenService jwtTokenService, IEmailService emailService, IBackgroundJobClient backgroundJobClient)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<string> Login(LoginDto dto)
        {
            var existUser = await _authRepository.GetUserByUsernameAsync(dto.username);

            var userToVerify = existUser ?? new User { PasswordHash = "invalid-hash-placeholder" };
            var result = VerifyHashPassword(userToVerify, userToVerify.PasswordHash, dto.password);

            if (existUser == null || result != PasswordVerificationResult.Success)
            {
                Log.Warning("Failed login attempt.");
                throw new ArgumentException("Invalid username or password.");
            }

            var token = _jwtTokenService.GenerateToken(existUser);
            Log.Information("User ID {UserId} logged in successfully.", existUser.UserId);
            return token;
        }

        public PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, inputPassword);
        }

        public async Task GenerateResetPasswordToken(string email)
        {
            var existEmail = await _authRepository.GetUserByEmailAsync(email);
            if (existEmail == null)
            {
                Log.Error("Invalid Email");
                throw new NotFoundException("Invalid Email");
            }

            var existingReset = await _authRepository.GetPasswordResetByEmailAsync(email);

            if (existingReset != null)
            {
                if (existingReset.ExpireDate > DateTime.UtcNow)
                {
                    Log.Error("A valid reset token already exists");
                    throw new ArgumentException("A valid reset token already exists. Please check your email.");
                }
                else
                {
                    _authRepository.RemovePasswordReset(existingReset);
                    await _authRepository.SaveChangesAsync();
                }
            }

            var token = Guid.NewGuid().ToString();
            await _authRepository.AddPasswordResetAsync(new PasswordReset
            {
                Email = email,
                Token = token,
                ExpireDate = DateTime.UtcNow.AddMinutes(30)
            });

            await _authRepository.SaveChangesAsync();

            _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(email, "Password Reset", $"Your password reset token is: {token}"));
        }

        public async Task ResetPassword(ResetPasswordRequestDto dto)
        {
            var passwordReset = await _authRepository.GetPasswordResetByTokenAsync(dto.Token);
            if (passwordReset == null)
            {
                Log.Error("Invalid password reset token.");
                throw new ArgumentException("Invalid or expired reset token.");
            }

            if (passwordReset.ExpireDate < DateTime.UtcNow)
            {
                Log.Error("Password reset token has expired.");
                _authRepository.RemovePasswordReset(passwordReset);
                await _authRepository.SaveChangesAsync();
                throw new ArgumentException("Reset token has expired. Please request a new password reset.");
            }

            var user = await _authRepository.GetUserByEmailAsync(passwordReset.Email);
            if (user == null)
            {
                Log.Error("User not found");
                throw new NotFoundException("User not found.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            _authRepository.UpdateUser(user);
            _authRepository.RemovePasswordReset(passwordReset);
            await _authRepository.SaveChangesAsync();

            Log.Information("Password reset successfully for user ID {UserId}", user.UserId);
        }
    }
}
