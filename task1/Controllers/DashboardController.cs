using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Models;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly ICarService _carService;

        public DashboardController(IUserService userService, ICustomerService customerService, ICarService carService)
        {
            _userService = userService;
            _customerService = customerService;
            _carService = carService;
        }

        [Authorize(Policy = "ViewDashboard")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
            {
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });
            }

            var totalUsers = await _userService.GetCountAsync(tenantId);
            var totalCars = await _carService.GetCountAsync(tenantId);
            var totalCustomers = await _customerService.GetCountAsync(tenantId);
            var topCustomer = await _customerService.GetCustomerWithMostCarsAsync(tenantId);

            return Ok(new ApiResponse<object>
            {
                Data = new
                {
                    totalUsers,
                    totalCars,
                    totalCustomers,
                    topCustomer = topCustomer == null ? null : new { topCustomer.Value.Name, topCustomer.Value.CarCount, cars = topCustomer.Value.Cars }
                }
            });
        }
    }
}
