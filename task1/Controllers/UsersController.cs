using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Application.Services;
using task1.Models;
using task1;

namespace task1.Controllers
{
    [Authorize]
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

        [Authorize(Policy = "ViewUsers")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var users = await _userService.GetAllAsync(tenantId, role);
            return Ok(new ApiResponse<List<UserModel>> { Data = users });
        }

        [Authorize(Policy = "ViewUsers")]
        [HttpGet("paginate")]
        public async Task<IActionResult> PaginateUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 4, [FromQuery] string? role = null)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var users = await _userService.PaginateUsersAsync(tenantId, page, pageSize, role);
            return Ok(new ApiResponse<List<UserModel>> { Data = users });
        }

        [Authorize(Policy = "ViewUsers")]
        [HttpGet("count")]
        public async Task<IActionResult> GetUsersCount([FromQuery] string? role = null)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var count = await _userService.GetCountAsync(tenantId, role);
            return Ok(new ApiResponse<int> { Data = count });
        }

        [Authorize(Policy = "ViewUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var user = await _userService.GetByIdAsync(tenantId, id);
            if (user == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "User not found." } });
            return Ok(new ApiResponse<UserModel> { Data = user });
        }

        [Authorize(Policy = "CreateUser")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserModel? model)
        {
            if (model == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
            if (string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Phone number and password are required." } });
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            try
            {
                var user = await _userService.AddUserAsync(tenantId, model);
                var role = User.FindFirst("role")?.Value ?? "User";
                var name = ActionNotificationHelper.GetDisplayName(User);
                var message = ActionNotificationHelper.Format(role, name, "added a user");
                return Ok(new ApiResponse<object> { Data = new { user, message } });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("phone") || ex.Message.Contains("Phone"))
            {
                return Conflict(new ApiResponse<object> { Error = new ApiError { Code = "CONFLICT", Message = ex.Message } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = ex.Message } });
            }
        }

        [Authorize(Policy = "DeleteUser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            if (!await _userService.DeleteUserAsync(tenantId, id)) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "User not found." } });
            return Ok(new ApiResponse<object> { Data = null });
        }

        [Authorize(Policy = "EditUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserModel? model)
        {
            if (model == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            try
            {
                var updated = await _userService.EditUserAsync(tenantId, id, model);
                if (updated == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "User not found." } });
                return Ok(new ApiResponse<UserModel> { Data = updated });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("phone") || ex.Message.Contains("Phone"))
            {
                return Conflict(new ApiResponse<object> { Error = new ApiError { Code = "CONFLICT", Message = ex.Message } });
            }
        }

        [Authorize(Policy = "ChangeUserRole")]
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Role is required." } });
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var normalizedRole = UserRoles.Normalize(request.Role);

            try
            {
                var updated = await _userService.ChangeRoleAsync(tenantId, id, normalizedRole);
                if (updated == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "User not found." } });
                return Ok(new ApiResponse<UserModel> { Data = updated });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Invalid role." } });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = ex.Message } });
            }
        }

        private bool TryGetTenantId(out Guid tenantId)
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out tenantId) && tenantId != Guid.Empty;
        }
    }

    public class ChangeRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
