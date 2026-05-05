using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<PasswordReset?> GetPasswordResetByEmailAsync(string email)
        {
            return await _context.PasswordResets.FirstOrDefaultAsync(pr => pr.Email == email);
        }

        public async Task<PasswordReset?> GetPasswordResetByTokenAsync(string token)
        {
            return await _context.PasswordResets.FirstOrDefaultAsync(pr => pr.Token == token);
        }

        public async Task AddPasswordResetAsync(PasswordReset passwordReset)
        {
            await _context.PasswordResets.AddAsync(passwordReset);
        }

        public void RemovePasswordReset(PasswordReset passwordReset)
        {
            _context.PasswordResets.Remove(passwordReset);
        }

        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
