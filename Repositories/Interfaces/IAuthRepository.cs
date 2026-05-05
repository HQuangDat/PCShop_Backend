using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<PasswordReset?> GetPasswordResetByEmailAsync(string email);
        Task<PasswordReset?> GetPasswordResetByTokenAsync(string token);
        Task AddPasswordResetAsync(PasswordReset passwordReset);
        void RemovePasswordReset(PasswordReset passwordReset);
        void UpdateUser(User user);
        Task SaveChangesAsync();
    }
}
