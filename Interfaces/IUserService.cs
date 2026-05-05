using Gridify;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;

namespace PCShop_Backend.Interfaces
{
    public interface IUserService
    {
        Task<Paging<RoleDto>> getRoles(GridifyQuery gridifyQuery);
        Task<RoleDto> getRoleById(int roleId);
        Task CreateRole(CreateRoleDto dto);
        Task UpdateRole(int roleId, UpdateRoleDto dto);
        Task DeleteRole(int roleId);

        Task<Paging<UserDto>> getUsers(GridifyQuery gridifyQuery);
        Task<UserDto> GetUserById(int id);
        Task RegisterUser(RegisterUserDto dto);
        Task DeleteUser(int userId);
        Task UpdateUser(int userId, UpdateUserDto dto);
    }
}
