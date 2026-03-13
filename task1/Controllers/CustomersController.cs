using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1;
using task1.Application.Services;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;

        public CustomersController(ICustomerService customerService, INotificationService notificationService)
        {
            _customerService = customerService;
            _notificationService = notificationService;
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpGet]
        public async Task<List<CustomerModel>> GetCustomers()
        {
            return await _customerService.GetAllAsync();
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel? customerModel)
        {
            if (customerModel == null) return BadRequest();
            var addedCustomer = await _customerService.AddCustomerAsync(customerModel);
            var role = User.FindFirst("role")?.Value ?? string.Empty;
            if (role == UserRoles.GeneralManager || role == UserRoles.SocialMediaManager)
            {
                var name = ActionNotificationHelper.GetDisplayName(User);
                var message = ActionNotificationHelper.Format(role, name, "added a customer");
                await _notificationService.RecordAsync(message);
                return Ok(new { customer = addedCustomer, message });
            }
            return Ok(addedCustomer);
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerModel? customerModel)
        {
            if (customerModel == null) return BadRequest();
            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerModel);
            if (updatedCustomer == null) return NotFound();
            return Ok(updatedCustomer);
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (!await _customerService.DeleteCustomerAsync(id)) return NotFound();
            return Ok();
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? query)
        {
            var customers = await _customerService.Search(query);
            return Ok(customers);
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpGet("paginate")]
        public async Task<IActionResult> PaginateCustomers([FromQuery] int page = 1, [FromQuery] string? sortBy = null)
        {
            var customers = await _customerService.PaginateCustomersAsync(page, sortBy);
            return Ok(customers);
        }

        [Authorize(Roles = "Admin,General Manager")]
        [HttpGet("count")]
        public async Task<IActionResult> GetCustomersCount()
        {
            var count = await _customerService.GetCountAsync();
            return Ok(count);
        }
    }
}
