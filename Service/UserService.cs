using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Exceptions;
using PCShop_Backend.Models;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace PCShop_Backend.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICacheService _cacheService;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ICacheService cacheService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _cacheService = cacheService;
        }

        //------------Role service----------------
        //Function lay danh sach vai tro co phan trang va loc
        public async Task<Paging<RoleDto>> getRoles(GridifyQuery query)
        {
            var rawKey = $"Roles_{query.Page}_{query.PageSize}_{query.Filter}_{query.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<RoleDto>>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

            var rolesQuery = _context.Roles.Select(role => new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description!
            });

            var result = await rolesQuery.GridifyAsync(query);

            await _cacheService.SetAsync(key, result);

            return result;
        }

        //Function lay thong tin vai tro theo ID
        public async Task<RoleDto> getRoleById(int roleId)
        {
            var rawKey = $"Role_{roleId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<RoleDto>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

            var role = await _context.Roles.FindAsync(roleId);
            if(role == null)
            {
                throw new NotFoundException("Role not found");
            }

            var roleDto = new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description!
            };

            await _cacheService.SetAsync(key, roleDto);

            return roleDto;
        }

        //Function tao vai tro
        public async Task CreateRole(CreateRoleDto dto)
        {
            var role = _context.Roles.Add(new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description
            });

            await _context.SaveChangesAsync();
        }

        //Function xoa vai tro
        public async Task DeleteRole(int roleId)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (existingRole == null)
            {
                throw new NotFoundException("Role not found");
            }
            _context.Roles.Remove(existingRole);
            await _context.SaveChangesAsync();

            var rawKey = $"Role_{roleId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }

        //Function update vai tro
        public async Task UpdateRole(int roleId, UpdateRoleDto dto)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (existingRole == null)
            {
                throw new NotFoundException("Role not found");
            }
            existingRole.RoleName = dto.RoleName;
            existingRole.Description = dto.Description;

            await _context.SaveChangesAsync();

            var rawKey = $"Role_{roleId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }

        //------------User service----------------

        //Function lay danh sach nguoi dung co phan trang va loc
        public async Task<Paging<UserDto>> getUsers(GridifyQuery gridifyQuery)
        {
            var rawKey = $"Users_{gridifyQuery.Page}_{gridifyQuery.PageSize}_{gridifyQuery.Filter}_{gridifyQuery.OrderBy}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<Paging<UserDto>>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

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

            await _cacheService.SetAsync(key, result);

            return result;
        }

        //Function lay thong tin nguoi dung theo ID
        public async Task<UserDto> GetUserById(int id)
        {
            var rawKey = $"User_{id}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var cachedData = await _cacheService.GetAsync<UserDto>(key);
            if (cachedData != null)
            {
                return cachedData;
            }

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
                throw new NotFoundException("User not found");
            }

            await _cacheService.SetAsync(key, existingUser);

            return existingUser;
        }

        //Function dang ky tai khoan 
        public async Task RegisterUser(RegisterUserDto dto)
        {
            if(await _context.Users.AnyAsync(e=>e.Email == dto.Email || e.Username == dto.Username))
            {
                throw new ConflictException("Email or Username already exists");
            }
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

        //Function xoa tai khoan nguoi dung
        public async Task DeleteUser(int userId)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser == null)
            {
                throw new NotFoundException("User not found");
            }
            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();

            var rawKey = $"User_{userId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }


        //Function update tai khoan nguoi dung
        public async Task UpdateUser(int userId, UpdateUserDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser == null)
            { 
                throw new NotFoundException("User not found");
            }
            existingUser.FullName = dto.FullName;
            existingUser.PhoneNumber = dto.PhoneNumber;
            existingUser.Address = dto.Address;
            existingUser.City = dto.City;
            existingUser.Country = dto.Country;

            await _context.SaveChangesAsync();

            var rawKey = $"User_{userId}";
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
            await _cacheService.RemoveAsync(key);
        }
    }
}
