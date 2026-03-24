using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Models;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsAdminService _claimsService;

        public ClaimsController(IClaimsAdminService claimsService)
        {
            _claimsService = claimsService;
        }

        [Authorize(Policy = "ViewUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claims = await _claimsService.GetAllAsync();
            var data = claims.Select(c => new ClaimDto { Id = c.Id, Name = c.Name, Category = c.Category }).ToList();
            return Ok(new ApiResponse<List<ClaimDto>> { Data = data });
        }
    }
}

