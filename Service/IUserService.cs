using Gridify;
using OpenQA.Selenium.DevTools.V139.CSS;
using PCShop_Backend.Dtos.UserDtos;

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
    }
}
