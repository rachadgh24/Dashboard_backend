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
            var isAdmin = User.IsInRole("Admin");
            var totalUsers = isAdmin ? await _userService.GetCountAsync() : (int?)null;
            var totalCars = await _carService.GetCountAsync();
            var totalCustomers = await _customerService.GetCountAsync();
            var topCustomer = await _customerService.GetCustomerWithMostCarsAsync();

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
