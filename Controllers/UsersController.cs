using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Interfaces;
using PCShop_Backend.Exceptions;
using Serilog;

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RoleList([FromQuery] GridifyQuery query)
        {
            var roles = await _userService.getRoles(query);
            Log.Information("Fetched roles list");
            return Ok(roles);
        }

        [HttpGet("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var role = await _userService.getRoleById(roleId);
            if (role == null)
                throw new NotFoundException($"Role with ID {roleId} not found.");

            Log.Information("Fetched role with ID {RoleId}", roleId);
            return Ok(role);
        }

        [HttpPost("roles/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            await _userService.CreateRole(dto);
            Log.Information("Created new role: {RoleName}", dto.RoleName);
            return Ok(new { message = "New role added successfully!" });
        }

        [HttpPut("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int roleId, UpdateRoleDto dto)
        {
            await _userService.UpdateRole(roleId, dto);
            Log.Information("Updated role ID {RoleId}", roleId);
            return Ok(new { message = "Role updated successfully!" });
        }

        [HttpDelete("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            await _userService.DeleteRole(roleId);
            Log.Information("Deleted role with ID {RoleId}", roleId);
            return Ok(new { message = "Role deleted successfully!" });
        }

        //------------User Endpoint----------------
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList([FromQuery] GridifyQuery query)
        {
            var users = await _userService.getUsers(query);
            Log.Information("Fetched users list");
            return Ok(users);
        }

        [HttpGet("/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user == null)
                throw new NotFoundException($"User with ID {userId} not found.");

            Log.Information("Fetched user with ID {UserId}", userId);
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
        {
            await _userService.RegisterUser(dto);
            Log.Information("User registered successfully");
            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPut("/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDto dto)
        {
            var currentUserId = int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : 0;
            if (currentUserId != userId && !User.IsInRole("Admin"))
                throw new UnauthorizedException("You can only update your own profile unless you are an admin.");

            await _userService.UpdateUser(userId, dto);
            Log.Information("Updated user ID {UserId}", userId);
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpDelete("/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _userService.DeleteUser(userId);
            Log.Information("Deleted user with ID {UserId}", userId);
            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
