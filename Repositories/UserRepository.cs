using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;

namespace PCShop_Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<User> QueryUsers() => _context.Users;

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.Where(u => u.UserId == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UserExistsAsync(string email, string username)
        {
            return await _context.Users.AnyAsync(u => u.Email == email || u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void RemoveUser(User user)
        {
            _context.Users.Remove(user);
        }

        public IQueryable<Role> QueryRoles() => _context.Roles;

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        public void AddRole(Role role)
        {
            _context.Roles.Add(role);
        }

        public void RemoveRole(Role role)
        {
            _context.Roles.Remove(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
