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
        var cars = await _carService.GetAllAsync();
        return Ok(new ApiResponse<List<CarModel>> { Data = cars });
    }
    [Authorize(Policy = "ViewCar")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id)
    {
        var car = await _carService.GetByIdAsync(id);
        if (car == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<CarModel> { Data = car });
    }
    [Authorize(Policy = "CreateCar")]
    [HttpPost]
    public async Task<IActionResult> AddCar([FromBody] CarModel? carModel)
    {
        if (carModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
        var addedCar = await _carService.AddCarAsync(carModel);
        var role = User.FindFirst("role")?.Value ?? string.Empty;
        if (role == UserRoles.GeneralManager || role == UserRoles.SocialMediaManager)
        {
            var name = ActionNotificationHelper.GetDisplayName(User);
            var message = ActionNotificationHelper.Format(role, name, "added a car");
            await _notificationService.RecordAsync(message);
            return Ok(new ApiResponse<object> { Data = new { car = addedCar, message } });
        }
        return Ok(new ApiResponse<CarModel> { Data = addedCar });
    }
    [Authorize(Policy = "EditCar")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCar(int id, [FromBody] CarModel? carModel)
    {
        if (carModel == null) return BadRequest(new ApiResponse<object> { Error = new ApiError { Code = "VALIDATION_ERROR", Message = "Request body is required." } });
        var updatedCar = await _carService.UpdateCarAsync(id, carModel);
        if (updatedCar == null) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<CarModel> { Data = updatedCar });
    }
    [Authorize(Policy = "DeleteCar")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        if (!await _carService.DeleteCarAsync(id)) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Car not found." } });
        return Ok(new ApiResponse<object> { Data = null });
    }

    [Authorize(Policy = "ViewCars")]
    [HttpGet("paginate")]
    public async Task<IActionResult> PaginateCars([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        var cars = await _carService.PaginateCarsAsync(page, pageSize);
        return Ok(new ApiResponse<List<CarModel>> { Data = cars });
    }

    [Authorize(Policy = "ViewCars")]
    [HttpGet("count")]
    public async Task<IActionResult> GetCarsCount()
    {
        var count = await _carService.GetCountAsync();
        return Ok(new ApiResponse<int> { Data = count });
    }
}
}