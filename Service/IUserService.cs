using Gridify;
using OpenQA.Selenium.DevTools.V139.CSS;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;

namespace PCShop_Backend.Service
{
    public interface IUserService
    {
        //------------Role service interface  ----------------
        Task<Paging<RoleDto>> getRoles(GridifyQuery gridifyQuery);
        Task<RoleDto> getRoleById(int roleId);
        Task CreateRole(CreateRoleDto dto);
        Task UpdateRole(int userId, UpdateRoleDto dto);
        Task DeleteRole(int roleId);

        //------------User service interface  ----------------
        Task<Paging<UserDto>> getUsers(GridifyQuery gridifyQuery);
        Task<UserDto> GetUserById(int id);
        Task RegisterUser(RegisterUserDto dto);
        Task DeleletUser(int userId);
        Task UpdateUser(int userId, UpdateUserDto dto);
    }
}
