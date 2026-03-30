using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1;
using task1.Application.Services;
using task1.Models;

namespace task1.Controllers{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly INotificationService _notificationService;

        public CarsController(ICarService carService, INotificationService notificationService)
        {
            _carService = carService;
            _notificationService = notificationService;
        }
    
    [Authorize(Policy = "ViewCars")]
    [HttpGet]
    public async Task<IActionResult> GetCars()
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var cars = await _carService.GetAllAsync(tenantId);
        return Ok(new ApiResponse<List<CarModel>> { Data = cars });
    }
    [Authorize(Policy = "ViewCar")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id)
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var car = await _carService.GetByIdAsync(tenantId, id);
        if (car == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<CarModel> { Data = car });
    }
    [Authorize(Policy = "CreateCar")]
    [HttpPost]
    public async Task<IActionResult> AddCar([FromBody] CarModel? carModel)
    {
        if (carModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var addedCar = await _carService.AddCarAsync(tenantId, carModel);
        var role = User.FindFirst("role")?.Value ?? string.Empty;
        if (role == UserRoles.GeneralManager || role == UserRoles.SocialMediaManager)
        {
            var name = ActionNotificationHelper.GetDisplayName(User);
            var message = ActionNotificationHelper.Format(role, name, "added a car");
            await _notificationService.RecordAsync(message, tenantId);
            return Ok(new ApiResponse<object> { Data = new { car = addedCar, message } });
        }
        return Ok(new ApiResponse<CarModel> { Data = addedCar });
    }
    [Authorize(Policy = "EditCar")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCar(int id, [FromBody] CarModel? carModel)
    {
        if (carModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var updatedCar = await _carService.UpdateCarAsync(tenantId, id, carModel);
        if (updatedCar == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<CarModel> { Data = updatedCar });
    }
    [Authorize(Policy = "DeleteCar")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        if (!await _carService.DeleteCarAsync(tenantId, id)) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<object> { Data = null });
    }

    [Authorize(Policy = "ViewCars")]
    [HttpGet("paginate")]
    public async Task<IActionResult> PaginateCars([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var cars = await _carService.PaginateCarsAsync(tenantId, page, pageSize);
        return Ok(new ApiResponse<List<CarModel>> { Data = cars });
    }

    [Authorize(Policy = "ViewCars")]
    [HttpGet("count")]
    public async Task<IActionResult> GetCarsCount()
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

        var count = await _carService.GetCountAsync(tenantId);
        return Ok(new ApiResponse<int> { Data = count });
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out tenantId) && tenantId != Guid.Empty;
    }
}
}