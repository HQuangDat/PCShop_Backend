using Microsoft.AspNetCore.Identity;
using PCShop_Backend.Dtos.AuthDtos;
using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public interface IAuthService
    {
        Task Login(LoginDto dto);
        Task Logout();
        PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword);
        Task ResetPassword(ResetPasswordRequestDto dto);
    }
}
