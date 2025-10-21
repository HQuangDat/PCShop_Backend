using Gridify;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PCShop_Backend.Dtos.UserDtos;
using PCShop_Backend.Service;
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

        //------------Role Controller----------------
        [HttpGet("roles")]
        public async Task<IActionResult> RoleList([FromQuery] GridifyQuery query)
        {
            var roles = await _userService.getRoles(query);
            return Ok(roles);
        }

        [HttpGet("roles/{roleId}")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var role = await _userService.getRoleById(roleId);
            return Ok(role);
        }

        [HttpPost("roles/create")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            await _userService.CreateRole(dto);
            return Ok(new { message = "New role added successfully!" });
        }

        [HttpPut("roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, UpdateRoleDto dto)
        {
            await _userService.UpdateRole(roleId, dto);
            return Ok(new { message = "Role updated successfully!" });
        }

        [HttpDelete("roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            await _userService.DeleteRole(roleId);
            return Ok(new { message = "Role deleted successfully!" });
        }
    }
}
