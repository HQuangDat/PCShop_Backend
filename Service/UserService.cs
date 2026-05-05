using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Models;
using PCShop_Backend.Repositories.Interfaces;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICacheService _cacheService;

        public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _cacheService = cacheService;
        }

        // ============ Roles ============
        public async Task<Paging<RoleDto>> getRoles(GridifyQuery query)
        {
            var rawKey = $"Roles_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<RoleDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _userRepository.QueryRoles()
                .Select(role => new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description!
                }).GridifyAsync(query);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<RoleDto> getRoleById(int roleId)
        {
            var rawKey = $"Role_{roleId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<RoleDto>(key);
            if (cachedData != null)
                return cachedData;

            var role = await _userRepository.GetRoleByIdAsync(roleId);
            if (role == null)
                throw new NotFoundException("Role not found");

            var dto = new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description!
            };

            await _cacheService.SetAsync(key, dto);
            return dto;
        }

        public async Task CreateRole(CreateRoleDto dto)
        {
            _userRepository.AddRole(new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description
            });
            await _userRepository.SaveChangesAsync();
        }

        public async Task UpdateRole(int roleId, UpdateRoleDto dto)
        {
            var existingRole = await _userRepository.GetRoleByIdAsync(roleId);
            if (existingRole == null)
                throw new NotFoundException("Role not found");

            existingRole.RoleName = dto.RoleName;
            existingRole.Description = dto.Description;
            await _userRepository.SaveChangesAsync();

            var rawKey = $"Role_{roleId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task DeleteRole(int roleId)
        {
            var existingRole = await _userRepository.GetRoleByIdAsync(roleId);
            if (existingRole == null)
                throw new NotFoundException("Role not found");

            _userRepository.RemoveRole(existingRole);
            await _userRepository.SaveChangesAsync();

            var rawKey = $"Role_{roleId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        // ============ Users ============
        public async Task<Paging<UserDto>> getUsers(GridifyQuery gridifyQuery)
        {
            var rawKey = $"Users_{gridifyQuery.Page}_{gridifyQuery.PageSize}_{gridifyQuery.Filter}_{gridifyQuery.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<UserDto>>(key);
            if (cachedData != null)
                return cachedData;

            var result = await _userRepository.QueryUsers()
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
                }).GridifyAsync(gridifyQuery);

            await _cacheService.SetAsync(key, result);
            return result;
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var rawKey = $"User_{id}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<UserDto>(key);
            if (cachedData != null)
                return cachedData;

            var existingUser = await _userRepository.QueryUsers()
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
                throw new NotFoundException("User not found");

            await _cacheService.SetAsync(key, existingUser);
            return existingUser;
        }

        public async Task RegisterUser(RegisterUserDto dto)
        {
            if (await _userRepository.UserExistsAsync(dto.Email, dto.Username))
                throw new ConflictException("Email or Username already exists");

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
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();
            Log.Information("New user registered with ID {UserId}", user.UserId);
        }

        public async Task DeleteUser(int userId)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                throw new NotFoundException("User not found");

            _userRepository.RemoveUser(existingUser);
            await _userRepository.SaveChangesAsync();

            var rawKey = $"User_{userId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }

        public async Task UpdateUser(int userId, UpdateUserDto dto)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                throw new NotFoundException("User not found");

            existingUser.FullName = dto.FullName;
            existingUser.PhoneNumber = dto.PhoneNumber;
            existingUser.Address = dto.Address;
            existingUser.City = dto.City;
            existingUser.Country = dto.Country;
            await _userRepository.SaveChangesAsync();

            var rawKey = $"User_{userId}";
            var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}
