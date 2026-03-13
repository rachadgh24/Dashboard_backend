using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Application.Services;
using task1;

namespace task1.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public UsersController(IUserService userService, INotificationService notificationService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserModel>>> GetUsers([FromQuery] string? role = null)
        {
            return await _userService.GetAllAsync(role);
        }

        [HttpGet("paginate")]
        public async Task<IActionResult> PaginateUsers([FromQuery] int page = 1)
        {
            var users = await _userService.PaginateUsersAsync(page);
            return Ok(users);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUsersCount()
        {
            var count = await _userService.GetCountAsync();
            return Ok(count);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserModel? model)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Email and password are required.");
            if (!UserRoles.IsValid(model.Role))
                return BadRequest($"Role must be one of: {string.Join(", ", UserRoles.All)}");

            try
            {
                var user = await _userService.AddUserAsync(model);
                var role = User.FindFirst("role")?.Value ?? "Admin";
                var name = ActionNotificationHelper.GetDisplayName(User);
                var message = ActionNotificationHelper.Format(role, name, "added a user");
                await _notificationService.RecordAsync(message);
                return Ok(new { user, message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await _userService.DeleteUserAsync(id)) return NotFound();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserModel? model)
        {
            if (model == null) return BadRequest();
            try
            {
                var updated = await _userService.EditUserAsync(id, model);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("email"))
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest("Role is required.");
            var normalizedRole = UserRoles.Normalize(request.Role);
            if (!UserRoles.All.Contains(normalizedRole, StringComparer.Ordinal))
                return BadRequest($"Role must be one of: {string.Join(", ", UserRoles.All)}");

            try
            {
                var updated = await _userService.ChangeRoleAsync(id, normalizedRole);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException)
            {
                return BadRequest($"Role must be one of: {string.Join(", ", UserRoles.All)}");
            }
        }
    }

    public class ChangeRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
