using BusinessLogic.DTOs.requests;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { Succeeded = true, Data = users });
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] RoleUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new { Succeeded = false, Message = "Role is required" });
            }

            try
            {
                var user = await _userService.ChangeUserRoleAsync(id, request.Role);
                return Ok(new { Succeeded = true, Message = "User role updated successfully", Data = user });
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found")
                {
                    return NotFound(new { Succeeded = false, Message = ex.Message });
                }
                return BadRequest(new { Succeeded = false, Message = ex.Message });
            }
        }
    }
}
