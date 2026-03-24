using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.DataLayer.Entities;
using task1.DataLayer.Interfaces;
using task1.Models;

namespace task1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleClaimsService _roleClaimsService;

        public AuthController(
            JwtService jwtService,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRoleClaimsService roleClaimsService)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _roleClaimsService = roleClaimsService;
        }

        /// <summary>
        /// Returns the current user's permissions (claims with category) for the frontend to show/hide sections and actions.
        /// </summary>
        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var roleName = User.FindFirst("role")?.Value;
            if (string.IsNullOrWhiteSpace(roleName))
                return Ok(new ApiResponse<MePermissionsResponse> { Data = new MePermissionsResponse { Claims = new List<ClaimDto>() } });

            var claimNames = await _roleClaimsService.GetClaimNamesForRoleAsync(roleName);
            var allClaims = await _roleRepository.GetAllClaimsAsync();
            var nameSet = new HashSet<string>(claimNames, StringComparer.OrdinalIgnoreCase);
            var claims = allClaims
                .Where(c => nameSet.Contains(c.Name))
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

            var phoneNumber = request.PhoneNumber.Trim();
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var roleName = user.Role?.Name ?? "Admin";
                var token = _jwtService.GenerateToken(user.Id.ToString(), user.PhoneNumber, user.Name, roleName);
                return Ok(new ApiResponse<object> { Data = new { token } });
            }

            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Invalid phone number or password." } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Phone number and password are required." } });
            }

            var normalizedPhoneNumber = request.PhoneNumber.Trim();
            var existingUser = await _userRepository.GetByPhoneNumberAsync(normalizedPhoneNumber);
            if (existingUser != null)
            {
                return Conflict(new ApiResponse<object> { Error = new ApiError { Code = "CONFLICT", Message = "Phone number is already registered." } });
            }

            var adminRole = await _roleRepository.GetByNameAsync("Admin");
            if (adminRole == null)
                return StatusCode(500, new ApiResponse<object> { Error = new ApiError { Code = "SERVER_ERROR", Message = "Admin role not found. Run database migrations and seed data." } });

            var newUser = new User
            {
                PhoneNumber = normalizedPhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Name = request.Name?.Trim() ?? string.Empty,
                RoleId = adminRole.Id
            };

            var createdUser = await _userRepository.AddUserAsync(newUser);
            await _userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(createdUser.Id.ToString(), createdUser.PhoneNumber, createdUser.Name, adminRole.Name);
            return Ok(new ApiResponse<object> { Data = new { token } });
        }
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}