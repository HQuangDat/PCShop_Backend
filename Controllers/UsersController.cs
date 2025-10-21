using Gridify;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Service;
using Serilog;
using System.Runtime.CompilerServices;

namespace PCShop_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        //------------Role Endpoint----------------
        [HttpGet("roles")]
        public async Task<IActionResult> RoleList([FromQuery] GridifyQuery query)
        {
            var roles = await _userService.getRoles(query);
            Log.Information("Fetched roles: {@roles}", roles);
            return Ok(roles);
        }

        [HttpGet("roles/{roleId}")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var role = await _userService.getRoleById(roleId);
            Log.Information("Fetched role: {@role}", role);
            return Ok(role);
        }

        [HttpPost("roles/create")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            await _userService.CreateRole(dto);
            Log.Information($"Created new role: {dto.RoleName}");
            return Ok(new { message = "New role added successfully!" });
        }

        [HttpPut("roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, UpdateRoleDto dto)
        {
            await _userService.UpdateRole(roleId, dto);
            Log.Information($"Updated role ID {roleId} with new data: {@dto}");
            return Ok(new { message = "Role updated successfully!" });
        }

        [HttpDelete("roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            await _userService.DeleteRole(roleId);
            Log.Information($"Deleted role with ID: {roleId}");
            return Ok(new { message = "Role deleted successfully!" });
        }

        //------------User Endpoint----------------
        [HttpGet("users")]
        public async Task<IActionResult> UserList([FromQuery] GridifyQuery query)
        {
            var users = await _userService.getUsers(query);
            Log.Information("Fetched users: {@users}", users);
            return Ok(users);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetUserById(userId);
            Log.Information("Fetched user: {@user}", user);
            return Ok(user);
        }

        [HttpPost("users/register")]
        public async Task<IActionResult> RegisterUser(RegisterUserDto dto)
        {
            return Ok();
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDto dto)
        {
            await _userService.UpdateUser(userId, dto);
            Log.Information($"Updated user ID {userId} with new data: {@dto}");
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _userService.DeleletUser(userId);
            Log.Information($"Deleted user with ID: {userId}");
            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
