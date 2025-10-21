using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Data;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Models;

namespace PCShop_Backend.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
