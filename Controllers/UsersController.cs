using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.UserDtos.CreateDto;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;
using PCShop_Backend.Service;
using PCShop_Backend.Exceptions;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RoleList([FromQuery] GridifyQuery query)
        {
            try
            {
                var roles = await _userService.getRoles(query);
                Log.Information("Fetched roles: {@roles}", roles);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching roles");
                throw;
            }
        }

        [HttpGet("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            try
            {
                if (roleId <= 0)
                    throw new ArgumentException("Role ID must be greater than 0.");

                var role = await _userService.getRoleById(roleId);
                if (role == null)
                    throw new NotFoundException($"Role with ID {roleId} not found.");

                Log.Information("Fetched role: {@role}", role);
                return Ok(role);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching role with ID {RoleId}", roleId);
                throw;
            }
        }

        [HttpPost("roles/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentException("Role data cannot be null.");

                if (string.IsNullOrWhiteSpace(dto.RoleName))
                    throw new ArgumentException("Role name is required.");

                await _userService.CreateRole(dto);
                Log.Information($"Created new role: {dto.RoleName}");
                return Ok(new { message = "New role added successfully!" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict while creating role: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating role");
                throw;
            }
        }

        [HttpPut("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int roleId, UpdateRoleDto dto)
        {
            try
            {
                if (roleId <= 0)
                    throw new ArgumentException("Role ID must be greater than 0.");

                if (dto == null)
                    throw new ArgumentException("Role data cannot be null.");

                if (string.IsNullOrWhiteSpace(dto.RoleName))
                    throw new ArgumentException("Role name is required.");

                await _userService.UpdateRole(roleId, dto);
                Log.Information($"Updated role ID {roleId} with new data: {@dto}");
                return Ok(new { message = "Role updated successfully!" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Role not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating role with ID {RoleId}", roleId);
                throw;
            }
        }

        [HttpDelete("roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            try
            {
                if (roleId <= 0)
                    throw new ArgumentException("Role ID must be greater than 0.");

                await _userService.DeleteRole(roleId);
                Log.Information($"Deleted role with ID: {roleId}");
                return Ok(new { message = "Role deleted successfully!" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("Role not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting role with ID {RoleId}", roleId);
                throw;
            }
        }

        //------------User Endpoint----------------
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList([FromQuery] GridifyQuery query)
        {
            try
            {
                var users = await _userService.getUsers(query);
                Log.Information("Fetched users: {@users}", users);
                return Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching users");
                throw;
            }
        }

        [HttpGet("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0.");

                var user = await _userService.GetUserById(userId);
                if (user == null)
                    throw new NotFoundException($"User with ID {userId} not found.");

                Log.Information("Fetched user: {@user}", user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching user with ID {UserId}", userId);
                throw;
            }
        }

        [HttpPost("users/register")]
        public async Task<IActionResult> RegisterUser(RegisterUserDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentException("User registration data cannot be null.");

                if (string.IsNullOrWhiteSpace(dto.Email))
                    throw new ArgumentException("Email is required.");

                if (string.IsNullOrWhiteSpace(dto.Password))
                    throw new ArgumentException("Password is required.");

                await _userService.RegisterUser(dto);
                Log.Information("User registered successfully: {Email}", dto.Email);
                return Ok(new { message = "User registered successfully!" });
            }
            catch (ConflictException ex)
            {
                Log.Warning("Conflict during user registration: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error registering user");
                throw;
            }
        }

        [HttpPut("users/{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDto dto)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0.");

                if (dto == null)
                    throw new ArgumentException("User data cannot be null.");

                // Check if the user is updating their own profile or is an admin
                var currentUserId = int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : 0;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    throw new UnauthorizedException("You can only update your own profile unless you are an admin.");
                }

                await _userService.UpdateUser(userId, dto);
                Log.Information($"Updated user ID {userId} with new data: {@dto}");
                return Ok(new { message = "User updated successfully!" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("User not found while updating: {Message}", ex.Message);
                throw;
            }
            catch (UnauthorizedException ex)
            {
                Log.Warning("Unauthorized update attempt for user {UserId}: {Message}", userId, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating user with ID {UserId}", userId);
                throw;
            }
        }

        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than 0.");

                await _userService.DeleteUser(userId);
                Log.Information($"Deleted user with ID: {userId}");
                return Ok(new { message = "User deleted successfully!" });
            }
            catch (NotFoundException ex)
            {
                Log.Warning("User not found while deleting: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting user with ID {UserId}", userId);
                throw;
            }
        }
    }
}
