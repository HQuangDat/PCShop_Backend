using PCShop_Backend.Models;

namespace PCShop_Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Users
        IQueryable<User> QueryUsers();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UserExistsAsync(string email, string username);
        Task AddUserAsync(User user);
        void RemoveUser(User user);

        // Roles
        IQueryable<Role> QueryRoles();
        Task<Role?> GetRoleByIdAsync(int roleId);
        void AddRole(Role role);
        void RemoveRole(Role role);

        Task SaveChangesAsync();
    }
}
