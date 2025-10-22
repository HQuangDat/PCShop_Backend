using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Models;
using Serilog;

namespace PCShop_Backend.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        //------------Role service----------------
        public async Task<Paging<RoleDto>> getRoles(GridifyQuery query)
        {
            var rolesQuery = _context.Roles.Select(role => new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description
            });

            var result = await rolesQuery.GridifyAsync(query);
            return result;
        }

        public async Task<RoleDto> getRoleById(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if(role == null)
            {
                throw new Exception("Role not found");
            }
            return new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description
            };
        }

        public async Task CreateRole(CreateRoleDto dto)
        {
            var role = _context.Roles.Add(new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description
            });

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRole(int roleId)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (existingRole == null)
            {
                throw new Exception("Role not found");
            }
            _context.Roles.Remove(existingRole);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRole(int userId, UpdateRoleDto dto)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == userId);
            if (existingRole == null)
            {
                throw new Exception("Role not found");
            }
            existingRole.RoleName = dto.RoleName;
            existingRole.Description = dto.Description;
            await _context.SaveChangesAsync();
        }

        //------------User service----------------

        public async Task<Paging<UserDto>> getUsers(GridifyQuery gridifyQuery)
        {
            var UserQuery = _context.Users.Select(user => new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                LoyaltyPoints = user.LoyaltyPoints,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            });

            var result = await UserQuery.GridifyAsync(gridifyQuery);
            return result;
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var existingUser = await _context.Users
                .Where(u => u.UserId == id)
                .Select(user => new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    LoyaltyPoints = user.LoyaltyPoints,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                })
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            return existingUser;
        }

        public async Task RegisterUser(RegisterUserDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 3,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LoyaltyPoints = 0
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            Log.Information($"New user registered: {dto.Username}");
        }

        public async Task DeleletUser(int userId)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }
            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(int userId, UpdateUserDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }
            existingUser.FullName = dto.FullName;
            existingUser.PhoneNumber = dto.PhoneNumber;
            existingUser.Address = dto.Address;
            existingUser.City = dto.City;
            existingUser.Country = dto.Country;
            await _context.SaveChangesAsync();
        }
    }
}
