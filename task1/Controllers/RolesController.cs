using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using task1.Application.Interfaces;
using task1.Models;
using task1.DataLayer.Interfaces;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolesAdminService _rolesService;
        private readonly IRoleRepository _roleRepository;
        private readonly IMemoryCache _cache;

        public RolesController(IRolesAdminService rolesService, IRoleRepository roleRepository, IMemoryCache cache)
        {
            _rolesService = rolesService;
            _roleRepository = roleRepository;
            _cache = cache;
        }

        [Authorize(Policy = "ViewUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleRepository.GetAllAsync();
            var data = roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name }).ToList();
            return Ok(new ApiResponse<List<RoleDto>> { Data = data });
        }

        [Authorize(Policy = "CreateUser")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Role name is required." } });

            try
            {
                var id = await _rolesService.CreateRoleAsync(request.Name);
                return Ok(new ApiResponse<object> { Data = new { id } });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object> { Error = new ApiError { Code = "CONFLICT", Message = ex.Message } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = ex.Message } });
            }
        }

        [Authorize(Policy = "ChangeUserRole")]
        [HttpPut("{id:int}/claims")]
        public async Task<IActionResult> SetClaims(int id, [FromBody] SetRoleClaimsRequest? request)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });

            try
            {
                await _rolesService.SetRoleClaimsAsync(id, request.ClaimIds ?? new List<int>());

                var roleName = await _rolesService.GetRoleNameByIdAsync(id);
                if (!string.IsNullOrWhiteSpace(roleName))
                    _cache.Remove($"roleClaims:{roleName}");

                return Ok(new ApiResponse<object> { Data = null });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = ex.Message } });
            }
        }
    }
}

