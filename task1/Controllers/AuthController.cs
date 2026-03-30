using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Models;

namespace task1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// </summary>
        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var roleName = User.FindFirst("role")?.Value;
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });
            }

            var permissions = await _authService.GetPermissionsAsync(roleName, tenantId);
            var claims = permissions.Claims
                .Select(c => new ClaimDto { Id = c.Id, Name = c.Name, Category = c.Category })
                .ToList();
            return Ok(new ApiResponse<MePermissionsResponse> { Data = new MePermissionsResponse { Claims = claims } });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Phone number and password are required." } });
            }

            var loginResult = await _authService.LoginAsync(request.PhoneNumber, request.Password);
            if (loginResult != null)
                return Ok(new ApiResponse<object> { Data = new { token = loginResult.Token } });

            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Invalid phone number or password." } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrganizationName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Organization name, phone number, and password are required." } });
            }

            try
            {
                var registerResult = await _authService.RegisterAsync(
                    request.OrganizationName,
                    request.Name,
                    request.LastName,
                    request.PhoneNumber,
                    request.Password);
                return Ok(new ApiResponse<object> { Data = new { token = registerResult.Token } });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new ApiResponse<object> { Error = new ApiError { Code = "CONFLICT", Message = ex.Message } });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new ApiResponse<object> { Error = new ApiError { Code = "SERVER_ERROR", Message = ex.Message } });
            }
        }
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}